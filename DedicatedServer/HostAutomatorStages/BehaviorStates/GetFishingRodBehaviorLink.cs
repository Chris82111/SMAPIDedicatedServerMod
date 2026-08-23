using DedicatedServer.HostAutomatorStages.BehaviorStates;
using DedicatedServer.Utils;
using StardewValley;
using StardewValley.Locations;

namespace DedicatedServer.HostAutomatorStages
{
    internal class GetFishingRodBehaviorLink : BehaviorLink
    {
        #region Required in derived class

        public override int WaitTimeAutoLoad { get; set; } = 60;
        public override int WaitTime { get; set; }

        public override void Process()
        {
            if (false == hasSeenEvent && MainController.IsIdle())
            {
                //If we don't get the fishing rod, Willy isn't available
                if (false == isGettingFishingRod &&
                    false == Game1.player.eventsSeen.Contains("739330") &&
                    Game1.currentLocation is Farm &&
                    Game1.player.hasQuest("13") && 
                    Game1.timeOfDay <= 1710 && 
                    false == Utility.isFestivalDay(Game1.Date.DayOfMonth, Game1.Date.Season)
                ){
                    isGettingFishingRod = true;
                    MainController.IdleLockEnable();
                    Game1.player.warpFarmer(WarpPoints.beachWarp);
                }
                else if (isGettingFishingRod &&
                    Game1.player.eventsSeen.Contains("739330") &&
                    Game1.currentLocation is Beach
                ){
                    isGettingFishingRod = false;
                    hasSeenEvent = true;
                    MainController.IdleLockEnable();
                    Game1.player.warpFarmer(WarpPoints.farmWarp);
                }
            }
        }

        #endregion

        private static bool isGettingFishingRod = false;

        private bool hasSeenEvent = false;

        public GetFishingRodBehaviorLink()
        {
            if (Game1.player.eventsSeen.Contains("739330"))
            {
                hasSeenEvent = true;
            }
        }
    }
}
