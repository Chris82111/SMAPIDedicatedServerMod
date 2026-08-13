using DedicatedServer.HostAutomatorStages.BehaviorStates;
using DedicatedServer.Utils;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Reflection;

namespace DedicatedServer.HostAutomatorStages
{
    internal class TransitionFestivalAttendanceBehaviorLink : BehaviorLink
    {
        #region Required in derived class

        public override int WaitTimeAutoLoad { get; set; } = (int)(60 * 0.2);
        public override int WaitTime { get; set; }

        public override void Process()
        {
            var numberOfPlayers = MainController.NumberOfPlayers;

            if (Festivals.ShouldAttend(numberOfPlayers) && 
                false == Festivals.IsWaitingToAttend())
            {
                WaitForFestivalAttendance();
            }
            else if (
                false == Festivals.ShouldAttend(numberOfPlayers) && 
                Festivals.IsWaitingToAttend())
            {
                StopWaitingForFestivalAttendance();
            }
            else if(Festivals.ShouldLeave(numberOfPlayers) &&
                false == Festivals.IsWaitingToLeave())
            {
                WaitForFestivalEnd();
            }
            else if (
                false == Festivals.ShouldLeave(numberOfPlayers) &&
                Festivals.IsWaitingToLeave())
            {
                StopWaitingForFestivalEnd();
            }
            else
            {
                Tuple<int, int> voteCounts = UpdateFestivalStartVotes();
                if (voteCounts != null)
                {
                    if (voteCounts.Item1 == voteCounts.Item2)
                    {
                        // Start the festival
                        SendChatMessage($"{voteCounts.Item1} / {voteCounts.Item2} votes casted. Starting the festival event...");

                        if (Festivals.IsTodayLuau && Game1.player.team.luauIngredients.Count > 0)
                        {
                            // If it's the Luau and the pot isn't empty, add a duplicate of someone else's item to the pot. It (mostly) doesn't matter
                            // which item is duplicated. Indeed, the total luau score is simply equal to the lowest score (or some extremum) of any item
                            // added, with two exceptions: 1) if anyone adds the mayor's shorts, the score is set to a magic number (6)
                            // and all other items added are ignored, and 2) if anyone doesn't add an item, the score is set to a magic number (5).
                            // This means that having X players put in X items is no different from having X+1 players put in X+1 items, where the
                            // additional item is a duplicate of one of the original X items. This is the intention. The only possible concern is that it
                            // looks like putting in better items will improve relationships more. But it's probably not all that noticeable of a difference
                            // anyways. So just duplicate the first element with Item.getOne().
                            Game1.player.team.luauIngredients.Add(Game1.player.team.luauIngredients[0].getOne());
                        }
                        Game1.CurrentEvent.answerDialogueQuestion(null, "yes");

                        DisableFestivalChatBox();
                    }
                    else
                    {
                        SendChatMessage($"{voteCounts.Item1} / {voteCounts.Item2} votes casted.");
                    }
                }

                //WaitTime = 0;
            }
        }

        #endregion

        private static readonly MethodInfo info = typeof(Game1).GetMethod("performWarpFarmer", BindingFlags.Static | BindingFlags.NonPublic);

        private readonly FestivalChatBox festivalChatBox;

        private int numFestivalStartVotes = 0;
        private int numFestivalStartVotesRequired = 0;

        public TransitionFestivalAttendanceBehaviorLink()
        {
            MainController.helper.Events.GameLoop.DayStarted += NewDay;

            festivalChatBox = new FestivalChatBox();
        }

        ~TransitionFestivalAttendanceBehaviorLink() => Dispose();

        public void Dispose()
        {
            MainController.helper.Events.GameLoop.DayStarted -= NewDay;
            DisableFestivalChatBox();
        }

        public Tuple<int, int> UpdateFestivalStartVotes()
        {
            if (festivalChatBox.IsEnabled())
            {
                int numFestivalStartVotes = festivalChatBox.NumVoted();
                if (numFestivalStartVotes != this.numFestivalStartVotes || MainController.NumberOfPlayers != numFestivalStartVotesRequired)
                {
                    this.numFestivalStartVotes = numFestivalStartVotes;
                    numFestivalStartVotesRequired = MainController.NumberOfPlayers;
                    return Tuple.Create(numFestivalStartVotes, numFestivalStartVotesRequired);
                }
            }
            return null;
        }

        public void EnableFestivalChatBox()
        {
            festivalChatBox.Enable();
            numFestivalStartVotes = 0;
            numFestivalStartVotesRequired = MainController.NumberOfPlayers;
        }

        public void DisableFestivalChatBox()
        {
            festivalChatBox.Disable();
        }

        public void NewDay(object sender, DayStartedEventArgs e)
        {
            numFestivalStartVotes = 0;
            numFestivalStartVotesRequired = MainController.NumberOfPlayers;
        }

        public void SendChatMessage(string message)
        {
            festivalChatBox.SendChatMessage(message);
        }

        private static string GetLocationOfFestival()
        {
            if (1 == Game1.weatherIcon)
            {
                return Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[0];
            }

            return null;
        }

        private void WaitForFestivalAttendance()
        {
            var location = Game1.getLocationFromName(GetLocationOfFestival());
            var warp = new Warp(0, 0, location.NameOrUniqueName, 0, 0, false);
            Game1.netReady.SetLocalReady("festivalStart", ready: true);
            Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", allowCancel: true, delegate (Farmer who)
            {
                Game1.exitActiveMenu();
                info.Invoke(null, new object[] { Game1.getLocationRequest(warp.TargetName), 0, 0, Game1.player.facingDirection.Value });

                if (Festivals.IsHostDecidingNextStep)
                {
                    // Don't enable chat box on spirit's eve nor feast of the winter star
                    EnableFestivalChatBox();
                }
            });

            // Wait for festival attendance
        }

        private static void StopWaitingForFestivalAttendance()
        {
            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog rcd)
            {
                rcd.closeDialog(Game1.player);
            }
            Game1.netReady.SetLocalReady("festivalStart", false);

            // Stop waiting for festival attendance
        }

        private void WaitForFestivalEnd()
        {
            Game1.netReady.SetLocalReady("festivalEnd", ready: true);
            Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", allowCancel: true, delegate (Farmer who)
            {
                Game1.currentLocation.currentEvent.forceEndFestival(who);
                DisableFestivalChatBox();
            });

            // Wait for festival end
        }

        private static void StopWaitingForFestivalEnd()
        {
            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog rcd)
            {
                rcd.closeDialog(Game1.player);
            }
            Game1.netReady.SetLocalReady("festivalEnd", false);

            // Stop waiting for festival end;
        }
    }
}
