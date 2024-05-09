using ContentPatcher;
using HarmonyLib;
using StardewModdingAPI;
using System;

namespace EscasModdingPlugins
{
    public class ModEntry : Mod
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

            //load config.json
            ModConfig.Initialize(helper, Monitor);

            //initialize mod interactions
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched_InitializeModInteractions;

            //initialize Content Patcher tokens
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched_InitializeCPTokens;

            //initialize Harmony and mod features
            Harmony harmony = new Harmony(ModManifest.UniqueID);

            //fish locations
            HarmonyPatch_FishLocations.ApplyPatch(harmony, Monitor);

            //custom order boards
            HarmonyPatch_CustomOrderBoards.ApplyPatch(harmony, Monitor);
            DisplayNewOrderExclamationPoint.Enable(helper, Monitor);
            Command_CustomBoard.Enable(helper, Monitor);

            //destroyable bushes
            HarmonyPatch_DestroyableBushes.ApplyPatch(harmony, Monitor);

            //bed placement
            HarmonyPatch_BedPlacement.ApplyPatch(harmony, Monitor);
            HarmonyPatch_PassOutSafely.ApplyPatch(harmony, Monitor);

            //kitchen features
            ActionKitchen.Enable(Monitor);
            HarmonyPatch_AllowMiniFridges.ApplyPatch(harmony, Monitor);

            //water color
            WaterColor.Enable(helper, Monitor);
        }

        /// <summary>Initializes Content Patcher tokens through its API, if available.</summary>
        private void GameLoop_GameLaunched_InitializeCPTokens(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            try
            {
                var api = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher"); //try to get CP's API

                if (api == null)
                    return;

                //initialize EMP's custom tokens
                api.RegisterToken(ModManifest, "GameStateQuery", new GameStateQueryToken());
            }
            catch (Exception ex)
            {
                Monitor.Log($"An error occurred while initializing Content Patcher tokens. Content packs that rely on EMP's tokens might not work correctly. Full error message: \n{ex.ToString()}", LogLevel.Error);
            }
        }

        /// <summary>Initializes mod interactions when all mods have finished loading.</summary>
        private void GameLoop_GameLaunched_InitializeModInteractions(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            ModInteractions.GMCM.Initialize(Helper, Monitor, ModManifest);
        }

        /**************/
        /* API method */
        /**************/

        /// <summary>Generates an API instance for another SMAPI mod.</summary>
        /// <remarks>See <see cref="IEmpApi"/> for documentation.</remarks>
        /// <returns>A new API instance.</returns>
        public override object GetApi() => new EmpApi();
    }
}
