# Esca's Modding Plugins (EMP)
A mod for the game Stardew Valley that adds new data assets, map/tile properties, and features for other mods to use.

## Contents
* [Installation](#installation)
     * [Multiplayer Note](#multiplayer-note)
* [Features](#features)
     * [Fish Locations](#fish-locations)

## Installation
1. **Install the latest version of [SMAPI](https://smapi.io/).**
2. **Download EMP** from [the Releases page on GitHub](https://github.com/Esca-MMC/EscasModdingPlugins/releases), Nexus Mods, or ModDrop.
3. **Unzip EMP** into the `Stardew Valley\Mods` folder.

Mods that use EMP should now work correctly. For information about creating mods, see the sections below.

### Multiplayer Note
* It is recommended that **all players** install this mod for multiplayer sessions.

## Features
EMP provides the following features:

Feature | Summary | Uses data assets | Uses map properties | Uses tile properties
--------|---------|-----------------|---------------------|---------------------
Fish Locations | A location (map) can have multiple "zones" with different fish, including fish from other locations. (See the [Data/Locations](https://stardewvalleywiki.com/Modding:Location_data) asset.) | ✓ | ✘ | ✓

### Fish Locations
This feature allows players to catch different groups of fish at a single in-game location (map). It gives custom maps more control over which fish are used from the [Data/Locations](https://stardewvalleywiki.com/Modding:Location_data) asset. It can also control whether crab pots catch "ocean" or "freshwater" animals from the [Data/Fish](https://stardewvalleywiki.com/Modding:Fish_data) asset.

Mods can use this feature through either data assets or tile properties. If both exist for a specific tile, the data asset will be used.

#### Data asset
EMP adds this data asset to the game: `Mods/Esca.EMP/FishLocations`

The asset can be edited through Content Patcher's "EditData" action like any other data asset; SMAPI (C#) mods can also edit the asset through "IAssetEditor" methods.

It supports the following fields:

Field | Value | Example | Required? | Description
------|-------|---------|-----------|------------
(entry key) | Any unique string | `"YourName.CPExamplePack 1"` | Required | The unique ID (key) for this entry. Any text can be used, but including the UniqueID from your mod's manifest.json is recommended.
Locations | A list of location names | `["Farm", "BusStop"]` | Required | The list of locations this entry should affect.
TileAreas | A list of tile areas | `[ {"X":0, "Y":0, "Width":999, "Height":999} ]` | Required | The list of rectangular tile areas this entry should affect. The earlier example will affect the entire map.
UseZone | Any integer | `-1` | Optional | In the affected area, only fish with this "zone" ID in Data/Locations can be caught. Note that fish with the ID `-1` can be caught in any zone.
UseLocation | A location name | `"Mountain"` | Optional | Fish in the affected area will come from the named location. Refer to the keys (location names) in the Data/Locations asset.
UseOceanCrabPots | true or false | `false` | Optional | Crab pots in the affected area will catch "ocean" results if this is true, or "freshwater" results if this is false.
Priority | Any integer | `0` | Optional | If a specific tile is affected by more than one entry, the entry will the highest priority will be used. Defaults to 0 if not provided.

Below is an example content.json file for a Content Patcher mod. It modifies some areas of the Farm to catch fish from the Forest's river, and crab pots there will catch "freshwater" results.
```js
{
	"Format": "1.23.0",
	"Changes": [
	{
		"Action": "EditData",
		"Target": "Mods/Esca.EMP/FishLocations",
		"Entries": {
			"YourName.CPExamplePack 1": {	//give the entry a unique key
				"Locations": [ "Farm" ],	//this entry modifies the farm's fish
				"TileAreas": [
					{"X":0, "Y":0, "Width": 50, "Height": 50},	//modify fish from tiles 0,0 - 49,49
					{"X":80, "Y":90, "Width": 2, "Height": 4}	//modify fish from tiles 80,90 - 81,93
				],
				"UseLocation": "Forest", 	//use fish from the "Forest" data in Data/Locations
				"UseZone": 0, 				//use fish set to either zone 0 (river) or -1 (everywhere) in Data/Locations
				"UseOceanCrabPots": false	//use "freshwater" crab pot results from Data/Fish
			}
		}
	]
}
```

#### Tile properties
Fish locations can also be controlled by this tile property: `Esca.EMP/FishLocations`

If a tile has this property and is not already being modified by the data asset (see above), it will use different fish based on the tile property's value.

The tile property uses the following format: `<UseZone> [UseLocation] [UseOceanCrabPots]`

Field | Value | Example | Required? | Description
------|-------|---------|-----------|------------
UseZone | Any integer | `-1` | Required | Only fish with this "zone" ID in Data/Locations will be caught. Note that fish with the ID `-1` can be caught in any zone.
UseLocation | A location name | `Mountain` | Optional | Fish caught from this tile will come from this location. Refer to the keys (location names) in the Data/Locations asset.
UseOceanCrabPots | true or false | `false` | Optional | Crab pots on this tile will catch "ocean" results if this is true, or "freshwater" results if this is false.

Below is an example tile property where fish will be caught from the Forest's river and pots will catch "freshwater" results: `-1 Forest false`

![Esca.EMP/FishLocations: -1 Forest false](docs/images/FishLocations_TileProperty.png)