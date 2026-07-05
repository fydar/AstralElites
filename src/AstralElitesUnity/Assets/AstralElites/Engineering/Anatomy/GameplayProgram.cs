using HuskyUnity.Engineering.Bootstrap;
using UnityEngine;
using UnityEngine.Rendering;

namespace HuskyUnity.Gameplay.Anatomy
{
    /// <summary>
    /// <para>The <see cref="Program"/> used by the Husky project.</para>
    /// </summary>
    /// <remarks>
    /// <para>Configures the host with the <see cref="GameplayHostedService"/> hosted service.</para>
    /// </remarks>
    [CreateAssetMenu(menuName = "Husky/GameplayProgram")]
    public class GameplayProgram : Program
    {
        [SerializeField] private GraphicsStateCollection graphicsStateCollection;

        /// <inheritdoc/>
        public override void ConfigureHostBuilder(
            IHostBuilder hostBuilder)
        {
            if (graphicsStateCollection != null)
            {
                graphicsStateCollection.WarmUpProgressively(1000);
            }

            hostBuilder.UseHostedService(new GameplayHostedService(this));
        }
    }
}
