using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// An async operation that downloads a bundle once, loads all assets into a
/// global cache, and fulfills requests via the cache.
/// </summary>
public class BunnyLoadAssetOperation<T> : CustomYieldInstruction
    where T : UnityEngine.Object
{
    private string _bundleName;
    private string _assetName;

    public T asset { get; private set; }
    public string error { get; private set; }
    public bool isDone { get; private set; }

    public BunnyLoadAssetOperation(string bundleUrl, string bundleName, string assetName)
    {
        _bundleName = bundleName;
        _assetName = assetName;

        if (CheckCache())
        {
            isDone = true;
            return;
        }

        if (!BunnyLoader.ActiveDownloads.ContainsKey(_bundleName))
        {
            var state = new BunnyLoader.DownloadState();

            foreach (var loadedBundle in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (string.Equals(loadedBundle.name, _bundleName, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[AssetLoader] Hijacking previously loaded bundle: {_bundleName}");
                    state.Bundle = loadedBundle;
                    state.AssetOp = state.Bundle.LoadAllAssetsAsync();
                    BunnyLoader.ActiveDownloads[_bundleName] = state;
                    return;
                }
            }

            state.WebRequest = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl);
            state.WebOp = state.WebRequest.SendWebRequest();

            BunnyLoader.ActiveDownloads[_bundleName] = state;
        }
    }

    public override bool keepWaiting
    {
        get
        {
            if (isDone) return false;

            if (CheckCache())
            {
                isDone = true;
                return false;
            }

            if (BunnyLoader.ActiveDownloads.TryGetValue(_bundleName, out var state))
            {
                if (state.IsComplete)
                {
                    if (!string.IsNullOrEmpty(state.Error))
                        error = state.Error;
                    else
                        error = $"Asset '{_assetName}' could not be found inside the bundle '{_bundleName}'.";

                    isDone = true;
                    return false;
                }

                ProcessDownloadState(state);
            }

            return true;
        }
    }

    private bool CheckCache()
    {
        if (BunnyLoader.Cache.TryGetValue(_bundleName, out var bundleCache))
        {
            if (bundleCache.TryGetValue(_assetName, out var obj))
            {
                asset = obj as T;
                return true;
            }
        }
        return false;
    }

    private void ProcessDownloadState(BunnyLoader.DownloadState state)
    {
        if (state.WebOp != null && !state.WebOp.isDone) return;

        if (state.Bundle == null && state.AssetOp == null)
        {
            if (state.WebRequest.result != UnityWebRequest.Result.Success)
            {
                state.Error = state.WebRequest.error;
                CompleteState(state);
                return;
            }

            state.Bundle = DownloadHandlerAssetBundle.GetContent(state.WebRequest);
            if (state.Bundle == null)
            {
                state.Error = "Bundle downloaded, but could not be parsed.";
                CompleteState(state);
                return;
            }

            state.AssetOp = state.Bundle.LoadAllAssetsAsync();
            return;
        }

        if (state.AssetOp != null && !state.AssetOp.isDone)
        {
            return;
        }

        if (state.AssetOp == null || !state.AssetOp.isDone || state.IsComplete)
        {
            return;
        }

        var bundleCache = new Dictionary<string, UnityEngine.Object>(StringComparer.OrdinalIgnoreCase);

        foreach (var obj in state.AssetOp.allAssets)
        {
            bundleCache[obj.name] = obj;
        }

        BunnyLoader.Cache[_bundleName] = bundleCache;

        // Free the compressed memory
        // Unfortunately, AudioSource's that aren't preloaded require their compressed data
        // to remain loaded so that they can be decompressed.
        // state.Bundle.Unload(false);

        CompleteState(state);
    }

    private void CompleteState(BunnyLoader.DownloadState state)
    {
        state.IsComplete = true;
        if (state.WebRequest != null)
        {
            state.WebRequest.Dispose();
        }
        BunnyLoader.ActiveDownloads.Remove(_bundleName);
    }
}
