using DedicatedServer.HostAutomatorStages.BehaviorStates;
using DedicatedServer.Utils;
using StardewValley;
using StardewValley.Locations;

namespace DedicatedServer.HostAutomatorStages
{
    internal class UnlockCommunityCenterBehaviorLink : BehaviorLink
    {
        #region Required in derived class

        public override int WaitTimeAutoLoad { get; set; } = 60;
        public override int WaitTime { get; set; }

        public override void Process()
        {
            if (false == hasSeenEvent && MainController.IsIdle())
            {
                if (false == isUnlocking &&
                    false == Game1.player.eventsSeen.Contains("611439") &&
                    Game1.currentLocation is Farm &&
                    Game1.stats.DaysPlayed > 4 &&
                    Game1.timeOfDay >= 800 &&
                    Game1.timeOfDay <= 1300 &&
                    false == Game1.IsRainingHere(Game1.getLocationFromName("Town")) &&
                    false == Utility.isFestivalDay(Game1.Date.DayOfMonth, Game1.Date.Season)
                ){
                    isUnlocking = true;
                    MainController.IdleLockEnable();
                    MainController.Warp(WarpPoints.townWarp);
                }
                else if (isUnlocking &&
                    Game1.player.eventsSeen.Contains("611439") &&
                    Game1.currentLocation is Town
                ){
                    isUnlocking = false;
                    hasSeenEvent = true;
                    MainController.IdleLockEnable();
                    MainController.Warp(WarpPoints.farmWarp);
                }
            }
        }

        #endregion

        private bool isUnlocking = false;

        private bool hasSeenEvent = false;

        public UnlockCommunityCenterBehaviorLink()
        {
            if (Game1.player.eventsSeen.Contains("611439"))
            {
                hasSeenEvent = true;
            }
        }
    }
}
