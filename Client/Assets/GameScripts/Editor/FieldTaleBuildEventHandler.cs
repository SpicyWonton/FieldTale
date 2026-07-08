//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using FieldTale;
using GameFramework;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace FieldTale.Editor
{
    public sealed class FieldTaleBuildEventHandler : IBuildEventHandler
    {
        private const string UpdatePrefixUriFormat = "http://localhost:8080/Full/{0}_{1}/{2}";

        private string m_ApplicableGameVersion;
        private int m_InternalResourceVersion;
        private string m_OutputDirectory;

        public bool ContinueOnFailure
        {
            get
            {
                return false;
            }
        }

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions,
            string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            m_ApplicableGameVersion = applicableGameVersion;
            m_InternalResourceVersion = internalResourceVersion;
            m_OutputDirectory = outputDirectory;

            string streamingAssetsPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            Directory.CreateDirectory(streamingAssetsPath);

            string[] fileNames = Directory.GetFiles(streamingAssetsPath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                if (fileName.Contains(".gitkeep"))
                {
                    continue;
                }

                File.Delete(fileName);
            }

            RemoveEmptySubDirectories(streamingAssetsPath);
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions,
            string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
        }

        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode)
        {
            string platformPath = platform.ToString();
            string updatePrefixUri = Utility.Text.Format(UpdatePrefixUriFormat, m_ApplicableGameVersion.Replace('.', '_'), m_InternalResourceVersion, platformPath);

            string versionInfoJson = Utility.Text.Format(
                "{{\n" +
                "  \"ForceUpdateGame\": false,\n" +
                "  \"LatestGameVersion\": \"{0}\",\n" +
                "  \"InternalGameVersion\": 0,\n" +
                "  \"InternalResourceVersion\": {1},\n" +
                "  \"UpdatePrefixUri\": \"{2}\",\n" +
                "  \"VersionListLength\": {3},\n" +
                "  \"VersionListHashCode\": {4},\n" +
                "  \"VersionListCompressedLength\": {5},\n" +
                "  \"VersionListCompressedHashCode\": {6}\n" +
                "}}",
                m_ApplicableGameVersion, m_InternalResourceVersion, updatePrefixUri,
                versionListLength, versionListHashCode, versionListCompressedLength, versionListCompressedHashCode);

            string versionInfoPath = Utility.Path.GetRegularPath(Path.Combine(m_OutputDirectory, Utility.Text.Format("{0}Version.txt", platformPath)));
            File.WriteAllText(versionInfoPath, versionInfoJson);
            Debug.LogFormat("Generate version info '{0}' for '{1}'.", versionInfoPath, platformPath);
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            if (outputPackedSelected)
            {
                CopyDirectoryToStreamingAssets(outputPackedPath);
            }
            else if (outputPackageSelected)
            {
                CopyDirectoryToStreamingAssets(outputPackagePath);
            }
        }

        private static void CopyDirectoryToStreamingAssets(string sourcePath)
        {
            if (!Directory.Exists(sourcePath))
            {
                return;
            }

            string streamingAssetsPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string destFileName = Utility.Path.GetRegularPath(Path.Combine(streamingAssetsPath, fileName.Substring(sourcePath.Length)));
                FileInfo destFileInfo = new FileInfo(destFileName);
                if (!destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(fileName, destFileName);
            }
        }

        private static void RemoveEmptySubDirectories(string rootDirectory)
        {
            string[] directoryNames = Directory.GetDirectories(rootDirectory, "*", SearchOption.AllDirectories);
            for (int i = directoryNames.Length - 1; i >= 0; i--)
            {
                if (Directory.GetFileSystemEntries(directoryNames[i]).Length <= 0)
                {
                    Directory.Delete(directoryNames[i]);
                }
            }
        }
    }
}
