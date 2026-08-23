using DedicatedServer.Config;
using DedicatedServer.HostAutomatorStages;
using DedicatedServer.Network;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace DedicatedServer
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        // TODO: Make the host icon on the map invisible to everyone else

        // TODO: See the warnings

        private int waitCounter;
        private ModConfig config;
        private IModHelper helper;

        /// <summary>
        ///         The mod entry point, called after the mod is first loaded.
        /// </summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            this.config = helper.ReadConfig<ModConfig>();

            Port.Patch(ModManifest.UniqueID, base.Monitor, this.config);

            Network.Multiplayer.SetPlayerLimit(this.config);

            this.waitCounter = 60;
            this.Enable();
        }

        private void Enable() => helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

        private void Disable() => helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;

        /// <summary>
        ///         Ensure that the game environment is in a stable state before the mod starts executing.
        /// <br/>   Without a waiting time, an invitation code is almost never generated; with a waiting
        /// <br/>   time of 1 second, it is very rare that no more codes are generated.
        /// <br/>   First waits until the condition is met and then waits a certain number of update cycles.
        /// </summary>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.activeClickableMenu is StardewValley.Menus.TitleMenu)
            {
                this.waitCounter--;
            }

            if (0 >= this.waitCounter)
            {
                this.Disable();

                _ = new StartFarmStage(base.Helper, base.Monitor, this.config);
            }
        }
    }
}
