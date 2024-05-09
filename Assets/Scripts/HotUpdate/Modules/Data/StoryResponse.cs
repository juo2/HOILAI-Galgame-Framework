using System;
using System.Collections.Generic;

[Serializable]
public class StoryResponse
{
    public string code; // 错误码
    public string msg; // 错误信息
    public List<StoryData> data; // 剧本数据列表
}

[Serializable]
public class StoryData
{
    public string id; // 剧本id
    public string title; // 标题
    public string description; // 介绍
    public string createTime; // 创建时间
#if UNITY_EDITOR
    public bool isEditor = false; //是否测试数据
#endif
}