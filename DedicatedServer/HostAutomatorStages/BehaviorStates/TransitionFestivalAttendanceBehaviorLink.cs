using DedicatedServer.HostAutomatorStages.BehaviorStates;
using DedicatedServer.Utils;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Reflection;

namespace DedicatedServer.HostAutomatorStages
{
    enum TransitionFestival
    {
        /// <summary> default value and set at each end of the day </summary>
        Uninit = 0,

        /// <summary> Set after the day is started, 
        /// <br/>   <see cref="TransitionFestivalAttendanceBehaviorLink.OnDayStarted"/></summary>
        NoFestivalDay,

        /// <summary> Set after the day is started,
        /// <br/>   <see cref="TransitionFestivalAttendanceBehaviorLink.OnDayStarted"/>,
        /// <br/>   waits for the participation time slot </summary>
        FestivalDay,


        /// <summary> This status is achieved as soon as the time slot for participating in the festival has been reached. </summary>
        FestivalGoingOn,

        /// <summary> Someone wants to go the festival </summary>
        WaitingForFestivalAttendance,

        /// <summary> Transition period until the game starts the festival </summary>
        StartingFestival,

        /// <summary> The festival has the option to launch an event </summary>
        AtFestivalChatBox,

        /// <summary> The festival does not offer any special events. </summary>
        AtFestival,

        /// <summary> Someone wants to leave the festival (from chatbox) </summary>
        WaitingForFestivalEndChatBox,

        /// <summary> Someone wants to leave the festival </summary>
        WaitingForFestivalEnd,

        /// <summary> Transition period until the game ends the festival </summary>
        EndingFestival,

        /// <summary> The festival is over </summary>
        FestivalIsOver,        
    }
    
    internal class TransitionFestivalAttendanceBehaviorLink : BehaviorLink
    {
        #region Required in derived class

        public override int WaitTimeAutoLoad { get; set; } = (int)(60 * 0.2);
        public override int WaitTime { get; set; }

