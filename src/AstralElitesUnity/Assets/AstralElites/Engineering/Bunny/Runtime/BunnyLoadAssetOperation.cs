using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Fydar.Bunny
{
    /// <summary>
    /// An async operation that downloads a bundle once, loads all assets into a
    /// global cache, and fulfills requests via the cache.
    /// </summary>
    public class BunnyLoadAssetOperation<T> : CustomYieldInstruction
    where T : UnityEngine.Object
    {
        private readonly BunnyAssetResult<T> result;
        private readonly string bundleName;
        private readonly string assetName;
        private bool isDone;

        public BunnyLoadAssetOperation(BunnyAssetResult<T> result)
        {
            this.result = result;
            isDone = true;
        }

        public BunnyLoadAssetOperation(BunnyAssetResult<T> result, string bundleUrl, string bundleName, string assetName)
        {
            this.result = result;
            this.bundleName = bundleName;
            this.assetName = assetName;

            if (CheckCache())
            {
                isDone = true;
                return;
            }

            if (!BunnyLoader.activeDownloads.ContainsKey(this.bundleName))
            {
                var state = new BunnyLoader.DownloadState();

                foreach (var loadedBundle in AssetBundle.GetAllLoadedAssetBundles())
                {
                    if (string.Equals(loadedBundle.name, this.bundleName, StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.Log($"[AssetLoader] Hijacking previously loaded bundle: {this.bundleName}");
                        state.assetBundle = loadedBundle;
                        state.assetBundleRequest = state.assetBundle.LoadAllAssetsAsync();
                        BunnyLoader.activeDownloads[this.bundleName] = state;
                        return;
                    }
                }

                state.webRequest = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);
                state.webRequestOperation = state.webRequest.SendWebRequest();

                BunnyLoader.activeDownloads[this.bundleName] = state;
            }
        }

        public override bool keepWaiting
        {
            get
            {
                if (isDone)
                {
                    return false;
                }

                if (CheckCache())
                {
                    isDone = true;
                    return false;
                }

                if (BunnyLoader.activeDownloads.TryGetValue(bundleName, out var state))
                {
                    if (state.isComplete)
                    {
                        Finish(string.IsNullOrEmpty(state.error)
                            ? $"Asset '{assetName}' could not be found inside the bundle '{bundleName}'."
                            : state.error);
                        return false;
                    }

                    ProcessDownloadState(state);
                }

                return true;
            }
        }

        private bool CheckCache()
        {
            if (BunnyLoader.objectCache.TryGetValue(bundleName, out var bundleCache))
            {
                if (bundleCache.TryGetValue(assetName, out var obj))
                {
                    result.Asset = obj as T;
                    return true;
                }
            }
            return false;
        }

        private void Finish(string error = null)
        {
            result.Error = error;
            isDone = true;
        }

        private void ProcessDownloadState(BunnyLoader.DownloadState state)
        {
            if (state.webRequestOperation != null && !state.webRequestOperation.isDone)
            {
                return;
            }

            if (state.assetBundle == null && state.assetBundleRequest == null)
            {
                if (state.webRequest.result != UnityWebRequest.Result.Success)
                {
                    state.error = state.webRequest.error;
                    CompleteState(state);
                    return;
                }

                state.assetBundle = DownloadHandlerAssetBundle.GetContent(state.webRequest);
                if (state.assetBundle == null)
                {
                    state.error = "Bundle downloaded, but could not be parsed.";
                    CompleteState(state);
                    return;
                }

                state.assetBundleRequest = state.assetBundle.LoadAllAssetsAsync();
                return;
            }

            if (state.assetBundleRequest != null && !state.assetBundleRequest.isDone)
            {
                return;
            }

            if (state.assetBundleRequest == null || !state.assetBundleRequest.isDone || state.isComplete)
            {
                return;
            }

            var bundleCache = new Dictionary<string, UnityEngine.Object>(StringComparer.OrdinalIgnoreCase);

            foreach (var obj in state.assetBundleRequest.allAssets)
            {
                bundleCache[obj.name] = obj;
            }

            BunnyLoader.objectCache[bundleName] = bundleCache;

            // Unfortunately, AudioSource's that aren't preloaded require their compressed data
            // to remain loaded so that they can be decompressed.
            // state.Bundle.Unload(false);

            CompleteState(state);
        }

        private void CompleteState(BunnyLoader.DownloadState state)
        {
            state.isComplete = true;
            state.webRequest?.Dispose();
            BunnyLoader.activeDownloads.Remove(bundleName);
        }
    }
}
