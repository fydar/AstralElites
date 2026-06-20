using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HuskyUnity.Engineering.SceneOwnership
{
    public static class OwnershipUtilities
    {
        private static readonly Dictionary<SceneHandle, ISceneOwner> handleToOwner;

        static OwnershipUtilities()
        {
            handleToOwner = new Dictionary<SceneHandle, ISceneOwner>();

            SceneManager.sceneUnloaded += OnBeforeSceneUnloaded;
        }

        public static void ClaimOwnership(ISceneOwner owner, Scene scene)
        {
            handleToOwner[scene.handle] = owner;
        }

        public static ISceneOwner GetOwner(Scene scene)
        {
            if (handleToOwner.TryGetValue(scene.handle, out var sceneOwner))
            {
                return sceneOwner;
            }
            throw new KeyNotFoundException($"The given scene '{scene.name}' ({scene.handle}) does not have an owner.");
        }

        public static bool IsOwned(Scene scene)
        {
            return handleToOwner.ContainsKey(scene.handle);
        }

        private static void OnBeforeSceneUnloaded(Scene scene)
        {
            handleToOwner.Remove(scene.handle);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        internal static void Main()
        {
            handleToOwner.Clear();
        }
    }
}
