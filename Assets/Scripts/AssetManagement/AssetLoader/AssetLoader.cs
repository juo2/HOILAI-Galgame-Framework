using System.Collections;
namespace AssetManagement
{
    abstract public class AssetLoader : IEnumerator
    {
        public object Current { get { return null; } }
        public bool MoveNext() { return !IsDone(); }
        public void Reset() { }
        virtual public float GetProgress() { return 0.0f; }
        abstract public void Update();
        abstract public bool IsDone();
        virtual public string Error { get { return null; } }
        virtual public void Dispose() { }
    }
}
