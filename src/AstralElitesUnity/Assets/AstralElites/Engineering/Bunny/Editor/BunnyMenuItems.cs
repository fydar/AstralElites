#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace Fydar.Bunny.Editor
{
    public class BunnyMenuItems
    {
        private const string useAssetDatabaseMenuPath = "Bunny/Use AssetDatabase in Editor";

        [MenuItem(useAssetDatabaseMenuPath)]
        public static void ToggleUseAssetDatabase()
        {
            bool current = EditorPrefs.GetBool(BunnyEditorLoader.useAssetDatabasePrefKey, true);
            EditorPrefs.SetBool(BunnyEditorLoader.useAssetDatabasePrefKey, !current);
        }

        [MenuItem(useAssetDatabaseMenuPath, isValidateFunction: true)]
        public static bool ToggleUseAssetDatabaseValidate()
        {
            Menu.SetChecked(useAssetDatabaseMenuPath, EditorPrefs.GetBool(BunnyEditorLoader.useAssetDatabasePrefKey, true));
            return true;
        }

        [MenuItem("Assets/Build WebGL AssetBundles")]
        public static void BuildAllAssetBundles()
        {
            string assetBundleDirectory = "Assets/StreamingAssets/AssetBundles";

            if (!Directory.Exists(assetBundleDirectory))
            {
                Directory.CreateDirectory(assetBundleDirectory);
            }

            string[] allBundleNames = AssetDatabase.GetAllAssetBundleNames();
            var builds = new List<AssetBundleBuild>();
            foreach (string bundleName in allBundleNames)
            {
                if (string.Equals(bundleName, "unifiedraytracing", StringComparison.OrdinalIgnoreCase))
                    continue;

                builds.Add(new AssetBundleBuild
                {
                    assetBundleName = bundleName,
                    assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName),
                });
            }

            BuildPipeline.BuildAssetBundles(
                assetBundleDirectory,
                builds.ToArray(),
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.AssetBundleStripUnityVersion,
                BuildTarget.WebGL
            );
        }
    }
}
#endif
