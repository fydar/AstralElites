namespace Fydar.Bunny
{
    public class BunnyAssetResult<T> where T : UnityEngine.Object
    {
        public T Asset { get; internal set; }
        public string Error { get; internal set; }
    }
}
