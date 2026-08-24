using DedicatedServer.Chat;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Linq;
using System.Reflection;

namespace DedicatedServer.Utils
{
    internal class ServerHost
    {        
        private static EventDrivenChatBox chatBox;

        public ServerHost(EventDrivenChatBox chatBox)
        {
            ServerHost.chatBox = chatBox;
        }

        /// <summary>
        ///         Empty the host inventory, tools are not deleted
        /// </summary>
        static public void EmptyHostInventory()
        {
            for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
            {
                var item = Game1.player.Items[i];

                if (null == item) continue;

                if (item.canBeTrashed())
                {
                    chatBox?.textBoxEnter($" Item {item.Name} deleted");
                    Game1.player.removeItemFromInventory(item);
                }
            }
        }

        /// <summary>
        ///         Ensure that a number of free slots are available
        /// </summary>
        /// <param name="numberOfFreeSlot"></param>
        /// <returns>The result of the method:
        /// <br/>   true : The requested slots are free
        /// <br/>   false: The requested slots could not be provided</returns>
        static public bool EnsureFreeSlotNumber(int numberOfFreeSlot)
        {
            for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
            {
                var item = Game1.player.Items[i];

                if (null == item)
                {
                    numberOfFreeSlot--;
                }
                else
                {
                    if (item.canBeTrashed())
                    {
                        chatBox?.textBoxEnter($" Item {item.Name} deleted");
                        Game1.player.removeItemFromInventory(item);
                        numberOfFreeSlot--;
                    }
                }

                if(numberOfFreeSlot <= 0)
                {
                    break;
                }
            }

            if(0 < numberOfFreeSlot)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        ///         Deletes the last set of items when the inventory is full
        /// <br/>   Goes from right to left, tools are not deleted
        /// </summary>
        static public void ClearLastItem()
        {
            if (Game1.player.isInventoryFull())
            {
                for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
                {
                    var item = Game1.player.Items[i];

                    if (null == item) continue;

                    if (item.canBeTrashed())
                    {
                        chatBox?.textBoxEnter($" Item {item.Name} dumped");
                        Game1.player.removeItemFromInventory(item);
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///         Closes all open menus
        /// </summary>
        static public void ForceClosingAllMenu()
        {
            while (null != Game1.activeClickableMenu)
            {
                Game1.activeClickableMenu = null;
                if (Game1.CurrentEvent != null)
                {
                    Game1.CurrentEvent.CurrentCommand++;
                }

                Game1.playSound("bigDeSelect");
            }
        }

        static public void UpdateAllTools(int upgradeLevel)
        {
            foreach (var item in Game1.player.Items)
            {
                if (item is Tool)
                {
                    UpdateTool((Tool)item, upgradeLevel);
                }
            }
        }

        static public void UpdateAxe(int upgradeLevel)
        {
            if (Game1.player.Items.OfType<Axe>().FirstOrDefault() is Axe axe)
            {
                UpdateTool(axe, upgradeLevel);
            }
        }

        /// <summary>
        ///         Upgrades a tool to the give upgrade level.
        /// <br/>   Scythe, Pan, and Sword are not handled.
        /// <br/>   
        /// <br/>   Level:
        /// <br/>   0 = Basic
        /// <br/>   1 = Copper
        /// <br/>   2 = Steel
        /// <br/>   3 = Gold
        /// <br/>   4 = Iridium
        /// </summary>
        /// <param name="tool"></param>
        /// <param name="upgradeLevel"></param>
        static public void UpdateTool(Tool tool, int upgradeLevel)
        {
            if (null == tool) { return; }

            if (typeof(Pan) == tool.GetType() || typeof(MeleeWeapon) == tool.GetType())
            {
                return;
            }

            // Change behaviour
            Tool newTool = GetNewToolAndUpdateBehaviour(tool, upgradeLevel);

            if (null == newTool) { return; }

            MigrateLegacyItemId.Invoke(newTool, null); // Change look

            // Changes to the animation when in use
            /// From <see cref="ToolDataDefinition.GetRawData"/>
            if (null != newTool.ItemId)
            {
                if (Game1.toolData.TryGetValue(newTool.ItemId, out var data))
                {
                    newTool.SetSpriteIndex(data.SpriteIndex);
                }
            }

            Game1.player.removeItemFromInventory(tool);

            Game1.player.addItemToInventory(newTool);

            newTool.actionWhenClaimed();

            //Clint update process
            //Game1.player.toolBeingUpgraded.Value = tool;
            //Game1.player.toolBeingUpgraded.Value = null;
            //Game1.player.daysLeftForToolUpgrade.Value = 0;
        }

        /// <summary>
        ///         The <see cref="Tool.MigrateLegacyItemId"/> method is overridden by a derived
        /// <br/>   class; the method of the derived class is called.
        /// </summary>
        static private readonly MethodInfo MigrateLegacyItemId = 
            typeof(Tool).GetMethod(
                "MigrateLegacyItemId",
                BindingFlags.Instance | BindingFlags.NonPublic);


        private static Tool GetNewToolAndUpdateBehaviour(Tool tool, int? upgradeLevel)
        {
            ConstructorInfo constructor;

            if (null == upgradeLevel)
            {
                upgradeLevel = tool.UpgradeLevel;
            }

            if (typeof(MeleeWeapon) == tool.GetType())
            {
                constructor = tool
                    .GetType()
                    .GetConstructor(new Type[1] { typeof(string) });

                if (null == constructor) { return null; }

                return (Tool)constructor.Invoke(new object[] { tool.ItemId });
            }
            else if (typeof(Pan) == tool.GetType())
            {
                constructor = tool
                    .GetType()
                    .GetConstructor(new Type[1] { typeof(int) });

                if (null == constructor) { return null; }
                
                upgradeLevel = Math.Max(1, Math.Min(upgradeLevel.Value, 4));

                return (Tool)constructor.Invoke(new object[] { upgradeLevel });
            }

            constructor = tool
                .GetType()
                .GetConstructor(Array.Empty<Type>());

            if (null == constructor) { return null; }

            var newTool = (Tool)constructor.Invoke(Array.Empty<object>());

            upgradeLevel = Math.Max(0, Math.Min(upgradeLevel.Value, 4));

            newTool.UpgradeLevel = upgradeLevel.Value;

            return newTool;
        }
    }
}
