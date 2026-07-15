using GameFramework;

namespace FieldTale.HotUpdate
{
    public static class AssetUtility
    {
        public static string GetSceneAsset(string sceneName)
        {
            return Utility.Text.Format("Assets/GameRes/Scenes/{0}.unity", sceneName);
        }

        public static string GetDataTableAsset(string tableName)
        {
            return Utility.Text.Format("Assets/GameRes/DataTables/{0}.json", tableName);
        }

        public static string GetUIAsset(string folder, string uiName)
        {
            return Utility.Text.Format("Assets/GameRes/UI/UIForms/{0}/{1}.prefab", folder, uiName);
        }

        public static string GetDllAsset(string dllName)
        {
            return Utility.Text.Format("Assets/GameRes/Dlls/{0}.bytes", dllName);
        }
    }
}