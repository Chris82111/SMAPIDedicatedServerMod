﻿using DedicatedServer.Chat;
using DedicatedServer.Config;
using DedicatedServer.Utils;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DedicatedServer.MessageCommands
{
    internal class ServerCommandListener
    {
        private EventDrivenChatBox chatBox;

        private ModConfig config;

        private IModHelper helper;

        public ServerCommandListener(IModHelper helper, ModConfig config, EventDrivenChatBox chatBox)
        {
            this.helper  = helper;
            this.config  = config;
            this.chatBox = chatBox;
        }

        public void Enable()
        {
            chatBox.ChatReceived += chatReceived;
        }

        public void Disable()
        {
            chatBox.ChatReceived -= chatReceived;
        }

        private void chatReceived(object sender, ChatEventArgs e)
        {
            var tokens = e.Message.Split(' ');

            if (0 == tokens.Length) { return; }

            string command = tokens[0].ToLower();

            if(Game1.player.UniqueMultiplayerID == e.SourceFarmerId)
            {
                switch (command)
                {
                    case "letmeplay":
                        chatBox.textBoxEnter($"The host is now a player, all host functions are deactivated." + TextColor.Green);
                        HostAutomation.LetMePlay();
                        break;

                    #region DEBUG_COMMANDS
                    #if false

                    case "letmecontrol":
                        HostAutomation.LetMeControl();
                        break;

                    case "multiplayer":
                        MultiplayerOptions.EnableServer = true;
                        break;

                    case "singleplayer":
                        MultiplayerOptions.EnableServer = false;
                        break;

                    case "farm":
                        var farm = Game1.getLocationFromName("farm") as Farm;
                        var mainFarmHouseEntry = farm.GetMainFarmHouseEntry();
                        Game1.player.warpFarmer(new Warp(mainFarmHouseEntry.X, mainFarmHouseEntry.Y, farm.NameOrUniqueName, mainFarmHouseEntry.X, mainFarmHouseEntry.Y, false, false));
                        break;

                    case "mine":
                        var mine = Game1.getLocationFromName("Mine") as Mine;
                        Game1.player.warpFarmer(new Warp(18, 13, mine.NameOrUniqueName, 18, 13, false));
                        break;

                    case "location":
                        var location = Game1.player.getTileLocation();
                        chatBox.textBoxEnter("location: " + Game1.player.currentLocation.ToString());
                        chatBox.textBoxEnter("x: " + location.X + ", y:" + location.Y);
                        break;

                    #endif
                    #endregion
                }
            }
            else
            {
                if (ChatBox.privateMessage != e.ChatKind)
                {
                    return;
                }
            }      

            string param = 1 < tokens.Length ? tokens[1].ToLower() : "";

            var sourceFarmer = Game1.otherFarmers.Values
                .Where(farmer => farmer.UniqueMultiplayerID == e.SourceFarmerId)
                .FirstOrDefault()?
                .Name ?? Game1.player.Name;

            switch (command)
            {
                case "takeover":
                    chatBox.textBoxEnter($"Control has been transferred to the host, all host functions are switched on." + TextColor.Aqua);
                    HostAutomation.TakeOver();
                    break;

                case "safeinvitecode": // /message ServerBot SafeInviteCode
                    MultiplayerOptions.SaveInviteCode();
                    if (MultiplayerOptions.IsInviteCodeAvailable)
                    {
                        chatBox.textBoxEnter($"Your invite code is saved in the mod folder in the file {MultiplayerOptions.inviteCodeSaveFile}." + TextColor.Green);
                    }
                    else
                    {
                        chatBox.textBoxEnter($"The game has no invite code." + TextColor.Red);
                    }
                    break;

                case "invitecode": // /message ServerBot InviteCode
                    chatBox.textBoxEnter($"Invite code: {MultiplayerOptions.InviteCode}" + ("" == MultiplayerOptions.InviteCode ? TextColor.Red : TextColor.Green) );
                    break;

                case "invisible": // /message ServerBot Invisible
                    Invisible.InvisibleOverwrite = !Invisible.InvisibleOverwrite;
                    chatBox.textBoxEnter($"The host is invisible {Invisible.InvisibleOverwrite}");
                    break;

                case "sleep": // /message ServerBot Sleep
                    if (false == HostAutomation.EnableHostAutomation)
                    {
                        chatBox.textBoxEnter($"Cannot start sleep because the host is controlled by the player." + TextColor.Red);
                        break;
                    }
                    if (Sleeping.ShouldSleepOverwrite)
                    {
                        Sleeping.ShouldSleepOverwrite = false;
                        chatBox.textBoxEnter($"The host is back on his feet." + TextColor.Aqua);
                    }
                    else
                    {
                        chatBox.textBoxEnter($"The host will go to sleep." + TextColor.Green);
                        Sleeping.ShouldSleepOverwrite = true;
                    }
                    
                    break;

                case "resetday": // /message ServerBot ResetDay
                    RestartDay.ResetDay((seconds) => chatBox.textBoxEnter($"Attention: Server will reset the day in {seconds} seconds" + TextColor.Orange));
                    break;

                case "shutdown": // /message ServerBot Shutdown
                    RestartDay.ShutDown((seconds) => chatBox.textBoxEnter($"Attention: Server will shut down in {seconds} seconds" + TextColor.Orange));
                    break;

                case "spawnmonster": // /message ServerBot SpawnMonster
                    if (MultiplayerOptions.SpawnMonstersAtNight)
                    {
                        chatBox.textBoxEnter($"No more monsters will appear." + TextColor.Green);
                        MultiplayerOptions.SpawnMonstersAtNight = false;
                    }
                    else
                    {
                        chatBox.textBoxEnter($"Monsters will appear." + TextColor.Red);
                        MultiplayerOptions.SpawnMonstersAtNight = true;
                    }
                    break;

                case "mbp": // /message ServerBot mbp on
                case "movebuildpermission":
                case "movepermissiong":
                    // As the host you can run commands in the chat box, using a forward slash(/) before the command.
                    // See: <seealso href="https://stardewcommunitywiki.com/Multiplayer"/>

                    var moveBuildPermissionParameter = new List<string>() { "off", "owned", "on" };

                    if (moveBuildPermissionParameter.Any(param.Equals))
                    {
                        if (config.MoveBuildPermission == param)
                        {
                            chatBox.textBoxEnter("Parameter for MoveBuildPermission is already " + config.MoveBuildPermission + TextColor.Orange);
                        }
                        else
                        {
                            config.MoveBuildPermission = param;
                            chatBox.textBoxEnter($"Changed MoveBuildPermission to {config.MoveBuildPermission}" + TextColor.Green );
                            chatBox.textBoxEnter("/mbp " + config.MoveBuildPermission);
                            helper.WriteConfig(config);
                        }
                    }
                    else
                    {
                        chatBox.textBoxEnter($"Only the following parameters are valid for MoveBuildPermission: {String.Join(", ", moveBuildPermissionParameter.ToArray())}" + TextColor.Red);
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
