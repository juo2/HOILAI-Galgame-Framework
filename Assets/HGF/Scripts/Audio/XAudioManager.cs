using AssetManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
namespace XAudio
{
    public class XAudioManager : MonoBehaviour
    {
        private static XAudioManager m_Instance;
        private Dictionary<string, AudioMixerGroup> m_AudioMixerGroupMap = new Dictionary<string, AudioMixerGroup>();

        public static XAudioManager instance
        { 
            get
            {
                if(m_Instance == null)
                {
                    GameObject go = new GameObject("XAudioManager");
                    m_Instance = go.AddComponent<XAudioManager>();
                    DontDestroyOnLoad(go);
                }

                return m_Instance;
            }
        }

        XAudioSource uiSource;
        XAudioSource gameSource;

        AudioMixer audioMixer;
        AssetManagement.AssetInternalLoader loader;

        public bool isInitSuccessful = false;

        public void Init()
        {
            LoadTemplateAsset();
            AudioSource ui = gameObject.AddComponent<AudioSource>();
            AudioSource game = gameObject.AddComponent<AudioSource>();

            uiSource = new XAudioSource();
            gameSource = new XAudioSource();

            uiSource.audioSource = ui;
            gameSource.audioSource = game;
        }

        void LoadTemplateAsset()
        {
#if UNITY_EDITOR
            if (AssetManagement.AssetManager.Instance.AssetLoaderOptions == null)
                AssetManagement.AssetManager.Instance.Initialize(new GameLoaderOptions());
#endif
            loader = AssetManagement.AssetUtility.LoadAsset<AudioMixer> ("AudioMixer.mixer");
            loader.onComplete += LoadDone;
        }
        private void LoadDone(AssetManagement.AssetInternalLoader load)
        {
            loader.onComplete -= LoadDone;

            if (string.IsNullOrEmpty(load.Error))
                audioMixer = load.GetRawObject<AudioMixer>();

            AudioMixerGroup[] amgs = audioMixer.FindMatchingGroups("Master");

            foreach (AudioMixerGroup item in amgs)
            {
                if (m_AudioMixerGroupMap.ContainsKey(item.name))
                    XLogger.ERROR(string.Format("AudioManager::Start m_AudioMixerGroupMap exist key={0} ", item.name));
                else
                    m_AudioMixerGroupMap.Add(item.name, item);
            }

            Debug.Log("XAudioManager.LoadDone finish");

            uiSource.mixerGroup = m_AudioMixerGroupMap["UI"];
            gameSource.mixerGroup = m_AudioMixerGroupMap["Game"];

            isInitSuccessful = true;
        }

        public XAudioSource PlayUIMusic(string assetName)
        {
            uiSource.Play(assetName);
            return uiSource;
        }

        public XAudioSource PlayGameMusic(string assetName)
        {
            gameSource.Play(assetName);
            return gameSource;
        }

        public float GetCurrentGameTime()
        {
            
            return gameSource.audioSource.time;
        }

        public float GetTotalGameTime()
        {
            return gameSource.audioSource.clip.length;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (gameSource != null)
                gameSource.Update();
        }

        private void OnDestroy()
        {
            if (uiSource != null)
                uiSource.OnDestroy();

            if (gameSource != null)
                gameSource.OnDestroy();

            m_AudioMixerGroupMap.Clear();

            if (audioMixer != null && AssetManagement.AssetCache.ContainsRawObject(audioMixer))
            {
                AssetManagement.AssetUtility.DestroyAsset(audioMixer);
                audioMixer = null;
            }
        }
    }

}
