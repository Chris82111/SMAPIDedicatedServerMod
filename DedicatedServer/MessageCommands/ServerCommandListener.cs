//#define USE_DEBUG

using DedicatedServer.Chat;
using DedicatedServer.HostAutomatorStages;
using DedicatedServer.Utils;
#if USE_DEBUG
using StardewModdingAPI.Events;
#endif
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;

namespace DedicatedServer.MessageCommands
{
    internal abstract class ServerCommandListener
    {
        #if USE_DEBUG

        private const string hardwoodItemId = "709";
        private const string iridiumSprinkler = "645";
        private const string beanStarter = "473";
        private const string pepperSeed = "482";
        private const string eggplantSeeds = "488";

        #region Crafts Room 

        // Spring foraging bundle
        private const string wildHorseRadish = "16";
        private const string daffodil = "18";
        private const string leek = "20";
        private const string dandelion = "22";

        // Summer foraging bundle
        private const string grape = "398";
        private const string spice_berry = "396";
        private const string sweet_pea = "402";

        // Fall foraging bundle
        private const string common_mushroom = "404";
        private const string wild_plum = "406";
        private const string hazelnut = "408";
        private const string blackberry = "410";

        // Winter foraging bundle
        private const string winter_root = "412";
        private const string crystal_fruit = "414";
        private const string snow_yam = "416";
        private const string crocus = "418";

        // Exotic foraging bundle
        private const string coconut = "88";
        private const string cactus_fruit = "90";
        private const string cave_carrot = "78";
        private const string red_mushroom = "420";
        private const string purple_mushroom = "422";
        private const string pine_tar = "726";
        private const string morel = "257";

        #endregion

        #region Pantry

        // spring_crops
        private const string parsnip = "24";
        private const string green_bean = "188";
        private const string cauliflower = "190";
        private const string potato = "192";

        // summer crops:
        private const string tomato = "256";
        private const string hot_pepper = "258";
        private const string blueberry = "260";
        private const string melon = "254";

        // fall crops
        private const string corn = "270";
        private const string eggplant = "272";
        private const string pumpkin = "276";
        private const string yam = "280";

        // animal bundle
        private const string large_milk = "186";
        private const string large_egg_brown = "182";
        private const string large_egg = "176";
        private const string large_goat_milk = "438";
        private const string wool = "440";
        private const string duck_egg = "442";

        // artisan_bundle
        private const string truffle_oil = "432";
        private const string cloth = "428";
        private const string goat_cheese = "426";
        private const string cheese = "424";
        private const string honey = "340";
        private const string jelly = "344";
        private const string apple = "613";
        private const string apricot = "634";
        private const string orange = "635";
        private const string peach = "636";
        private const string pomegranate = "637";
        private const string cherry = "638";

        #endregion

        #region Fish Tank

        // river fish:
        private const string sunfish = "145";
        private const string catfish = "143";
        private const string shad = "706";
        private const string tiger_trout = "699";

        // lake fish:
        private const string largemouth_bass = "136";
        private const string carp = "142";
        private const string bullhead = "700";
        private const string sturgeon = "698";

        // ocean fish:
        private const string sardine = "131";
        private const string tuna = "130";
        private const string red_snapper = "150";
        private const string tilapia = "701";

        // night fishing:
        private const string waleye = "140";
        private const string bream = "132";
        private const string eel = "148";

        // crab pot:
        private const string lobster = "715";
        private const string crayfish = "716";
        private const string crab = "717";
        private const string cockle = "718";
        private const string mussel = "719";
        private const string shrimp = "720";
        private const string snail = "721";
        private const string periwinkle = "722";
        private const string oyster = "723";
        private const string clam = "372";

        // specialty fish:
        private const string pufferfish = "128";
        private const string ghostfish = "156";
        private const string sandfish = "164";
        private const string woodskip = "734";

        #endregion

        #region Boiler Room

        // blacksmith bundle:
        private const string copper_bar = "334";
        private const string iron_bar = "335";
        private const string gold_bar = "336";

