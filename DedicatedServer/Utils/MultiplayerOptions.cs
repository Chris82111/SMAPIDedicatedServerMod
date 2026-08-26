using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Linq;

namespace DedicatedServer.Utils
{
    /// <summary>
    /// Offers multiplayer options and multiplayer support functions
    /// </summary>
    internal static class MultiplayerOptions
    {
        /// <summary>
        ///         The server mode option that you see in the game settings,
        /// <br/>   Options -> Multiplayer -> Server Mode
        /// <br/>  
        /// <br/>   Read the current state with <see cref="Game1.options.serverPrivacy"/>
        /// <br/>   Setzen Sie ihn mit der Methode<see cref="Game1.options.setServerMode()"/>
        /// </summary>
        public enum ServerModeTypes
        {
            InviteOnly  = ServerPrivacy.InviteOnly,
            FriendsOnly = ServerPrivacy.FriendsOnly,
            Offline     = ServerPrivacy.Public,
        }

        /// <summary>
        /// Effects the variable <see cref="Game1.multiplayerMode"/>
        /// </summary>
        private enum GameOptionMenue
        {
            /// <summary>
            /// Set game to singleplayer, without multiplayer options
            /// </summary>
            Singleplayer = 0,

            // / <summary>
            // / Set game to multiplayer, as client.
            // / </summary>
            //multiplayerClient = 1,

            /// <summary>
            /// Set game to multiplayer, as host with multiplayer options
            /// </summary>
            MultiplayerServer = 2
        }
        
        //private static IModHelper helper;

        //private static ModConfig config;

        //private static EventDrivenChatBox chatBox;

        public const string inviteCodeSaveFile = "invite_code.txt";

        public static void Init()
        {
            if (MainController.config.TryActivatingInviteCode)
            {
                TryActivatingInviteCode();
            }
        }

        public static void Reset()
        {
            Disable();
        }

        /// <summary>
        ///         Checks whether the invitation code is available
        /// </summary>
        /// <returns>
        ///         true : Invitation code is available
        /// <br/>   false: Invitation code is not available</returns>
        public static bool IsInviteCodeAvailable
        {
            get { return Game1.server?.canOfferInvite() ?? false; }
        }

        /// <summary>
        ///         Reads the invitation code
        /// </summary>
        public static string InviteCode
        {
            get
            {
                try { return Game1.server?.getInviteCode() ?? ""; }
                catch { return ""; }
            }
        }

        /// <summary>
        ///         Saves your invitaion code in a file (<see cref="inviteCodeSaveFile"/>).
        /// </summary>
        public static void SaveInviteCode()
        {
            string inviteCode = InviteCode;
            try
            {
                MainController.helper.Data.WriteJsonFile(inviteCodeSaveFile, inviteCode);
            }
            catch { }
        }

        /// <summary>
        ///         Sets and gets the option whether players can connect directly via IP address.
        /// <br/>   Check <see cref="EnableServer"/> beforehand, if the server is offline you cannot join the server
        /// </summary>
        public static bool IpConnectionsEnabled
        {
            get { return Game1.options.ipConnectionsEnabled; }
            set { Game1.options.ipConnectionsEnabled = value; }
        }

        /// <summary>
        ///         Activates and deactivates the server by switching
        /// <br/>   between a multiplayer and a single-player game
        /// <br/>   
        /// <br/>   If the game did not create an invitation code at startup,
        /// <br/>   toggling this property will create an invitation code.
        /// </summary>
        public static bool EnableServer
        {
            get {
                // You cannot use `Game1.IsServer`, it only checks
                // if `Game1.multiplayerMode` is 2 and not if the
                // server is running.
                return Game1.options.enableServer;
            }
            set
            {
                if (value)
                {
                    if (false == EnableServer)
                    {
                        // Enables the server
                        ServerMode = ServerModeTypes.FriendsOnly;

                        // Changes the host options menu and `Game1.IsServer` checks this option whether the server is online.
                        Game1.multiplayerMode = (int)GameOptionMenue.MultiplayerServer;
                    }
                }
                else
                {
                    if (true == EnableServer)
                    {
                        KickAll();

                        // Disable the server
                        ServerMode = ServerModeTypes.Offline;

                        // Changes the host options menu and `Game1.IsServer` checks this option whether the server is online.
                        Game1.multiplayerMode = (int)GameOptionMenue.Singleplayer;
                    }
                }
            }
        }

