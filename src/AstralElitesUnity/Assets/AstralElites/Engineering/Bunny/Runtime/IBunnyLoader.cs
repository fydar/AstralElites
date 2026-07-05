using UnityEngine;

namespace Fydar.Bunny
{
    public interface IBunnyLoader
    {
        BunnyLoadAssetOperation<T> CreateOperation<T>(
            BunnyReference<T> reference,
            string baseBundlePath,
            BunnyAssetResult<T> result)
            where T : Object;
    }
}
