namespace FieldTale
{
    [System.Serializable]
    public class VersionInfo
    {
        public bool ForceUpdateGame;
        public string LatestGameVersion;
        public int InternalGameVersion;
        public int InternalResourceVersion;
        public string UpdatePrefixUri;
        public int VersionListLength;
        public int VersionListHashCode;
        public int VersionListCompressedLength;
        public int VersionListCompressedHashCode;
    }
}
