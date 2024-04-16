using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace AssetManagement
{
    public abstract class AssetLoaderOptions
    {
        /// <summary>
        /// 根据传入的资源名 返回AssetBundle名
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public abstract string GetAssetBundleName(string assetName);

        /// <summary>
        /// 返回AssetBundle的所有依赖
        /// </summary>
        /// <param name="assetBundlename"></param>
        /// <returns></returns>
        public abstract string[] GetAssetBundleDependence(string assetBundlename);

        /// <summary>
        /// 返回AssetBundle的大小
        /// </summary>
        /// <param name="assetBundlename"></param>
        /// <returns></returns>
        public abstract int GetAssetBundleByteSize(string assetBundlename);

        /// <summary>
        /// 根据资源名返回资源路径如   a.png -> Assets/a.png
        /// </summary>
        /// <param name="asssetName"></param>
        /// <returns></returns>
        public abstract string GetAssetPathAtName(string asssetName);

        /// <summary>
        /// 返回AssetBundle的Crc校验值
        /// </summary>
        /// <param name="assetBundlename"></param>
        /// <returns></returns>
        public abstract uint GetAssetBundleCrc(string assetBundlename);

        /// <summary>
        /// 返回AssetBundle的哈希值
        /// </summary>
        /// <param name="assetBundlename"></param>
        /// <returns></returns>
        public abstract Hash128 GetAssetBundleHash(string assetBundlename);

        /// <summary>
        /// 资产md5
        /// </summary>
        /// <param name="assetBundlename"></param>
        /// <returns></returns>
        public abstract string GetAssetBundleMd5(string assetBundlename);

        /// <summary>
        /// 返回资源包是否在首包
        /// </summary>
        /// <param name="assetBundlename"></param>
        /// <returns></returns>
        public abstract bool GetAssetBundleIsBuildin(string assetBundlename);

        /// <summary>
        /// 返回资源包是否需要下载
        /// </summary>
        /// <param name="assetBundlename"></param>
        /// <returns></returns>
        public abstract bool GetAssetBundleIsNeeedDownload(string assetBundlename);

        /// <summary>
        /// AssetBundle每帧加载的个数
        /// </summary>
        /// <returns></returns>
        public abstract int GetAssetBundleLoadMaxNum();

        /// <summary>
        /// 下载队列每帧下载的个数
        /// </summary>
        /// <returns></returns>
        public abstract int GetDownLoaderMaxNum();

        /// <summary>
        /// 返回资源下载路径
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="index">-1为默认地址</param>
        /// <returns></returns>
        public abstract string GetAssetDownloadUrl(string assetPath, int index = -1);

        /// <summary>
        /// 返回资源下载保存路径
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public abstract string GetAssetDownloadSavePath(string assetPath);

        /// <summary>
        /// 返回内置资源路径
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public abstract string GetBuildinAssetPath(string assetPath);


        /// <summary>
        /// 编辑器模式加载
        /// </summary>
        /// <returns></returns>
        public abstract bool IsEditorLoad(string assetName);

        /// <summary>
        /// 是否存在此资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public abstract bool ContainsAsset(string assetName);

        /// <summary>
        /// 记录加载资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public abstract void RecordAsset(string assetName);

        public abstract List<string> GetDontUnloadList();
    }

    public class DefaultAssetLoaderOptions : AssetLoaderOptions
    {
        protected AssetBundleManifest m_AssetBundleManifest;

        public DefaultAssetLoaderOptions()
        {

        }

        public DefaultAssetLoaderOptions(AssetBundleManifest manifest)
        {
            this.m_AssetBundleManifest = manifest;
        }

        public override string GetAssetBundleName(string assetName)
        {
            return string.Empty;
        }

        public override string[] GetAssetBundleDependence(string assetBundlename)
        {
            return this.m_AssetBundleManifest.GetAllDependencies(assetBundlename);
        }

        public override int GetAssetBundleByteSize(string assetBundlename)
        {
            return 0;
        }

        public override uint GetAssetBundleCrc(string assetBundlename)
        {
            return 0;
        }

        public override Hash128 GetAssetBundleHash(string assetBundlename)
        {
            return default(Hash128);
        }

        public override bool GetAssetBundleIsBuildin(string assetBundlename)
        {
            return true;
        }

        public override bool GetAssetBundleIsNeeedDownload(string assetBundlename)
        {
            return false;
        }

        public override string GetAssetDownloadUrl(string assetPath, int index = -1)
        {
            if (index == -1)
                return AssetDefine.RemoteDownloadUrl + assetPath;

            if (AssetDefine.RemoteSpareUrls.Count > index)
                return AssetDefine.RemoteSpareUrls[index];

            XLogger.ERROR_Format("DefaultAssetLoaderOptions::GetAssetDownloadUrl. AssetDefine.RemoteSpareUrls.Count:{0} index:{1}", AssetDefine.RemoteSpareUrls.Count, index);
            return string.Empty;
        }

        public override string GetAssetDownloadSavePath(string assetPath)
        {
            return AssetDefine.ExternalSDCardsPath + assetPath;
        }

        public override string GetBuildinAssetPath(string assetPath)
        {
            return AssetDefine.BuildinAssetPath + assetPath;
        }

        public override int GetAssetBundleLoadMaxNum()
        {
            return -1;
        }

        public override int GetDownLoaderMaxNum()
        {
            return 3;
        }

        public override string GetAssetPathAtName(string asssetName)
        {
            return asssetName;
        }

        public override bool IsEditorLoad(string assetName)
        {
            return true;
        }

        public override string GetAssetBundleMd5(string assetBundlename)
        {
            return string.Empty;
        }

        public override bool ContainsAsset(string assetName)
        {
            return false;
        }

        public override void RecordAsset(string assetName)
        {

        }

        public override List<string> GetDontUnloadList()
        {
            return null;
        }

    }
}
