# BuildingAndRaceCompatibilityFix

BuildingAndRaceCompatibilityFix is a QoL mod made for WorldBox using NML as its modloader that fixes a common compatibility issue between mods that add new building to the game and mods that add more races to the game.

## Building

The project, as uploaded on this repo, cannot be built out of the box. It assumes that all of it's dependency DLLs like NML and a publicised Assembly-CSharp.dll file are located within a Libraries directory in the parent directory of BuildingAndRaceCompatibilityFix.
Note that being able to build it isn't necessary for loading the mod into your game, since NML compiles mods on the fly.
