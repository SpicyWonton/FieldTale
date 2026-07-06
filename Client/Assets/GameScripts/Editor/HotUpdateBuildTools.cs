using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace FieldTale.Editor
{
    public static class HotUpdateBuildTools
    {
        private const string HotUpdateDllName = "FieldTale.HotUpdate.dll";
        private const string HotUpdateDllBytesAssetPath = "Assets/HybridCLRGenerate/FieldTale.HotUpdate.dll.bytes";

        [MenuItem("FieldTale/Build/Prepare HotUpdate DLL", priority = 100)]
        public static void PrepareHotUpdateDll()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(buildTarget, EditorUserBuildSettings.development);

            string hotUpdateDllOutputDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget);
            string sourceDllPath = GetFullPath(hotUpdateDllOutputDir, HotUpdateDllName);
            if (!File.Exists(sourceDllPath))
            {
                Debug.LogErrorFormat("Hot update dll not found: {0}", sourceDllPath);
                return;
            }

            string targetDllBytesPath = GetFullPath(HotUpdateDllBytesAssetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(targetDllBytesPath));
            File.Copy(sourceDllPath, targetDllBytesPath, true);

            AssetDatabase.ImportAsset(HotUpdateDllBytesAssetPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh();

            Debug.LogFormat("Prepared hot update dll: {0} -> {1}", sourceDllPath, HotUpdateDllBytesAssetPath);
        }

        private static string GetFullPath(params string[] paths)
        {
            string path = Path.Combine(paths);
            return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(SettingsUtil.ProjectDir, path));
        }
    }
}
