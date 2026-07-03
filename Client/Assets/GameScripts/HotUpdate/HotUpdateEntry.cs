using System;
using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using HybridCLR;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace FieldTale.HotUpdate
{
    public static class HotUpdateEntry
    {
        public static readonly string[] AOTDllNames =
        {
            // "mscorlib.dll",
            // "System.dll",
            // "System.Core.dll", // 如果使用了Linq，需要这个
        };

        private static int AOTFlag;
        private static int AOTLoadFlag;


        public static void Start()
        {
#if UNITY_EDITOR
            StartHotfix();
#else
            // 为aot assembly加载原始metadata， 这个代码放aot或者热更新都行。
            // 一旦加载后，如果AOT泛型函数对应native实现不存在，则自动替换为解释模式执行。

            // 可以加载任意aot assembly的对应的dll。但要求dll必须与unity build过程中生成的裁剪后的dll一致，而不能直接使用原始dll。
            // 我们在BuildProcessor里添加了处理代码，这些裁剪后的dll在打包时自动被复制到 {项目目录}/HybridCLRData/AssembliesPostIl2CppStrip/{Target} 目录。

            // 注意，补充元数据是给AOT dll补充元数据，而不是给热更新dll补充元数据。
            // 热更新dll不缺元数据，不需要补充，如果调用LoadMetadataForAOTAssembly会返回错误。

            AOTFlag = AOTDllNames.Length;
            AOTLoadFlag = 0;
            for (int i = 0; i < AOTFlag; i++)
            {
                string dllName = AOTDllNames[i];
                string assetName = Utility.Text.Format("Assets/Game/HybridCLR/Dlls/{0}.bytes", dllName);
                GameEntry.Resource.LoadAsset(assetName, new LoadAssetCallbacks(OnLoadAOTDllSuccess, OnLoadAssetFail));
            }
#endif
        }

        private static void StartHotfix()
        {
            // 重置流程组件，初始化热更新流程。
            FrameworkRoot.Fsm.DestroyFsm<IProcedureManager>();
            var procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            ProcedureBase[] procedures =
            {
                new ProcedureChangeScene(),
                new ProcedureMain(),
                new ProcedurePreload(),
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
                StartHotfix();
            }
        }
    }
}
