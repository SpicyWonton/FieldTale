using Luban.CodeTarget;
using Luban.Defs;

namespace Luban.CSharp.CodeTarget;

// 覆盖内置的 "cs-simple-json"（Priority 更高）。
// 和基类的区别：table/bean 对应的代码文件路径不再单纯由 Namespace 决定，
// 而是优先读取 DirOrganizedTableImporter 打上的 "outputDir" tag 来决定文件应该落在哪个子目录，
// 从而把"生成代码里的 C# 命名空间"和"代码文件的物理存放目录"彻底解耦。
[CodeTarget("cs-simple-json", Priority = 10)]
public class DirOrganizedCsharpSimpleJsonCodeTarget : CsharpSimpleJsonCodeTarget
{
    public override void Handle(GenerationContext ctx, OutputFileManifest manifest)
    {
        var tasks = new List<Task<OutputFile>>();
        tasks.Add(Task.Run(() =>
        {
            var writer = new CodeWriter();
            GenerateTables(ctx, ctx.ExportTables, writer);
            return CreateOutputFile($"{GetFileNameWithoutExtByTypeName(ctx.Target.Manager)}.{FileSuffixName}", writer.ToResult(FileHeader));
        }));

        foreach (var table in ctx.ExportTables)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                GenerateTable(ctx, table, writer);
                return CreateOutputFile($"{GetOutputPath(table.Tags, table.Name, table.FullName)}.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
        }

        foreach (var bean in ctx.ExportBeans)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                GenerateBean(ctx, bean, writer);
                return CreateOutputFile($"{GetOutputPath(bean.Tags, bean.Name, bean.FullName)}.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
        }

        foreach (var @enum in ctx.ExportEnums)
        {
            tasks.Add(Task.Run(() =>
            {
                var writer = new CodeWriter();
                GenerateEnum(ctx, @enum, writer);
                return CreateOutputFile($"{GetOutputPath(@enum.Tags, @enum.Name, @enum.FullName)}.{FileSuffixName}", writer.ToResult(FileHeader));
            }));
        }

        Task.WaitAll(tasks.ToArray());
        foreach (var task in tasks)
        {
            manifest.AddFile(task.Result);
        }
    }

    // outputDir 存在时：文件路径 = outputDir/类型名（忽略 Namespace 对路径的影响）。
    // outputDir 不存在时：回退到内置的默认行为（Namespace 决定路径）。
    private string GetOutputPath(Dictionary<string, string> tags, string typeName, string fullName)
    {
        if (tags != null && tags.TryGetValue("outputDir", out var dir) && !string.IsNullOrEmpty(dir))
        {
            return $"{dir}/{GetFileNameWithoutExtByTypeName(typeName)}";
        }
        return GetFileNameWithoutExtByTypeName(fullName);
    }
}
