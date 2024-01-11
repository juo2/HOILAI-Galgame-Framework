using System;
using System.IO;
using UnityEngine;
using UnityEditor;


public class TerrainReplaceSplatmap : ScriptableWizard
{
    public Texture2D m_Splatmap;
    public Texture2D m_NewTex;
    public bool m_FlipVertical;


    public void OnWizardUpdate()
    {
        helpString = "替换地形中的Splat贴图!";
        isValid = m_Splatmap && m_NewTex;
    }


    public void OnWizardCreate()
    {
        if (m_NewTex.format != TextureFormat.ARGB32 && m_NewTex.format != TextureFormat.RGB24 && m_NewTex.format != TextureFormat.RGBA32)
        {
            EditorUtility.DisplayDialog("格式错误", "必须是 ARGB32、RGB24 格式 -> " + m_NewTex.format, "Cancel");
            return;
        }

        int newTexW = m_NewTex.width;
        if (Mathf.ClosestPowerOfTwo(newTexW) != newTexW)
        {
            EditorUtility.DisplayDialog("大小错误", "宽度和高度必须是2的幂!", "Cancel");
            return;
        }

        try
        {
            var pixels = m_NewTex.GetPixels();
            if (m_FlipVertical)
            {
                var h = newTexW; // always square in unity
                for (var y = 0; y < h / 2; y++)
                {
                    var otherY = h - y - 1;
                    for (var x = 0; x < newTexW; x++)
                    {
                        var swapval = pixels[y * newTexW + x];
                        pixels[y * newTexW + x] = pixels[otherY * newTexW + x];
                        pixels[otherY * newTexW + x] = swapval;
                    }
                }
            }

            m_Splatmap.Reinitialize(m_NewTex.width, m_NewTex.height, m_NewTex.format, true);
            m_Splatmap.SetPixels(pixels);
            m_Splatmap.Apply();
        }
        catch (Exception err)
        {
            EditorUtility.DisplayDialog("Not readable", err.Message, "Cancel");
            return;
        }
    }


    [MenuItem("XGame/Examine/Terrain/Replace Splatmap...")]
    static void Replace()
    {
        ScriptableWizard.DisplayWizard<TerrainReplaceSplatmap>("ReplaceSplatmap", "Replace");
    }


    [MenuItem("XGame/Examine/Terrain/Export Texture")]
    static void Apply()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
        {
            EditorUtility.DisplayDialog("Select Texture", "You Must Select a Texture first!", "Ok");
            return;
        }

        var bytes = texture.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/exported_texture.png", bytes);
        AssetDatabase.Refresh();
    }
}