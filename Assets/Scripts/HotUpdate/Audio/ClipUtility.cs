using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ClipUtility
{
    public static AudioClip FilterAudioClip(AudioClip bgm, float loopTime, AudioClip[] clips)
    {
        int sampleRate = bgm.frequency;
        int channelCount = bgm.channels;
        int frameCount = Mathf.RoundToInt(bgm.length * sampleRate);

        float[] bgmData = new float[frameCount * channelCount];
        bgm.GetData(bgmData, 0);

        int totalFrameCount = Mathf.RoundToInt(loopTime * sampleRate);
        float[] resultData = new float[totalFrameCount * channelCount];

        // 复制背景音乐的音频数据到 resultData 中
        for (int i = 0; i < frameCount * channelCount; i++)
        {
            resultData[i] = bgmData[i];
        }

        // 播放需要录制的 n 段 AudioClip，并将它们的音频数据累加到 resultData 中
        int offset = frameCount;
        for (int i = 0; i < clips.Length; i++)
        {
            AudioClip clip = clips[i];
            int clipFrameCount = Mathf.RoundToInt(clip.length * sampleRate);
            float[] clipData = new float[clipFrameCount * channelCount];
            clip.GetData(clipData, 0);

            for (int j = 0; j < clipFrameCount * channelCount; j++)
            {
                resultData[offset + j] += clipData[j];
            }

            offset += clipFrameCount;
        }

        // 创建一个新的 AudioClip，并将录制的音频数据复制到它的音频数据中
        AudioClip resultClip = AudioClip.Create("Recorded Audio", totalFrameCount, channelCount, sampleRate, false);
        resultClip.SetData(resultData, 0);

        return resultClip;
    }

    public static void AudioClipToWav(AudioClip clip, string filePath)
    {
        // 通过 GetOutputData 获取音频数据
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        // 将音频数据转为 16 位 PCM 格式
        Int16[] intData = new Int16[samples.Length];
        const float rescaleFactor = 32767;
        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
        }

        // 将数据写入 WAV 文件
        byte[] byteData = new byte[intData.Length * 2];
        Buffer.BlockCopy(intData, 0, byteData, 0, byteData.Length);
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                // 写入 RIFF 标识符和文件大小
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + byteData.Length);

                // 写入格式和格式块大小
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16);

                // 写入格式信息
                writer.Write((ushort)1);
                writer.Write((ushort)clip.channels);
                writer.Write(clip.frequency);
                writer.Write(clip.frequency * clip.channels * 2);
                writer.Write((ushort)(clip.channels * 2));
                writer.Write((ushort)16);

                // 写入数据块标识符和文件大小
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(byteData.Length);

                // 写入音频数据
                writer.Write(byteData);
            }
        }
    }
}