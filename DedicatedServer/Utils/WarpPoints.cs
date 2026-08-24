using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace DedicatedServer.Utils
{
    internal abstract class WarpPoints
    {
        private static readonly Farm _farmLocation = Game1.getLocationFromName("Farm") as Farm;
        private static readonly FarmHouse _farmHouseLocation = Game1.getLocationFromName("FarmHouse") as FarmHouse;
        private static readonly Forest _forestLocation = Game1.getLocationFromName("Forest") as Forest;
        private static readonly Town _townLocation = Game1.getLocationFromName("Town") as Town;
        private static readonly Mine _mineLocation = Game1.getLocationFromName("Mine") as Mine;
        private static readonly Beach _beachLocation = Game1.getLocationFromName("Beach") as Beach;
        private static readonly Mountain _mountainLocation = Game1.getLocationFromName("Mountain") as Mountain;
        private static readonly CommunityCenter _communityCenterLocation = Game1.getLocationFromName("CommunityCenter") as CommunityCenter;
        private static readonly Desert _desertLocation = Game1.getLocationFromName("Desert") as Desert;

        private static Point FarmEntryPoint => _farmLocation.GetMainFarmHouseEntry();
        private static Point FarmHouseEntryPoint => _farmHouseLocation.getEntryLocation();

        private static readonly Point _townNorthWestEntryPoint = new Point(0, 54);
        private static readonly Point _mineEntryPoint = new Point(18, 13);
        private static readonly Point _beachEntryPoint = new Point(38, 0);
        private static readonly Point _robinPoint = new Point(12, 26);
        private static readonly Point _clintPoint = new Point(94, 82);
        private static readonly Point _pierrePoint = new Point(43, 57);
        private static readonly Point _communityCenterEntrancePoint = new Point(33, 22);
        private static readonly Point _wizzardPoint = new Point(5, 27);
        private static readonly Point _busPoint = new Point(18, 28);

        /// <summary>
        ///         Warppoint on the farm
        /// <br/>
        /// <br/>   As the host is invisible and cannot be interacted with, the position
        /// <br/>   does not matter. A visible position simply allows you to interact with
        /// <br/>   the host when it has been made visible again with the `Invisible` command.
        /// <br/>   +2 places the host on the veranda on the right.
        /// </summary>
        public static Warp farmWarp
        {
            get
            {
                var location = FarmEntryPoint;
                return new Warp(
                    location.X + 2, location.Y,
                    _farmLocation.NameOrUniqueName,
                    location.X + 2, location.Y,
                    false, false);
            }
        }

        /// <summary>
        ///         Warppoint into the farmhouse
        /// </summary>
        public static Warp farmHouseWarp
        {
            get
            {
                var location = FarmHouseEntryPoint;
                return new Warp(
                    location.X, location.Y,
                    _farmHouseLocation.NameOrUniqueName,
                    location.X, location.Y,
                    false, false);
            }
        }

        /// <summary>
        ///         Warppoint to town, northwest entrance
        /// </summary>
        public static readonly Warp townWarp = new Warp(
            _townNorthWestEntryPoint.X, _townNorthWestEntryPoint.Y,
            _townLocation.NameOrUniqueName,
            _townNorthWestEntryPoint.X, _townNorthWestEntryPoint.Y,
            false, false);

        /// <summary>
        ///         Warppoint to mine
        /// </summary>
        public static readonly Warp mineWarp = new Warp(
            _mineEntryPoint.X, _mineEntryPoint.Y,
            _mineLocation.NameOrUniqueName,
            _mineEntryPoint.X, _mineEntryPoint.Y,
            false, false);


        /// <summary>
        ///         Warppoint to beach
        /// </summary>
        public static readonly Warp beachWarp = new Warp(
            _beachEntryPoint.X, _beachEntryPoint.Y,
            _beachLocation.NameOrUniqueName,
            _beachEntryPoint.X, _beachEntryPoint.Y,
            false, false);

        /// <summary>
        ///         Warp to Robin
        /// </summary>
        public static readonly Warp robinWarp = new Warp(
            _robinPoint.X, _robinPoint.Y,
            _mountainLocation.NameOrUniqueName,
            _robinPoint.X, _robinPoint.Y,
            false, false);

        /// <summary>
        ///         Warp to Clint
        /// </summary>
        public static readonly Warp clintWarp = new Warp(
            _clintPoint.X, _clintPoint.Y,
            _townLocation.NameOrUniqueName,
            _clintPoint.X, _clintPoint.Y,
            false, false);

        /// <summary>
        ///         Warp to Pierre
        /// </summary>
        public static readonly Warp pierreWarp = new Warp(
            _pierrePoint.X, _pierrePoint.Y,
            _townLocation.NameOrUniqueName,
            _pierrePoint.X, _pierrePoint.Y,
            false, false);

        /// <summary>
        ///         Warp to the Community Center
        /// </summary>
        public static readonly Warp communityCenterWarp = new Warp(
            _communityCenterEntrancePoint.X, _communityCenterEntrancePoint.Y,
            _communityCenterLocation.NameOrUniqueName,
            _communityCenterEntrancePoint.X, _communityCenterEntrancePoint.Y,
            false, false);

        /// <summary>
        ///         Warp in front of the wizard's tower
        /// </summary>
        public static readonly Warp wizzardWarp = new Warp(
            _wizzardPoint.X, _wizzardPoint.Y,
            _forestLocation.NameOrUniqueName,
            _wizzardPoint.X, _wizzardPoint.Y,
            false, false);

        /// <summary>
        ///         Warp to the desert, right in front of the bus
        /// </summary>
        public static readonly Warp desertWarp = new Warp(
            _busPoint.X, _busPoint.Y,
            _desertLocation.NameOrUniqueName,
            _busPoint.X, _busPoint.Y,
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
