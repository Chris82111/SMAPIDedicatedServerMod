using DedicatedServer.Utils;
using StardewModdingAPI.Events;
using StardewValley;
using System;

namespace DedicatedServer.HostAutomatorStages
{
    internal abstract class RestartDayWorker
    {
        private static int time;
        private static bool keepsCurrentDay;
        private static bool quit;
        private static Action<int> action;

        public static void Reset()
        {
            DisableOnSaved();
            DisableOnOneSecondUpdateTicked();
        }

        /// <summary>
        ///         Saves the game and resets the day
        /// <br/>
        /// <br/>   1. Prevents the pausing of the dedicated server.
        /// <br/>   2. Conditional: Retains the current day.
        /// <br/>   3. Kicks out all online players.
        /// <br/>   4. Saves the game.
        /// <br/>   5. Resets the normal pause behavior of the dedicated server.
        /// <br/>   6. Conditional: Quit the game.
        /// <br/>   
        /// <br/>   Attention:
        /// <br/>   If something is wrong with the host, if it is
        /// <br/>   blocked in any way, then it will not work.
        /// </summary>
        /// <param name="time">Wait time in seconds</param>
        /// <param name="keepsCurrentDay">
        ///         true : The day counter retains the current day.
        /// <br/>   false: The day counter is incremented. </param>
        /// <param name="quit">
        ///         true : After saving the game the game is quit
        /// <br/>   false: The game will be continued </param>
        /// <param name="action">
        ///         Function that is executed every second until the
        /// <br/>   event is executed. The parameter is the remaining time. </param>
        public static void SavesGameRestartsDay(int time = 0, bool keepsCurrentDay = true, bool quit = false, Action<int> action = null)
        {
            HostAutomation.EnableHostAutomation = true;
            HostAutomation.PreventPauseUntilNextDay();

            if (0 < RestartDayWorker.time)
            {
                RestartDayWorker.time = time;
                return;
            }

            RestartDayWorker.time = time;
            RestartDayWorker.keepsCurrentDay = keepsCurrentDay;
            RestartDayWorker.quit = quit;
            RestartDayWorker.action = action;

            EnableOnOneSecondUpdateTicked();
        }

        /// <summary>
        ///         Executes the actions of the <see cref="SavesGameRestartsDay"/> function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void SavesGameRestartsDayWorker(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (0 < time)
            {
                time--;
                action?.Invoke(time);
                return;
            }

            DisableOnOneSecondUpdateTicked();

            if (keepsCurrentDay)
            {
                if (Game1.dayOfMonth > 1)
                {
                    Game1.stats.DaysPlayed--;
                    Game1.dayOfMonth--;
                }
            }

            foreach (var farmer in Game1.otherFarmers.Values)
            {
                Game1.server.kick(farmer.UniqueMultiplayerID);
            }

            MainController.Warp(WarpPoints.farmHouseWarp);

            Game1.player.isInBed.Value = true;
            Game1.currentLocation.answerDialogueAction("Sleep_Yes", null);

            if (quit)
            {
                EnableOnSaved();
            }
        }

        private static void EnableOnSaved()
        {
            MainController.helper.Events.GameLoop.Saved += OnSavedQuit;
        }

        private static void DisableOnSaved()
        {
            MainController.helper.Events.GameLoop.Saved -= OnSavedQuit;
        }

        private static void EnableOnOneSecondUpdateTicked()
        {
            MainController.helper.Events.GameLoop.OneSecondUpdateTicked += SavesGameRestartsDayWorker;
        }

        private static void DisableOnOneSecondUpdateTicked()
        {
            MainController.helper.Events.GameLoop.OneSecondUpdateTicked -= SavesGameRestartsDayWorker;
        }

        /// <summary>
        ///         After the game is saved, the game should be closed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSavedQuit(object sender, SavedEventArgs e)
        {
            DisableOnSaved();

            Game1.quit = true;
        }
    }
}