        /// <summary>
        ///         Changes the Server Mode option
        /// </summary>
        public static ServerModeTypes ServerMode {
            get
            {
                return (ServerModeTypes)Game1.options.serverPrivacy;
            }
            set
            {
                switch (value)
                {
                    case ServerModeTypes.Offline:
                        Game1.options.setServerMode("offline");
                        // Sets in-game option to offline
                        Game1.options.serverPrivacy = ServerPrivacy.Public;
                        break;
                    case ServerModeTypes.FriendsOnly:
                        Game1.options.setServerMode("friends");
                        break;
                    case ServerModeTypes.InviteOnly:
                        Game1.options.setServerMode("invite");
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        ///         Kicks all online players
        /// </summary>
        public static void KickAll()
        {
            foreach (var farmer in Game1.otherFarmers.Values.ToList())
            {
                Game1.server.kick(farmer.UniqueMultiplayerID);
            }
        }

        /// <summary>
        ///         Changes the settings so that monsters spawn on
        /// <br/>   the farm or not. Spawned monsters are not reset.
        /// <br/>   
        /// <br/>   true : The monsters will appear
        /// <br/>   false: No monsters will appear
        /// </summary>
        public static bool SpawnMonstersAtNight
        {
            get
            {
                return Game1.spawnMonstersAtNight;
            }
            set
            {
                Game1.spawnMonstersAtNight = value;
                Game1.game1.SetNewGameOption("SpawnMonstersAtNight", value);
                MainController.config.SpawnMonstersOnFarmAtNight = value;
                MainController.helper.WriteConfig(MainController.config);
            }
        }

        #region TRY_ACTIVATING_INVITE_CODE

        private enum TryActivatingStates
        {
            None,
            WaitForInviteCode,
            DisableServer,
            WaitUntilActivation,
            EnableServer,
            WaitForNewInviteCode,
        }

        #region i18n

        private static string _couldNotReceiveInvitationCode => MainController.helper.Translation.Get(
            "DedicatedServer.Utils.MultiplayerOptions.couldNotReceiveInvitationCode");

        private static string _couldReceiveInvitationCode => MainController.helper.Translation.Get(
            "DedicatedServer.Utils.MultiplayerOptions.couldReceiveInvitationCode",
            new { inviteCode = InviteCode });

        private static string _infoServerShuttingDown => MainController.helper.Translation.Get(
            "DedicatedServer.Utils.MultiplayerOptions.infoServerShuttingDown",
            new { time = _time});

        private static string _infoServerStartingUp => MainController.helper.Translation.Get(
            "DedicatedServer.Utils.MultiplayerOptions.infoServerStartingUp",
            new { time = _time });

        #endregion

        private static int _time;

        private static TryActivatingStates _tryActivatingState;

        private static readonly int[] _tryActivatingWaitTimes = { 9, 3, 9 };

        /// <summary>
        ///         Attempts to obtain the invitation code.
        /// <br/>   If successful, it is saved in a file.
        /// <br/>   If this is not successful in the first time period,
        /// <br/>   the multiplayer server is closed, a second time is
        /// <br/>   waited for and the multiplayer server is restarted.
        /// <br/>   In the third section, another attempt is made to
        /// <br/>   obtain the invite code.
        /// </summary>
        /// <returns>
        ///         true : if the handler has been started
        /// <br/>   false: the handler is already running and could not be started.</returns>
        public static bool TryActivatingInviteCode()
        {
            if (TryActivatingStates.None != _tryActivatingState) { return false; }

            _time = _tryActivatingWaitTimes[0];
            _tryActivatingState = TryActivatingStates.WaitForInviteCode;

            Enable();

            return true;
        }
        private static void Enable()
        {
            MainController.helper.Events.GameLoop.OneSecondUpdateTicked += TryActivatingInviteCodeWorker;
        }

        private static void Disable()
        {
            MainController.helper.Events.GameLoop.OneSecondUpdateTicked -= TryActivatingInviteCodeWorker;
        }

        private static void TryActivatingInviteCodeWorker(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (0 < _time)
            {
                _time--;
            }

            switch (_tryActivatingState)
            {
                case TryActivatingStates.None:
                    Disable();
                    return;

                case TryActivatingStates.WaitForInviteCode:
                    if("" != InviteCode)
                    {
                        SaveInviteCode();
                        _tryActivatingState = TryActivatingStates.None;
                        MainController.chatBox.textBoxEnter(_couldReceiveInvitationCode + TextColor.Green);
                        MainController.monitor.Log($"Could receive the invitation code {InviteCode}", LogLevel.Warn);
                        return;
                    }
                    if(0 == _time)
                    {
                        _tryActivatingState = TryActivatingStates.DisableServer;
                    }
                    MainController.chatBox.textBoxEnter(_infoServerShuttingDown + TextColor.Yellow);
                    MainController.monitor.Log($"Attention: Server will shut down in {_time} seconds", LogLevel.Debug);
                    break;

                case TryActivatingStates.DisableServer:
                    _time = _tryActivatingWaitTimes[1];
                    EnableServer = false;
                    _tryActivatingState = TryActivatingStates.WaitUntilActivation;
                    break;

                case TryActivatingStates.WaitUntilActivation:
                    if (0 == _time)
                    {
                        _tryActivatingState = TryActivatingStates.EnableServer;
                    }
                    MainController.chatBox.textBoxEnter(_infoServerStartingUp + TextColor.Yellow);
                    MainController.monitor.Log($"Attention: The server is started in {_time} seconds", LogLevel.Debug);
                    break;

                case TryActivatingStates.EnableServer:
                    _time = _tryActivatingWaitTimes[2];
                    _tryActivatingState = TryActivatingStates.WaitForNewInviteCode;
                    EnableServer = true;
                    break;

                case TryActivatingStates.WaitForNewInviteCode:
                    if ("" != InviteCode)
                    {
                        SaveInviteCode();
                        _tryActivatingState = TryActivatingStates.None;
                        MainController.chatBox.textBoxEnter(_couldReceiveInvitationCode + TextColor.Green);
                        MainController.monitor.Log($"Could receive the invitation code {InviteCode}", LogLevel.Info);
                        return;
                    }
                    else if (0 == _time)
                    {
                        _tryActivatingState = TryActivatingStates.None;
                        MainController.chatBox.textBoxEnter(_couldNotReceiveInvitationCode + TextColor.Red);
                        MainController.monitor.Log($"Attention: Invitation code could not be retrieved", LogLevel.Warn);
                        return;
                    }

                    // wait

                    break;
            }
        }

        #endregion
    }
}
