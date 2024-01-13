using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using AssetManagement;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XGUI
{
    public class XImage : Image
    {
        static Color s_DefulatColor = new Color(1, 1, 1, 0);
        private bool m_LoadTag = false;
        private Sprite m_RawSprite;
        private Color m_CacheColor;
        [SerializeField]
        private string m_SpriteAssetName;
        private string m_CurSpriteAssetName;
        [SerializeField]
        private string m_ImageUrl;
        [SerializeField]
        private bool m_ChangeClearOld = true;
        [SerializeField]
        private bool m_SetNativeSize = true;
        [SerializeField]
        private bool m_Visible = true;
        [SerializeField]
        private bool m_WaitFrame = false;
        
        [SerializeField]
        private Sprite[] m_Sprites;
        [SerializeField]
        private Sprite m_ErrorSprite;
        private bool m_DefRaycastTarget;
        public UnityAction onComplete;
        public bool autoSetNativeSize { get { return m_SetNativeSize; } set { m_SetNativeSize = value; } }

        protected override void Awake()
        {
            base.Awake();
            this.m_DefRaycastTarget = raycastTarget;
            this.m_CacheColor = color;
        }

        protected override void Start()
        {
            base.Start();
            if (Application.isPlaying)
            {
                //没有放图片的Image直接透明
                if (sprite == null && base.color.a == 1)
                    base.color = s_DefulatColor;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (m_CurSpriteAssetName != m_SpriteAssetName)
            {
                if (!string.IsNullOrEmpty(m_SpriteAssetName) && Application.isPlaying)
                {
                    StartCoroutine(LoadAsync());
                }
            }
        }


        public bool changeClearOld
        {
            get { return m_ChangeClearOld; }
            set { m_ChangeClearOld = value; }
        }


        public override Color color
        {
            get
            {
                return base.color;
            }
            set
            {
                this.m_CacheColor = value;
                base.color = value;
            }
        }

        public override bool raycastTarget
        {
            get { return base.raycastTarget; }
            set
            {
                this.m_DefRaycastTarget = value;
                if (m_Visible)
                    base.raycastTarget = value;
            }
        }

        public virtual bool visible
        {
            get { return m_Visible; }
            set
            {
                if (value == m_Visible)
                    return;
                m_Visible = value;
                base.raycastTarget = this.m_DefRaycastTarget;
                SetAllDirty();
            }
        }


        public string spriteAssetName
        {
            get { return m_SpriteAssetName; }
            set
            {
                if (m_SpriteAssetName == value)
                    return;

                if (string.IsNullOrEmpty(value))
                {
                    ClearSprite();
                    return;
                }

                if (m_ChangeClearOld)
                    ClearSprite();

                m_SpriteAssetName = value;

                if (!m_LoadTag)
                {
                    if (!IsActive())
                        return;
                    m_LoadTag = true;
                    StopAllCoroutines();
                    StartCoroutine(LoadAsync());
                }
            }
        }


        public string imageUrl
        {
            get { return m_ImageUrl; }
            set
            {
                if (string.IsNullOrEmpty(value) || value.Equals(m_ImageUrl))
                    return;

                m_ImageUrl = value;

                if (!m_LoadTag)
                {
                    if (!IsActive())
                        return;
                    m_LoadTag = true;
                    StopAllCoroutines();
                    StartCoroutine(DownLoaderAsync());
                }
            }
        }

        IEnumerator LoadAsync()
        {
            if (m_WaitFrame)
            {
                //int count = Random.Range(1, 5);
                //for (int i = 0; i < count; i++)
                //    yield return 0;
            }

            this.m_LoadTag = false;


            if (AssetCache.ContainsRawObject(this.spriteAssetName))
            {
                SetSprite(AssetCache.GetRawObject<Sprite>(this.spriteAssetName));
                yield break;
            }

            AssetInternalLoader loader = null;
            if (AssetUtility.Contains(this.spriteAssetName))
                loader = AssetUtility.LoadAsset<Sprite>(this.spriteAssetName);

            //资源不存在
            if (loader == null)
            {
                string assetname = m_SpriteAssetName;
                ClearSprite();
                m_SpriteAssetName = assetname;
                SetSprite(m_ErrorSprite);
                yield break;
            }

            //loader.Update();

            if (loader.IsDone())
            {
                SetSprite(string.IsNullOrEmpty(loader.Error) ? loader.GetRawObject<Sprite>() : m_ErrorSprite);
                yield break;
            }

            string loadAssetName = this.spriteAssetName;

            yield return loader;

            if (loadAssetName == this.spriteAssetName)
                SetSprite(string.IsNullOrEmpty(loader.Error) ? loader.GetRawObject<Sprite>() : m_ErrorSprite);
            else
                if (!string.IsNullOrEmpty(m_SpriteAssetName))
                yield return LoadAsync();
        }


        protected virtual void SetSprite(Sprite sp)
        {
            string assetname = m_SpriteAssetName;
            m_CurSpriteAssetName = m_SpriteAssetName;
            ClearSprite();
            if (sp != null)
            {
                m_SpriteAssetName = assetname;
                m_RawSprite = sp;
                sprite = m_RawSprite;
                if (m_SetNativeSize)
                    SetNativeSize();
                base.color = this.m_CacheColor;
            }
            if (onComplete != null) onComplete.Invoke();
        }

        IEnumerator DownLoaderAsync()
        {
            this.m_LoadTag = false;

            UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(this.m_ImageUrl);
#if UNITY_5
            yield return uwr.Send();
#elif UNITY_2017_1_OR_NEWER
            yield return uwr.SendWebRequest();
#endif


            if (string.IsNullOrEmpty(uwr.error))
            {
                Texture2D tex2d = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
                if (tex2d != null)
                {
                    tex2d.Compress(false);
                    tex2d.name = "ximage www texture";
                    Sprite nsp = Sprite.Create(tex2d, new Rect(0, 0, tex2d.width, tex2d.height), new Vector2(0.5f, 0.5f));
                    nsp.name = "ximage www texture";
                    SetSprite(nsp);
                }
            }

            uwr.Dispose();
        }

        public void ClearSprite()
        {
            if (this.m_RawSprite != null)
            {
                if (AssetCache.ContainsRawObject(this.m_RawSprite))
                    AssetUtility.DestroyAsset(this.m_RawSprite);
                this.m_RawSprite = null;
                this.sprite = null;
            }

            this.m_SpriteAssetName = null;
            this.m_CurSpriteAssetName = null;

            if (IsActive())
                base.color = s_DefulatColor;
        }

        protected override void OnDestroy()
        {
            if (this.onComplete != null)
                this.onComplete = null;
            this.ClearSprite();
            base.OnDestroy();
        }

    }
}
