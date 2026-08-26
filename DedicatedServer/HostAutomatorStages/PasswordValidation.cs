using DedicatedServer.Chat;
using DedicatedServer.Config;
using DedicatedServer.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DedicatedServer.HostAutomatorStages
{
    internal class PasswordValidation
    {
        #region i18n

        public static string _notAuthorizedMessage => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.PasswordValidation.notAuthorizedMessage");
        
        #endregion

        private IModHelper helper;

        private ModConfig config;

        private EventDrivenChatBox chatBox;

        private static List<long> ids = new List<long>();

        private static bool passwordValidationDisabled = false;

        private static List<string> allowedPasswordProtectedFunctions = new List<string>();

        public PasswordValidation(IModHelper helper, ModConfig config, EventDrivenChatBox chatBox)
        {
            this.helper = helper;
            this.config = config;
            this.chatBox = chatBox;

            allowedPasswordProtectedFunctions.AddRange(
                typeof(PasswordProtectedCommands).GetProperties()
                    .Select(p => p.Name.ToLower()).ToList());
        }

        public void Enable()
        {
            if (null != config.Password && "" == config.Password)
            {
                passwordValidationDisabled = true;
                return;
            }

            chatBox.ChatReceived += chatReceived;
            helper.Events.Multiplayer.PeerDisconnected += PlayerHasLoggedOut;
            AddId(Game1.player.UniqueMultiplayerID);
            passwordValidationDisabled = false;
        }

        public void Disable(bool validationDisable = false)
        {
            chatBox.ChatReceived -= chatReceived;
            helper.Events.Multiplayer.PeerDisconnected -= PlayerHasLoggedOut;
            ids = new List<long>();
            passwordValidationDisabled = validationDisable;
        }

        /// <summary>
        ///         Checks whether the specified user is currently logged in.
        /// <br/>   
        /// <br/>  - The check can be deactivated via <see cref="passwordValidationDisabled"/>
        /// <br/>  - The check is always performed if the property is null otherwise the
        /// <br/>    check can be deactivated if the property is false, e.g. with <c>p => p.Sleep</c>
        /// <br/>    The corresponding class is: <see cref="PasswordProtectedCommands"/>
        /// </summary>
        /// <param name="id">Id of the farmer</param>
        /// <param name="property">null or the correcponding property as delegate</param>
        /// <returns></returns>
        public static bool IsAuthorized(long id, Func<PasswordProtectedCommands, bool> property = null)
        {
            if (passwordValidationDisabled)
            {
                return true;
            }

            if (null != property && false == property(MainController.config.PasswordProtected))
            {
                return true;
            }

            var contains = ids.Contains(id);

            if (false == contains)
            {
                MainController.chatBox.textBoxEnter(PasswordValidation._notAuthorizedMessage + TextColor.Red);
            }

            return contains;
        }

        private void chatReceived(object sender, ChatEventArgs e)
        {
            var tokens = e.Message.Split(' ');

            if (0 == tokens.Length) { return; }

            if (Game1.player.UniqueMultiplayerID != e.SourceFarmerId)
            {
                if (ChatBox.privateMessage != e.ChatKind) { return; }
            }

            string command = tokens[0].ToLower();

            string param = 1 < tokens.Length ? tokens[1] : "";

            long id = e.SourceFarmerId;

            var sourceFarmer = Game1.getAllFarmhands() // Host is null
                .Where(farmer => farmer.UniqueMultiplayerID == id)
                .FirstOrDefault()
                ?? Game1.player;

            switch (command)
            {
                case "login": // /message ServerBot LogIn xxxx
                    LogIn(sourceFarmer, password: param);
                    break;

                case "logout": // /message ServerBot LogOut
                    LogOut(sourceFarmer);
                    break;

                case "passwordprotectshow": // /message ServerBot PasswordProtectShow
                    PasswordProtectShow(sourceFarmer);
                    break;

                case "passwordprotectadd": // /message ServerBot PasswordProtectAdd xxxx
                    PasswordProtectAdd(sourceFarmer, command: param);
                    break;

                case "passwordprotectremove": // /message ServerBot PasswordProtectRemove xxxx
                    PasswordProtectRemove(sourceFarmer, command: param);
                    break;
            }
        }

        /// <summary>
        ///         Separates the properties according to the state of the properties
        /// </summary>
        /// <param name="passwordProtected"></param>
        /// <param name="passwordUnprotected"></param>
        private void PasswordProtectedFunctionState(out string passwordProtected, out string passwordUnprotected)
        {
            List<PropertyInfo> info;
            List<string> list;

            var properties = config.PasswordProtected.GetType().GetProperties();

            info = properties
                .Where(p => true == (bool)p.GetValue(config.PasswordProtected))
                .ToList();

            list = info
                .Select(p => $"{p.Name}: {p.GetValue(config.PasswordProtected)}")
                .ToList();

            passwordProtected = string.Join(", ", list.ToArray());

            info = properties
                .Where(p => false == (bool)p.GetValue(config.PasswordProtected))
                .ToList();

            list = info
                .Select(p => $"{p.Name}: {p.GetValue(config.PasswordProtected)}")
                .ToList();

            passwordUnprotected = string.Join(", ", list.ToArray());
        }

        /// <summary>
        ///         Checks the password entered and logs in a player.
        /// </summary>
        /// <param name="sourceFarmer"></param>
        /// <param name="password">Password set in the config file <see cref="config.Password"/></param>
        private void LogIn(Farmer sourceFarmer, string password)
        {
            if (null != config.Password && password == config.Password)
            {
                if (AddId(sourceFarmer.UniqueMultiplayerID))
                {
                    WriteToPlayer(sourceFarmer, "Password correct." + TextColor.Green);
                }
                else
                {
                    WriteToPlayer(sourceFarmer, "You were already logged in." + TextColor.Green);
                }
            }
            else
            {
                WriteToPlayer(sourceFarmer, "Password wrong." + TextColor.Red);
            }
        }

        /// <summary>
        ///         Logs out a player
        /// </summary>
        /// <param name="sourceFarmer"></param>
        private void LogOut(Farmer sourceFarmer)
        {
            if (RemoveId(sourceFarmer.UniqueMultiplayerID))
            {
                WriteToPlayer(sourceFarmer, "Successfully logged out." + TextColor.Green);
            }
            else
            {
                WriteToPlayer(sourceFarmer, "You were not logged in." + TextColor.Aqua);
            }
        }

        /// <summary>
        ///         Shows an overview of the currently protected commands.
        /// </summary>
        /// <param name="sourceFarmer"></param>
        private void PasswordProtectShow(Farmer sourceFarmer)
        {
            if (false == PasswordValidation.IsAuthorized(sourceFarmer.UniqueMultiplayerID))
            {
                return;
            }

            PasswordProtectedFunctionState(out string passwordProtected, out string passwordUnprotected);
            if ("" != passwordProtected)
            {
                WriteToPlayer(sourceFarmer, "These commands are protected");
                WriteToPlayer(sourceFarmer, passwordProtected + TextColor.Green);
            }
            if ("" != passwordUnprotected)
            {
                WriteToPlayer(sourceFarmer, "These commands are unprotected");
                WriteToPlayer(sourceFarmer, passwordUnprotected + TextColor.Orange);
            }
        }

        /// <summary>
        ///         Adds password protection.
        /// </summary>
        /// <param name="sourceFarmer"></param>
        /// <param name="command">This is the name of a property of the class <see cref="PasswordProtectedCommands"/></param>
        private void PasswordProtectAdd(Farmer sourceFarmer, string command)
        {
            if (false == PasswordValidation.IsAuthorized(sourceFarmer.UniqueMultiplayerID))
            {
                return;
            }

            command = command.ToLower();
            if (allowedPasswordProtectedFunctions.Contains(command))
            {
                var property = config.PasswordProtected.GetType().GetProperties()
                    .Where(p => p.Name.ToLower() == command)
                    .FirstOrDefault();

                if (null != property)
                {
                    property.SetValue(config.PasswordProtected, true, null);
                    WriteToPlayer(sourceFarmer, $"added \"{command}\"" + TextColor.Green);
                }

                helper.WriteConfig(config);
            }
            else
            {
                WriteToPlayer(sourceFarmer, $"Parameter \"{command}\" is unknown." + TextColor.Red);
            }
        }

        /// <summary>
        ///         Removes password protection.
        /// </summary>
        /// <param name="sourceFarmer"></param>
        /// <param name="command">This is the name of a property of the class <see cref="PasswordProtectedCommands"/></param>
        private void PasswordProtectRemove(Farmer sourceFarmer, string command)
        {
            if (false == PasswordValidation.IsAuthorized(sourceFarmer.UniqueMultiplayerID))
            {
                return;
            }

            command = command.ToLower();
            if (allowedPasswordProtectedFunctions.Contains(command))
            {
                var property = config.PasswordProtected.GetType().GetProperties()
                    .Where(p => p.Name.ToLower() == command)
                    .FirstOrDefault();

                if (null != property)
                {
                    property.SetValue(config.PasswordProtected, false, null);
                    WriteToPlayer(sourceFarmer, $"Removed \"{command}\"" + TextColor.Orange);
                }

                helper.WriteConfig(config);
            }
            else
            {
                WriteToPlayer(sourceFarmer, $"Parameter \"{command}\" is unknown." + TextColor.Red);
            }
        }

        /// <summary>
        ///         Logs out a player when he leaves the game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerHasLoggedOut(object sender, PeerDisconnectedEventArgs e)
        {
            RemoveId(e.Peer.PlayerID);
        }

        private void WriteToPlayer(Farmer farmer, string message)
        {
            if (null == farmer || farmer.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID)
            {
                chatBox.textBoxEnter($" {message}");
            }
            else
            {
                chatBox.textBoxEnter($"/message {farmer.Name} {message}");
            }
        }

        private bool AddId(long id)
        {
            if (false == ids.Contains(id))
            {
                ids.Add(id);
                return true;
            }

            return false;
        }

        private bool RemoveId(long id)
        {
            return ids.Remove(id);
        }

    }
}
