using System.IO;
using UnityEditor;
using UnityEngine;

namespace Fydar.Bunny.Editor
{
    internal class BunnyEditorLoader : IBunnyLoader
    {
        internal const string useAssetDatabasePrefKey = "Bunny.UseAssetDatabase";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterLoader()
        {
            BunnyLoader.Loader = new BunnyEditorLoader();
        }

        public BunnyLoadAssetOperation<T> CreateOperation<T>(BunnyReference<T> reference, string baseBundlePath, BunnyAssetResult<T> result) where T : UnityEngine.Object
        {
            if (EditorPrefs.GetBool(useAssetDatabasePrefKey, true))
            {
                string[] paths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(
                    reference.assetBundleName, reference.assetName);

                if (paths.Length > 0)
                    result.Asset = AssetDatabase.LoadAssetAtPath<T>(paths[0]);
                else
                    result.Error = $"Asset '{reference.assetName}' not found in bundle '{reference.assetBundleName}' via AssetDatabase.";

                return new BunnyLoadAssetOperation<T>(result);
            }

            string fullPath = Path.Combine(baseBundlePath, reference.assetBundleName).Replace("\\", "/");
            return new BunnyLoadAssetOperation<T>(result, fullPath, reference.assetBundleName, reference.assetName);
        }
    }
}
