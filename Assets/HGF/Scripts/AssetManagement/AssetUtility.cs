
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetManagement
{
    public class AssetUtility
    {
        public static AssetInternalLoader LoadAsset<T>(string assetName)
        {
            return LoadAsset(assetName, typeof(T));
        }

        public static AssetInternalLoader LoadAsset(string assetName, System.Type type)
        {
            return AssetManager.Instance.LoadBundleAsset(assetName, type);
        }


        public static AssetInternalLoader LoadScene(string assetName, UnityEngine.SceneManagement.LoadSceneMode mode = UnityEngine.SceneManagement.LoadSceneMode.Single)
        {
            AssetInternalLoader loader = AssetManager.Instance.LoadBundleAsset(assetName, typeof(Object));
            if (loader != null)
            {
                loader.isSceneLoad = true;
                loader.loadSceneMode = mode;
            }
            return loader;
        }

        public static AssetInternalLoader PreLoadAsset(string assetName, Type type = null)
        {
            AssetInternalLoader loader = AssetManager.Instance.LoadBundleAsset(assetName, type != null ? type : typeof(Object));
            if (loader != null)
            {
                loader.isPreLoad = true;
            }
            return loader;
        }

        //预加载列表
        public static void PreLoadAssetGroup(string preStr)
        {
            string[] downArray = preStr.Split(',');
            foreach (var item in downArray)
                PreLoadAsset(item);
        }

        public static void PreLoadGameObject(string preName, int stype = 1)
        {
            string poolName = string.Empty;

            if (stype == 1)
                poolName = GopManager.Effect;//特效
            else if (stype == 2)
                poolName = GopManager.Avatar;//模型

            GameObjectPool pool = GopManager.Instance.TryGet(poolName);
            bool had = pool.ContainsKey(preName);
            if (had)
            {
                //已经存在
                return;
            }

            AssetInternalLoader load = AssetManager.Instance.LoadBundleAsset(preName, typeof(GameObject));//加载
            load.onComplete += (AssetInternalLoader load2) =>
            {
                if (string.IsNullOrEmpty(load2.Error))
                {
                    GameObject obj = load2.Instantiate<GameObject>();
                    pool.Release(obj, preName, false);
                }

            };
        }

        public static void DestroyAsset(Object obj, float t = 0.0f)
        {
            AssetCache.DestroyAsset(obj, t);
        }

        public static void UnloadAsset(string assetName)
        {
            //AssetManager.Instance.UnloadAsset(assetName);
        }

        //public static void UnloadSceneAsset(string assetName)
        //{
        //    AssetManager.Instance.UnloadScene(assetName);
        //}


        public static void UnloadAllObjectPool(string[] ignores)
        {
            GopManager.Instance.ClearAll(ignores);
        }


        public static string GetAssetMD5(string assetName)
        {
            string abName = AssetManager.Instance.GetAssetBundleName(assetName);
            return AssetManager.Instance.GetAssetBundleMd5(abName);
        }


        /// <summary>
        /// 项目中是否存在此资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static bool Contains(string assetName)
        {
            return !string.IsNullOrEmpty(assetName) && AssetManager.Instance.AssetLoaderOptions != null && AssetManager.Instance.AssetLoaderOptions.ContainsAsset(assetName);
        }

        /// <summary>
        /// 本地是否存在此资源
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static bool ContainsLocal(string assetName)
        {
            return true;
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="force">是否立即卸载</param>
        public static IEnumerator UnloadUnusedAssets(bool force = false)
        {
            yield return null;
            //yield return AssetManager.Instance.UnloadUnusedAssets(force);
        }

        public static void UnloadAllAssets()
        {
            AssetManager.Instance.UnloadAllAssets();
        }

        public static void UnloadAllAssets(float time)
        {
            //#if UNITY_EDITOR

            //#else
            if (CheckUnloadTime(time))
                AssetManager.Instance.UnloadAllAssets();
            //#endif

        }

        /// <summary>
        /// 检查现在是否到达了离上次清理的指定的清理时间点
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool CheckUnloadTime(float time)
        {
            return Time.realtimeSinceStartup - AssetManager.unloadtimetag >= time;
        }

        /// <summary>
        /// 重置gc时间点，让gc再延后一点，注意此接口为临时接口
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static void ResetUnloadTime(float time = 0)
        {
            //延迟一下
            AssetManager.unloadtimetag = Time.realtimeSinceStartup - time;
        }





        public static void UnloadUnusedCount()
        {
            AssetManager.Instance.UnloadUnusedAssetsCount();
        }

        /// <summary>
        /// 返回此资源需要下载的资源包数量
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static int GetNeedDownloadBundleCount(string assetName)
        {
            List<string> list = ListPool<string>.Get();
            AssetManager.Instance.GetNeedDownloadAssetBundleByAssetName(assetName, ref list);
            int result = list.Count;
            ListPool<string>.Release(list);
            return result;
        }

        /// <summary>
        /// 返回此资源需要下载的资源包数量
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static string[] GetNeedDownloadBundleNames(string assetName)
        {
            List<string> list = ListPool<string>.Get();
            AssetManager.Instance.GetNeedDownloadAssetBundleByAssetName(assetName, ref list);
            string[] result = list.ToArray();
            ListPool<string>.Release(list);
            return result;
        }

        /// <summary>
        /// 请求下载资源 若资源本地已经存在则无需下载
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <param name="priority">优先级</param>
        /// <returns></returns>
        public static int DownloadAssetByAssetName(string assetName, int priority = 1)
        {
            List<string> list = ListPool<string>.Get();
            AssetManager.Instance.GetNeedDownloadAssetBundleByAssetName(assetName, ref list);
            int result = list.Count;
            if (result > 0)
            {
                string assetBundleName = null;
                string md5 = null;
                for (int i = 0; i < result; i++)
                {
                    assetBundleName = list[i];
                    md5 = AssetManager.Instance.GetAssetBundleMd5(assetBundleName);
                    AssetBundleDownloader.GetAssetBundle(assetBundleName, null, md5, md5, 0, priority);
                }
            }
            ListPool<string>.Release(list);
            return result;
        }

        /// <summary>
        /// 返回一个资源关联到的所有包的信息
        /// </summary>
        /// <returns></returns>
        public static string GetAssetConcernInfo(string assetName)
        {

            string assetBundleName = AssetManager.Instance.GetAssetBundleName(assetName);
            if (string.IsNullOrEmpty(assetBundleName))
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            string[] deps = AssetManager.Instance.GetAssetBundleDependence(assetBundleName);
            string format = "{0,-50},{1,-20},{2,-50},{3,-50},{4,-50}\n";

            sb.AppendFormat(format, "Name", "Size", "Hash", "L_md5", "S_md5");

            List<string> list = new List<string>(deps);
            list.Insert(0, assetBundleName);

            foreach (var abName in list)
            {
                string pathLocalPath = AssetManager.Instance.GetAssetDownloadSavePath(abName);
                string lmd5 = File.Exists(pathLocalPath) ? XFileUtility.FileMd5(pathLocalPath) : string.Empty;
                sb.AppendFormat(format, abName,
                    AssetManager.Instance.GetAssetBundleSize(abName),
                    AssetManager.Instance.GetAssetBundleHash(abName),
                    lmd5,
                    AssetManager.Instance.GetAssetBundleMd5(abName));
            }
            return sb.ToString();
        }



        public static List<string> FindAssets(string pattern)
        {
            GameLoaderOptions options = (GameLoaderOptions)AssetManager.Instance.AssetLoaderOptions;
            if (!options.xAssetManifest) return null;
            List<string> result = null;

            try
            {
                Regex regex = new Regex(pattern);
                foreach (var item in options.xAssetManifest.assetsOrmBundles)
                {
                    if (regex.IsMatch(item.Key))
                    {
                        if (result == null)
                            result = new List<string>();

                        result.Add(item.Key);
                    }
                }
            }
            catch (Exception e)
            {
                XLogger.ERROR_Format("AssetUtility.FindAssets {0}", e.ToString());
            }
            return result;
        }
    }
}
