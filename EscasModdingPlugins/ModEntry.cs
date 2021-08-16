﻿using HarmonyLib;
using StardewModdingAPI;

namespace EscasModdingPlugins
{
    public class ModEntry : Mod, IAssetLoader
    {
        /// <summary>The beginning of each each map/tile property name implemented by this mod.</summary>
        public static readonly string PropertyPrefix = "Esca.EMP/";
        /// <summary>The beginning of each asset name implemented by this mod.</summary>
        public static readonly string AssetPrefix = "Mods/Esca.EMP/";

        /// <summary>Runs once after all mods are loaded by SMAPI. Initializes file data, events, and Harmony patches.</summary>
        public override void Entry(IModHelper helper)
        {
            //initialize utilities
            AssetHelper.Initialize(helper);
            TileData.Monitor = Monitor;

            //apply patches
            Harmony harmony = new Harmony(ModManifest.UniqueID);
            HarmonyPatch_FishLocations.ApplyPatch(harmony, Monitor);
        }

        /************************/
        /* IAssetLoader methods */
        /************************/

        public bool CanLoad<T>(IAssetInfo asset) => AssetHelper.CanLoad<T>(asset); //use AssetHelper
        public T Load<T>(IAssetInfo asset) => AssetHelper.Load<T>(asset); //use AssetHelper
    }
}