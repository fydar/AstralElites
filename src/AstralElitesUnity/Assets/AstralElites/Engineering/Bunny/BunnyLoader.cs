using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

using Object = UnityEngine.Object;

public static class BunnyLoader
{
    internal static readonly Dictionary<string, Dictionary<string, Object>> Cache =
        new(StringComparer.OrdinalIgnoreCase);

    internal static readonly Dictionary<string, DownloadState> ActiveDownloads =
        new(StringComparer.OrdinalIgnoreCase);

    internal class DownloadState
    {
        public UnityWebRequest WebRequest;
        public UnityWebRequestAsyncOperation WebOp;
        public AssetBundle Bundle;
        public AssetBundleRequest AssetOp;
        public string Error;
        public bool IsComplete;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCacheState()
    {
        Cache.Clear();
        foreach (var state in ActiveDownloads.Values)
        {
            state.WebRequest?.Dispose();
        }
        ActiveDownloads.Clear();
    }

    public static BunnyLoadAssetOperation<T> LoadAssetAsync<T>(BunnyReference<T> reference, string baseBundlePath) where T : UnityEngine.Object
    {
        string fullPath = Path.Combine(baseBundlePath, reference.assetBundleName).Replace("\\", "/");

        // We pass the bundle name into the request so it knows how to index the global cache
        return new BunnyLoadAssetOperation<T>(fullPath, reference.assetBundleName, reference.assetName);
    }
}
