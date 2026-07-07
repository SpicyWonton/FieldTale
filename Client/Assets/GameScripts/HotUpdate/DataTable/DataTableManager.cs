using System;
using System.Collections.Generic;
using FieldTale.DataTables;
using GameFramework;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;
using Luban.SimpleJSON;

namespace FieldTale.HotUpdate
{
    public static class DataTableManager
    {
        private static readonly string[] s_DataTableNames =
        {
            "Scene/tbscene",
        };

        private static readonly Dictionary<string, string> s_TableTexts = new Dictionary<string, string>();
        private static readonly LoadAssetCallbacks s_LoadAssetCallbacks = new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFailure);

        private static Action<bool> s_LoadCompleteCallback = null;
        private static int s_LoadingCount = 0;
        private static bool s_LoadingHasError = false;

        public static Tables Tables
        {
            get
            {
                return s_Tables;
            }
        }

        private static Tables s_Tables = null;

        public static int Count
        {
            get
            {
                return s_TableTexts.Count;
            }
        }

        public static bool HasTable(string tableName)
        {
            return s_TableTexts.ContainsKey(tableName);
        }

        public static string GetTableText(string tableName)
        {
            if (!s_TableTexts.TryGetValue(tableName, out string tableText))
            {
                throw new KeyNotFoundException(Utility.Text.Format("Luban data table '{0}' is not loaded.", tableName));
            }

            return tableText;
        }

        public static void LoadTables(Action<bool> loadCompleteCallback)
        {
            s_LoadCompleteCallback = loadCompleteCallback;
            s_LoadingHasError = false;

            HashSet<string> loadingTableNames = new HashSet<string>();
            for (int i = 0; i < s_DataTableNames.Length; i++)
            {
                string tableName = s_DataTableNames[i];
                if (string.IsNullOrEmpty(tableName))
                {
                    Log.Warning("Luban data table name is invalid.");
                    continue;
                }

                if (!s_TableTexts.ContainsKey(tableName))
                {
                    loadingTableNames.Add(tableName);
                }
            }

            s_LoadingCount = loadingTableNames.Count;
            if (s_LoadingCount <= 0)
            {
                CompleteLoading();
                return;
            }

            foreach (string tableName in loadingTableNames)
            {
                string assetName = AssetUtility.GetDataTableAsset(tableName);
                FrameworkRoot.Resource.LoadAsset(assetName, s_LoadAssetCallbacks, tableName);
            }
        }

        private static void OnLoadAssetSuccess(string assetName, object asset, float duration, object userData)
        {
            string tableName = (string)userData;
            TextAsset textAsset = asset as TextAsset;
            if (textAsset == null)
            {
                Log.Error("Luban data table asset '{0}' is invalid.", assetName);
                s_LoadingHasError = true;
            }
            else
            {
                s_TableTexts[tableName] = textAsset.text;
                Log.Info("Load Luban data table '{0}' OK.", assetName);
            }

            FinishOneTableLoading();
        }

        private static void OnLoadAssetFailure(string assetName, LoadResourceStatus status, string errorMessage, object userData)
        {
            Log.Error("Load Luban data table '{0}' failed, status '{1}', error message '{2}'.", assetName, status, errorMessage);
            s_LoadingHasError = true;
            FinishOneTableLoading();
        }

        private static void FinishOneTableLoading()
        {
            s_LoadingCount--;
            if (s_LoadingCount <= 0)
            {
                CompleteLoading();
            }
        }

        private static void CompleteLoading()
        {
            if (!s_LoadingHasError)
            {
                s_Tables = new Tables(tableName => JSONNode.Parse(GetTableText(tableName)));
            }

            Action<bool> loadCompleteCallback = s_LoadCompleteCallback;
            s_LoadCompleteCallback = null;
            loadCompleteCallback?.Invoke(!s_LoadingHasError);
        }
    }
}
