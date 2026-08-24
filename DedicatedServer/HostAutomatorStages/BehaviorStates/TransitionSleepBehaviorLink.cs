using DedicatedServer.HostAutomatorStages.BehaviorStates;
using DedicatedServer.Utils;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Reflection;

namespace DedicatedServer.HostAutomatorStages
{
    internal class TransitionSleepBehaviorLink : BehaviorLink
    {
        #region Required in derived class

        public override int WaitTimeAutoLoad { get; set; } = (int)(60 * 0.2);
        public override int WaitTime { get; set; }

        public override void Process()
        {
            if (Utils.Sleeping.ShouldSleep() &&
                false == Utils.Sleeping.IsSleeping())
            {
                // After the required number of players have triggered sleep once,
                // the pause is deactivated until the next day.
                HostAutomation.PreventPauseUntilNextDay();

                if (Game1.currentLocation is FarmHouse)
                {
                    MainController.monitor.Log($"The host lies down in bed", LogLevel.Debug);

                    Game1.player.isInBed.Value = true;
                    Game1.player.sleptInTemporaryBed.Value = true;
                    Game1.player.timeWentToBed.Value = Game1.timeOfDay;
                    Game1.netReady.SetLocalReady("sleep", ready: true);
                    Game1.dialogueUp = false;

                    Game1.activeClickableMenu = new ReadyCheckDialog("sleep", allowCancel: true, delegate
                    {
                        Game1.player.isInBed.Value = true;
                        Game1.player.sleptInTemporaryBed.Value = true;
                        info.Invoke(Game1.currentLocation, Array.Empty<object>());
                    }, delegate (Farmer who)
                    {
                        if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog rcd)
                        {
                            rcd.closeDialog(who);
                        }

                        who.timeWentToBed.Value = 0;
                    });

                    if (false == Game1.player.team.announcedSleepingFarmers.Contains(Game1.player))
                    {
                        Game1.player.team.announcedSleepingFarmers.Add(Game1.player);
                    }
                }
                else
                {
                    MainController.monitor.Log($"Warp to sleep", LogLevel.Debug);

                    MainController.Warp(WarpPoints.farmHouseWarp);
                    WaitTime = 60;
                }
            }
            else if (!Utils.Sleeping.ShouldSleep() && Utils.Sleeping.IsSleeping())
            {
                MainController.monitor.Log($"Cancel sleep", LogLevel.Debug);

                if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog rcd)
                {
                    rcd.closeDialog(Game1.player);
                }
                Game1.netReady.SetLocalReady("sleep", false);
            }
            else
            {
                WaitTime = 0;
            }
        }

        #endregion

        private static MethodInfo info = typeof(GameLocation).GetMethod("doSleep", BindingFlags.Instance | BindingFlags.NonPublic);
    }
}
