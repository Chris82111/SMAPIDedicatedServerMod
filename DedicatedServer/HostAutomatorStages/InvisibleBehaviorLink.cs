﻿using DedicatedServer.Utils;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DedicatedServer.HostAutomatorStages
{
    /// <summary>
    ///         This class can make the host invisible
    /// <br/>   
    /// <br/>   - As long as someone is invisible, you cannot give them anything.
    /// <br/>   - After someone has been warped/teleported, they will be visible again.
    /// </summary>
    internal class InvisibleBehaviorLink : BehaviorLink
    {
        public InvisibleBehaviorLink(BehaviorLink next = null) : base(next)
        {
        }

        /// <summary>
        ///         Overwrites the behavior of the class <see cref="InvisibleBehaviorLink"/>
        /// <br/>   
        /// <br/>   true : The behavior of this class is not changed.
        /// <br/>   false: Host is visible
        /// <br/>   
        /// <br/>   Works only if <see cref="OnUpdateTicked"/> ticks
        /// </summary>
        protected static bool InvisibleOverwrite { set; get; } = true;

        public override void Process(BehaviorState state)
        {
            switch (InvisibleOverwrite)
            {
                case true:
                    if (SetInvisibleDisplayOnChanges())
                    {
                        state.SetWaitTicks(60);
                    }
                    else
                    {
                        processNext(state);
                    }
                    break;

                case false:
                    if (SetVisibleDisplayOnChanges())
                    {
                        state.SetWaitTicks(60);
                    }
                    else
                    {
                        processNext(state);
                    }
                    break;
            }
        }

        /// <summary>
        ///         Makes the host invisible
        /// </summary>
        /// <returns>
        ///         true : Visibility has been changed.
        /// <br/>   false: Visibility has not been changed.</returns>
        protected static bool SetInvisibleDisplayOnChanges()
        {
            bool changed = false;

            if (Game1.displayFarmer)
            {
                Game1.displayFarmer = false;
                changed = true;
            }

            return changed;
        }

        /// <summary>
        ///         Makes the host visible
        /// </summary>
        /// <param name="forcedRefresh">
        ///         true : Forces the change of visibility.
        /// <br/>   false: Changes the visibility only when required.</param>
        /// <returns>
        ///         true : Visibility has been changed.
        /// <br/>   false: Visibility has not been changed.</returns>
        protected static bool SetVisibleDisplayOnChanges(bool forcedRefresh = false)
        {
            bool changed = false;

            if (false == Game1.displayFarmer || forcedRefresh)
            {
                Game1.displayFarmer = true;
                // Refresh to make bot back to visible
                Game1.player.warpFarmer(new Warp(
                    Game1.player.getTileX(), Game1.player.getTileY(),
                    Game1.player.currentLocation.Name,
                    Game1.player.getTileX(), Game1.player.getTileY(),
                    false, false));
                changed = true;
            }

            return changed;
        }

    }
}