        public override void Process()
        {
            bool isEvent;
            int ready;
            int required;
            switch (TransitionFestival)
            {
                case TransitionFestival.Uninit:
                case TransitionFestival.NoFestivalDay:
                case TransitionFestival.FestivalIsOver:
                    break;


                case TransitionFestival.FestivalDay:
                    if (Festivals.IsTheFestivalGoingOn())
                    {
                        TransitionFestival = TransitionFestival.FestivalGoingOn;
                        break;
                    }
                    break;


                case TransitionFestival.FestivalGoingOn:
                    if (false == Festivals.IsTheFestivalGoingOn())
                    {
                        TransitionFestival = TransitionFestival.FestivalIsOver;
                        break;
                    }

                    ready = Game1.netReady.GetNumberReady("festivalStart");
                    if (0 < ready)
                    {
                        TransitionFestival = TransitionFestival.WaitingForFestivalAttendance;
                        WaitForFestivalAttendance();
                        break;
                    }
                    break;


                case TransitionFestival.WaitingForFestivalAttendance:
                    if (false == Festivals.IsTheFestivalGoingOn())
                    {
                        TransitionFestival = TransitionFestival.FestivalIsOver;
                        break;
                    }

                    ready = Game1.netReady.GetNumberReady("festivalStart");
                    if (1 >= ready)
                    {
                        TransitionFestival = TransitionFestival.FestivalGoingOn;
                        StopWaitingForFestivalAttendance();
                        break;
                    }

                    required = Game1.netReady.GetNumberRequired("festivalStart");
                    if (required <= ready)
                    {
                        TransitionFestival = TransitionFestival.StartingFestival;
                        _timeout = Game1.timeOfDay + 30;
                        break;
                    }
                    break;


                case TransitionFestival.StartingFestival:
                    if (_timeout <= Game1.timeOfDay)
                    {
                        // This is only necessary if I want to go to a festival at the last minute
                        // but a cutscene pops up. In that case, the game doesn't switch to the
                        // festival, and the state machine freezes.
                        TransitionFestival = TransitionFestival.FestivalGoingOn;
                        StopWaitingForFestivalAttendance();
                        break;
                    }

                    isEvent = null != Game1.CurrentEvent;
                    if (isEvent)
                    {
                        if (Festivals.IsHostDecidingNextStep)
                        {
                            // Don't enable chat box on spirit's eve nor feast of the winter star
                            festivalChatBox.Enable();
                            TransitionFestival = TransitionFestival.AtFestivalChatBox;
                        }
                        else
                        {
                            TransitionFestival = TransitionFestival.AtFestival;
                        }
                        break;
                    }
                    break;

                case TransitionFestival.AtFestivalChatBox:
                    festivalChatBox.Update();
                    festivalChatBox.CheckVisible();

                    var peopleVoted = festivalChatBox.NumberOfPeopleWhoVoted();
                    var numberVoters = festivalChatBox.NumberOfVoters();

                    if (peopleVoted >= numberVoters)
                    {
                        if (0 == peopleVoted)
                        {
                            OnEventMassDisconnect();
                        }

                        SendChatMessage(_startFestival);

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

                        festivalChatBox.Disable();

                        // At the Stardw Valley Fair, you must manually exit the festival after the event.
                        if (Festivals.IsTodayStardewValleyFair)
                        {
                            TransitionFestival = TransitionFestival.AtFestival;
                        }
                        else
                        {
                            TransitionFestival = TransitionFestival.EndingFestival;
                            DelayForDanceOfTheMoonlightJellies();
                        }

                        break;
                    }

                    ready = Game1.netReady.GetNumberReady("festivalEnd");
                    if (0 < ready)
                    {
                        TransitionFestival = TransitionFestival.WaitingForFestivalEndChatBox;
                        WaitForFestivalEnd();
                        break;
                    }

                    break;


                case TransitionFestival.AtFestival:
                    if (0 == MainController.NumberOfPlayers)
                    {
                        // In a real festival, no player can join; if all players are disconnected,
                        // the host must end the event 
                        OnEventMassDisconnect();
                        TransitionFestival = TransitionFestival.EndingFestival;
                        DelayForDanceOfTheMoonlightJellies();
                        WaitForFestivalEnd();
                        break;
                    }

                    ready = Game1.netReady.GetNumberReady("festivalEnd");
                    if (0 < ready)
                    {
                        TransitionFestival = TransitionFestival.WaitingForFestivalEnd;
                        WaitForFestivalEnd();
                        break;
                    }
                    
                    break;

                case TransitionFestival.WaitingForFestivalEndChatBox:
                    ready = Game1.netReady.GetNumberReady("festivalEnd");
                    if (1 >= ready)
                    {
                        TransitionFestival = TransitionFestival.AtFestivalChatBox;
                        StopWaitingForFestivalEnd();
                        break;
                    }

                    required = Game1.netReady.GetNumberRequired("festivalEnd");
                    if (required <= ready)
                    {
                        TransitionFestival = TransitionFestival.EndingFestival;
                        DelayForDanceOfTheMoonlightJellies();
                        break;
                    }
                    break;

                case TransitionFestival.WaitingForFestivalEnd:
                    ready = Game1.netReady.GetNumberReady("festivalEnd");
                    if (1 >= ready)
                    {
                        TransitionFestival = TransitionFestival.AtFestival;
                        StopWaitingForFestivalEnd();
                        break;
                    }

                    required = Game1.netReady.GetNumberRequired("festivalEnd");
                    if (required <= ready)
                    {
                        TransitionFestival = TransitionFestival.EndingFestival;
                        DelayForDanceOfTheMoonlightJellies();
                        break;
                    }
                    break;

                case TransitionFestival.EndingFestival:
                    if (0 == MainController.NumberOfPlayers)
                    {
                        // If the players are disconnected at the end, the host doesn't
                        // completely leave the festival, and a player who logs in ends
                        // up at the festival without any NPCs. This can be fixed by
                        // logging out and back in, but this way, the day just runs smoothly.
                        OnEventMassDisconnect();
                    }

                    isEvent = null != Game1.CurrentEvent;
                    if (false == isEvent)
                    {
                        TransitionFestival = TransitionFestival.FestivalIsOver;
                        break;
                    }
                    break;


                default:
                    throw new Exception($"All states of 'TransitionFestival' are set, ({_transitionFestival}) is unknown.");
            }
        }

        #endregion

        private static readonly MethodInfo info = typeof(Game1).GetMethod("performWarpFarmer", BindingFlags.Static | BindingFlags.NonPublic);

        private TransitionFestival _transitionFestival = default;
        
        public TransitionFestival TransitionFestival
        {
            get => _transitionFestival;
            private set
            {
                _transitionFestival = value;
                OnTransitionFestivalChanged();
            }
        }

        #region i18n

        private string _startFestival => MainController.helper.Translation.Get(
            "DedicatedServer.HostAutomatorStages.TransitionFestivalAttendanceBehaviorLink.startFestival");

        #endregion

        /// <summary>
        /// Called if the property <see cref="TransitionFestival"/> is changed.
        /// </summary>
        public event EventHandler<TransitionFestival> TransitionFestivalChanged;

