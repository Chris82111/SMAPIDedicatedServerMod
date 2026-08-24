using DedicatedServer.Config;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Reflection;

namespace DedicatedServer.Network
{
    internal static class Multiplayer
    {
        /// <summary>
        /// <see cref="Game1.multiplayer"/>
        /// </summary>
        private static StardewValley.Multiplayer _multiplayer = (StardewValley.Multiplayer)
            typeof(Game1).GetField("multiplayer", BindingFlags.NonPublic | BindingFlags.Static).GetValue(Game1.game1);

        private static IMonitor _monitor;

        public static void Init(IMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        ///         Unlocks the ability to buy new houses from Robin.
        /// <br/>   
        /// <br/>   - The number set here includes the host.
        /// <br/>   - A standard world can start with 4 houses.
        /// <br/>   - An additional 4 houses can be built so that 7 players and the host (8 in total) can play.
        /// <br/>   - If the number is later reduced, any houses that have already been placed will not be deleted.
        /// <br/>   - As long as there are enough houses, players can join.
        /// <br/>   - The number can be set to less than 8 (the default); sometimes more houses are built,
        /// <br/>     sometimes fewer (which is odd), so this method handles that.
        /// <br/>   - After making the change, you should wait until the clock changes (0:10).
        /// </summary>
        /// <param name="modConfig"></param>
        public static void SetPlayerLimit(int playerLimit)
        {
            playerLimit = Math.Max(8, playerLimit);

            _multiplayer.playerLimit = playerLimit;

            _monitor?.Log($"The maximum number of players has been set to {GetPlayerLimit()}.", LogLevel.Info);
        }

        public static void SetPlayerLimit(ModConfig modConfig)
        {
            SetPlayerLimit(modConfig.PlayerLimit);
        }

        public static int GetPlayerLimit()
        {
            return _multiplayer.playerLimit;
        }
    }
}
