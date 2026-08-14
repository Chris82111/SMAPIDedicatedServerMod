using DedicatedServer.Chat;
using DedicatedServer.Utils;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DedicatedServer.HostAutomatorStages
{
    class FarmerDecisionDto
    {
        public bool Vote { get; set; } = false;
        public bool Visible { get; set; } = false;
    }

    internal class FestivalChatBox
    {
        private const string entryMessage = "When you wish to start the festival, type \"start\" into chat. If you'd like to cancel your vote, type \"cancel\".";

        public Dictionary<long, FarmerDecisionDto> FarmerDecision = new();

        public NPC Lewis { get; private set; }

        private bool enabled = false;

        public FestivalChatBox()
        {
        }

        public bool IsEnabled()
            => enabled;

        public void Enable()
        {
            if (false == enabled)
            {
                enabled = true;

                Update();

                MainController.chatBox.textBoxEnter(entryMessage);
                MainController.chatBox.ChatReceived += OnChatReceived;
            }
        }

        public void Disable()
        {
            if (enabled)
            {
                enabled = false;

                FarmerDecision.Clear();

                Lewis = null;

                MainController.chatBox.ChatReceived -= OnChatReceived;
            }
        }

        public void EventStarted(object sender, EventArgs e)
        {
            Lewis = Npc.GetNpc("Lewis");
        }

        public void CheckVisible()
        {
            foreach (var farmer in Game1.otherFarmers.Values.ToList())
            {
                if (Npc.IsFarmerInFront(farmer, Lewis, 2, 0))
                {
                    VisibleStart(farmer.UniqueMultiplayerID);
                }
                else
                {
                    VisibleCancel(farmer.UniqueMultiplayerID);
                }
            }
        }

        public int NumberOfPeopleWhoVoted()
        {
            if (false == enabled) { return -1; }

            if (MainController.NumberOfPlayers != FarmerDecision.Count)
            {
                Update();
            }

            return FarmerDecision.Count(f => f.Value.Visible || f.Value.Vote);
        }

        public int NumberOfVoters()
        {
            if (false == enabled){ return -1; }

            if (MainController.NumberOfPlayers != FarmerDecision.Count)
            {
                Update();
            }

            return FarmerDecision.Count();
        }

        public void Update()
        {
            var dummy = new Dictionary<long, FarmerDecisionDto>();
            FarmerDecisionDto item;
            foreach (var farmer in MainController.OnlineFarmers())
            {
                if (FarmerDecision.TryGetValue(farmer.Key, out item))
                {
                    dummy.Add(farmer.Key, item);
                }
                else
                {
                    dummy.Add(farmer.Key, new FarmerDecisionDto());
                }  
            }

            FarmerDecision = dummy;
        }

        private void VisibleStart(long id)
        {
            if (false == FarmerDecision.TryGetValue(id, out var item)) { return; }

            if (item.Vote) { return; }

            if (false == item.Visible)
            {
                item.Visible = true;
                SendChatMessage($"{NumberOfPeopleWhoVoted()} / {NumberOfVoters()} votes casted.");
            }
        }

        private void VisibleCancel(long id)
        { 
            if (false == FarmerDecision.TryGetValue(id, out var item)) { return; }

            if (item.Vote) { return; }

            if (true == item.Visible)
            {
                item.Visible = false;
                SendChatMessage($"{NumberOfPeopleWhoVoted()} / {NumberOfVoters()} votes casted.");
            }
        }

        private void OnChatReceived(object sender, ChatEventArgs e)
        {
            long id = e.SourceFarmerId;

            // For example, if a player leaves the game (ChatKind = 2)
            if (0 == id) { return; }

            if (Game1.player.UniqueMultiplayerID == id) { return; }
            
            if (false == FarmerDecision.TryGetValue(id, out FarmerDecisionDto item)) { return; }

            if (item.Visible) { return; }

            switch (e.Message.ToLowerInvariant())
            {
                case "start":
                    if (false == item.Vote)
                    {
                        item.Vote = true;
                        SendChatMessage("Vote started");
                        SendChatMessage($"{NumberOfPeopleWhoVoted()} / {NumberOfVoters()} votes casted.");
                    }
                    break;

                case "cancel":
                    if (true == item.Vote)
                    {
                        item.Vote = false;
                        SendChatMessage("Vote canceled");
                        SendChatMessage($"{NumberOfPeopleWhoVoted()} / {NumberOfVoters()} votes casted.");
                    }
                    break;
            }
        }

        private static void SendChatMessage(string message)
            => MainController.chatBox.textBoxEnter(message);
    }
}
