#!/bin/sh
# Init mod related path variables
MOD_BIN_PATH=~/RiderProjects/Modding/KeyMods/BuildingAndRaceCompatibilityFix/bin/Release
NML_MODS_FOLDER_PATH=~/Library/Application\ Support/Steam/steamapps/common/worldbox/Mods

# Init mod distributable folder structure
cd "$NML_MODS_FOLDER_PATH" || exit
rm "./BuildingAndRaceCompatibilityFix.zip"
rm -rf "./BuildingAndRaceCompatibilityFix"
rm -rf "./KEYMASTERER_BUILDINGANDRACECOMPATIBILITYFIX" # mod folder that NML auto generates if BuildingAndRaceCompatibilityFix.zip is unzipped by it
mkdir "./BuildingAndRaceCompatibilityFix"

# Copy built files into distributable folder
cd "$MOD_BIN_PATH" || exit
mv "./BuildingAndRaceCompatibilityFix.dll" "$NML_MODS_FOLDER_PATH/BuildingAndRaceCompatibilityFix/BuildingAndRaceCompatibilityFix.dll"
mv "./BuildingAndRaceCompatibilityFix.pdb" "$NML_MODS_FOLDER_PATH/BuildingAndRaceCompatibilityFix/BuildingAndRaceCompatibilityFix.pdb"

# Copy build assets into distributable folder
cd "../../" || exit
cp -R "./Assemblies" "$NML_MODS_FOLDER_PATH/BuildingAndRaceCompatibilityFix/"
cp -R "./EmbeddedResources" "$NML_MODS_FOLDER_PATH/BuildingAndRaceCompatibilityFix/"
cp -R "./GameResources" "$NML_MODS_FOLDER_PATH/BuildingAndRaceCompatibilityFix/"
cp -R "./Locales" "$NML_MODS_FOLDER_PATH/BuildingAndRaceCompatibilityFix/"
cp "./icon.png" "$NML_MODS_FOLDER_PATH/BuildingAndRaceCompatibilityFix/icon.png"
cp -R "./mod.json" "$NML_MODS_FOLDER_PATH/BuildingAndRaceCompatibilityFix/mod.json"

# Compress distributable folder into zip file
cd "$NML_MODS_FOLDER_PATH" || exit
zip -r "./BuildingAndRaceCompatibilityFix.zip" "./BuildingAndRaceCompatibilityFix"
