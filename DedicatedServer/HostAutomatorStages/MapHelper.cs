using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Reflection;

namespace DedicatedServer.HostAutomatorStages;

/// <summary>
///         To use the code, take a look at the following example:
/// <br/>
/// <br/>
/// <code>
/// // Add this to the <see cref="ModEntry.Entry"/> method
/// _harmony = new Harmony(ModManifest.UniqueID);
/// _mapHelper = new HostAutomatorStages.MapHelper(Monitor, _harmony);
/// _mapHelper.Patch();
///
/// // Add this to the class:
/// private Harmony? _harmony;
/// private HostAutomatorStages.MapHelper? _mapHelper;
/// </code>
/// </summary>
public sealed class MapHelper
{
    private const byte FarmerDeltaMessageType = 0;

    private static readonly Vector2 HiddenPosition = new Vector2(100f, 100f);

    private readonly IMonitor _monitor;
    private readonly Harmony _harmony;
    private static MapHelper _instance;

    public MapHelper(IMonitor monitor, Harmony harmony)
    {
        _monitor = monitor;
        _harmony = harmony;
    }

    public void Patch()
    {
        _instance = this;

        ApplyHarmonyPatch();
    }

    public void Unpatch()
    {
        MethodInfo? method = AccessTools.Method(
            typeof(Farmer),
            "queueMessage",
            new[]
            {
                typeof(byte),
                typeof(Farmer),
                typeof(object[])
            });

        if (null != method)
        {
            _harmony.Unpatch(
                method,
                HarmonyPatchType.Prefix,
                _harmony.Id);
        }

        if (ReferenceEquals(_instance, this))
        {
            _instance = null;
        }
    }

    private void ApplyHarmonyPatch()
    {
        MethodInfo method = AccessTools.Method(
            typeof(Farmer),
            "queueMessage",
            new[]
            {
                typeof(byte),
                typeof(Farmer),
                typeof(object[])
            });

        if (null == method)
        {
            throw new MissingMethodException(
                typeof(Farmer).FullName,
                "queueMessage(byte, Farmer, object[])");
        }

        _harmony.Patch(
            method,
            prefix: new HarmonyMethod(
                typeof(MapHelper),
                nameof(QueueMessagePrefix)));
    }

    private static bool QueueMessagePrefix(
        Farmer __instance,
        byte messageType,
        Farmer sourceFarmer,
        object[] data)
    {
        MapHelper? instance = _instance;

        if (instance == null) { return true; }

        // Only the host needs to hide its position.
        if (!Context.IsMainPlayer) { return true; }

        // Only Farmer delta messages.
        if (messageType != FarmerDeltaMessageType) { return true; }

        // Only modify synchronization of the host Farmer.
        if (sourceFarmer.UniqueMultiplayerID !=
            Game1.player.UniqueMultiplayerID) { return true; }

        // Never modify the host's own local message.
        if (__instance.UniqueMultiplayerID ==
            Game1.player.UniqueMultiplayerID) { return true; }

        // Farmer delta format:
        // data[0] = Farmer ID
        // data[1] = serialized delta
        
        if (data.Length < 2) { return true; }

        if (data[1] is not byte[] originalDelta) { return true; }

        try
        {
            // Get the host's actual network root.
            NetFarmerRoot root = Game1.Multiplayer.farmerRoot(sourceFarmer.UniqueMultiplayerID);

            if (root == null)
            {
                instance._monitor.Log(
                    "Couldn't find the host NetFarmerRoot.",
                    LogLevel.Warn);

                return true;
            }

            // Clone the network tree.
            // The real Farmer is never modified.
            NetRoot<Farmer> clone = root.Clone();

            Farmer? cloneFarmer = clone.Value;

            if (null == cloneFarmer) { return true; }

            Desert desertLocation = Game1.getLocationFromName("Desert") as Desert;
            cloneFarmer.currentLocation = desertLocation;

            // Change only the cloned Farmer.
            cloneFarmer.Position = HiddenPosition;


            // Serialize the cloned tree.
            byte[] fakeDelta =
                Game1.Multiplayer.writeObjectDeltaBytes(
                    clone);

            
            //Replace the outgoing delta.
            //The recipient therefore receives the
            //fake position instead of the real position.
            data[1] = fakeDelta;

#if DEBUG
            instance._monitor.Log(
                $"Hidden host from {__instance.Name}: " +
                $"{sourceFarmer.Position} -> " +
                $"{HiddenPosition} " +
                $"({originalDelta.Length} -> " +
                $"{fakeDelta.Length} bytes)",
                LogLevel.Trace);
#endif
        }
        catch (Exception ex)
        {
            // If anything fails, preserve the original
            // packet rather than sending potentially
            // invalid data.
            data[1] = originalDelta;

            instance._monitor.Log(
                $"Failed to hide host position from " +
                $"{__instance.Name}: {ex}",
                LogLevel.Error);
        }

        return true;
    }
}