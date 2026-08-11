using StardewValley;
using StardewValley.Extensions;
using System;
using System.Collections.Generic;

namespace DedicatedServer.Utils
{
    internal class Festivals
    {
        #region Festivals

        /// <summary>
        ///         Find out if there's a specific festival taking place today.
        /// <br/>   The original functions are:
        /// <br/>   <see cref="Utility.isFestivalDay()"/>
        /// <br/>   <see cref="Utility.IsPassiveFestivalDay()"/>
        /// </summary>
        public static bool IsTodaySpecificFestival
        { 
            get
            {
                var day = Game1.dayOfMonth;
                return Game1.currentSeason switch
                {
                    "spring" => day is 13 or 15 or 16 or 17 or 24,
                    "summer" => day is 11 or 20 or 21 or 28,
                    "fall" => day is 16 or 27,
                    "winter" => day is 8 or 12 or 13 or 15 or 16 or 17 or 25,
                    _ => false
                };
            }
        }

        /// <summary>
        /// <br/>   Time passes: false
        /// <br/>   NPC the host must talk to in order to trigger the next step: Lewis
        /// </summary>
        public static bool IsTodayEggFestival
            => Game1.currentSeason.EqualsIgnoreCase("spring") && Game1.dayOfMonth == 13;

        /// <summary>
        /// <br/>   Time passes: true
        /// <br/>   No real festival, no trigger
        /// </summary>
        public static bool IsTodayDesertFestival
            => Game1.currentSeason.EqualsIgnoreCase("spring") && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17;

        /// <summary>
        /// <br/>   Time passes: false
        /// <br/>   NPC the host must talk to in order to trigger the next step: Lewis
        /// </summary>
        public static bool IsTodayFlowerDance
            => Game1.currentSeason.EqualsIgnoreCase("spring") && Game1.dayOfMonth == 24;

        /// <summary>
        /// <br/>   Time passes: false
        /// <br/>   NPC the host must talk to in order to trigger the next step: Lewis
        /// </summary>
        public static bool IsTodayLuau
            => Game1.currentSeason.EqualsIgnoreCase("summer") && Game1.dayOfMonth == 11;

        /// <summary>
        /// <br/>   Time passes: true
        /// <br/>   No real festival, no trigger
        /// </summary>
        public static bool IsTodayTroutDerby
            => Game1.currentSeason.EqualsIgnoreCase("summer") && Game1.dayOfMonth >= 20 && Game1.dayOfMonth <= 21;

        /// <summary>
        /// <br/>   Time passes: false
        /// <br/>   NPC the host must talk to in order to trigger the next step: Lewis
        /// </summary>
        public static bool IsTodayDanceOfTheMoonlightJellies
            => Game1.currentSeason.EqualsIgnoreCase("summer") && Game1.dayOfMonth == 28;

        /// <summary>
        /// <br/>   Time passes: false
        /// <br/>   NPC the host must talk to in order to trigger the next step: Lewis
        /// </summary>
        public static bool IsTodayStardewValleyFair
            => Game1.currentSeason.EqualsIgnoreCase("fall") && Game1.dayOfMonth == 16;

        /// <summary>
        /// <br/>   Time passes: true
        /// <br/>   It's a festival, but you can just leave whenever you want, no trigger
        /// </summary>
        public static bool IsTodaySpiritsEve
            => Game1.currentSeason.EqualsIgnoreCase("fall") && Game1.dayOfMonth == 27;

        /// <summary>
        /// <br/>   Time passes: false
        /// <br/>   NPC the host must talk to in order to trigger the next step: Lewis
        /// </summary>
        public static bool IsTodayFestivalOfIce
            => Game1.currentSeason.EqualsIgnoreCase("winter") && Game1.dayOfMonth == 8;

        /// <summary>
        /// <br/>   Time passes: true
        /// <br/>   No real festival, no trigger
        /// </summary>
        public static bool IsTodaySquidFest
            => Game1.currentSeason.EqualsIgnoreCase("winter") && Game1.dayOfMonth >= 12 && Game1.dayOfMonth <= 13;

        /// <summary>
        /// <br/>   Time passes: true
        /// <br/>   No real festival, no trigger
        /// </summary>
        public static bool IsTodayBeachNightMarket
            => Game1.currentSeason.EqualsIgnoreCase("winter") && Game1.dayOfMonth >= 15 && Game1.dayOfMonth <= 17;

        /// <summary>
        /// <br/>   Time passes: false
        /// <br/>   It's a festival, but you can just leave whenever you want, no trigger
        /// </summary>
        public static bool IsTodayFeastOfTheWinterStar
            => Game1.currentSeason.EqualsIgnoreCase("winter") && Game1.dayOfMonth == 25;

        /// <summary>
        /// Don't enable chat box on spirit's eve nor feast of the winter star
        /// 
        /// OLD:
        ///  if ((Game1.currentSeason != "fall" || Game1.dayOfMonth != 27) &&
        ///  (Game1.currentSeason != "winter" || Game1.dayOfMonth != 25) )
        /// </summary>
        public static bool IsHostDecidingNextStep
        {
            get
            {
#if true
                var day = Game1.dayOfMonth;
                return Game1.currentSeason switch
                {
                    "spring" => day is 13 or 24, // 15 or 16 or 17 DesertFestival (no real festival)
                    
                    "summer" => day is 11 or 28, // 20 or 21, TroutDerby (no real festival)
                    
                    "fall" => day is 16, // 27 SpiritsEve (It's a festival, but you can just leave whenever you want)

                    // 12 or 13 SquidFest (no real festival)
                    // 15 or 16 or 17 BeachNightMarket (no real festival)
                    // 25 FeastOfTheWinterStar (It's a festival, but you can just leave whenever you want)
                    "winter" => day is 8,

                    _ => false
                };
#else
                // Like the old logic
                if ((Game1.currentSeason != "fall" || Game1.dayOfMonth != 27) &&
                    (Game1.currentSeason != "winter" || Game1.dayOfMonth != 25))
                {
                    // Don't enable chat box on spirit's eve nor feast of the winter star
                    return true;
                }

                return false;
#endif
            }
        }

#endregion

        public static bool IsWaitingToAttend()
        {
            return MainController.IsReady("festivalStart");
        }
        public static bool OthersWaitingToAttend(int numOtherPlayers)
        {
            return MainController.GetNumberReady("festivalStart") == (numOtherPlayers + (IsWaitingToAttend() ? 1 : 0));
        }
        
        public static bool ShouldAttend(int numOtherPlayers)
        {
            return numOtherPlayers > 0 && OthersWaitingToAttend(numOtherPlayers) && Utility.isFestivalDay() && !IsTodayBeachNightMarket && Game1.timeOfDay >= Utility.getStartTimeOfFestival() && Game1.timeOfDay <= getFestivalEndTime();
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

        private static int getFestivalEndTime()
        {
            if (Game1.weatherIcon == 1)
            {
                return Convert.ToInt32(Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[1].Split(' ')[1]);
            }

            return -1;
        }
    }
}
