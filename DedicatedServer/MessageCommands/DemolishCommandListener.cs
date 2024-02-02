﻿using DedicatedServer.Chat;
using DedicatedServer.HostAutomatorStages;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DedicatedServer.MessageCommands
{
    internal class DemolishCommandListener
    {
        private EventDrivenChatBox chatBox;

        public DemolishCommandListener(EventDrivenChatBox chatBox)
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

        private void destroyCabin(Farmer farmer, Building building, Farm f)
        {
            Action buildingLockFailed = delegate
            {
                WriteToPlayer(farmer, $"Error: {Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_LockFailed")}");
            };
            Action continueDemolish = delegate
            {
                if (building.daysOfConstructionLeft.Value > 0 || building.daysUntilUpgrade.Value > 0)
                {
                    WriteToPlayer(farmer, $"Error: {Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_DuringConstruction")}");
                }
                else if (building.indoors.Value != null && building.indoors.Value is AnimalHouse && (building.indoors.Value as AnimalHouse).animalsThatLiveHere.Count > 0)
                {
                    WriteToPlayer(farmer, $"Error: {Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_AnimalsHere")}");
                }
                else if (building.indoors.Value != null && building.indoors.Value.farmers.Any())
                {
                    WriteToPlayer(farmer, $"Error: {Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere")}");
                }
                else
                {
                    if (building.indoors.Value != null && building.indoors.Value is Cabin)
                    {
                        foreach (Farmer allFarmer in Game1.getAllFarmers())
                        {
                            if (allFarmer.currentLocation != null && allFarmer.currentLocation.Name == (building.indoors.Value as Cabin).GetCellarName())
                            {
                                WriteToPlayer(farmer, $"Error: {Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_PlayerHere")}");
                                return;
                            }
                        }
                    }

                    if (building.indoors.Value is Cabin && (building.indoors.Value as Cabin).farmhand.Value.isActive())
                    {
                        WriteToPlayer(farmer, $"Error: {Game1.content.LoadString("Strings\\UI:Carpenter_CantDemolish_FarmhandOnline")}");
                    }
                    else
                    {
                        building.BeforeDemolish();
                        Chest chest = null;
                        if (building.indoors.Value is Cabin)
                        {
                            List<Item> list = (building.indoors.Value as Cabin).demolish();
                            if (list.Count > 0)
                            {
                                chest = new Chest(playerChest: true);
                                chest.fixLidFrame();
                                chest.items.Set(list);
                            }
                        }

                        if (f.destroyStructure(building))
                        {
                            _ = building.tileY.Value;
                            _ = building.tilesHigh.Value;
                            Game1.flashAlpha = 1f;
                            building.showDestroyedAnimation(Game1.getFarm());
                            Utility.spreadAnimalsAround(building, f);
                            if (chest != null)
                            {
                                f.objects[new Vector2(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2)] = chest;
                            }
                        }
                    }
                }
            };

            Game1.player.team.demolishLock.RequestLock(continueDemolish, buildingLockFailed);
        }

        private Action genDestroyCabinAction(Farmer farmer, Building building)
        {
            void destroyCabinAction()
            {
                Farm f = Game1.getFarm();
                destroyCabin(farmer, building, f);
            }

            return destroyCabinAction;
        }

        private Action genCancelDestroyCabinAction(Farmer farmer)
        {
            void cancelDestroyCabinAction()
            {
                WriteToPlayer(farmer, "Action canceled.");
            }

            return cancelDestroyCabinAction;
        }

        private void chatReceived(object sender, ChatEventArgs e)
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

            if (tokens[0] == "demolish")
            {
                
                if (false == PasswordValidation.IsAuthorized(e.SourceFarmerId, p => p.Demolish))
                {
                    chatBox.textBoxEnter(PasswordValidation.notAuthorizedMessage);
                    return;
                }

                var sourceFarmer = Game1.otherFarmers.Values
                    .Where(farmer => farmer.UniqueMultiplayerID == e.SourceFarmerId)
                    .FirstOrDefault()
                    ?? Game1.player;


                if (tokens.Length != 1)
                {
                    WriteToPlayer(sourceFarmer, "Error: Invalid command usage.");
                    WriteToPlayer(sourceFarmer, "Usage: demolish");
                    return;
                }

                // Find the farmer it came from and determine their location
                var location = sourceFarmer.currentLocation;

                if (location is Farm f)
                {
                    var tileLocation = sourceFarmer.getTileLocation();
                    switch (sourceFarmer.facingDirection.Value)
                    {
                        case 1: // Right
                            tileLocation.X++;
                            break;
                        case 2: // Down
                            tileLocation.Y++;
                            break;
                        case 3: // Left
                            tileLocation.X--;
                            break;
                        default: // 0 = up
                            tileLocation.Y--;
                            break;
                    }
                    foreach (var building in f.buildings)
                    {
                        if (building.occupiesTile(tileLocation))
                        {
                            // Determine if the building can be demolished
                            var demolishCheckBlueprint = new BluePrint(building.buildingType.Value);
                            if (demolishCheckBlueprint.moneyRequired < 0)
                            {
                                // Hard-coded magic number (< 0) means it cannot be demolished
                                WriteToPlayer(sourceFarmer, "Error: This building can't be demolished.");
                                return;
                            }
                            else if (demolishCheckBlueprint.name == "Shipping Bin")
                            {
                                int num = 0;
                                foreach (var b in Game1.getFarm().buildings)
                                {
                                    if (b is ShippingBin)
                                    {
                                        num++;
                                    }

                                    if (num > 1)
                                    {
                                        break;
                                    }
                                }

                                if (num <= 1)
                                {
                                    // Must have at least one shipping bin at all times.
                                    WriteToPlayer(sourceFarmer, "Error: Can't demolish the last shipping bin.");
                                    return;
                                }
                            }

                            if (building.indoors.Value is Cabin)
                            {
                                Cabin cabin = building.indoors.Value as Cabin;
                                if (cabin.farmhand.Value != null && cabin.farmhand.Value.isCustomized.Value)
                                {
                                    // The cabin is owned by someone. Ask the player if they're certain; record in memory the action to destroy the building.
                                    var responseActions = new Dictionary<string, Action>();
                                    responseActions["yes"] = genDestroyCabinAction(sourceFarmer, building);
                                    responseActions["no"] = genCancelDestroyCabinAction(sourceFarmer);
                                    chatBox.RegisterFarmerResponseActionGroup(sourceFarmer.UniqueMultiplayerID, responseActions);
                                    WriteToPlayer(sourceFarmer, "This cabin belongs to a player. Are you sure you want to remove it? Message me \"yes\" or \"no\".");
                                    return;
                                }
                            }

                            // The cabin doesn't belong to anyone. Destroy it immediately without confirmation.
                            destroyCabin(sourceFarmer, building, f);
                            return;
                        }
                    }

                    WriteToPlayer(sourceFarmer, "Error: No building found. You must be standing next to a building and facing it.");
                }
                else
                {
                    WriteToPlayer(sourceFarmer, "Error: You cannot demolish buildings outside of the farm.");
                }
            }
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
    }
}