        // geologist bundle:
        private const string quartz = "80";
        private const string earth_crystal = "86";
        private const string frozen_tear = "84";
        private const string fire_quartz = "82";

        // adventurers bundle:
        private const string slime = "766"; 
        private const string solar_essence = "768";

        # endregion

        #region Bulletin Board

        // chefs bundle:
        private const string maple_syrup = "724";
        private const string fiddlehead_fern = "259";
        private const string truffle = "430";
        private const string poppy = "376";
        private const string maki_roll = "228";
        private const string fried_egg = "194";

        // dye bundle:
        // red_mushroom
        private const string sea_urchin = "397";
        private const string sunflower = "421";
        private const string duck_feather = "444";
        private const string aquamarine = "62";
        private const string red_cabbage = "266";

        // field research bundle:
        // purple_mushroom
        private const string nautilus_shell = "392";
        private const string chub = "702";
        private const string frozen_geode = "536";

        // fodder bundle:
        private const string wheat = "262";
        private const string hay = "178";
        private const string apples = "613";

        // enchanters bundle:
        private const string wine = "348";
        private const string rabbits_foot = "446";
        private const string oak_resin = "725";
        // pomegranate

        #endregion

        #endif

        public static void Enable()
        {
            MainController.chatBox.ChatReceived += ChatReceived;
        }

        public static void Disable()
        {
            MainController.chatBox.ChatReceived -= ChatReceived;
        }

        #region DEBUG_SKIP_DAYS
        #if USE_DEBUG

        // Each day is run so that all events are executed normally.

        private static int dayOfMonth = -1;
        private static Season season;
        private static void EnableSkipDays(int dayOfMonth, Season season)
        {
            ServerCommandListener.dayOfMonth = dayOfMonth;
            ServerCommandListener.season = season;
            MainController.helper.Events.GameLoop.OneSecondUpdateTicked += SkipDays;
            SkipDays(null, null);
        }

        private static void DisableSkipDays()
        {
            dayOfMonth = -1;
            MainController.helper.Events.GameLoop.OneSecondUpdateTicked -= SkipDays;
            Sleeping.ShouldSleepOverwrite = false;
        }

        private static void SkipDays(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (dayOfMonth > Game1.dayOfMonth || Game1.season != season)
            {
                if(false == Sleeping.ShouldSleepOverwrite)
                {
                    Sleeping.ShouldSleepOverwrite = true;
                }
            }
            else
            {
                DisableSkipDays();
            }
        }

        #endif
        #endregion

