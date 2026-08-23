using DedicatedServer.HostAutomatorStages.BehaviorStates;
using DedicatedServer.Utils;
using StardewValley;

namespace DedicatedServer.HostAutomatorStages
{
    internal class EndCommunityCenterBehaviorLink : BehaviorLink
    {
        #region Required in derived class

        public override int WaitTimeAutoLoad { get; set; } = 60;
        public override int WaitTime { get; set; }

        public override void Process()
        {
            if (false == hasSeenEvent && MainController.IsIdle())
            {
                if (false == isEnding && 
                    false == Game1.player.eventsSeen.Contains("191393") &&
                    Game1.player.hasCompletedCommunityCenter() &&
                    false == Game1.IsRainingHere(Game1.getLocationFromName("Town")) &&
                    false == Utility.isFestivalDay(Game1.Date.DayOfMonth, Game1.Date.Season)
                ){
                    isEnding = true;
                    MainController.IdleLockEnable();
                    MainController.Warp(WarpPoints.townWarp);
                }
                else if (true == isEnding &&
                    true == Game1.player.eventsSeen.Contains("191393")
                ){
                    isEnding = false;
                    hasSeenEvent = true;
                    MainController.IdleLockEnable();
                    MainController.Warp(WarpPoints.farmWarp);
                }
            }   
        }

        #endregion

        private bool isEnding = false;

        private bool hasSeenEvent = false;

        public EndCommunityCenterBehaviorLink()
        {
            if (Game1.player.eventsSeen.Contains("191393"))
            {
                hasSeenEvent = true;
            }
        }
    }
}
