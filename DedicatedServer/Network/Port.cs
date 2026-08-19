using DedicatedServer.Config;
using HarmonyLib;
using StardewModdingAPI;
using System;

namespace DedicatedServer.Network
{
    internal static class Port
    {
        public static UInt16? ServerPort { get; set; } = null;

        private class NetPeerConfigPatch
        {
            public static void Prefix(ref int value)
            {
                value = (int)ServerPort;
            }
        }

        public static void Patch(string uniqueId, IMonitor monitor, ModConfig config)
        {
            if (string.Empty == uniqueId)
            {
                throw new ArgumentNullException(nameof(uniqueId), "The variable must not be null.");
            }

            if (null == config.Port)
            {
                monitor.Log($"The standard port 24642 is used.", LogLevel.Info);
                return;
            }
            monitor.Log($"The server's port has been changed to {config.Port}.", LogLevel.Info);


            var original = Type
                .GetType("Lidgren.Network.NetPeerConfiguration, Lidgren.Network")
                .GetProperty("Port")
                .GetSetMethod();

            var prefix = typeof(NetPeerConfigPatch).GetMethod("Prefix");
            
            if (null == original || null == prefix)
            {
                throw new InvalidOperationException("Couldn't find Lidgren port setter. Report the error and use the default port by setting the 'Port' field in the configuration file to null.");
            }

            var harmony = new Harmony(uniqueId);

            harmony.Patch(
                original: original,
                prefix: new HarmonyMethod(prefix));
        }
    }
}
