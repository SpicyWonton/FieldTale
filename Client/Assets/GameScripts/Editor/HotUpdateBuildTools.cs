using System.IO;
using HybridCLR.Editor;
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

        private static readonly string[] AOTDllNames =
        {
            "Fantasy.Unity.dll",
            "GameFramework.dll",
            "System.Core.dll",
            "System.dll",
            "UnityEngine.CoreModule.dll",
            "UnityGameFramework.Runtime.dll",
            "mscorlib.dll",
        };

        [MenuItem("FieldTale/Build/Prepare HotUpdate DLL", priority = 100)]
        public static void PrepareHotUpdateDll()
        {
            PrepareHotUpdateDll(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("FieldTale/Build/Prepare AOT Metadata DLL", priority = 101)]
        public static void PrepareAOTMetadataDll()
        {
            PrepareAOTMetadataDll(EditorUserBuildSettings.activeBuildTarget);
        }

        [MenuItem("FieldTale/Build/Prepare HotUpdate And AOT DLL", priority = 102)]
        public static void PrepareAllDlls()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            bool hotUpdatePrepared = PrepareHotUpdateDll(buildTarget);
            bool aotPrepared = PrepareAOTMetadataDll(buildTarget);
            if (hotUpdatePrepared && aotPrepared)
            {
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("FieldTale/Build/HotUpdate DLL Window", priority = 200)]
        public static void OpenWindow()
        {
            HotUpdateBuildWindow.ShowWindow();
        }

        public static bool PrepareHotUpdateDll(BuildTarget buildTarget)
        {
            string hotUpdateDllOutputDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget);
            bool success = CopyDlls(hotUpdateDllOutputDir, HotUpdateDllNames, GetHotUpdateDllBytesAssetPath, "hot update");
            if (success)
            {
                AssetDatabase.Refresh();
            }

            return success;
        }

        public static bool PrepareAOTMetadataDll(BuildTarget buildTarget)
        {
            string aotDllOutputDir = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
            bool success = CopyDlls(aotDllOutputDir, AOTDllNames, GetAOTMetadataDllBytesAssetPath, "AOT metadata");
            if (success)
            {
                AssetDatabase.Refresh();
            }

            return success;
        }

        private static bool CopyDlls(string sourceDirectory, string[] dllNames, System.Func<string, string> getTargetAssetPath, string dllType)
        {
            bool success = true;
            for (int i = 0; i < dllNames.Length; i++)
            {
                string dllName = dllNames[i];
                string sourceDllPath = GetFullPath(sourceDirectory, dllName);
                if (!File.Exists(sourceDllPath))
                {
                    Debug.LogErrorFormat("{0} dll not found for target platform: {1}", dllType, sourceDllPath);
                    success = false;
                    continue;
                }

                string targetDllBytesAssetPath = getTargetAssetPath(dllName);
                string targetDllBytesPath = GetFullPath(targetDllBytesAssetPath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetDllBytesPath));
                File.Copy(sourceDllPath, targetDllBytesPath, true);
                AssetDatabase.ImportAsset(targetDllBytesAssetPath, ImportAssetOptions.ForceUpdate);
                Debug.LogFormat("Prepared {0} dll: {1} -> {2}", dllType, sourceDllPath, targetDllBytesAssetPath);
            }

            return success;
        }

        private static string GetFullPath(params string[] paths)
        {
            string path = Path.Combine(paths);
            return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(SettingsUtil.ProjectDir, path));
        }

        private static string GetHotUpdateDllBytesAssetPath(string dllName)
        {
            return Path.Combine("Assets/HybridCLRGenerate", dllName).Replace('\\', '/') + ".bytes";
        }

        private static string GetAOTMetadataDllBytesAssetPath(string dllName)
        {
            return Path.Combine("Assets/GameRes/Dlls", dllName).Replace('\\', '/') + ".bytes";
        }
    }

    public sealed class HotUpdateBuildWindow : EditorWindow
    {
        private static readonly BuildTarget[] SupportedBuildTargets =
        {
            BuildTarget.StandaloneWindows64,
            BuildTarget.StandaloneOSX,
            BuildTarget.StandaloneLinux64,
            BuildTarget.Android,
            BuildTarget.iOS,
            BuildTarget.WebGL,
        };

        private BuildTarget m_BuildTarget;

        public static void ShowWindow()
        {
            HotUpdateBuildWindow window = GetWindow<HotUpdateBuildWindow>("HotUpdate Build");
            window.minSize = new Vector2(420f, 180f);
            window.Show();
        }

        private void OnEnable()
        {
            m_BuildTarget = EditorUserBuildSettings.activeBuildTarget;
            if (System.Array.IndexOf(SupportedBuildTargets, m_BuildTarget) < 0)
            {
                m_BuildTarget = SupportedBuildTargets[0];
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Prepare platform-specific HybridCLR DLL resources", EditorStyles.boldLabel);
            EditorGUILayout.Space(6f);

            int targetIndex = System.Array.IndexOf(SupportedBuildTargets, m_BuildTarget);
            targetIndex = EditorGUILayout.Popup("Build Target", targetIndex, GetBuildTargetNames());
            if (targetIndex >= 0 && targetIndex < SupportedBuildTargets.Length)
            {
                m_BuildTarget = SupportedBuildTargets[targetIndex];
            }

            EditorGUILayout.HelpBox(
                "The selected target only chooses the HybridCLR output directory. It does not switch Unity's active build target.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Prepare HotUpdate DLL"))
                {
                    HotUpdateBuildTools.PrepareHotUpdateDll(m_BuildTarget);
                }

                if (GUILayout.Button("Prepare AOT Metadata"))
                {
                    HotUpdateBuildTools.PrepareAOTMetadataDll(m_BuildTarget);
                }
            }

            if (GUILayout.Button("Prepare All"))
            {
                bool hotUpdatePrepared = HotUpdateBuildTools.PrepareHotUpdateDll(m_BuildTarget);
                bool aotPrepared = HotUpdateBuildTools.PrepareAOTMetadataDll(m_BuildTarget);
                if (hotUpdatePrepared && aotPrepared)
                {
                    AssetDatabase.Refresh();
                }
            }
        }

        private static string[] GetBuildTargetNames()
        {
            string[] names = new string[SupportedBuildTargets.Length];
            for (int i = 0; i < SupportedBuildTargets.Length; i++)
            {
                names[i] = SupportedBuildTargets[i].ToString();
            }

            return names;
        }
    }
}
