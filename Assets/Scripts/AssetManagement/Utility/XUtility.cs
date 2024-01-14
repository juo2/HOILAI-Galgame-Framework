using System;
using UnityEngine;
using System.Collections;
using System.Text;

public class XUtility
{
    public static System.Reflection.Assembly LoadAssembly(string file)
    {
        return System.Reflection.Assembly.Load(System.IO.File.ReadAllBytes(file));
    }

    public static string FormatBytes(long bytes)
    {
        string result = string.Empty;
        if (bytes >= 1048576)
            result = string.Format("{0} MB", (bytes / 1048576f).ToString("f2"));
        else if (bytes >= 1024)
            result = string.Format("{0} KB", (bytes / 1024f).ToString("f2"));
        else
            result = string.Format("{0} B", bytes);
        return result;
    }

    public static string FormatBytes(double bytes)
    {
        return FormatBytes((long)bytes);
    }

    //加层
    public static int AddMask(int target, params int[] masks)
    {
        for (int i = 0; i < masks.Length; i++)
            target |= 1 << masks[i];
        return target;
    }

    //减层
    public static int SubMask(int target, params int[] masks)
    {
        for (int i = 0; i < masks.Length; i++)
            target &= ~(1 << masks[i]);
        return target;
    }


    //public static bool ScreenPointToScene(Vector3 screenPos, out RaycastHit hitInfo, params string[] layerMask)
    //{
    //    screenPos = XCamera.mainCamera.ViewportToScreenPoint(XCamera.guiCamera.ScreenToViewportPoint(screenPos));
    //    Ray ray = XCamera.mainCamera.ScreenPointToRay(screenPos);
    //    bool b = Physics.Raycast(ray, out hitInfo, Mathf.Infinity, LayerMask.GetMask(layerMask));
    //    Debug.DrawRay(ray.origin, ray.direction * 1000f, b ? Color.green : Color.red, 3f);
    //    return b;
    //}

    //public static bool ScreenPointToScene(float x, float y, float z, out RaycastHit hitInfo, params string[] layerMask)
    //{
    //    return ScreenPointToScene(new Vector3(x, y, z), out hitInfo, layerMask);
    //}


    //当前游戏版本
    public static string GetGameVerion()
    {
        StringBuilder sb = new StringBuilder();
        if (XAssetsFiles.s_CurrentVersion != null)
        {
            sb.AppendFormat("Dev    {0}", Application.version);
            sb.AppendFormat(".{0}", XAssetsFiles.s_CurrentVersion.p_DevVersion.gitVer);
        }
        else
        {
            sb.Append(Application.version);
        }

        return sb.ToString();
    }


    public static uint ColorToUint(Color color)
    {
        return (uint)(color.a * 0xff) << 24 | (uint)(color.r * 0xff) << 16 | (uint)(color.g * 0xff) << 8 | (uint)(color.b * 0xff);
    }

    public static void UintToColor(uint value, out Color color)
    {
        color.a = (float)((value & 0xff000000) >> 24) / 0xff;
        color.r = (float)((value & 0xff0000) >> 16) / 0xff;
        color.g = (float)((value & 0xff00) >> 8) / 0xff;
        color.b = (float)((value & 0xff)) / 0xff;
    }


    //是否有连上wifi/4g/或有线
    public static bool IsNetworkValid() { return Application.internetReachability != NetworkReachability.NotReachable; }
    //是否为wifi下
    public static bool IsNetworkWifi() { return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork; }
    //是否为移动网络
    public static bool IsNetwork4G() { return Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork; }

    //是否支持astc贴图
    public static int IsSupportsASTC()
    {
        for (int i = TextureFormat.ASTC_4x4.ToInt(); i <= TextureFormat.ASTC_12x12.ToInt(); i++)
        {
            if (!SystemInfo.SupportsTextureFormat((TextureFormat)i))
            {
                return i;
            }
        }
        return -1;
    }

    public static int GetGameBatches()
    {
#if UNITY_EDITOR
        return UnityEditor.UnityStats.batches;
#else
        return 0;
#endif

    }

    public static bool isContainString(String str,String subStr)
    {
        return str.Contains(subStr);
    }
}
