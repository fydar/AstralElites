using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

using Object = UnityEngine.Object;

namespace Fydar.Bunny
{
    public static class BunnyLoader
    {
        internal class DownloadState
        {
            internal UnityWebRequest webRequest;
            internal UnityWebRequestAsyncOperation webRequestOperation;
            internal AssetBundle assetBundle;
            internal AssetBundleRequest assetBundleRequest;
            internal string error;
            internal bool isComplete;
        }

        internal static readonly Dictionary<string, Dictionary<string, Object>> objectCache =
            new(StringComparer.OrdinalIgnoreCase);

        internal static readonly Dictionary<string, DownloadState> activeDownloads =
            new(StringComparer.OrdinalIgnoreCase);

        public static IBunnyLoader Loader { get; internal set; } = new BunnyRuntimeLoader();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCacheState()
        {
            objectCache.Clear();
            foreach (var state in activeDownloads.Values)
            {
                state.webRequest?.Dispose();
            }
            activeDownloads.Clear();
        }

        public static BunnyLoadAssetOperation<T> LoadAssetAsync<T>(BunnyReference<T> reference, string baseBundlePath, out BunnyAssetResult<T> result) where T : UnityEngine.Object
        {
            result = new BunnyAssetResult<T>();
            return Loader.CreateOperation(reference, baseBundlePath, result);
        }

        private class BunnyRuntimeLoader : IBunnyLoader
        {
            public BunnyLoadAssetOperation<T> CreateOperation<T>(BunnyReference<T> reference, string baseBundlePath, BunnyAssetResult<T> result) where T : UnityEngine.Object
            {
                string fullPath = Path.Combine(baseBundlePath, reference.assetBundleName).Replace("\\", "/");
                return new BunnyLoadAssetOperation<T>(result, fullPath, reference.assetBundleName, reference.assetName);
            }
        }
    }
}
