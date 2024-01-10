using DedicatedServer.Chat;
using StardewValley;

namespace DedicatedServer.MessageCommands
{
    internal class InvisibleCommandListener
    {
        private EventDrivenChatBox chatBox;

        public InvisibleCommandListener(EventDrivenChatBox chatBox)
        {
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
            var tokens = e.Message.ToLower().Split(' ');
            if (tokens.Length == 0)
            {
                return;
            }
            // Private message chatKind is 3
            if (e.ChatKind == 3 && tokens[0] == "invisible")
            {
                Game1.displayFarmer = !Game1.displayFarmer;
                if(Game1.displayFarmer){ // Refresh to make bot back to visible
                    Game1.player.warpFarmer(new Warp(Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.currentLocation.Name, Game1.player.getTileX(), Game1.player.getTileY(), false, false));
                }
                chatBox.textBoxEnter($"Is now {Game1.displayFarmer}");
            }
        }
    }
}