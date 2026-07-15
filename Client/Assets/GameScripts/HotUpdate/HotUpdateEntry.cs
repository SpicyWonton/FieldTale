using HybridCLR;
using UnityEngine;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Resource;
using GameFramework.Procedure;
using UnityGameFramework.Runtime;

namespace FieldTale.HotUpdate
{
    public static class HotUpdateEntry
    {
        public static readonly string[] AOTDllNames =
        {
            "Fantasy.Unity.dll",
            "GameFramework.dll",
            "System.Core.dll",
            "System.dll",
            "UnityEngine.CoreModule.dll",
            "UnityGameFramework.Runtime.dll",
            "mscorlib.dll",
        };

        private static int AOTFlag;
        private static int AOTLoadFlag;

        public static void Start()
        {
#if UNITY_EDITOR
            LaunchHotUpdate();
#else
            AOTFlag = AOTDllNames.Length;
            AOTLoadFlag = 0;
            for (int i = 0; i < AOTFlag; i++)
            {
                string dllName = AOTDllNames[i];
                FrameworkRoot.Resource.LoadAsset(AssetUtility.GetDllAsset(dllName), new LoadAssetCallbacks(OnLoadAOTDllSuccess, OnLoadAOTDllFail));
            }
#endif
        }

        private static void LaunchHotUpdate()
        {
            // 重置流程组件，初始化热更新流程。
            FrameworkRoot.Fsm.DestroyFsm<IProcedureManager>();
            var procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            ProcedureBase[] procedures =
            {
                new ProcedureChangeScene(),
                new ProcedureMain(),
                new ProcedurePreload(),
                new ProcedureLogin(),
            };
            procedureManager.Initialize(GameFrameworkEntry.GetModule<IFsmManager>(), procedures);
            procedureManager.StartProcedure<ProcedurePreload>();
        }

        private static void OnLoadAOTDllSuccess(string assetName, object asset, float duration, object userdata)
        {
            TextAsset dll = (TextAsset) asset;
            byte[] dllBytes = dll.bytes;
            // 加载assembly对应的dll，会自动为它hook。一旦aot泛型函数的native函数不存在，用解释器版本代码
            var err = RuntimeApi.LoadMetadataForAOTAssembly(dllBytes, HomologousImageMode.SuperSet);
            Log.Info($"LoadMetadataForAOTAssembly:{assetName}. ret:{err}");
            if (++AOTLoadFlag == AOTFlag)
            {
                LaunchHotUpdate();
            }
        }

        private static void OnLoadAOTDllFail(string assetName, LoadResourceStatus status, string errormessage, object userdata)
        {
            Log.Error("Load asset failed. {0}", errormessage);
        }
    }
}
