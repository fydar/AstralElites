using HuskyUnity.Engineering.Bootstrap;
using HuskyUnity.Engineering.SceneManagement;
using HuskyUnity.Engineering.SceneManagement.BootFlow;
using HuskyUnity.Engineering.SceneOwnership;
using System;
using UnityEngine.SceneManagement;

namespace HuskyUnity.Gameplay.Anatomy
{
    /// <summary>
    /// <para>The <see cref="IHostedService"/> implementation used for the Husky project.</para>
    /// </summary>
    public class GameplayHostedService : IHostedService, ISceneOwner
    {
        private readonly GameplayProgram gameplayProgram;

        internal GameplayHostedService(
            GameplayProgram gameplayProgram)
        {
            this.gameplayProgram = gameplayProgram;
        }

        /// <inheritdoc/>
        public void OnStartHost(IHost host)
        {
            var bootFlow = new BootFlow(host);
            var sceneManager = new StashingSceneManager(this, bootFlow);
            bootFlow.SceneManager = sceneManager;

            sceneManager.StartBootFlow();
        }

        /// <inheritdoc/>
        public void OnStopHost(IHost host)
        {
        }
    }

    public class BootFlow : IBootFlow
    {
        public StashingSceneManager SceneManager { get; internal set; }

        private readonly IHost host;

        internal BootFlow(
            IHost host)
        {
            this.host = host;
        }

        public void Execute(Scene bootScene)
        {
            if (Array.IndexOf(host.ExecutionContext.CommandLineArguments, "background") >= 0)
            {
                AudioManager.DisableAudio = true;
                CursorManager.DisableCustomCursor = true;
                SceneManager.LoadSceneAsync("Background");
            }
            else
            {
                SceneManager.LoadSceneAsync("Interface");
            }
        }
    }
}
