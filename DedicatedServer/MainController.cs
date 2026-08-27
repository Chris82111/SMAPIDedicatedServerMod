using DedicatedServer.Chat;
using DedicatedServer.Config;
using DedicatedServer.HostAutomatorStages.BehaviorStates;
using DedicatedServer.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network.NetReady;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DedicatedServer
{
    internal static class MainController
    {
        private static FieldInfo multiplayerFieldInfo = typeof(Game1).GetField("multiplayer", BindingFlags.NonPublic | BindingFlags.Static);

        private static Multiplayer multiplayer => (Multiplayer)multiplayerFieldInfo.GetValue(null);

        public static IModHelper helper { get; private set; }
        public static IMonitor monitor { get; private set; }
        public static ModConfig config { get; private set; }
        public static EventDrivenChatBox chatBox { get; private set; }

        public static void InitStaticVariables(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            MainController.helper = helper;
            MainController.monitor = monitor;
            MainController.config = config;
        }

        public static void InitChatBox(EventDrivenChatBox chatBox)
        {
            MainController.chatBox = chatBox;
        }

        public static void Dispose()
        {
            MainController.helper = null;
            MainController.monitor = null;
            MainController.config = null;
            MainController.chatBox = null;
        }

        #region Idle Check

        /// <summary>
        /// Checks if the host is idle.
        /// </summary>
        /// <returns>
        ///         true: The host can be interrupt
        /// <br/>   false: The host must not be interrupt</returns>
        public static bool IsIdle()
        {
            if (0 > idleLockTime &&
                null != Game1.currentLocation &&
                false == Game1.MasterPlayer.IsBusyDoingSomething()
            ){ 
                return true;
            }
            return false;
        }

        public static void IdleLockEnable(int ticks = 5)
        {
            if (0 > idleLockTime)
            {
                idleLockTime = ticks;
                MainController.helper.Events.GameLoop.UpdateTicked += IdleWorker;
            }
        }

        public static void IdleLockDisable()
        {
            idleLockTime = -1;
            MainController.helper.Events.GameLoop.UpdateTicked -= IdleWorker;
        }

        private static int idleLockTime = -1;

        private static void IdleWorker(object sender, UpdateTickedEventArgs e)
        {
            if (0 < idleLockTime)
            {
                idleLockTime--;
            }
            else
            {
                IdleLockDisable();
            }
        }

        #endregion

        #region Warp

        /// <summary>
        ///         Warps the host, you can use a warp point from <see cref="WarpPoints"/>
        /// <br/>   The warp set up wait ticks to wait for possible event
        /// </summary>
        /// <param name="warp"></param>
        public static void Warp(Warp warp)
        {
            if (null == warp)
            {
                Refresh();
                return;
            }

            Game1.player.warpFarmer(warp);

            // Set up wait ticks to wait for possible event
            BehaviorChain.WaitTime = 60;
        }

        /// <summary>
        ///         Warps the host to the same position.
        /// <br/>   Refresh to make bot back to visible
        /// </summary>
        public static void Refresh()
        {
            Warp(WarpPoints.masterPlayerWarp);
        }

        #endregion

        #region Game

        public static void SetMonth(Season season)
        {
            switch (season)
            {
                case Season.Spring: Game1.currentSeason = "spring"; break;
                case Season.Summer: Game1.currentSeason = "summer"; break;
                case Season.Fall: Game1.currentSeason = "fall"; break;
                case Season.Winter: Game1.currentSeason = "winter"; break;
                default: break;
            }
        }

        public static void SetMonth(int month)
        {
            switch (month)
            {
                case 1: Game1.currentSeason = "spring"; break;
                case 2: Game1.currentSeason = "summer"; break;
                case 3: Game1.currentSeason = "fall"; break;
                case 4: Game1.currentSeason = "winter"; break;
                default: break;
            }
        }

        public static void SetDay(int day)
        {
            if (28 >= day && 1 <= day)
            {
                day = day - Game1.dayOfMonth;
                Game1.stats.DaysPlayed += (uint)day;
                Game1.dayOfMonth += day;
            }
        }

        /// <summary>
        ///         List of farmers of saved games
        /// </summary>
        /// <returns>
        ///         List of <see cref="Farmer"/>
        /// </returns>
        public static List<Farmer> GetSaveGameFarmers()
        {
            object result = findSaveGames.Invoke(obj: null, parameters: new object[] { "" });
            List<Farmer> farmers = result as List<Farmer>;
            return farmers;
        }

        public static Farmer GetFarmerOfSaveGameOrDefault(string farmName, List<Farmer> saveGameFarmers)
        {
            var farmers = saveGameFarmers
                .Where(farmer => farmer.farmName.ToString() == farmName)
                .ToList();

            switch (farmers.Count)
            {
                case 0:
                    monitor.Log($"No farm found.", LogLevel.Info);
                    return null;

                case 1:
                    var foundFarmer = farmers[0];

                    monitor.Log($"One farm found (farm name, host farmer, directory name):", LogLevel.Info);
                    monitor.Log($"  {foundFarmer.farmName}, {foundFarmer.Name}, {foundFarmer.slotName}", LogLevel.Info);

                    if (false == foundFarmer.slotCanHost)
                    {
                        monitor.Log($"The farm you found actually exists, but no one can join it because " +
                            $"it's not a multiplayer game and/or there are no huts.", LogLevel.Error);
                        Exit(-1);
                    }

                    return foundFarmer;

                default:
                    monitor.Log($"Several farms with the same name were found. " +
                        $"Please make sure there is only one farm with this name. " +
                        $"The following farms were found (farm name, host farmer, directory name):", LogLevel.Error);

                    foreach (Farmer farmer in farmers)
                    {
                        monitor.Log($"  {farmer.farmName}, {farmer.Name}, {farmer.slotName}", LogLevel.Info);
                    }
                    Exit(-1);
                    throw new Exception("Error while Exit() is called.");
            }
        }

        public static Farmer GetFarmerOfSaveGameOrDefault(string farmName)
        {
            return GetFarmerOfSaveGameOrDefault(farmName, GetSaveGameFarmers());
        }

        private static MethodInfo findSaveGames = typeof(LoadGameMenu).GetMethod("FindSaveGames", BindingFlags.Static | BindingFlags.NonPublic);

        #endregion

        #region Players

        /// <summary>
        ///         Get number of all players who are currently connected, without the host
        /// <br/>   
        /// <br/>   The following does not update in an event:
        /// <code>
        ///   Game1.otherFarmers;
        ///   Game1.getOnlineFarmers();
        /// </code>
        /// </summary>
        public static int NumberOfPlayers
        {
            get
            {
                int number = 0;
                foreach (var farmer in Game1.getOnlineFarmers())
                {
                    if (false == multiplayer.isDisconnecting(farmer))
                    {
                        number++;
                    }
                }
                return number -1;
            }
        }

        /// <summary>
        ///         Get number of all players who are currently connected, without the host
        /// <br/>   
        /// <br/>   Attention: This property does not update in an event.
        /// </summary>
        public static int NumberOfPlayersNoneEventUpdate
        {
            get => Game1.getOnlineFarmers().Count - 1;
        }
        
        /// <summary>
        ///         Get a List of online farmers, without the host
        /// <br/>   
        /// <br/>   The following does not update in an event:
        /// <code>
        ///   Game1.otherFarmers;
        ///   Game1.getOnlineFarmers();
        /// </code>
        /// </summary>
        /// <returns>List of online farmers</returns>
        public static List<Farmer> GetOnlineFarmers()
        {
            var onlineFarmers = new List<Farmer>();
            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (Game1.player.UniqueMultiplayerID == farmer.UniqueMultiplayerID) { continue; }

                if (false == multiplayer.isDisconnecting(farmer))
                {
                    onlineFarmers.Add(farmer);
                }
            }
            return onlineFarmers;
        }

        /// <summary>
        ///         Get a List of online farmers, without the host
        /// <br/>   
        /// <br/>   Attention: This function does not update in an event.
        /// </summary>
        /// <returns>List of online farmers</returns>
        public static List<Farmer> GetOnlineFarmersNoneEventUpdate()
        {
            var onlineFarmers = new List<Farmer>();
            foreach (var farmer in Game1.getOnlineFarmers())
            {
                if (Game1.player.UniqueMultiplayerID == farmer.UniqueMultiplayerID) { continue; }

                onlineFarmers.Add(farmer);
            }
            return onlineFarmers;
        }

        /// <summary>
        ///         Get a Dictionary of online farmers, without the host
        /// <br/>   
        /// <br/>   The following does not update in an event:
        /// <code>
        ///   Game1.otherFarmers;
        ///   Game1.getOnlineFarmers();
        /// </code>
        /// </summary>
        /// <returns>Dictionary of online farmers</returns>
        public static Dictionary<long, Farmer> OnlineFarmers()
        {
            var otherPlayers = new Dictionary<long, Farmer>();
            
            foreach (var farmer in Game1.otherFarmers.Values)
            {
                if (false == multiplayer.isDisconnecting(farmer))
                {
                    otherPlayers.Add(farmer.UniqueMultiplayerID, farmer);
                }
            }

            return otherPlayers;
        }

        #endregion

        #region Mailbox 

        /// <summary>
        ///         Open mailbox
        /// <br/>   
        /// <br/>   It's the same as if you were standing in front of the mailbox and right-clicking.
        /// <br/>   Then you need to process the start menu `LetterViewerMenu`.
        /// </summary>
        public static void OpenMailbox()
        {
            Game1.getFarm().mailbox();
        }

        /// <summary>
        ///         Opens the mailbox if there are letters and returns whether there were letters.
        /// </summary>
        /// <returns>
        ///         true: There were letters in the mailbox.
        /// <br/>   false: The mailbox was empty</returns>
        public static bool OpenMailboxIfHasMail()
        {
            if(HasMail())
            {
                OpenMailbox();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether the mailbox contains messages
        /// </summary>
        /// <returns>
        ///         true: There are letters in the mailbox
        /// <br/>   false: The mailbox is empty</returns>
        public static bool HasMail()
        {
            return 0 < MailCount();
        }

        /// <summary>
        /// Returns the number of emails in the mailbox
        /// </summary>
        /// <returns>Number of emails in the mailbox</returns>
        public static int MailCount()
        {
            // The command `Game1.MasterPlayer.mailbox.Count` can also be used
            return Game1.mailbox.Count;
        }

        #endregion

        #region Alias with documentation

        public static void Exit(int statusCode)
        {
            monitor.Log("Exiting...", LogLevel.Error);
            Environment.Exit(statusCode);
        }

        /// <summary>
        ///         Get whether all required players are ready to proceed,
        /// <br/>   including the host
        /// <br/>
        /// <br/>   <c>return ReadyCheckHelper.IsReady("festivalStart");</c>
        /// <br/>   <c>return ReadyCheckHelper.IsReady("festivalEnd");</c>
        /// <br/>   <c>return ReadyCheckHelper.IsReady("ready_for_save");</c>
        /// </summary>
        /// <param name="checkName">The ready check ID.</param>
        /// <returns>
        ///         true : If all required players are ready
        /// <br/>   false: If one required player is not ready</returns>
        public static bool IsReady(string checkName)
        {
            return Game1.netReady.IsReady(checkName);
        }

        /// <summary>
        ///         Get the number of players that are ready to proceed,
        /// <br/>   including the host
        /// </summary>
        /// <param name="checkName">The ready check ID.</param>
        /// <returns></returns>
        public static int GetNumberReady(string checkName)
        {
            return Game1.netReady.GetNumberReady(checkName);
        }

        /// <summary>
        ///         Like the function <see cref="IsReady(string)"/>, but excludes the host.
        /// </summary>
        /// <param name="checkName"></param>
        /// <returns></returns>
        public static bool IsReadyPlayers(string checkName)
        {
            return IsReadyPlayers(checkName, OnlineFarmers());
        }

        /// <summary>
        ///         Accesses the internal private variables of the <see cref="StardewValley.Network.NetReady.ReadySynchronizer"/>
        /// <br/>   class and checks whether the specified farmers are ready.
        /// </summary>
        /// <param name="checkName">The ready check ID.</param>
        /// <param name="farmers">Required player</param>
        /// <returns>
        ///         true : If all required players are ready
        /// <br/>   false: If one of the required player is not ready</returns>
        public static bool IsReadyPlayers(string checkName, Dictionary<long, Farmer> farmers)
        {
            List<Farmer> fs = farmers.Values.ToList();
            return IsReadyPlayers(checkName, fs);
        }

        /// <summary><inheritdoc cref="IsReadyPlayers(string, Dictionary{long, Farmer})"/></summary>
        /// <param name="checkName"><inheritdoc cref="IsReadyPlayers(string, Dictionary{long, Farmer})"/></param>
        /// <param name="farmers"><inheritdoc cref="IsReadyPlayers(string, Dictionary{long, Farmer})"/></param>
        /// <returns><inheritdoc cref="IsReadyPlayers(string, Dictionary{long, Farmer})"/></returns>
        public static bool IsReadyPlayers(string checkName, List<Farmer> farmers)
        {
            var farmersUniqueMultiplayerIDs = farmers.Select(f => f.UniqueMultiplayerID).ToList();

            if (0 == farmersUniqueMultiplayerIDs.Count) { return false; }

            var readyChecks = ReadyChecks.GetValue(Game1.netReady) as IDictionary;
            var keys = readyChecks.Keys.Cast<string>().ToList();
            var values = readyChecks.Values.Cast<object>().ToList();

            for (int i = 0; i < readyChecks.Count; i++)
            {
                if (keys[i] == checkName)
                {
                    object ServerReadyCheckInstance = values[i];

                    if (null == ReadyStates)
                    {
                        ReadyStates = ServerReadyCheckInstance.GetType().GetField("ReadyStates", BindingFlags.NonPublic | BindingFlags.Instance);
                    }

                    var readyStatesColection = ReadyStates.GetValue(ServerReadyCheckInstance) as IDictionary;
                    var uniqueMultiplayerIDs = readyStatesColection.Keys.Cast<long>().ToList();
                    var readyStates = readyStatesColection.Values.Cast<object>().Select(v => Enum.ToObject(typeof(ReadyState), v)).ToList().Cast<ReadyState>().ToList();

                    for (int j = 0; j < readyStatesColection.Count; j++)
                    {
                        if (farmersUniqueMultiplayerIDs.Contains(uniqueMultiplayerIDs[j]))
                        {
                            if (ReadyState.NotReady == readyStates[j])
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
            }

            return false;
        }

        private enum ReadyState : byte
        {
            /// <summary> Not marked as ready to proceed with the check. </summary>            
            NotReady,
            /// <summary> Ready to proceed, but can still cancel. </summary>
            Ready,
            /// <summary> Ready to proceed, and can no longer cancel. </summary>            
            Locked
        }

        /// <summary>
        ///         Information about the `(<see cref="BindingFlags.NonPublic"/> | <see cref="BindingFlags.Instance"/>)` `ReadyChecks` field
        /// <br/>   of the class <see cref="StardewValley.Network.NetReady.ReadySynchronizer"/>
        /// </summary>
        private static FieldInfo ReadyChecks = typeof(ReadySynchronizer).GetField("ReadyChecks", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        ///         Information about the `(<see cref="BindingFlags.NonPublic"/> | <see cref="BindingFlags.Instance"/>)` `ReadyStates` field
        /// <br/>   of the class <see cref="StardewValley.Network.NetReady.Internal.ServerReadyCheck"/>
        /// <br/>   Since the class does not change, the variable only needs to be defined once.
        /// </summary>
        private static FieldInfo ReadyStates = null;

        #endregion
    }
}
