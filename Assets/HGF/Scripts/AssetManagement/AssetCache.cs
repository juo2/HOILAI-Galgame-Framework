using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace AssetManagement
{
    public class AssetCache
    {
        public class InstanceObjectInfo
        {
            public Object m_InstanceObject;
            public XAssetBundle m_Owner;
            public RawObjectInfo m_RawInfo;
        }

        public class RawObjectInfo
        {
            public XAssetBundle m_Owner;
            public int m_ReferenceCount = 0;
            public Object m_Object;
            public string m_AssetName;
        }


        //所有已经实例化对象
        public static Dictionary<int, InstanceObjectInfo> AllInstanceObjectMap = new Dictionary<int, InstanceObjectInfo>(100);
        //所有加载出来的源对象
        public static Dictionary<Object, RawObjectInfo> AllRawObjectMap = new Dictionary<Object, RawObjectInfo>(100);

        public static Dictionary<string, RawObjectInfo> AllRawObjectNameMap = new Dictionary<string, RawObjectInfo>(100);

        internal static Object GetCacheRawObject(string assetName)
        {
            RawObjectInfo rawObject;
            if (AllRawObjectNameMap.TryGetValue(assetName, out rawObject))
                return rawObject.m_Object;
            return null;
        }

        internal static Object InstantiateObject(XAssetBundle xAssetBundle, Object obj, string assetName, Transform parent = null)
        {

            if (obj == null || obj.Equals(null))
            {
                Debug.LogError("XAssetBundle::Instantiate obj == null");
                return null;
            }

            Object instance = obj is GameObject ? Object.Instantiate(obj, parent) : obj;
            AddReference(xAssetBundle, instance, obj, assetName);
            return instance;
        }

        internal static Object AddRawObject(XAssetBundle xAssetBundle, Object obj, string assetName)
        {
            if (obj == null || obj.Equals(null))
            {
                Debug.LogError("XAssetBundle::AddRawObject obj == null");
                return null;
            }
            AddReference(xAssetBundle, obj, obj, assetName);
            return obj;
        }

        internal static Object AddRawObjectNotRef(XAssetBundle xAssetBundle, Object obj, string assetName)
        {
            if (obj == null || obj.Equals(null))
            {
                Debug.LogError("XAssetBundle::AddRawObject obj == null");
                return null;
            }
            AddReference(xAssetBundle, obj, obj, assetName, false);
            return obj;
        }


        /// <summary>
        /// 最快的返回一个缓存中的源对象并增加引用计数。注意在不使用对象时要用 DestroyAsset.DestroyAsset 来解除引用 以便在一定时间后彻底销毁该对象 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static T GetRawObject<T>(string assetName) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
                return null;

            RawObjectInfo rawObject;
            if (!AllRawObjectNameMap.TryGetValue(assetName, out rawObject))
                return default(T);

            rawObject.m_ReferenceCount++;
            if (rawObject.m_Owner != null)
                rawObject.m_Owner.RawReferenceCount++;

            return (T)rawObject.m_Object;
        }

        public static bool ContainsInstanceObject(Object obj)
        {
            return obj != null && AllInstanceObjectMap.ContainsKey(obj.GetInstanceID());
        }

        public static bool ContainsRawObject(Object obj)
        {
            return obj != null && AllRawObjectMap.ContainsKey(obj);
        }

        public static bool ContainsRawObject(string assetName)
        {
            if (string.IsNullOrEmpty(assetName)) return false;

            RawObjectInfo rawinfo;
            if (AllRawObjectNameMap.TryGetValue(assetName, out rawinfo))
            {
                if (rawinfo.m_Object.Equals(null))
                {
                    AllRawObjectNameMap.Remove(assetName);
                    return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        public static T InstantiateObject<T>(string assetName, Transform parent = null) where T : Object
        {
            if (string.IsNullOrEmpty(assetName))
                return null;

            RawObjectInfo rawInfo;
            if (!ContainsRawObject(assetName))
                return null;
            rawInfo = AllRawObjectNameMap[assetName];

            return InstantiateObject(rawInfo.m_Owner, rawInfo.m_Object, assetName, parent) as T;
        }

        public static void DestroyAsset(Object obj, float time)
        {
            if (obj == null || obj.Equals(null))
            {
                //对象为空或者空指针对象
                if (obj == null)
                    Debug.LogError("AssetsCache::DestroyObject obj == null ");
                if (obj.Equals(null))
                    Debug.LogError("AssetsCache::DestroyObject obj.Equals(null)");
                return;
            }

            InstanceObjectInfo instanceInfo;
            AllInstanceObjectMap.TryGetValue(obj.GetInstanceID(), out instanceInfo);

            RawObjectInfo rawObjectInfo;
            AllRawObjectMap.TryGetValue(obj, out rawObjectInfo);

            if (instanceInfo == null && rawObjectInfo == null)
            {
                //无此对象缓存
                Debug.LogErrorFormat("AssetsCache:DestroyAsset Object Cache not exist {0}", obj);
                return;
            }

            UnReference(obj, time);
        }
        static void AddReference(XAssetBundle xAssetBundle, Object instanceObj, Object rawObj, string assetName, bool addRef = true)
        {

            RawObjectInfo rawInfo;
            if (!AllRawObjectMap.TryGetValue(rawObj, out rawInfo))
            {
                rawInfo = new RawObjectInfo();
                rawInfo.m_Object = rawObj;
                rawInfo.m_Owner = xAssetBundle;
                rawInfo.m_AssetName = assetName;
                AllRawObjectMap.Add(rawObj, rawInfo);
            }

            if (!string.IsNullOrEmpty(assetName) && !AllRawObjectNameMap.ContainsKey(assetName))
                AllRawObjectNameMap.Add(assetName, rawInfo);

            if (instanceObj != rawObj)
            {
                int instanceID = instanceObj.GetInstanceID();
                if (!AllInstanceObjectMap.ContainsKey(instanceID))
                {
                    InstanceObjectInfo info = new InstanceObjectInfo();
                    info.m_Owner = xAssetBundle;
                    info.m_RawInfo = rawInfo;
                    info.m_InstanceObject = instanceObj;
                    AllInstanceObjectMap.Add(instanceID, info);
                }
                else
                {
                    Debug.LogErrorFormat("XAssetBundle::AddReference AllInstanceObjectMap exist {0}", instanceObj);
                    return;
                }
            }

            //不计算引用计数，一般用来预加载
            if (!addRef)
                return;

            ++rawInfo.m_ReferenceCount;
            if (xAssetBundle != null)
                ++xAssetBundle.RawReferenceCount;
        }
        static void UnReference(Object obj, float time = 0.0f)
        {
            Object rawObject = obj;
            RawObjectInfo rawInfo;
            if (!AllRawObjectMap.TryGetValue(obj, out rawInfo))
            {
                //Debug.LogErrorFormat("XAssetBundle::UnReference AllRawObjectMap not exist {0}", obj);
                //return;
            }

            InstanceObjectInfo instanceInfo;
            int instanceID = obj.GetInstanceID();
            if (AllInstanceObjectMap.TryGetValue(instanceID, out instanceInfo))
            {
                rawInfo = instanceInfo.m_RawInfo;
                rawObject = rawInfo.m_Object;
                AllInstanceObjectMap.Remove(instanceID);
                Destroy(obj, time, true);
            }


            if (rawInfo != null)
            {
                if (--rawInfo.m_ReferenceCount <= 0)
                {
                    AllRawObjectMap.Remove(rawObject);

                    if (!string.IsNullOrEmpty(rawInfo.m_AssetName) &&
                        AllRawObjectNameMap.ContainsKey(rawInfo.m_AssetName))
                    {
                        AllRawObjectNameMap.Remove(rawInfo.m_AssetName);
                    }


                    //编辑器卸载
                    if (rawInfo.m_Owner == null)
                        Destroy(rawInfo.m_Object);
                }

                if (rawInfo.m_Owner != null)
                {
                    --rawInfo.m_Owner.RawReferenceCount;
                    if (rawInfo.m_Owner.RawReferenceCount <= 0)
                    {
                        rawInfo.m_Owner.BeginDestoryTime = (int)Time.time + 5;
                    }
                }
            }
        }

        static void Destroy(Object obj, float time = 0.0f, bool isInstance = false)
        {
            if (isInstance)
            {
                if (obj is GameObject)
                {
                    Object.Destroy(obj, time);
                }
                else if (obj is Material)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            else
            {
                if (obj is TextAsset)
                    Object.DestroyImmediate(obj);
                if (!(obj is GameObject))
                    Resources.UnloadAsset(obj);
            }
        }



        public static void CheckAssetBundleRawObjectRef(XAssetBundle xAssetBundle)
        {
            foreach (var item in AllRawObjectMap)
            {
                if (item.Value.m_Owner == xAssetBundle)
                {
                    if (item.Value.m_ReferenceCount > 0)
                        return;
                }

            }
            xAssetBundle.BeginDestoryTime = (int)Time.time + 30;
        }



        static List<RawObjectInfo> s_TempRawObjectList = new List<RawObjectInfo>();
        static List<int> s_TempInstanceObjectList = new List<int>();
        public static void UnloadCacheByAssetBudnle(XAssetBundle xAssetBundle)
        {
            s_TempRawObjectList.Clear();
            s_TempInstanceObjectList.Clear();

            foreach (var item in AllRawObjectMap)
                if (item.Value.m_Owner == xAssetBundle)
                    s_TempRawObjectList.Add(item.Value);


            foreach (var item in s_TempRawObjectList)
                AllRawObjectMap.Remove(item.m_Object);

            //===========================================

            foreach (var item in AllInstanceObjectMap)
                if (item.Value.m_Owner == xAssetBundle)
                    s_TempInstanceObjectList.Add(item.Key);

            foreach (var item in s_TempInstanceObjectList)
                AllInstanceObjectMap.Remove(item);


            s_TempRawObjectList.Clear();
            s_TempInstanceObjectList.Clear();
        }


        //卸载未使用的raw对象
        public static void UnloadUnusedObject()
        {
            //NullExamine();

            //if (s_tempList.Count > 0) s_tempList.Clear();

            //foreach (var item in AllRawObjectMap)
            //{
            //    if (item.Value.m_ReferenceCount < 1)
            //        s_tempList.Add(item.Value);
            //}

            //foreach (var item in s_tempList)
            //{
            //    if (item.m_Object)
            //        AllRawObjectMap.Remove(item.m_Object);
            //    if (!string.IsNullOrEmpty(item.m_AssetName))
            //        AllRawObjectNameMap.Remove(item.m_AssetName);
            //}
            //if (s_tempList.Count > 0) s_tempList.Clear();
        }

        //空对象检查
        static List<int> s_tempRemoveList = new List<int>();
        public static void NullExamine()
        {
            //foreach (var item in AllInstanceObjectMap)
            //{
            //    if (!item.Value.m_InstanceObject || item.Value.m_InstanceObject.Equals(null))
            //    {
            //        RawObjectInfo rawInfo = item.Value.m_RawInfo;
            //        if (--rawInfo.m_ReferenceCount <= 0)
            //        {
            //            AllRawObjectMap.Remove(rawInfo.m_Object);
            //            if (!string.IsNullOrEmpty(rawInfo.m_AssetName) &&
            //                AllRawObjectNameMap.ContainsKey(rawInfo.m_AssetName))
            //            {
            //                AllRawObjectNameMap.Remove(rawInfo.m_AssetName);
            //            }

            //            //编辑器卸载
            //            if (rawInfo.m_Owner == null)
            //                Destroy(rawInfo.m_Object);
            //        }

            //        if (rawInfo.m_Owner != null)
            //            --rawInfo.m_Owner.RawReferenceCount;

            //        s_tempRemoveList.Add(item.Key);
            //    }
            //}

            //if (s_tempRemoveList.Count > 0)
            //{
            //    foreach (var id in s_tempRemoveList)
            //        AllInstanceObjectMap.Remove(id);
            //    s_tempRemoveList.Clear();
            //}
        }

        public static void ClearCache()
        {
            AllInstanceObjectMap.Clear();
            AllRawObjectMap.Clear();
            AllRawObjectNameMap.Clear();
        }
    }
}
