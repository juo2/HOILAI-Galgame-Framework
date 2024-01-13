using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace AssetManagement
{
    public class AssetBundleDownloader : AssetFileThreadDownloader
    {
        private string m_AssetBundleName;
        public static AssetBundleDownloader GetAssetBundle(string assetPath, string downloadPath, string md5 = null, string version = null, int timeout = 0, int priority = 0)
        {

            AssetBundleDownloader loader = AssetDownloadManager.Instance.GetDownloadInstance<AssetBundleDownloader>(assetPath, timeout, priority);
            loader.m_WebUrl = AssetManager.Instance.AssetLoaderOptions.GetAssetDownloadUrl(assetPath);
            loader.m_WebUrl2 = AssetManager.Instance.AssetLoaderOptions.GetAssetDownloadUrl(assetPath, 0);
            loader.m_Md5 = md5;
            loader.m_Timeout = timeout;
            loader.m_Priority = priority;
            loader.m_DownloadPath = string.IsNullOrEmpty(downloadPath) ? AssetManager.Instance.AssetLoaderOptions.GetAssetDownloadSavePath(assetPath) : downloadPath;
            loader.m_Version = version;
            loader.m_AssetBundleName = assetPath;
            return loader;
        }

        public override int totalByteSize
        {
            get { return AssetManager.Instance.GetAssetBundleSize(m_AssetBundleName); }
        }
    }
}
