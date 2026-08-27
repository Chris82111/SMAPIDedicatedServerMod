# SMAPI Dedicated Server Mod for Stardew Valley

<div align="center">

  [![Shields](https://img.shields.io/badge/Stardew_Valley-1.6.15-153C86)](https://www.stardewvalley.net/ "Link to Stardew Valley")
  [![Shields](https://img.shields.io/badge/SMAPI-4.5.2-5cb811)](https://smapi.io/ "Link to SMAPI")
   ![Shields](https://img.shields.io/badge/.NET-6.0-512BD4)
  [![Shields](https://img.shields.io/badge/forked_from-_-blue)](https://github.com/ObjectManagerManager/SMAPIDedicatedServerMod "Link to the Original Repository")
<!-- [![Shields](https://img.shields.io/badge/Docker_Version-_-2496ED)](https://github.com/Chris82111/StardewValleyViaDocker "Link to StardewValleyViaDocker ") -->

</div>

This mod provides a dedicated (headless) server for Stardew Valley, powered by SMAPI. It turns the host farmer into an automated bot to facilitate multiplayer gameplay.

## In-Game Features

### Festivals

In some festivals, the host starts the event. Players must vote using `start` (en) and `cancel` (en). Once all players have agreed to start, the host begins the event. Alternatively, a player can stand in front of Lewis. After 10 seconds, this is also considered consent to start. If a player steps back from Lewis or opens a menu, their consent is revoked.

During festivals, the player must interact with the host to start the event:

| Key | Start | Cancel  |
| --- | ----- | ------- |
| en | start  | cancel  |
| de | start  | stop    |
| fr | lancer | annuler |

### Language

The commands and output that appear during normal gameplay have been translated:

| Key | File |
| --- | --- |
| en | [default.json](./DedicatedServer/i18n/default.json) |
| de | [de.json](./DedicatedServer/i18n/de.json) |
| fr | [fr.json](./DedicatedServer/i18n/fr.json) |


## Configuration File

Upon running SMAPI with the mod installed for the first time, a `config.json` file will be generated in the mod's folder. This file specifies which farm will be loaded on startup, farm creation options, host automation details, and other mod configuration options. Default values will be provided, which can then be modified. Here is an overview of the available settings:

### Startup options

- `Port`: The game's port can be customized. `null` prevents the port from being overwritten.
- `Language`: All standard languages are accepted as input: en, ja, ru, zh, pt, es, de, th, fr, ko, it, tr, and hu. Unfortunately, the mod's text only supports the languages listed under [Language](#language).
- `PlayerLimit`: Sets the maximum number of houses you can buy from Robin. The minimum and default value is 8.
- `FarmName`: The farm's name. The farm's name must match, and the farm must have cabins for the other players. The farm will then be loaded and hosted for co-op. If no farm exists, a new farm will be created based on the specified farm creation options and then hosted for co-op. If multiple farms exist or no cabin is available, the game crashes and displays an error message.

### Farm Creation Options

- `StartingCabins`: The number of starting cabins for the farm. Must be an integer in {0, 1, 2, 3}.
- `CabinLayout`: Specifies the starting cabin layout. Options are "nearby" or "separate".
- `ProfitMargin`: The farm's profit margin. Options are "normal", "75%", "50%", and "25%".
- `MoneyStyle`: Determines whether money is shared or separate among farmers. Options are "shared" or "separate".
- `FarmType`: The type of farm. Options include "standard", "riverland", "forest", "hilltop", "wilderness", "fourcorners", "beach", and "meadowlands".
- `CommunityCenterBundles`: The community center bundle type. Options are "normal" or "remixed".
- `GuaranteeYear1Completable`: Set to `true` or `false` to determine if the community center should be guaranteed completable during the first year.
- `MineRewards`: The mine rewards type. Options are "normal" or "remixed".
- `SpawnMonstersOnFarmAtNight`: Set to `true` or `false` to determine if monsters should spawn on the farm at night.
- `RandomSeed`: An optional integer specifying the farm's random seed.

### Host Automation Options

- `PetBreed`: An integer in [0, 9] specifying the pet breed index.  
  0 selects the leftmost breed; 9 selects the rightmost breed:  
  <picture> <img alt="Index 0" src="./Docs/PetBreed/0.png" height="30" /> </picture>
  <picture> <img alt="Index 1" src="./Docs/PetBreed/1.png" height="30" /> </picture>
  <picture> <img alt="Index 2" src="./Docs/PetBreed/2.png" height="30" /> </picture>
  <picture> <img alt="Index 3" src="./Docs/PetBreed/3.png" height="30" /> </picture>
  <picture> <img alt="Index 4" src="./Docs/PetBreed/4.png" height="30" /> </picture>
  <picture> <img alt="Index 5" src="./Docs/PetBreed/5.png" height="30" /> </picture>
  <picture> <img alt="Index 6" src="./Docs/PetBreed/6.png" height="30" /> </picture>
  <picture> <img alt="Index 7" src="./Docs/PetBreed/7.png" height="30" /> </picture>
  <picture> <img alt="Index 8" src="./Docs/PetBreed/8.png" height="30" /> </picture>
  <picture> <img alt="Index 9" src="./Docs/PetBreed/9.png" height="30" /> </picture>  
  The index clearly shows whether a dog or a cat is desired.  
  If no pet is desired, the value should be set to `-1`.
- `PetName`: The desired pet name. Irrelevant if `AcceptPet` is `false`.
- `MushroomsOrBats`: Choose between the mushroom or bat cave. Options are "mushrooms" or "bats" (case insensitive).
- `PurchaseJojaMembership`: Set to `true` or `false` to determine if the automated host should "purchase" (acquire for free) a Joja membership when available, committing to the Joja route. Defaults to `false`.

### Additional Options

- `EnableCropSaver`: Set to `true` or `false` to enable or disable the crop saver feature. When enabled, seasonal crops planted by players and fully grown before the season's end are guaranteed to give at least one more harvest before dying. For example, a spring crop planted by a player and fully grown before Summer 1 will not die immediately on Summer 1. Instead, it'll provide exactly one more harvest, even if it's a crop that ordinarily produces multiple harvests. Defaults to `true`.
- `MoveBuildPermission`: Change farmhands permissions to move buildings from the Carpenter's shop. Is set each time the server is started and can be changed in the game. Set to `off` to entirely disable moving buildings, set to `owned` to allow farmhands to move buildings that they purchased, or set to `on` to allow moving all buildings.
- `Password`: Password used to log in. It must be changed to a secure password. An empty string `""` means no password. Any check fails if the value is set to `null`.
- `PasswordProtected`: The password protection of individual functions can be switched on (`true`) and off (`false`) in the group.
- `UpgradeHouseLevelBasedOnFarmhand`: Set this value to `true` or `false` to activate the automatic upgrade of the host farmhouse according to the highest farmhouse level of a player.

## In Game Command

All commands in the game must be sent privately to the player `ServerBot`. For example, you must write the following `/message ServerBot MoveBuildPermission on`. Depending on the setting of the [`Password`](#additional-options) option, the commands can only be executed after you have logged in. Enter `/message ServerBot Login` followed by the password.

- `Build`: Builds a new cabin for more players at the place where the player is looking at. Allowed parameters are `Stone_Cabin`, `Plank_Cabin`, and `Log_Cabin`.
- `Demolish`: Destroys any building the player is looking at, not only cabins.
- `Pause`: (Toggle command) \
  Pause the game
- `TakeOver`: The host player returns control to the host, all host functions are switched on. Cancels the [`LetMePlay`](#host-in-game-command) command
- `UpdateHouseLevel`: Can be used with and without a parameter.
  - Without a parameter:
    - The command triggers the check function and the host farmhouse is upgraded to the next level if another farmer has a farmhouse with a higher farmhouse level than the host.
    - The additional options entry `UpgradeHouseLevelBasedOnFarmhand` is ignored.
  - With a parameter from 0 to 3:
    - The host farmhouse will be upgraded immediately, please note:
      - It is not safe to be in the farmhouse while the farmhouse is being upgraded or downgraded.
      - Please remove all items, decorations and so on.
      - Some items will be stored in a chest, but there are only 36 places to store items.
      - Everything else will be deleted.  
      - All beds will be destroyed and a new bed will be set up.
      - The cellar cannot be removed.
- `SafeInviteCode`: A file `invite_code.txt` with the invitation code is created in this mods folder. If there is no invitation code, an empty string is saved.
- `InviteCode`: The invitation code is printed.
- `ForceInviteCode`: Forces the invitation code by closing and reopening the multiplayer server. There is an 8 second warning before the server stops, after another 2 seconds it is restarted.
- `Invisible`: (Toggle command) \
  Toggle the visibility of the farm host.
- `Sleep`: (Toggle command) \
  When it is sent, the host goes to bed. When all players leave the game or go to bed, the next day begins. On a second send, the host will get up and the mod's normal behavior will be restored.
- `ForceSleep`: Kick out all players and starts a new day.
- `ForceResetDay`: Kick out all players and restarts the day.
- `ForceShutDown`: Kick out all players, start a new day and shut down the server.
- `WalletSeparate`: Separate the wallets tonight.
- `WalletMerge`: Connect the wallets tonight.
- `SpawnMonster`: (Toggle command, Saved in config) \
  Changes the settings so that monsters spawn on the farm or not. Spawned monsters are not reset.
- `MoveBuildPermission` or
- `MovePermission` or
- `MBP`: (Saved in config) \
  Changes farmhands permissions to move buildings from the Carpenter's shop. Set to `off` to entirely disable moving buildings, set to `owned` to allow farmhands to move buildings that they purchased, or set to `on` to allow moving all buildings.

Administration:

- `LogIn`: This command is used to log in. The command is followed by the password that was defined in `config.json` under `Password`.
- `LogOut`: This command is used to log out.
- `PasswordProtectShow`: An overview of the commands is displayed. This shows which command is protected by a password and which can be executed by any player without restriction.
- `PasswordProtectAdd`: (Saved in config) \
  This command is followed by the name of a command which is now password-protected, so it can only be executed after logging in.
- `PasswordProtectRemove`: (Saved in config) \
  This removes a command from password protection. Every player can now use the command without restrictions.

## Host in Game Command

All these commands only work if you are the host. This allows you to take control of the server. The host sends the commands by entering them directly, without anything before or after:

- `LetMePlay`: Lets the player take over the host. All host functions are switched off. The `TakeOver` command must be entered to hand over the controller. \
  Please note that the host automation accepts gifts from events and NPCs and deletes items from the inventory if necessary.

## Version Update

When updating to a new version, please note the following points:

- Back up your game data first!
- If a new mod version is used, do not just replace the `DedicatedServer.dll`.
Remove all data from the folder. An old `config.json` cannot be used to start the game.

## Running the Server on Linux Without GUI

This mod can be run without the use of a GUI. To start the game, you must enter the following command:

```bash
xvfb-run -a "$HOME/GOG Games/Stardew Valley/game/StardewModdingAPI"
```

You can shut down the server from the started terminal session by pressing `Control-C`.
From another terminal session, you can send `Control-C` with `kill -SIGINT ....`.

```bash
ps -aux | grep StardewModdingAPI
kill -SIGINT ....
```

## Development

After cloning the repository and performing the first (probably incorrect) build, the SMAPI readme file can be opened. It describes how to create the `stardewvalley.targets` file in the user's home directory in order to select a specific version when multiple versions are installed.
([Linux](https://superuser.com/a/409223),
[macOS](https://www.cnet.com/how-to/how-to-find-your-macs-home-folder-and-add-it-to-finder/) or
[Windows](https://www.computerhope.com/issues/ch000109.htm).)  
The file must contain the following:

```xml
<Project>
    <PropertyGroup>
        <GamePath>C:/Program Files (x86)/Steam/steamapps/common/Stardew Valley</GamePath>
    </PropertyGroup>
</Project>
```
