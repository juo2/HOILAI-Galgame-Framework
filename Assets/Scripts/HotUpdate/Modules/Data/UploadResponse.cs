using System;

namespace XModules.Data
{
    [System.Serializable]
    public class UploadResponse
    {
        public string code;
        public string msg;
        public UploadData data;
    }

    [System.Serializable]
    public class UploadData
    {
        public bool success;
        public string path;
        public string msg;
        public string url;
    }
}