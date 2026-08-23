using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace DedicatedServer.Utils
{
    internal abstract class WarpPoints
    {
        private static readonly Farm farmLocation = Game1.getLocationFromName("Farm") as Farm;
        private static readonly FarmHouse farmHouseLocation = Game1.getLocationFromName("FarmHouse") as FarmHouse;
        private static readonly Forest forestLocation = Game1.getLocationFromName("Forest") as Forest;
        private static readonly Town townLocation = Game1.getLocationFromName("Town") as Town;
        private static readonly Mine mineLocation = Game1.getLocationFromName("Mine") as Mine;
        private static readonly Beach beachLocation = Game1.getLocationFromName("Beach") as Beach;
        private static readonly Mountain mountainLocation = Game1.getLocationFromName("Mountain") as Mountain;
        private static readonly CommunityCenter communityCenterLocation = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
        private static readonly Desert desertLocation = Game1.getLocationFromName("Desert") as Desert;

        private static Point FarmEntryLocation => farmLocation.GetMainFarmHouseEntry();
        private static Point FarmHouseEntryLocation => farmHouseLocation.getEntryLocation();

        private static readonly Point townNorthWestEntryLocation = new Point(0, 54);
        private static readonly Point mineEntryLocation = new Point(18, 13);
        private static readonly Point beachEntryLocation = new Point(38, 0);
        private static readonly Point robinLocation = new Point(12, 26);
        private static readonly Point clintLocation = new Point(94, 82);
        private static readonly Point pierreLocation = new Point(43, 57);
        private static readonly Point communityCenterEntranceLocation = new Point(33, 22);
        private static readonly Point wizzardLocation = new Point(5, 27);
        private static readonly Point busLocation = new Point(18, 28);

        /// <summary>
        ///         Warppoint on the farm
        /// <br/>
        /// <br/>   As the host is invisible and cannot be interacted with, the position
        /// <br/>   does not matter. A visible position simply allows you to interact with
        /// <br/>   the host when it has been made visible again with the `Invisible` command.
        /// <br/>   +2 places the host on the veranda on the right.
        /// </summary>
        public static Warp FarmWarp
        {
            get
            {
                var location = FarmEntryLocation;
                return new Warp(
                    location.X + 2, location.Y,
                    farmLocation.NameOrUniqueName,
                    location.X + 2, location.Y,
                    false, false);
            }
        }

        /// <summary>
        ///         Warppoint into the farmhouse
        /// </summary>
        public static Warp FarmHouseWarp
        {
            get
            {
                var location = FarmHouseEntryLocation;
                return new Warp(
                    location.X, location.Y,
                    farmHouseLocation.NameOrUniqueName,
                    location.X, location.Y,
                    false, false);
            }
        }

        /// <summary>
        ///         Warppoint to town, northwest entrance
        /// </summary>
        public static readonly Warp townWarp = new Warp(
            townNorthWestEntryLocation.X, townNorthWestEntryLocation.Y,
            townLocation.NameOrUniqueName,
            townNorthWestEntryLocation.X, townNorthWestEntryLocation.Y,
            false, false);

        /// <summary>
        ///         Warppoint to mine
        /// </summary>
        public static readonly Warp mineWarp = new Warp(
            mineEntryLocation.X, mineEntryLocation.Y,
            mineLocation.NameOrUniqueName,
            mineEntryLocation.X, mineEntryLocation.Y,
            false, false);


        /// <summary>
        ///         Warppoint to beach
        /// </summary>
        public static readonly Warp beachWarp = new Warp(
            beachEntryLocation.X, beachEntryLocation.Y,
            beachLocation.NameOrUniqueName,
            beachEntryLocation.X, beachEntryLocation.Y,
            false, false);

        /// <summary>
        ///         Warp to Robin
        /// </summary>
        public static readonly Warp robinWarp = new Warp(
            robinLocation.X, robinLocation.Y,
            mountainLocation.NameOrUniqueName,
            robinLocation.X, robinLocation.Y,
            false, false);

        /// <summary>
        ///         Warp to Clint
        /// </summary>
        public static readonly Warp clintWarp = new Warp(
            clintLocation.X, clintLocation.Y,
            townLocation.NameOrUniqueName,
            clintLocation.X, clintLocation.Y,
            false, false);

        /// <summary>
        ///         Warp to Pierre
        /// </summary>
        public static readonly Warp pierreWarp = new Warp(
            pierreLocation.X, pierreLocation.Y,
            townLocation.NameOrUniqueName,
            pierreLocation.X, pierreLocation.Y,
            false, false);

        /// <summary>
        ///         Warp to the Community Center
        /// </summary>
        public static readonly Warp communityCenterWarp = new Warp(
            communityCenterEntranceLocation.X, communityCenterEntranceLocation.Y,
            communityCenterLocation.NameOrUniqueName,
            communityCenterEntranceLocation.X, communityCenterEntranceLocation.Y,
            false, false);

        /// <summary>
        ///         Warp in front of the wizard's tower
        /// </summary>
        public static readonly Warp wizzardWarp = new Warp(
            wizzardLocation.X, wizzardLocation.Y,
            forestLocation.NameOrUniqueName,
            wizzardLocation.X, wizzardLocation.Y,
            false, false);

        /// <summary>
        ///         Warp to the desert, right in front of the bus
        /// </summary>
        public static readonly Warp desertWarp = new Warp(
            busLocation.X, busLocation.Y,
            desertLocation.NameOrUniqueName,
            busLocation.X, busLocation.Y,
            false, false);

        /// <summary>
        ///         Warp to the current host
        /// </summary>
        public static Warp masterPlayerWarp
        {
            get
            {
                Farmer farmer = Game1.MasterPlayer;
                return new Warp(
                    (int)farmer.Tile.X, (int)farmer.Tile.Y,
                    farmer.currentLocation.Name,
                    (int)farmer.Tile.X, (int)farmer.Tile.Y,
                    false, false);
            }
        }
    }
}
