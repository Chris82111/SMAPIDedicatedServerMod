using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DedicatedServer.Utils
{
    internal class HostHouseUpgrade
    {
        private static readonly string defaultBed = BedFurniture.DEFAULT_BED_INDEX;
        private static readonly string doubleBed = BedFurniture.DOUBLE_BED_INDEX;
        private static readonly string childBed = BedFurniture.CHILD_BED_INDEX;

        private static readonly FieldInfo farmerDaysUntilHouseUpgradeFieldInfo = typeof(Farmer).GetField("daysUntilHouseUpgrade");


        #region i18n

        private static string _canNotRemoveCellar = MainController.helper.Translation.Get(
            "DedicatedServer.Utils.HostHouseUpgrade.canNotRemoveCellar");

        private static string _houseHasNecessaryLevel = MainController.helper.Translation.Get(
            "DedicatedServer.Utils.HostHouseUpgrade.houseHasNecessaryLevel");

        private static string _updateIsInProgress = MainController.helper.Translation.Get(
            "DedicatedServer.Utils.HostHouseUpgrade.updateIsInProgress");

        private static string _hostUpdateNextDay = MainController.helper.Translation.Get(
            "DedicatedServer.Utils.HostHouseUpgrade.hostUpdateNextDay");

        private static string HostWillUpgradeInDays(int days)
        {
            return MainController.helper.Translation.Get(
            "DedicatedServer.Utils.HostHouseUpgrade.HostWillUpgradeInDays",
            new { days = days });
        }

        #endregion

        /// <summary>
        ///         Checks whether the host is upgrading their home
        /// </summary>
        public static bool IsHostUpgrading { get { return DaysUntilHouseUpgrade(Game1.player) > -1; } }

        /// <summary>
        ///         Gets the days until the house is updated
        /// </summary>
        /// <param name="farmer"></param>
        /// <returns></returns>
        public static int DaysUntilHouseUpgrade(Farmer farmer)
        {
            return ((NetInt)(farmerDaysUntilHouseUpgradeFieldInfo.GetValue(farmer)))?.Value ?? -1;
        }

        /// <summary>
        ///         Sets the days until the house is upgrades
        /// <br/>   Values less than or equal to 0 deactivate the upgrade
        /// </summary>
        /// <param name="farmer"></param>
        /// <param name="value">Number of days until the house is updated</param>
        public static void DaysUntilHouseUpgrade(Farmer farmer, int value)
        {
            if(0 >= value) { value = -1; }
            farmerDaysUntilHouseUpgradeFieldInfo.SetValue(farmer, new NetInt(value));
        }

        /// <summary>
        ///         Checks whether a cellar is available
        /// </summary>
        /// <param name="farmer"></param>
        /// <returns>
        ///         true:  Cellar is available
        /// <br/>   false: There is no cellar
        /// </returns>
        public static bool HasCellar(Farmer farmer)
        {
            return farmer.craftingRecipes.Keys.Contains("Cask");
        }

        /// <summary>
        ///         Checks whether a cellar is available
        /// </summary>
        /// <param name="location"></param>
        /// <returns>
        ///         true:  Cellar is available
        /// <br/>   false: There is no cellar
        /// </returns>
        public static bool HasCellar(FarmHouse location)
        {
            return location.owner.craftingRecipes.Keys.Contains("Cask");
            
        }

        /// <summary>
        ///         Checks whether the host's house needs to be upgraded based on all players
        /// </summary>
        public static bool NeedsUpgrade()
        {
            bool upgradeIsBeingExecuted = false;

            if (IsHostUpgrading)
            {
                return true;
            }
            else
            {
                int daysMin = int.MaxValue;
                int levelMax = int.MinValue;

                foreach (var farmer in Game1.getAllFarmhands().ToList())
                {
                    var localDays = DaysUntilHouseUpgrade(farmer);

                    if(levelMax < farmer.HouseUpgradeLevel)
                    {
                        levelMax = farmer.HouseUpgradeLevel;
                    }

                    if (0 <= localDays && // -1 means that no update is performed
                        localDays < daysMin && // select the lowest update days
                        farmer.HouseUpgradeLevel >= Game1.player.HouseUpgradeLevel) // only update if the determined update leads to a higher house level
                    {
                        daysMin = localDays;
                    }
                }

                if (daysMin != int.MaxValue && daysMin >= 0)
                {
                    MainController.chatBox.textBoxEnter(HostWillUpgradeInDays(daysMin));
                    DaysUntilHouseUpgrade(Game1.player, daysMin);
                    upgradeIsBeingExecuted = true;
                }
                else
                {
                    // Manual (if ModConfig.UpgradeHostHouseWithFarmhand is deactivated) or
                    // delayed upgrade (if ModConfig.UpgradeHostHouseWithFarmhand was deactivated)
                    if (levelMax > Game1.player.HouseUpgradeLevel)
                    {
                        MainController.chatBox.textBoxEnter(_hostUpdateNextDay);
                        DaysUntilHouseUpgrade(Game1.player, 1);
                        upgradeIsBeingExecuted = true;
                    }
                }
                return upgradeIsBeingExecuted;
            }
        }

        /// <summary>
        ///         Get all objects and removes them from the location. 
        /// <br/>   - objects and furniture are treated
        /// <br/>   - terrain features and large terrain features are not treated
        /// <br/>   - all others not treated are deleted
        /// </summary>
        /// <param name="location">A location, default (null) is the host's FarmHouse.</param>
        public static List<KeyValuePair<Vector2, StardewValley.Object>> GetAndRemoveAllItems(GameLocation location = null)
        {
            if(null == location)
            {
                location = Utility.getHomeOfFarmer(Game1.player);
            }
            
            List<KeyValuePair<Vector2, StardewValley.Object>> items = new List<KeyValuePair<Vector2, StardewValley.Object>>();


            List<KeyValuePair<Vector2, StardewValley.Object>> list1 = new List<KeyValuePair<Vector2, StardewValley.Object>>(location.Objects.Pairs);
            location.Objects.Clear();
            foreach (var item1 in list1)
            {
                if (item1.Value.GetType() == typeof(Chest))
                {
                    // The chest will not be added
                    foreach (var chestItem in ((Chest)item1.Value).Items)
                    {
                        items.Add(new KeyValuePair<Vector2, StardewValley.Object>(Vector2.Zero, (StardewValley.Object)chestItem));
                        item1.Value.performRemoveAction();
                    }
                }
                else
                {
                    items.Add(item1);
                    item1.Value.performRemoveAction();
                }
            }


            List<KeyValuePair<Vector2, TerrainFeature>> list2 = new List<KeyValuePair<Vector2, TerrainFeature>>(location.terrainFeatures.Pairs);
            location.terrainFeatures.Clear();
            foreach (var item2 in list2)
            {
                // All items are currently being deleted
            }


            foreach (var largeTerrainFeature in location.largeTerrainFeatures)
            {
                // All items are currently being deleted
            }


            List<KeyValuePair<Vector2, Furniture>> list3 = location.furniture.Select(f => { return new KeyValuePair<Vector2, Furniture>(f.TileLocation, f); }).ToList();
            location.furniture.Clear();
            foreach (var item3 in list3)
            {
                item3.Value.removeLights();

                if (item3.Value.GetType() != typeof(BedFurniture))
                {
                    items.Add(new KeyValuePair<Vector2, StardewValley.Object>(item3.Key, item3.Value));
                }
                item3.Value.performRemoveAction();

            }
            return items;
        }

        /// <summary>
        ///         Adds the standard bed or beds depending on the upgrade level of the house
        /// </summary>
        /// <param name="upgradeLevel">Upgrade level of the house</param>
        /// <param name="location"></param>
        public static void AddBed(int upgradeLevel, GameLocation location = null)
        {
            if (null == location)
            {
                location = Utility.getHomeOfFarmer(Game1.player);
            }

            switch (upgradeLevel)
            {
                case 0:
                    location.furniture.Add(new BedFurniture(defaultBed, new Vector2(9, 8)));
                    break;
                case 1:
                    location.furniture.Add(new BedFurniture(doubleBed, new Vector2(21, 3)));
                    break;
                case 2:
                case 3:
                    location.furniture.Add(new BedFurniture(childBed, new Vector2(37, 14)));
                    location.furniture.Add(new BedFurniture(childBed, new Vector2(41, 14)));
                    location.furniture.Add(new BedFurniture(doubleBed, new Vector2(42, 22)));
                    break;
                default:
                    location.furniture.Add(new BedFurniture(defaultBed, new Vector2(9, 8)));
                    break;
            }
        }

        /// <summary>
        ///         Obtains a safe position for a chest, depending on the upgrade level of the house
        /// </summary>
        /// <param name="upgradeLevel">Upgrade level of the house</param>
        /// <returns></returns>
        private static Vector2 GetChestPosition(int upgradeLevel)
        {
            switch (upgradeLevel)
            {
                case 0: return new Vector2(3, 8);
                case 1: return new Vector2(9, 7);
                case 2:
                case 3: return new Vector2(27, 27);
                default: return Vector2.Zero;
            }
        }

        public static void AddChestAndFillWithItems(List<Item> items, int upgradeLevel, GameLocation location = null)
        {
            if (null == location)
            {
                location = Utility.getHomeOfFarmer(Game1.player);
            }

            var chest = new Chest(true, GetChestPosition(upgradeLevel));

            foreach (var item in items)
            {
                chest.addItem(item);
            }

            location.Objects.Add(chest.TileLocation, chest);
        }

        /// <summary>
        ///         Upgrate and downgrade of the host's farme house.
        /// <br/>   - Please remove all items, decorations and so on.
        /// <br/>   - In case of doubt, everything will be deleted.
        /// <br/>   - It is not safe to be in the house while the house is being upgraded or downgraded.
        /// <br/>   - Some items will be stored in a chest, but there are only 36 places to store items.
        /// <br/>   - All beds will be destroyed and a new bed will be set up.
        /// <br/>   - The cellar is added to the map so that it cannot be removed.
        /// <br/>     <see cref="StardewValley.Locations.FarmHouse.setMapForUpgradeLevel(int)"/>
        /// </summary>
        /// <param name="level">Target upgrade level of house</param>
        public static void ManualUpdate(string level)
        {
            int targetLevel = -1;

            if (IsHostUpgrading)
            {
                MainController.chatBox.textBoxEnter(_updateIsInProgress);
                return;
            }

            try
            {
                targetLevel = Convert.ToInt16(level);
            }
            catch{}

            int oldLevel = Game1.player.HouseUpgradeLevel;

            if (oldLevel == targetLevel)
            {
                MainController.chatBox.textBoxEnter(_houseHasNecessaryLevel);
                return;
            }

            if( 0 <= targetLevel && 3 >= targetLevel)
            {
                FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);

                List<Item> items = GetAndRemoveAllItems(homeOfFarmer)
                    .Select(list => list.Value)
                    .ToList<Item>();


                Game1.player.HouseUpgradeLevel = targetLevel;
                homeOfFarmer.setMapForUpgradeLevel(targetLevel);
                Game1.stats.checkForBuildingUpgradeAchievements();
                Game1.player.performRenovation("FarmHouse");

                if(2 == targetLevel && HasCellar(homeOfFarmer))
                {
                    MainController.chatBox.textBoxEnter(_canNotRemoveCellar + TextColor.Red);
                }

                AddBed(targetLevel, homeOfFarmer);
                AddChestAndFillWithItems(items, targetLevel, homeOfFarmer);

                DaysUntilHouseUpgrade(Game1.player, -1);
            }
        }

#if false
        // Should not be used. Works but if a figure is inside the house,
        // it can be placed outside the room boundaries when upgrading
        public static void UpgradeToLevel(int targetLevel)
        {
            monitor.Log($"The host's farmhouse will upgraded to {targetLevel}", LogLevel.Debug);

            // The house level is only increased by 1 every day
            targetLevel = Game1.player.HouseUpgradeLevel + 1;

            /// The following is based on class Farmer, methode dayupdate(int timeWentToSleep)
            FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
            homeOfFarmer.moveObjectsForHouseUpgrade((int)targetLevel);
            Game1.player.HouseUpgradeLevel = targetLevel;
            homeOfFarmer.setMapForUpgradeLevel(targetLevel);
            Game1.stats.checkForBuildingUpgradeAchievements();
            Game1.player.autoGenerateActiveDialogueEvent("houseUpgrade_" + targetLevel);

            Game1.player.performRenovation("FarmHouse");

            DedicatedServer.Warp(WarpPoints.farmHouseWarp);
        }
#endif
    }
}
