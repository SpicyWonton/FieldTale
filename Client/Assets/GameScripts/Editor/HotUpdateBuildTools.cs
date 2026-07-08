using System.IO;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace FieldTale.Editor
{
    public static class HotUpdateBuildTools
    {
        private static readonly string[] HotUpdateDllNames =
        {
            "FieldTale.HotUpdate.dll",
        };

        [MenuItem("FieldTale/Build/Prepare HotUpdate DLL", priority = 100)]
        public static void PrepareHotUpdateDll()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(buildTarget, EditorUserBuildSettings.development);

            string hotUpdateDllOutputDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget);
            for (int i = 0; i < HotUpdateDllNames.Length; i++)
            {
                string dllName = HotUpdateDllNames[i];
                string sourceDllPath = GetFullPath(hotUpdateDllOutputDir, dllName);
                if (!File.Exists(sourceDllPath))
                {
                    Debug.LogErrorFormat("Hot update dll not found: {0}", sourceDllPath);
                    return;
                }

                string targetDllBytesAssetPath = GetDllBytesAssetPath(dllName);
                string targetDllBytesPath = GetFullPath(targetDllBytesAssetPath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetDllBytesPath));
                File.Copy(sourceDllPath, targetDllBytesPath, true);

                AssetDatabase.ImportAsset(targetDllBytesAssetPath, ImportAssetOptions.ForceUpdate);
                Debug.LogFormat("Prepared hot update dll: {0} -> {1}", sourceDllPath, targetDllBytesAssetPath);
            }

            AssetDatabase.Refresh();
        }

        private static string GetFullPath(params string[] paths)
        {
            string path = Path.Combine(paths);
            return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(SettingsUtil.ProjectDir, path));
        }

        private static string GetDllBytesAssetPath(string dllName)
        {
            return Path.Combine("Assets/HybridCLRGenerate", dllName).Replace('\\', '/') + ".bytes";
        }
    }
}
