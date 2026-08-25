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
        public Farmer Farmer { get; set; } = null;
        public bool Vote { get; set; } = false;
        public bool InFront { get; set; } = false;
        public bool Visible { get; set; } = false;
        public DateTime Timeout { get; set; } = DateTime.MaxValue;

        public FarmerDecisionDto(Farmer farmer)
        {
            Farmer = farmer;
        }
    }

    internal class FestivalChatBox
    {
        public event EventHandler InFrontEntered;
        public event EventHandler InFrontExited;

        public event EventHandler VisibleEntered;
        public event EventHandler VisibleExited;

        private Dictionary<long, FarmerDecisionDto> FarmerDecision = new();

        private NPC Lewis { get; set; }

        private bool enabled = false;

        private const int WaitTimeSeconds = 10;

        #region i18n

        private string _entryMessage1 => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.FestivalChatBox.entryMessage1",
            new { start = _keyStart });

        private string _entryMessage2 => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.FestivalChatBox.entryMessage2",
            new { cancel = _keyCancel });

        private string _entryMessage3 => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.FestivalChatBox.entryMessage3",
            new { seconds = WaitTimeSeconds });

        private string _keyStart => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.FestivalChatBox.keyStart");

        private string _keyCancel => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.FestivalChatBox.keyCancel");
        
        private string _sendVoteUpdate => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.FestivalChatBox.sendVoteUpdate",
            new { voted = NumberOfPeopleWhoVoted(), voters = NumberOfVoters() });

        private string _voteStarted => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.FestivalChatBox.voteStarted");

        private string _voteCanceled => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.FestivalChatBox.voteCanceled");

        #endregion

        public FestivalChatBox()
        {
            VisibleEntered += (s, e) => SendChatMessage(_sendVoteUpdate);
            VisibleExited += (s, e) => SendChatMessage(_sendVoteUpdate);
        }

        public bool IsEnabled()
            => enabled;

        public void Enable()
        {
            if (false == enabled)
            {
                enabled = true;

                Update();

                MainController.chatBox.ChatReceived += OnChatReceived;

                MainController.chatBox.textBoxEnter(_entryMessage1);
                MainController.chatBox.textBoxEnter(_entryMessage2);
                MainController.chatBox.textBoxEnter(_entryMessage3);
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


        private void OnInFrontEntered()
            => InFrontEntered?.Invoke(this, EventArgs.Empty);

        private void OnInFrontExited()
            => InFrontExited?.Invoke(this, EventArgs.Empty);

        private void OnVisibleEntered()
            => VisibleEntered?.Invoke(this, EventArgs.Empty);

        private void OnVisibleExited()
            => VisibleExited?.Invoke(this, EventArgs.Empty);

        /// <summary>
        ///         Updates the <see cref="FarmerDecision"/> dictionary, must be called periodically.
        /// <br/> 
        /// <br/>   Only if the number of players (<see cref="MainController.NumberOfPlayers"/>)
        /// <br/>   does not match the current number of players in the dictionary.
        /// <br/>   This works because no new players can join during an event.
        /// <br/>   The only possibility is for a connection to be interrupted,
        /// <br/>   which reduces the number of players.
        /// </summary>
        public void Update()
        {
            if (MainController.NumberOfPlayers == FarmerDecision.Count) { return; }

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
                    dummy.Add(farmer.Key, new FarmerDecisionDto(farmer.Value));
                }
            }

            FarmerDecision = dummy;
        }

        public void CheckVisible()
        {
            foreach (var item in FarmerDecision.Values)
            {
                if (Npc.IsFarmerInFront(item.Farmer, Lewis, 2, 0) && false == HasMenuOpen(item.Farmer))
                {
                    if (item.Vote) { return; }

                    if (false == item.InFront)
                    {
                        item.InFront = true;
                        item.Timeout = DateTime.UtcNow.AddSeconds(WaitTimeSeconds);
                        OnInFrontEntered();
                    }
                    else
                    {
                        if (false == item.Visible)
                        {
                            if (DateTime.UtcNow >= item.Timeout)
                            {
                                item.Visible = true;
                                OnVisibleEntered();
                            }
                        }
                    }
                }
                else
                {
                    if (item.Vote) { return; }

                    if (true == item.InFront)
                    {
                        item.InFront = false;
                        OnInFrontExited();

                        if (true == item.Visible)
                        {
                            item.Visible = false;
                            OnVisibleExited();
                        }
                    }
                }
            }
        }

        public int NumberOfPeopleWhoVoted()
        {
            if (false == enabled) { return -1; }

            return FarmerDecision.Count(f => f.Value.Visible || f.Value.Vote);
        }

        public int NumberOfVoters()
        {
            if (false == enabled){ return -1; }

            return FarmerDecision.Count;
        }
        
        private void OnChatReceived(object sender, ChatEventArgs e)
        {
            long id = e.SourceFarmerId;

            // For example, if a player leaves the game (ChatKind = 2)
            if (0 == id) { return; }

            if (Game1.player.UniqueMultiplayerID == id) { return; }
            
            if (false == FarmerDecision.TryGetValue(id, out FarmerDecisionDto item)) { return; }

            if (item.Visible) { return; }

            var message = e.Message.ToLowerInvariant();

            if (message.Equals(_keyStart))
            {
                if (false == item.Vote)
                {
                    item.Vote = true;
                    SendChatMessage(_voteStarted);
                    SendChatMessage(_sendVoteUpdate);
                }
            }
            else if (message.Equals(_keyCancel))
            {
                if (true == item.Vote)
                {
                    item.Vote = false;
                    SendChatMessage(_voteCanceled);
                    SendChatMessage(_sendVoteUpdate);
                }
            }
        }

        /// <summary>
        /// This is true for every menu, including a dialogue with an NPC
        /// </summary>
        /// <param name="farmer"></param>
        /// <returns>
        ///         true : A menu is open
        /// <br/>   false: No menu is open</returns>
        public static bool HasMenuOpen(Farmer farmer)
            => farmer.hasMenuOpen.Value;

        private static void SendChatMessage(string message)
            => MainController.chatBox.textBoxEnter(message);
    }
}
