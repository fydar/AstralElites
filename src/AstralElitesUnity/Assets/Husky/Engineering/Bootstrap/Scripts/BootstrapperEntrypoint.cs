using HuskyUnity.Engineering.Bootstrap.Internal;
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace HuskyUnity.Engineering.Bootstrap
{
    /// <summary>
    /// <para>Intercept the Unity entrypoint callback and create an <see cref="IHost"/> for the lifetime of the application.</para>
    /// </summary>
    public static class BootstrapperEntrypoint
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static BootstrapperConfiguration bootstrapperConfiguration;

        /// <summary>
        /// <para>The <see cref="Bootstrap.BootstrapperConfiguration"/> used by this <see cref="BootstrapperEntrypoint"/> to configure the <see cref="IHostBuilder"/>.</para>
        /// </summary>
        public static BootstrapperConfiguration BootstrapperConfiguration
        {
            get
            {
                if (bootstrapperConfiguration != null)
                {
                    return bootstrapperConfiguration;
                }

                bootstrapperConfiguration = Resources.Load<BootstrapperConfiguration>("BootstrapperConfiguration");

                if (bootstrapperConfiguration == null)
                {
                    bootstrapperConfiguration = ScriptableObject.CreateInstance<BootstrapperConfiguration>();

#if UNITY_EDITOR
                    var monoScript = MonoScript.FromScriptableObject(bootstrapperConfiguration);

                    string scriptPath = AssetDatabase.GetAssetPath(monoScript);
                    string scriptFolderPath = scriptPath[..scriptPath.LastIndexOf('/')];
                    string bootstrapFolderPath = scriptFolderPath[..scriptFolderPath.LastIndexOf('/')];

                    if (!AssetDatabase.IsValidFolder(bootstrapFolderPath + "/Resources"))
                    {
                        AssetDatabase.CreateFolder(bootstrapFolderPath, "Resources");
                    }

                    string newResourcePath = bootstrapFolderPath + "/Resources/BootstrapperConfiguration.asset";
                    AssetDatabase.CreateAsset(bootstrapperConfiguration, newResourcePath);
#else
					UnityEngine.Debug.LogError("Unable to find appropriate Bootstrapper configuration.");
#endif
                }

                return bootstrapperConfiguration;
            }
        }

        /// <summary>
        /// <para>The current singleton <see cref="IHost"/> that persists for the lifetime of the application.</para>
        /// </summary>
        public static IHost CurrentHost { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Main()
        {
#if UNITY_EDITOR
            var executionContext = new DefaultExecutionContext(Array.Empty<string>());
#else
			var executionContext = new DefaultExecutionContext(
				Environment.GetCommandLineArgs());
#endif

            var hostBuilder = new DefaultHostBuilder(executionContext);

            var configuration = BootstrapperConfiguration;
            if (configuration.Program != null)
            {
                configuration.Program.ConfigureHostBuilder(hostBuilder);
            }

            CurrentHost = hostBuilder.Build();
            CurrentHost.StartHost();

            Application.quitting -= ApplicationQuitting;
            Application.quitting += ApplicationQuitting;
        }

        private static void ApplicationQuitting()
        {
            CurrentHost?.StopHost();
        }
    }
}
