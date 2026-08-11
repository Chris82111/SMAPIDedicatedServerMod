using StardewValley;
using System;
using System.Collections.Generic;

namespace DedicatedServer.Utils
{
    internal class Festivals
    {
        private static int getFestivalEndTime()
        {
            if (Game1.weatherIcon == 1)
            {
                return Convert.ToInt32(Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[1].Split(' ')[1]);
            }

            return -1;
        }
        public static bool IsWaitingToAttend()
        {
            return MainController.IsReady("festivalStart");
        }
        public static bool OthersWaitingToAttend(int numOtherPlayers)
        {
            return MainController.GetNumberReady("festivalStart") == (numOtherPlayers + (IsWaitingToAttend() ? 1 : 0));
        }
        private static bool isTodayBeachNightMarket()
        {
            return Game1.currentSeason.Equals("winter") && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17;
        }
        public static bool ShouldAttend(int numOtherPlayers)
        {
            return numOtherPlayers > 0 && OthersWaitingToAttend(numOtherPlayers) && Utility.isFestivalDay() && !isTodayBeachNightMarket() && Game1.timeOfDay >= Utility.getStartTimeOfFestival() && Game1.timeOfDay <= getFestivalEndTime();
        }

        public static bool IsWaitingToLeave()
        {
            return MainController.IsReady("festivalEnd");
        }
        public static bool OthersWaitingToLeave(int numOtherPlayers)
        {
            return MainController.GetNumberReady("festivalEnd") == (numOtherPlayers + (IsWaitingToLeave() ? 1 : 0));
        }
        public static bool ShouldLeave(int numOtherPlayers)
        {
            return Game1.isFestival() && OthersWaitingToLeave(numOtherPlayers);
        }
    }
}
