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
        private const string HotUpdateAssemblyName = "FieldTale.HotUpdate";
        private const string HotUpdateAssemblyAssetName = "Assets/HybridCLRGenerate/FieldTale.HotUpdate.dll.bytes";
        private const string HotUpdateEntryTypeName = "FieldTale.HotUpdate.HotUpdateEntry";

        protected override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

#if UNITY_EDITOR
            Assembly hotUpdateAssembly = System.AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly.GetName().Name == HotUpdateAssemblyName);
            if (hotUpdateAssembly == null)
            {
                Log.Error("Can not find hot update assembly '{0}'.", HotUpdateAssemblyName);
                return;
            }

            LaunchHotUpdate(hotUpdateAssembly);
#else
            FrameworkRoot.Resource.LoadAsset(HotUpdateAssemblyAssetName, new LoadAssetCallbacks(OnLoadHotUpdateAssetSuccess, OnLoadAssetFail));
#endif
        }

        private void OnLoadHotUpdateAssetSuccess(string assetName, object asset, float duration, object userData)
        {
            TextAsset dll = (TextAsset)asset;
            Assembly hotUpdateAssembly = Assembly.Load(dll.bytes);
            Log.Info("Load hot update dll OK.");
            LaunchHotUpdate(hotUpdateAssembly);
        }

        private void OnLoadAssetFail(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Log.Error("Load hot update dll failed. " + errorMessage);
        }

        private void LaunchHotUpdate(Assembly hotUpdateAssembly)
        {
            // HybridCLR loads the hot-update assembly dynamically, so its generated
            // Fantasy registrars must be initialized explicitly before any message is used.
            string assemblyInitializerTypeName = $"Fantasy.Generated.{hotUpdateAssembly.GetName().Name.Replace('.', '_')}_AssemblyInitializer";
            Type assemblyInitializer = hotUpdateAssembly.GetType(assemblyInitializerTypeName);
            MethodInfo initializeMethod = assemblyInitializer?.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static);
            if (initializeMethod == null)
            {
                Log.Error("Hot-update Fantasy assembly initializer was not found: {0}", assemblyInitializerTypeName);
                return;
            }

            initializeMethod.Invoke(null, null);

            Type hotUpdateEntry = hotUpdateAssembly.GetType(HotUpdateEntryTypeName);
            hotUpdateEntry.GetMethod("Start").Invoke(null, null);
        }
    }
}
