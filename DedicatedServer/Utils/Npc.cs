using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Linq;

namespace DedicatedServer.Utils
{
    public class Npc
    {
        /// <summary>
        ///         Get an NPC by its name.
        /// <br/>   The location of the event can be found in <see cref="Game1.currentLocation;"/>
        /// </summary>
        /// <param name="name">The NPC name.</param>
        /// <returns>Returns the matching NPC if found, else null.</returns>
        public static NPC GetNpc(string name)
            => (null == Game1.CurrentEvent)
                ? Game1.getCharacterFromName(name, mustBeVillager: true)
                : Game1.CurrentEvent.actors.FirstOrDefault(x => 0 == string.Compare(x.Name, name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Calculates the distance between a farmer and an NPC
        /// </summary>
        /// <param name="farmer"></param>
        /// <param name="npc"></param>
        /// <returns>The distance between a farmer and an NPC</returns>
        public static double Distance(Farmer farmer, NPC npc)
        {
            if (farmer == null) { return double.MaxValue; }
            if (npc == null) { return double.MaxValue; }

            if (farmer.currentLocation.Name != npc.currentLocation.Name) { return double.MaxValue; }

            return Distance(npc.Tile, farmer.Tile);
        }

        /// <summary>
        /// Checks whether a farmer is near an NPC
        /// </summary>
        /// <param name="farmer"></param>
        /// <param name="npc"></param>
        /// <param name="range">Radius from the NPC</param>
        /// <returns>
        ///         true : The farmer is in the given range
        /// <br/>   false: The farmer is outside the specified radius</returns>
        public static bool IsFarmerWithinRange(Farmer farmer, NPC npc, double range)
        {
            if (farmer == null) { return false; }
            if (npc == null) { return false; }

            if (farmer.currentLocation.Name != npc.currentLocation.Name) { return false; }

            return 0 >= Distance(npc.Tile, farmer.Tile) - range;
        }

        /// <summary>
        /// Checks if a farmer is in front of an NPC
        /// </summary>
        /// <param name="farmer"></param>
        /// <param name="npc"></param>
        /// <param name="length">The distance from the NPC to the farmer</param>
        /// <param name="width">The width to the right and left, expressed as the number of tiles</param>
        /// <returns>
        ///         true : The farmer is in the front of the NPC
        /// <br/>   false: The farmer is not in the front of the NPC</returns>
        /// <exception cref="Exception"></exception>
        public static bool IsFarmerInFront(Farmer farmer, NPC npc, int length, int width)
        {
            if (farmer == null) { return false; }
            if (npc == null) { return false; }

            if (farmer.currentLocation.Name != npc.currentLocation.Name) { return false; }

            bool inRange;
            int xLow, xHeigh, yLow, yHeigh;

            switch (npc.FacingDirection)
            {
                case Game1.up:
                    xLow = (int)npc.Tile.X - width;
                    xHeigh = (int)npc.Tile.X + width;
                    yLow = (int)npc.Tile.Y - length;
                    yHeigh = (int)npc.Tile.Y;

                    inRange =
                        xLow <= farmer.Tile.X && farmer.Tile.X <= xHeigh &&
                        yLow <= farmer.Tile.Y && farmer.Tile.Y <= yHeigh;
                    break;

                case Game1.right:
                    xLow = (int)npc.Tile.X;
                    xHeigh = (int)npc.Tile.X + length;
                    yLow = (int)npc.Tile.Y - width;
                    yHeigh = (int)npc.Tile.Y + width;

                    inRange =
                        xLow <= farmer.Tile.X && farmer.Tile.X <= xHeigh &&
                        yLow <= farmer.Tile.Y && farmer.Tile.Y <= yHeigh;
                    break;

                case Game1.down:
                    xLow = (int)npc.Tile.X - width;
                    xHeigh = (int)npc.Tile.X + width;
                    yLow = (int)npc.Tile.Y;
                    yHeigh = (int)npc.Tile.Y + length;

                    inRange =
                        xLow <= farmer.Tile.X && farmer.Tile.X <= xHeigh &&
                        yLow <= farmer.Tile.Y && farmer.Tile.Y <= yHeigh;
                    break;

                case Game1.left:
                    xLow = (int)npc.Tile.X - length;
                    xHeigh = (int)npc.Tile.X;
                    yLow = (int)npc.Tile.Y - width;
                    yHeigh = (int)npc.Tile.Y + width;

                    inRange =
                        xLow <= farmer.Tile.X && farmer.Tile.X <= xHeigh &&
                        yLow <= farmer.Tile.Y && farmer.Tile.Y <= yHeigh;
                    break;

                default:
                    throw new Exception($"Value {npc.FacingDirection} is not supported as direction");
            }

            return inRange;
        }

        private static double Distance(Vector2 first, Vector2 second)
        {
            var difference = first - second;
            return Math.Sqrt(difference.X * difference.X + difference.Y * difference.Y);
        }
    }
}
