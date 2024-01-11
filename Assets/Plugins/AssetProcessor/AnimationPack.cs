using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPack : ScriptableObject
{
    [System.Serializable]
    public class AnimaData
    {
        public string key;
        public AnimationClip clip;

    }

    private Dictionary<string, AnimationClip> m_clipMap;

    [SerializeField]
    private List<AnimaData> m_list = new List<AnimaData>();

    public void AddClip(string name, AnimationClip clip)
    {
        AnimaData data = new AnimaData();
        data.key = name;
        data.clip = clip;

        for (int i = 0; i < m_list.Count; i++)
        {
            if (m_list[i].key == name)
            {
                m_list.RemoveAt(i);
                i--;
                break;
            }
        }

        m_list.Add(data);
    }

    public AnimationClip GetClip(string name)
    {
        AnimationClip clip;
        clipMap.TryGetValue(name, out clip);

        return clip;
    }

    public int Count
    {
        get { return m_list.Count; }
    }

    public Dictionary<string, AnimationClip> clipMap
    {
        get
        {
            if (m_clipMap == null)
            {
                m_clipMap = new Dictionary<string, AnimationClip>();
                for (int i = 0; i < m_list.Count; i++)
                {
                    if (!m_clipMap.ContainsKey(m_list[i].key))
                        m_clipMap.Add(m_list[i].key, m_list[i].clip);
                }
            }

            return m_clipMap;
        }
    }
    public void Clean()
    {
        m_clipMap.Clear();
        m_list.Clear();
    }

}
