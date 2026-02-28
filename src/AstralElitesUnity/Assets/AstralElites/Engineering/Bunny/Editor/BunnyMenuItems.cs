#if UNITY_EDITOR
using System.IO;
using UnityEditor;

public class BunnyMenuItems
{
    [MenuItem("Assets/Build WebGL AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets/AssetBundles";

        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.AssetBundleStripUnityVersion,
            BuildTarget.WebGL
        );
    }
}
#endif
