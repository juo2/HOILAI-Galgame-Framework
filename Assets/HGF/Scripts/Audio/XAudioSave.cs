using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class XAudioSave : MonoBehaviour
{
    public AudioClip clip1; // 需要录制的 AudioClip 数组\
    public AudioClip clip2; // 需要录制的 AudioClip 数组

    //public float loopTime = 1f; // 循环时间，单位为秒
    //public AudioSource bgm; // 背景音乐的 AudioSource 组件

    private AudioClip recordedAudio; // 录制的音频数据
    //private AudioClip filteredBGM; // 过滤后的背景音乐
    //private float currentTime; // 当前录制的时间
    //private bool isRecording; // 是否正在录制

    void Start()
    {
        //recordedAudio = AudioClip.Create("Recorded Audio", 0, 2, 44100, false); // 创建录制的音频数据容器
        //filteredBGM = AudioClip.Create("Filtered BGM", 0, 2, 44100, false); // 创建过滤后的背景音乐容器
        StartRecording();
    }

    void Update()
    {

    }

    public void StartRecording()
    {
        recordedAudio = AudioClip.Create("Recorded Audio", clip1.samples, clip1.channels, clip1.frequency, false); // 创建录制的音频数据容器



        int length1 = Mathf.FloorToInt(recordedAudio.frequency * 3.5f);

        float[] data1 = new float[length1];
        clip1.GetData(data1, 0);


        int length2 = Mathf.FloorToInt(recordedAudio.frequency * 3.5f);

        int length3 = recordedAudio.frequency * 1;

        float[] data2 = new float[length2];
        clip2.GetData(data2, 0);

        recordedAudio.SetData(data1, 0);

        recordedAudio.SetData(data2, length1+ length3);

        StopRecording();
    }

    public void StopRecording()
    {

        // 将 recordedAudio 中的音频数据导出为 WAV 文件
        string path = Path.Combine(Application.dataPath, "recorded_audio.wav");

        ClipUtility.AudioClipToWav(recordedAudio, path);

        Debug.Log("保存地址：" + path);

        //File.WriteAllBytes(path, wavData);
    }
}
