﻿using DedicatedServer.Chat;
using DedicatedServer.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    internal class SleepWorker
    {
        private static IModHelper helper = null;

        private static bool _ShouldSleepOverwrite = false;

        public SleepWorker(IModHelper helper)
        {
            SleepWorker.helper = helper;
        }

        /// <summary>
        ///         Checks whether the host is sleeping
        /// <br/>
        /// <br/>   If you you want to test for all required player use <c>Game1.netReady.IsReady("sleep");</c>
        /// </summary>
        /// <returns>
        ///         true : The host is sleeping
        /// <br/>   false: The host is not asleep
        /// </returns>
        public static bool IsSleeping()
        {  
            // Check if all players are ready:
            // `Game1.netReady.IsReady("sleep");`
            // 
            // Get number of ready players:
            // `Game1.netReady.GetNumberReady("sleep");`
            // 
            // Get number of required players:
            // `Game1.netReady.GetNumberRequired("sleep")}");`
            // 
            // In Bed region without sleeping:
            // `Game1.player.isInBed.Value.ToString();`
            // 
            // According to the `Game1.player.team.sleepAnnounceMode` the value will not refresh: 
            // `Game1.player.team.announcedSleepingFarmers.ToList().Select(f => f.UniqueMultiplayerID).ToList().Contains(Game1.player.UniqueMultiplayerID);`

            bool hostSleeping = false;
            if (Game1.activeClickableMenu is ReadyCheckDialog rcd)
            {
                if ("sleep" == rcd.checkName.ToLower())
                {
                    hostSleeping = true;

                    // TODO  The next line is to ensure that this works only if the player is in bed
                    // DEBUG
                    if (false == Game1.player.isInBed.Value)
                    {
                        throw new Exception("Host should sleep but is not in bed region.");
                    }
                }
            }
            return hostSleeping;
        }

        /// <summary>
        ///         Checks whether all players without the host are asleep
        /// </summary>
        /// <returns>
        ///         true : All other players are sleeping
        /// <br/>   false: Not all players are sleeping
        /// </returns>
        public static bool OthersInBed()
        {
            const int oneHostPlayer = 1;

            int readyPlayer = Game1.netReady.GetNumberRequired("sleep");

            if (0 == readyPlayer)
            {
                return false;
            }
            else
            {
                return Game1.netReady.GetNumberReady("sleep") >= (readyPlayer - oneHostPlayer);
            }            
        }

        /// <summary>
        ///         Checks whether the host should go to bed
        /// </summary>
        /// <returns>
        ///         true : The host should go to bed
        /// <br/>   false: The host should not go to bed</returns>
        public static bool ShouldSleep()
        {
            return (Game1.timeOfDay >= 2530) || ShouldSleepOverwrite || OthersInBed();
        }

        /// <summary>
        ///         Changes the behavior of the <see cref="ShouldSleep"/> function.
        /// <br/>   
        /// <br/>   true : The host should go to bed
        /// <br/>   false: The host should not go to bed or get up
        /// <br/>   
        /// <br/>   When all players leave the game, the next day is started.
        /// <br/>   
        /// <br/>   If the host is controlled by a player, the command must not
        /// <br/>   be executed.
        /// </summary>
        protected static bool ShouldSleepOverwrite
        {
            set
            {
                if (value)
                {
                    if (HostAutomation.EnableHostAutomation)
                    {
                        AddOnDayStarted(OnDayStartedWorker);
                        HostAutomation.PreventPauseUntilNextDay();
                        _ShouldSleepOverwrite = true;
                    }
                }
                else
                {
                    _ShouldSleepOverwrite = false;
                }
            }
            get
            {
                return _ShouldSleepOverwrite;
            }
        }

        private static void AddOnDayStarted(EventHandler<DayStartedEventArgs> handler)
        {
            helper.Events.GameLoop.DayStarted += handler;
        }

        private static void RemoveOnDayStarted(EventHandler<DayStartedEventArgs> handler)
        {
            helper.Events.GameLoop.DayStarted -= handler;
        }

        /// <summary>
        ///         Resets the variable <see cref="ShouldSleepOverwrite"/> at the beginning of the day.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnDayStartedWorker(object sender, DayStartedEventArgs e)
        {
            ShouldSleepOverwrite = false;
            RemoveOnDayStarted(OnDayStartedWorker);
        }
    }
}