        /// <summary>
        /// Called when the event starts.
        /// </summary>
        public event EventHandler EventStarting;

        /// <summary>
        /// Called after the event has started.
        /// </summary>
        public event EventHandler EventStarted;

        /// <summary>
        /// Called when the event ends.
        /// </summary>
        public event EventHandler EventEnding;

        /// <summary>
        /// Called after the event has ended.
        /// </summary>
        public event EventHandler EventEnded;

        /// <summary>
        /// Called when all players have left the server.
        /// </summary>
        public event EventHandler EventMassDisconnect;

        private readonly FestivalChatBox festivalChatBox;
        
        private bool _hasEventMassDisconnectInvokedBefore = false;

        private int _timeout;

        public TransitionFestivalAttendanceBehaviorLink()
        {
            MainController.helper.Events.GameLoop.DayStarted += OnDayStarted;
            MainController.helper.Events.GameLoop.DayEnding += OnDayEnding;


            TransitionFestivalChanged += TransitionFestivalChangedHandler;
            EventMassDisconnect += (o, s) => Sleeping.ShouldSleepOverwrite = true;


            festivalChatBox = new FestivalChatBox();
            EventStarted += festivalChatBox.EventStarted;
        }

        ~TransitionFestivalAttendanceBehaviorLink() => Dispose();

        public void Dispose()
        {
            festivalChatBox.Disable();
            EventStarted -= festivalChatBox.EventStarted;


            MainController.helper.Events.GameLoop.DayStarted -= OnDayStarted;
            MainController.helper.Events.GameLoop.DayEnding -= OnDayEnding;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            TransitionFestival = (Festivals.IsFestivalDay())
                ? TransitionFestival.FestivalDay
                : TransitionFestival.NoFestivalDay;

            _hasEventMassDisconnectInvokedBefore = false;
        }

        private void OnDayEnding(object sender, DayEndingEventArgs e)
        {
            TransitionFestival = TransitionFestival.Uninit;
        }

        private void OnTransitionFestivalChanged()
        {
            TransitionFestivalChanged?.Invoke(this, TransitionFestival);
        }

        private void TransitionFestivalChangedHandler(object sender, TransitionFestival transitionFestival)
        {
            switch (transitionFestival)
            {
                case TransitionFestival.StartingFestival:
                    OnEventStarting();
                    break;

                case TransitionFestival.AtFestivalChatBox:
                case TransitionFestival.AtFestival:
                    OnEventStarted();
                    break;

                case TransitionFestival.EndingFestival:
                    OnEventEnding();
                    break;

                case TransitionFestival.FestivalIsOver:
                    OnEventEnded();
                    break;
            }
        }

        private void OnEventStarting()
            => EventStarting?.Invoke(this, EventArgs.Empty);

        private void OnEventStarted()
            => EventStarted?.Invoke(this, EventArgs.Empty);

        private void OnEventEnding()
            => EventEnding?.Invoke(this, EventArgs.Empty);

        private void OnEventEnded()
            => EventEnded?.Invoke(this, EventArgs.Empty);

        
        private void OnEventMassDisconnect()
        {
            if (false == _hasEventMassDisconnectInvokedBefore)
            {
                _hasEventMassDisconnectInvokedBefore = true;
                EventMassDisconnect?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        ///         The "Dance of the Moonlight Jellies" festival creates a new festival
        /// <br/>   after the first one, so a waiting period must be observed; otherwise,
        /// <br/>   the next state will be triggered too early.
        /// </summary>
        private void DelayForDanceOfTheMoonlightJellies()
        {
            if (Festivals.IsTodayDanceOfTheMoonlightJellies)
            {
                WaitTime = 60 * 15;
            }
        }
                    

        #region Controls whether the host joins or leaves an event

        private void WaitForFestivalAttendance()
        {
            var location = Game1.getLocationFromName(Festivals.GetStandardLocationOfFestival());
            var warp = new Warp(0, 0, location.NameOrUniqueName, 0, 0, false);
            Game1.netReady.SetLocalReady("festivalStart", ready: true);
            Game1.activeClickableMenu = new ReadyCheckDialog("festivalStart", allowCancel: true, delegate (Farmer who)
            {
                Game1.exitActiveMenu();
                info.Invoke(null, new object[] { Game1.getLocationRequest(warp.TargetName), 0, 0, Game1.player.facingDirection.Value });
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
                festivalChatBox.Disable();
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

        #endregion


        private static void SendChatMessage(string message)
            => MainController.chatBox.textBoxEnter(message);
    }
}
