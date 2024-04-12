using System;

namespace XModules.Data
{
    [Serializable]
    public class BasicResponse
    {
        public string code; // 错误码
        public string msg; // 错误信息
        public object data; // 数据，可以为null
    }
}