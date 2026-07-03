using Luban.Defs;
using Luban.RawDefs;
using Luban.Utils;
using System.Text.RegularExpressions;

namespace Luban.Schema.Builtin;

// 覆盖内置的 "default" table importer（Priority 更高）。
// 和 DefaultTableImporter 的区别：
//   1. 数据文件所在的子目录默认不再拼进生成代码的命名空间，
//      避免出现"目录名和表/bean同名"导致的重复嵌套命名空间（如 Item.Item）。
//   2. 导出的数据文件默认按物理目录分类摆放（如 Item/tbitem.json），
//      而不是把目录名拼进文件名前缀（如 item_tbitem.json）。
// 两个行为都可以通过 xargs 开关恢复成和 DefaultTableImporter 一致：
//   -x tableImporter.includeDirInNamespace=true
//   -x tableImporter.groupOutputByDir=false
[TableImporter("default", Priority = 10)]
public class DirOrganizedTableImporter : ITableImporter
{
    private static readonly NLog.Logger s_logger = NLog.LogManager.GetCurrentClassLogger();

    public List<RawTable> LoadImportTables()
    {
        string dataDir = GenerationContext.GlobalConf.InputDataDir;

        string fileNamePatternStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "filePattern", false, "#([a-zA-Z0-9-.]+)(-.*)?$");
        string tableNamespaceFormatStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "tableNamespaceFormat", false, "{0}");
        string tableNameFormatStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "tableNameFormat", false, "Tb{0}");
        string valueTypeNameFormatStr = EnvManager.Current.GetOptionOrDefault("tableImporter", "valueTypeNameFormat", false, "{0}");
        bool includeDirInNamespace = EnvManager.Current.GetBoolOptionOrDefault("tableImporter", "includeDirInNamespace", false, false);
        bool groupOutputByDir = EnvManager.Current.GetBoolOptionOrDefault("tableImporter", "groupOutputByDir", false, true);
        var fileNamePattern = new Regex(fileNamePatternStr);
        var excelExts = new HashSet<string> { "xlsx", "xls", "xlsm", "csv" };

        var tables = new List<RawTable>();
        foreach (string file in Directory.GetFiles(dataDir, "*", SearchOption.AllDirectories))
        {
            if (FileUtil.IsIgnoreFile(dataDir, file))
            {
                continue;
            }
            string fileName = Path.GetFileName(file);
            string ext = Path.GetExtension(fileName).TrimStart('.');
            if (!excelExts.Contains(ext))
            {
                continue;
            }
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            var match = fileNamePattern.Match(fileNameWithoutExt);
            if (!match.Success || match.Groups.Count <= 1)
            {
                continue;
            }

            string relativePath = file.Substring(dataDir.Length + 1).TrimStart('\\').TrimStart('/');
            string relativeDir = Path.GetDirectoryName(relativePath) ?? "";
            string namespaceFromRelativePath = relativeDir.Replace('/', '.').Replace('\\', '.');
            string outputDirFromRelativePath = relativeDir.Replace('\\', '/');

            string rawTableFullName = match.Groups[1].Value;
            string rawTableNamespace = TypeUtil.GetNamespace(rawTableFullName);
            string rawTableName = TypeUtil.GetName(rawTableFullName);

            // 区别1：目录默认不再拼进命名空间（除非显式开启 includeDirInNamespace）。
            string tableNamespace = includeDirInNamespace
                ? TypeUtil.MakeFullName(namespaceFromRelativePath, string.Format(tableNamespaceFormatStr, rawTableNamespace))
                : string.Format(tableNamespaceFormatStr, rawTableNamespace);

            string tableName = string.Format(tableNameFormatStr, rawTableName);
            string valueTypeFullName = TypeUtil.MakeFullName(tableNamespace, string.Format(valueTypeNameFormatStr, rawTableName));
            string comment = match.Groups.Count >= 3 ? match.Groups[2].Value : null;
            comment = comment != null && comment.Length >= 1 ? comment.TrimStart('-').Trim() : "";

            // 区别2：导出数据文件按物理目录分类摆放，文件名本身不再带目录前缀。
            string outputFile = groupOutputByDir && !string.IsNullOrEmpty(outputDirFromRelativePath)
                ? $"{outputDirFromRelativePath}/{tableName.ToLower()}"
                : "";

            // 区别3：把目录信息作为 tag 传递下去，供自定义 CodeTarget 用来决定
            // 生成代码文件应该落在哪个子目录（与命名空间彻底解耦，见 DirOrganizedCsharpCodeTarget）。
            var tags = new Dictionary<string, string>();
            if (groupOutputByDir && !string.IsNullOrEmpty(outputDirFromRelativePath))
            {
                tags["outputDir"] = outputDirFromRelativePath;
            }

            var table = new RawTable()
            {
                Namespace = tableNamespace,
                Name = tableName,
                Index = "",
                ValueType = valueTypeFullName,
                ReadSchemaFromFile = true,
                Mode = TableMode.MAP,
                Comment = comment,
                Groups = new List<string> { },
                InputFiles = new List<string> { relativePath },
                OutputFile = outputFile,
                Tags = tags,
            };
            s_logger.Debug("import table file:{@}", table);
            tables.Add(table);
        }

        return tables;
    }
}
