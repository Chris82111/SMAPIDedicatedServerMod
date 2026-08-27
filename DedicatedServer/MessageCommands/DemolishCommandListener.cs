using DedicatedServer.Chat;
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
    internal abstract class DemolishCommandListener
    {
        public static void Enable()
        {
            MainController.chatBox.ChatReceived += chatReceived;
        }

        public static void Disable()
        {
            MainController.chatBox.ChatReceived -= chatReceived;
        }

        private static void destroyCabin(Farmer farmer, Building building, Farm f)
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

                    if (building.indoors.Value is Cabin && (building.indoors.Value as Cabin).owner.isActive())
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
                                chest.Items.Clear();
                                chest.Items.AddRange(list);
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

        private static Action genDestroyCabinAction(Farmer farmer, Building building)
        {
            void destroyCabinAction()
            {
                Farm f = Game1.getFarm();
                destroyCabin(farmer, building, f);
            }

            return destroyCabinAction;
        }

        private static Action genCancelDestroyCabinAction(Farmer farmer)
        {
            void cancelDestroyCabinAction()
            {
                WriteToPlayer(farmer, "Action canceled.");
            }

            return cancelDestroyCabinAction;
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

            if (tokens[0] == "demolish")
            {
                
                if (false == PasswordValidation.IsAuthorized(e.SourceFarmerId, p => p.Demolish))
                {
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
                    var tileLocation = sourceFarmer.Tile;
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
                            var carpenterMenu = new CarpenterMenu(StardewValley.Game1.builder_robin);

                            if (false == carpenterMenu.hasPermissionsToDemolish(building))
                            {
                                // Hard-coded magic number (< 0) means it cannot be demolished
                                WriteToPlayer(sourceFarmer, "Error: This building can't be demolished.");
                                return;
                            }

                            if (building.indoors.Value is Cabin)
                            {
                                Cabin cabin = building.indoors.Value as Cabin;
                                if (cabin.owner != null && cabin.owner.isCustomized.Value)
                                {
                                    // The cabin is owned by someone. Ask the player if they're certain; record in memory the action to destroy the building.
                                    var responseActions = new Dictionary<string, Action>();
                                    responseActions["yes"] = genDestroyCabinAction(sourceFarmer, building);
                                    responseActions["no"] = genCancelDestroyCabinAction(sourceFarmer);
                                    MainController.chatBox.RegisterFarmerResponseActionGroup(sourceFarmer.UniqueMultiplayerID, responseActions);
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

        private static void WriteToPlayer(Farmer farmer, string message)
        {
            if (null == farmer || farmer.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID)
            {
                MainController.chatBox.textBoxEnter($" {message}");
            }
            else
            {
                MainController.chatBox.textBoxEnter($"/message {farmer.Name} {message}");
            }
        }
    }
}
