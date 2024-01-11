using UnityEngine;
using System.Collections;
using AssetManagement;

#if UNITY_EDITOR
public class EditorAssetLoader : AssetInternalLoader
{
    private Object m_RawObject;
    public EditorAssetLoader(string assetName)
        : base(assetName)
    {

    }

    public EditorAssetLoader(string assetName,System.Type assetType)
        : base(assetName, assetType)
    {
        IsEditor = true;
    }

    public override void Update()
    {
        if (IsDone())
            return;

        if (m_AsyncOperation != null)
        {
            return;
        }

        if (isSceneLoad)
        {
            m_AsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(this.m_AssetName, loadSceneMode);
        }
        else
        {
            this.m_RawObject = UnityEditor.AssetDatabase.LoadAssetAtPath(this.m_AssetName, this.m_AssetType);

            //转成文件名缓存
            this.m_AssetName = System.IO.Path.GetFileName(this.m_AssetName);
            if (this.m_RawObject == null)
            {
                m_Error = string.Format("EditorAssetLoader::Update m_RawObject is null  m_AssetName={0} m_AssetType={1} ", this.m_AssetName, this.m_AssetType);
                Debug.LogErrorFormat("EditorAssetLoader::Update UnityEditor.AssetDatabase.LoadAssetAtPath m_RawObject is null m_AssetName={0} m_AssetType={1}", this.m_AssetName, this.m_AssetType);
            }
            this.m_IsDone = true;
        }
    }

    public override bool IsDone()
    {
        if (asyncOperation != null)
        {
            return asyncOperation.isDone;
        }

        return this.m_IsDone;
    }

    protected override Object GetRawObject()
    {
        return this.m_RawObject;
    }
}
#endif