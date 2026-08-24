using System;

namespace DedicatedServer.Config
{
    /// <summary>
    ///         By default, the file is located at the following path in Windows:
    /// <br/>   <c>"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\DedicatedServer\"</c>
    /// </summary>
    public class ModConfig
    {
        /// <summary>
        /// Server Port, if this value is set to null, this mod will not change the port, so Stardew Valley's default port (24642) will be used.
        /// </summary>
        public UInt16? Port { get; set; } = null;

        public string FarmName { get; set; } = "Stardew";

        // Options are 0, 1, 2, or 3.
        public int StartingCabins { get; set; } = 1;

        /// <summary>
        /// Sets the maximum number of houses you can buy from Robin. The minimum and default value is 8.
        /// </summary>
        public int PlayerLimit { get; set; } = 8;

        // Options are "nearby" or "separate"
        public string CabinLayout { get; set; } = "separate";
        
        // Options are "normal", "75%", "50%", or "25%"
        public string ProfitMargin { get; set; } = "normal";

        // Options are "shared" or "separate"
        public string MoneyStyle { get; set; } = "shared";

        // Options are "standard", "riverland", "forest", "hilltop", "wilderness", "fourcorners", "beach".
        public string FarmType { get; set; } = "standard";

        // Options are "normal" or "remixed".
        public string CommunityCenterBundles { get; set; } = "normal";
        
        public bool GuaranteeYear1Completable { get; set; } = false;

        // Options are "normal" or "remixed".
        public string MineRewards { get; set; } = "normal";

        public ulong? RandomSeed { get; set; } = null;

        public const int PetBreedMax = 9;
        public const int FirstDogIndex = 5;
        public bool ShouldAcceptPet() { return 0 <= this.PetBreed && PetBreedMax >= this.PetBreed; }
        public int GetPetBreedIndex() { return (ModConfig.FirstDogIndex <= this.PetBreed) ? this.PetBreed - ModConfig.FirstDogIndex : this.PetBreed; }

        // If no pet is desired, an invalid value, e.g. -1, must be set.
        // The options are 0 to 4 for cats and 5 to 9 for dogs.
        public int PetBreed { get; set; } = 0;

        // Nullable. Must not be null if AcceptPet is true. Any string.
        public string PetName { get; set; } = "Stella";

        // Options are "Mushrooms" or "Bats" (case-insensitive)
        public string MushroomsOrBats { get; set; } = "Mushrooms";

        // Enables the crop saver
        public bool EnableCropSaver = true;

        // Configures the automated host to purchase a Joja membership once available,
        // committing to the Joja route and removing the community center.
        public bool PurchaseJojaMembership = false;

        /// <summary>
        ///         Setting whether monsters spawn on the farm
        /// <br/>   
        /// <br/>   true : The monsters will appear
        /// <br/>   false: No monsters will appear
        /// </summary>
        public bool SpawnMonstersOnFarmAtNight { get; set; } = false;

        /// <summary>
        ///         Changes farmhands permissions to move buildings from the Carpenter's Shop.
        /// <br/>   
        /// <br/>   Is set each time the server is started and can be changed in the game.
        /// <br/>   "off" to entirely disable moving buildings.
        /// <br/>   "owned" to allow farmhands to move buildings that they purchased.
        /// <br/>   "on" to allow moving all buildings.
        /// </summary>
        public string MoveBuildPermission { get; set; } = "off";

        /// <summary>
        ///         Sets whether an attempt should be made to generate an
        /// <br/>   invitation code when the server is restarted.
        /// </summary>
        public bool TryActivatingInviteCode { get; set; } = true;

        /// <summary>
        ///         If this option is set to true, the upgrade level of the host's farmhouse
        /// <br/>   changes to the same, highest upgrade level of all farmers.
        /// <br/>   
        /// <br/>   With <see cref="MainController.Utils.HostHouseUpgrade.ManualUpdate(string)"/> 
        /// <br/>   you can downgrade or upgrade it manually. As long as this property is set to true,
        /// <br/>   the house is automatically upgraded again.
        /// </summary>
        public bool UpgradeHouseLevelBasedOnFarmhand { get; set; } = false;

        /// <summary>
        ///         Password used to log in
        /// <br/>
        /// <br/>   Must be changed to a secure password
        /// <br/>   - An empty string means no password
        /// <br/>   - Any check fails if the value is set to null
        /// </summary>
        public string Password { get; set; } = null;

        /// <summary>
        /// <inheritdoc cref = "PasswordProtectedCommands"/>
        /// </summary>
        public PasswordProtectedCommands PasswordProtected { get; set; } = new PasswordProtectedCommands();
    }

    /// <summary>
    ///         The properties of the class, which are named exactly the same
    /// <br/>   as the chat commands, allow the password protection of individual
    /// <br/>   functions to be switched on (true) and off (false).
    /// </summary>
    public class PasswordProtectedCommands
    {
        /// <summary>
        /// <see cref="MessageCommands.PauseCommandListener"/>
        /// </summary>
        public bool Pause { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.BuildCommandListener"/>
        /// </summary>
        public bool Build { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.DemolishCommandListener"/>
        /// </summary>
        public bool Demolish { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool LetMePlay { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool TakeOver { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool SafeInviteCode { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool InviteCode { get; set; } = true;
        
        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool ForceInviteCode { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool Invisible { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool Sleep { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool ForceSleep { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool ForceResetDay { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool ForceShutdown { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool Wallet { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool SpawnMonster { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool MoveBuildPermission { get; set; } = true;

        /// <summary>
        /// <seealso cref="MessageCommands.ServerCommandListener"/>
        /// </summary>
        public bool UpgradeHouseLevelBasedOnFarmhand { get; set; } = true;
        
    }
}