        private static void ChatReceived(object sender, ChatEventArgs e)
        {
            var tokens = e.Message.Split(' ');

            if (0 == tokens.Length) { return; }

            string command = tokens[0].ToLower();

            var sourceFarmer = Game1.otherFarmers.Values
                .Where(farmer => farmer.UniqueMultiplayerID == e.SourceFarmerId)
                .FirstOrDefault()
                ?? Game1.player;

            string param = 1 < tokens.Length ? tokens[1].ToLower() : "";

            if (Game1.player.UniqueMultiplayerID == e.SourceFarmerId)
            {
                switch (command)
                {
                    case "letmeplay":
                        LetMePlay(sourceFarmer);
                        break;

                    #region DEBUG_COMMANDS
                    #if USE_DEBUG

                    case "t9":
                        Game1.timeOfDay = 900;
                        break;
                    case "t18":
                        Game1.timeOfDay = 1800;
                        break;
                    case "t22":
                        Game1.timeOfDay = 2200;
                        break;

                    case "timereset":
                        if (Game1.dayOfMonth > 1)
                        {
                            Game1.stats.DaysPlayed--;
                            Game1.dayOfMonth--;
                        }
                        break;

                    case "settomonth":
                        int month = int.TryParse(param, out int resultMonth) ? resultMonth : 0;
                        MainController.SetMonth(month);
                        break;

                    case "settoday":
                        int day = int.TryParse(param, out int resultDay) ? resultDay : 0;
                        MainController.SetDay(day);
                        break;

                    case "seed":
                        Game1.player.addItemToInventory(new StardewValley.Object("472", 10)); // parsnip
                        Game1.player.addItemToInventory(new StardewValley.Object("473", 10)); // bean
                        break;

                    case "skipdays":
                        EnableSkipDays(24, Season.Winter);
                        break;

                    case "t1":
                        var a = Utility.getAllPets();
                        break;

                    case "pp": // Preventing the pause
                        HostAutomation.PreventPauseUntilNextDay();
                        break;

                    case "item":
                        if ("" != param)
                        {
                            int param2 = int.TryParse(2 < tokens.Length ? tokens[2] : "1", out int param2try) ? param2try : 1;
                            if (0 <= param2)
                            {
                                Game1.player.addItemToInventory(new StardewValley.Object(param, param2));
                            }
                        }
                        break;

                    case "inventory":
                        foreach(var inventoryItems in Game1.player.Items)
                        {
                            var itemId = inventoryItems?.ItemId;
                            MainController.chatBox.textBoxEnter($"itemId: {itemId}");
                        }
                        break;

                    case "items":
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.iridiumID, 999));
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.iridiumID, 999));
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.iridiumID, 999));
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.woodID, 999));
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.stoneID, 999));
                        Game1.player.addItemToInventory(new StardewValley.Object(iridiumSprinkler, 1));
                        break;

                    case "iridiumsprinkler":
                        Game1.player.addItemToInventory(new StardewValley.Object(iridiumSprinkler, 10));
                        break;

                    case "iridium":
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.iridiumID, 999));
                        break;


                    case "com11":
                        Game1.player.addItemToInventory(new StardewValley.Object(wildHorseRadish, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(daffodil, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(leek, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(dandelion, 1));
                        break;
                    case "com12":
                        Game1.player.addItemToInventory(new StardewValley.Object(grape, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(spice_berry, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(sweet_pea, 1));
                        break;
                    case "com13":
                        Game1.player.addItemToInventory(new StardewValley.Object(common_mushroom, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(wild_plum, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(hazelnut, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(blackberry, 1));
                        break;
                    case "com14":
                        Game1.player.addItemToInventory(new StardewValley.Object(winter_root, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(crystal_fruit, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(snow_yam, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(crocus, 1));
                        break;
                    case "com15":
                        Game1.player.addItemToInventory(new StardewValley.Object(coconut, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(cactus_fruit, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(cave_carrot, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(red_mushroom, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(purple_mushroom, 1));
                        break;
                    case "com16":
                        Game1.player.addItemToInventory(new StardewValley.Object(hardwoodItemId, 10));
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.stoneID, 99));
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.woodID, 2*99));
                        break;

                    case "com21":
                        Game1.player.addItemToInventory(new StardewValley.Object(parsnip, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(green_bean, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(cauliflower, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(potato, 1));
                        break;
                    case "com22":
                        Game1.player.addItemToInventory(new StardewValley.Object(tomato, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(hot_pepper, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(blueberry, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(melon, 1));
                        break;
                    case "com23":
                        Game1.player.addItemToInventory(new StardewValley.Object(corn, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(eggplant, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(pumpkin, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(yam, 1));
                        break;
                    case "com24":
                        Game1.player.addItemToInventory(new StardewValley.Object(large_milk, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(large_egg_brown, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(large_goat_milk, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(wool, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(duck_egg, 1));
                        break;
                    case "com25":
                        Game1.player.addItemToInventory(new StardewValley.Object(truffle_oil, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(cloth, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(goat_cheese, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(cheese, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(honey, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(jelly, 1));
                        break;
                    case "com26":
                        var i1 = new StardewValley.Object(parsnip, 5); i1.Quality = 3; Game1.player.addItemToInventory(i1);
                        i1 = new StardewValley.Object(melon, 5); i1.Quality = 3; Game1.player.addItemToInventory(i1);
                        i1 = new StardewValley.Object(pumpkin, 5); i1.Quality = 3; Game1.player.addItemToInventory(i1);
                        break;

                    case "com31":
                        Game1.player.addItemToInventory(new StardewValley.Object(sunfish, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(catfish, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(shad, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(tiger_trout, 1));
                        break;
                    case "com32":
                        Game1.player.addItemToInventory(new StardewValley.Object(largemouth_bass, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(carp, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(bullhead, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(sturgeon, 1));
                        break;
                    case "com33":
                        Game1.player.addItemToInventory(new StardewValley.Object(sardine, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(tuna, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(red_snapper, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(tilapia, 1));
                        break;
                    case "com34":
                        Game1.player.addItemToInventory(new StardewValley.Object(waleye, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(bream, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(eel, 1));
                        break;
                    case "com35":
                        Game1.player.addItemToInventory(new StardewValley.Object(lobster, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(crayfish, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(crab, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(cockle, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(mussel, 1));
                        break;
                    case "com36":
                        Game1.player.addItemToInventory(new StardewValley.Object(pufferfish, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(ghostfish, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(sandfish, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(woodskip, 1));
                        break;

                    case "com41":
                        Game1.player.addItemToInventory(new StardewValley.Object(copper_bar, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(iron_bar, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(gold_bar, 1));
                        break;
                    case "com42":
                        Game1.player.addItemToInventory(new StardewValley.Object(quartz, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(earth_crystal, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(frozen_tear, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(fire_quartz, 1));
                        break;
                    case "com43":
                        Game1.player.addItemToInventory(new StardewValley.Object(slime, 99));
                        Game1.player.addItemToInventory(new StardewValley.Object(solar_essence, 1));
                        break;

                    case "com51":
                        Game1.player.addItemToInventory(new StardewValley.Object(maple_syrup, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(fiddlehead_fern, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(truffle, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(poppy, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(maki_roll, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(fried_egg, 1));
                        break;
                    case "com52":
                        Game1.player.addItemToInventory(new StardewValley.Object(red_mushroom, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(sea_urchin, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(sunflower, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(duck_feather, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(aquamarine, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(red_cabbage, 1));
                        break;
                    case "com53":
                        Game1.player.addItemToInventory(new StardewValley.Object(purple_mushroom, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(nautilus_shell, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(chub, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(frozen_geode, 1));
                        break;
                    case "com54":
                        Game1.player.addItemToInventory(new StardewValley.Object(wheat, 10));
                        Game1.player.addItemToInventory(new StardewValley.Object(hay, 10));
                        Game1.player.addItemToInventory(new StardewValley.Object(apples, 3));
                        break;
                    case "com55":
                        Game1.player.addItemToInventory(new StardewValley.Object(wine, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(rabbits_foot, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(oak_resin, 1));
                        Game1.player.addItemToInventory(new StardewValley.Object(pomegranate, 1));
                        break;


                    case "wood":
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.woodID, 999));
                        break;

                    case "stone":
                        Game1.player.addItemToInventory(new StardewValley.Object(StardewValley.Object.stoneID, 999));
                        break;

                    case "hardwood":
                        Game1.player.addItemToInventory(new StardewValley.Object(hardwoodItemId, 999));
                        break;

                    case "emptyinventoryall": // /message serverbot EmptyInventoryAll
                        ServerHost.EmptyHostInventory();
                        break;

                    case "menu":
                        var menu = Game1.activeClickableMenu;
                        MainController.chatBox.textBoxEnter($" Menu is {(menu?.ToString() ?? "")}" + TextColor.Green);
                        break;
                                            
                    case "multiplayer":
                        MultiplayerOptions.EnableServer = true;
                        break;
                        
                    case "gold":
                        Game1.player.team.SetIndividualMoney(Game1.player, 1000);
                        break;

                    case "singleplayer":
                        MultiplayerOptions.EnableServer = false;
                        break;

                    case "farm":
                        MainController.Warp(WarpPoints.FarmWarp);
                        break;

                    case "house":
                        MainController.Warp(WarpPoints.FarmHouseWarp);
                        break;

                    case "mine":
                        MainController.Warp(WarpPoints.mineWarp);
                        break;

                    case "town":
                        MainController.Warp(WarpPoints.townWarp);
                        break;

                    case "beach":
                        MainController.Warp(WarpPoints.beachWarp);
                        break;

                    case "robin":
                        MainController.Warp(WarpPoints.robinWarp);
                        break;

                    case "clint":
                        MainController.Warp(WarpPoints.clintWarp);
                        break;

                    case "pierre":
                        MainController.Warp(WarpPoints.pierreWarp);
                        break;

                    case "communitycenter":
                        MainController.Warp(WarpPoints.communityCenterWarp);
                        break;

                    case "wizzard":
                        MainController.Warp(WarpPoints.wizzardWarp);
                        break;

                    case "desert":
                        MainController.Warp(WarpPoints.desertWarp);
                        break;                

                    case "location":
                        var location = Game1.player.Tile;
                        MainController.chatBox.textBoxEnter("location: " + Game1.player.currentLocation.ToString());
                        MainController.chatBox.textBoxEnter("x: " + location.X + ", y:" + location.Y);
                        break;

#endif
#endregion
                }
            }
            else
            {
                if (ChatBox.privateMessage != e.ChatKind)
                {
                    return;
                }
            }

            switch (command)
            {
                case "takeover": // /message ServerBot TakeOver
                    TakeOver(sourceFarmer);
                    break;

                case "updatehouselevel":  // /message ServerBot UpdateHouseLevel
                    UpdateHouseLevel(sourceFarmer, param);
                    break;

                case "safeinvitecode": // /message ServerBot SafeInviteCode
                    SafeInviteCode(sourceFarmer);
                    break;

                case "invitecode": // /message ServerBot InviteCode
                    InviteCode(sourceFarmer);
                    break;

                case "forceinvitecode": // /message ServerBot ForceInviteCode
                    ForceInviteCode(sourceFarmer);
                    break;

                case "invisible": // /message ServerBot Invisible
                    InvisibleSub(sourceFarmer);
                    break;

                case "sleep": // /message ServerBot Sleep
                    Sleep(sourceFarmer);
                    break;

                case "forcesleep": // /message ServerBot ForceSleep
                    ForceSleep(sourceFarmer);
                    break;

                case "forceresetday": // /message ServerBot ForceResetDay
                    ForceResetDay(sourceFarmer);
                    break;

                case "forceshutdown": // /message ServerBot ForceShutdown
                    ForceShutdown(sourceFarmer);
                    break;

                case "walletseparate": // /message ServerBot WalletSeparate
                    WalletSeparate(sourceFarmer);
                    break;

                case "walletmerge": // /message ServerBot WalletMerge
                    WalletMerge(sourceFarmer);
                    break;

                case "spawnmonster": // /message ServerBot SpawnMonster
                    SpawnMonster(sourceFarmer);
                    break;

                case "mbp": // /message ServerBot mbp on
                case "movebuildpermission":
                case "movepermission":
                    MoveBuildPermissionSub(sourceFarmer, param);
                    break;

                default:
                    break;
            }
        }

        private static void LetMePlay(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.LetMePlay))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            WriteToPlayer(null, $"The host is now a player, all host functions are deactivated." + TextColor.Green);
            HostAutomation.LetMePlay();
        }

        private static void TakeOver(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.TakeOver))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            WriteToPlayer(null, $"Control has been transferred to the host, all host functions are switched on." + TextColor.Aqua);
            HostAutomation.Reset();
        }

        private static void UpdateHouseLevel(Farmer farmer, string param)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.UpgradeHouseLevelBasedOnFarmhand))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            if ("" == param)
            {
                if (HostHouseUpgrade.NeedsUpgrade())
                {
                    WriteToPlayer(null, "A host farm house upgrade is being executed" + TextColor.Yellow);
                }
                else
                {
                    WriteToPlayer(null, "A host farm house upgrade is not necessary" + TextColor.Green);
                }
            }
            else
            {
                WriteToPlayer(null, $"The host farm house is upgraded to {param}" + TextColor.Orange);
                HostHouseUpgrade.ManualUpdate(param);
            }
        }

        private static void SafeInviteCode(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.SafeInviteCode))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            MultiplayerOptions.SaveInviteCode();
            if (MultiplayerOptions.IsInviteCodeAvailable)
            {
                WriteToPlayer(farmer, $"Your invite code is saved in the mod folder in the file {MultiplayerOptions.inviteCodeSaveFile}." + TextColor.Green);
            }
            else
            {
                WriteToPlayer(farmer, $"The game has no invite code." + TextColor.Red);
            }
        }
        
        private static void InviteCode(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.InviteCode))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }
            
            WriteToPlayer(farmer, 
                String.Format(Game1.content.LoadString("Strings\\UI:Server_InviteCode"), MultiplayerOptions.InviteCode) + 
                ("" == MultiplayerOptions.InviteCode ? TextColor.Red : TextColor.Green));
        }

        private static void ForceInviteCode(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.ForceInviteCode))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            MultiplayerOptions.TryActivatingInviteCode();
        }

        private static void InvisibleSub(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.Invisible))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            Invisible.InvisibleOverwrite = !Invisible.InvisibleOverwrite;
            WriteToPlayer(farmer, $"The host is invisible {Invisible.InvisibleOverwrite}" + TextColor.Aqua);
        }

        private static void Sleep(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.Sleep))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            if (false == HostAutomation.EnableHostAutomation)
            {
                WriteToPlayer(farmer, $"Cannot start sleep because the host is controlled by the player." + TextColor.Red);
                return;
            }

            if (Sleeping.ShouldSleepOverwrite)
            {
                Sleeping.ShouldSleepOverwrite = false;
                WriteToPlayer(null, $"The host is back on his feet." + TextColor.Aqua);
            }
            else
            {
                WriteToPlayer(null, $"The host will go to bed." + TextColor.Green);
                Sleeping.ShouldSleepOverwrite = true;
            }
        }

        private static void ForceSleep(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.ForceSleep))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }
            
            RestartDay.ForceSleep((seconds) => MainController.chatBox.textBoxEnter($"Attention: Server will start the next day in {seconds} seconds" + TextColor.Orange));
        }

        private static void ForceResetDay(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.ForceResetDay))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            RestartDay.ResetDay((seconds) => WriteToPlayer(null, $"Attention: Server will reset the day in {seconds} seconds" + TextColor.Orange));
        }

        private static void ForceShutdown(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.ForceShutdown))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            RestartDay.ShutDown((seconds) => WriteToPlayer(null, $"Attention: Server will shut down in {seconds} seconds" + TextColor.Orange));
        }

        private static void WalletSeparate(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.Wallet))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            Wallet.Separate(farmer);
        }
        
        private static void WalletMerge(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.Wallet))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            Wallet.Merge(farmer);
        }

        private static void SpawnMonster(Farmer farmer)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.SpawnMonster))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            if (MultiplayerOptions.SpawnMonstersAtNight)
            {
                WriteToPlayer(null, $"No more monsters will appear." + TextColor.Green);
                MultiplayerOptions.SpawnMonstersAtNight = false;
            }
            else
            {
                WriteToPlayer(null, $"Monsters will appear." + TextColor.Red);
                MultiplayerOptions.SpawnMonstersAtNight = true;
            }
        }

        private static void MoveBuildPermissionSub(Farmer farmer, string param)
        {
            if (false == PasswordValidation.IsAuthorized(farmer.UniqueMultiplayerID, p => p.MoveBuildPermission))
            {
                WriteToPlayer(farmer, PasswordValidation.notAuthorizedMessage);
                return;
            }

            if (MoveBuildPermission.parameter.Any(param.Equals))
            {
                if (MainController.config.MoveBuildPermission == param)
                {
                    WriteToPlayer(farmer, "Parameter for MoveBuildPermission is already " + MainController.config.MoveBuildPermission + TextColor.Orange);
                }
                else
                {
                    MainController.config.MoveBuildPermission = param;
                    MoveBuildPermission.Change(MainController.config.MoveBuildPermission);
                    MainController.helper.WriteConfig(MainController.config);
                }
            }
            else
            {
                WriteToPlayer(farmer, $"Only the following parameters are valid for MoveBuildPermission: {String.Join(", ", MoveBuildPermission.parameter.ToArray())}" + TextColor.Red);
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
