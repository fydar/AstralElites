using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class BunnyLoader
{
    // Moved caches to the non-generic class to ensure a single, global state
    internal static readonly Dictionary<string, Dictionary<string, UnityEngine.Object>> Cache =
        new Dictionary<string, Dictionary<string, UnityEngine.Object>>(StringComparer.OrdinalIgnoreCase);

    internal static readonly Dictionary<string, DownloadState> ActiveDownloads =
        new Dictionary<string, DownloadState>(StringComparer.OrdinalIgnoreCase);

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
            if (state.WebRequest != null) state.WebRequest.Dispose();
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
