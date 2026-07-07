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
    }
}