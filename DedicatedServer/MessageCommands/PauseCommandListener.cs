using DedicatedServer.Chat;
using DedicatedServer.HostAutomatorStages;
using StardewValley;
using StardewValley.Menus;

namespace DedicatedServer.MessageCommands
{
    internal abstract class PauseCommandListener
    {
        public static void Enable()
        {
            MainController.chatBox.ChatReceived += chatReceived;
        }

        public static void Disable()
        {
            MainController.chatBox.ChatReceived -= chatReceived;
        }

        private static void chatReceived(object sender, ChatEventArgs e)
        {
            var tokens = e.Message.ToLower().Split(' ');
            if (tokens.Length == 0)
            {
                return;
            }

            if (Game1.player.UniqueMultiplayerID != e.SourceFarmerId)
            {
                if (ChatBox.privateMessage != e.ChatKind) { return; }
            }

            if (tokens[0] == "pause")
            {
                if (false == PasswordValidation.IsAuthorized(e.SourceFarmerId, p => p.Pause))
                {
                    return;
                }

                Game1.netWorldState.Value.IsPaused = !Game1.netWorldState.Value.IsPaused;

                if (Game1.netWorldState.Value.IsPaused)
                {
                    MainController.chatBox.globalInfoMessage("Paused");
                    return;
                }
                MainController.chatBox.globalInfoMessage("Resumed");
            }
        }
    }
}
