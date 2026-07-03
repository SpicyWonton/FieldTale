using System;
using System.Linq;
using System.Reflection;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace FieldTale
{
    public class ProcedureLaunchHotUpdate : ProcedureBase
    {
        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

#if UNITY_EDITOR
            Assembly hotUpdateAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.GetName().Name == "HotUpdate");
            StartHotUpdate(hotUpdateAssembly);
#else
            FrameworkRoot.Resource.LoadAsset("Assets/HybridCLRGenerate/HotUpdate.dll.bytes", new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFail));
#endif
        }

        private void OnLoadAssetSuccess(string assetName, object asset, float duration, object userData)
        {
            TextAsset dll = (TextAsset)asset;
            Assembly hotUpdateAssembly = Assembly.Load(dll.bytes);
            Log.Info("Load hotUpdate dll successfully.");
            StartHotUpdate(hotUpdateAssembly);
        }

        private void OnLoadAssetFail(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Log.Error("Load hotUpdate dll failed. " + errorMessage);
        }

        private void StartHotUpdate(Assembly hotUpdateAssembly)
        {
            Type hotUpdateEntry = hotUpdateAssembly.GetType("FieldTale.HotUpdate.HotUpdateEntry");
            hotUpdateEntry.GetMethod("Start").Invoke(null, null);
        }
    }
}
