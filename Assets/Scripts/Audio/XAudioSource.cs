using AssetManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace XAudio
{
    public class XAudioSource
    {
        public AudioSource audioSource;
        public AudioMixerGroup mixerGroup;
        public UnityAction onComplete;
        public UnityAction onFinish;
        public string fileName = string.Empty;
        bool isFinish = false;
        HashSet<AudioClip> clipSet = new HashSet<AudioClip>();

        AssetManagement.AssetInternalLoader loader;

        public void Play(string assetName)
        {
            isFinish = false;
            fileName = assetName;
            if (AssetCache.ContainsRawObject(assetName))
            {
                PlayInternal(AssetCache.GetRawObject<AudioClip>(assetName));
                return;
            }

#if UNITY_EDITOR
            if (AssetManagement.AssetManager.Instance.AssetLoaderOptions == null)
                AssetManagement.AssetManager.Instance.Initialize(new GameLoaderOptions());
#endif
            loader = AssetManagement.AssetUtility.LoadAsset<AudioClip>(assetName);
            loader.onComplete += LoadClipDone;
        }

        void LoadClipDone(AssetManagement.AssetInternalLoader load)
        {
            loader.onComplete -= LoadClipDone;

            if (string.IsNullOrEmpty(load.Error))
                PlayInternal(load.GetRawObject<AudioClip>());
        }

        void PlayInternal(AudioClip audioClip)
        {
            if(!clipSet.Contains(audioClip))
            {
                clipSet.Add(audioClip);
            }


            audioSource.clip = audioClip;
            audioSource.outputAudioMixerGroup = mixerGroup;
            audioSource.Play();

            onComplete?.Invoke();

        }

        public void Update()
        {
            if(audioSource != null && audioSource.clip != null && !isFinish)
            {
                if(audioSource.time == audioSource.clip.length)
                {
                    isFinish = true;
                    onFinish?.Invoke();
                }
            }
        }

        public void OnDestroy()
        {
            foreach(var clip in clipSet)
            {
                if (clip != null && AssetManagement.AssetCache.ContainsRawObject(clip))
                {
                    AssetManagement.AssetUtility.DestroyAsset(clip);
                }
            }

            clipSet.Clear();
        }
    }
}
