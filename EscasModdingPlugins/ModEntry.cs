using ContentPatcher;
using HarmonyLib;
using StardewModdingAPI;
using System;

namespace EscasModdingPlugins
{
    public class ModEntry : Mod
    {
        /// <summary>The beginning of each asset name implemented by this mod.</summary>
        public static readonly string AssetPrefix = "Mods/Esca.EMP/";
        /// <summary>The beginning of each each map/tile property name implemented by this mod.</summary>
        public static readonly string PropertyPrefix = "Esca.EMP/";
        /// <summary>The beginning of each action, query, etc implemented by this mod. Complies with the "unique string ID" format recommended for SDV 1.6.</summary>
        public static readonly string OtherPrefix = "Esca.EMP_";

        /// <summary>Runs once after all mods are loaded by SMAPI. Initializes file data, events, and Harmony patches.</summary>
        public override void Entry(IModHelper helper)
        {
            //initialize utilities
            AssetHelper.Initialize(helper);
            TileData.Monitor = Monitor;

            //load config.json
            ModConfig.Initialize(helper, Monitor);

            //initialize GMCM config menu
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched_InitializeGMCM;

            //initialize Harmony
            Harmony harmony = new Harmony(ModManifest.UniqueID);

            /* initialize mod features */

            //bed placement
            HarmonyPatch_BedPlacement.ApplyPatch(harmony, Monitor);
            HarmonyPatch_PassOutSafely.ApplyPatch(harmony, Monitor);

            //Content Patcher tokens
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched_InitializeCPTokens;

            //custom order boards
            HarmonyPatch_CustomOrderBoards.ApplyPatch(harmony, Monitor);
            DisplayNewOrderExclamationPoint.Enable(helper, Monitor);
            Command_CustomBoard.Enable(helper, Monitor);

            //destroyable bushes
            HarmonyPatch_DestroyableBushes.ApplyPatch(harmony, Monitor);

            //fish locations
            HarmonyPatch_FishLocations.ApplyPatch(harmony, Monitor);

            //game state queries
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched_InitializeGameStateQueries;

            //kitchen features
            ActionKitchen.Enable(Monitor);
            HarmonyPatch_AllowMiniFridges.ApplyPatch(harmony, Monitor);

            //trigger actions
            Helper.Events.GameLoop.GameLaunched += GameLoop_InitializeTriggerActions;

            //water color
            WaterColor.Enable(helper, Monitor);
        }

        /// <summary>Initializes this mod's GMCM config menu.</summary>
        private void GameLoop_GameLaunched_InitializeGMCM(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            ModInteractions.GMCM.Initialize(Helper, Monitor, ModManifest);
        }

        /// <summary>Initializes custom Content Patcher tokens through its API, if available.</summary>
        private void GameLoop_GameLaunched_InitializeCPTokens(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            try
            {
                var api = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher"); //try to get CP's API

                if (api == null)
                    return;

                //initialize EMP's custom tokens
                api.RegisterToken(ModManifest, "GameStateQuery", new GameStateQueryToken());
                api.RegisterToken(ModManifest, "ModData", new ModDataToken());
                api.RegisterToken(ModManifest, "ModDataKeys", new ModDataKeysToken());
                api.RegisterToken(ModManifest, "PlayerStat", new PlayerStatToken());
            }
            catch (Exception ex)
            {
                Monitor.Log($"An error occurred while initializing Content Patcher tokens. Content packs that rely on EMP's tokens might not work correctly. Full error message: \n{ex.ToString()}", LogLevel.Error);
            }
        }

        /// <summary>Initializes custom Game State Queries (a.k.a. GSQs or queries) and related features.</summary>
        private void GameLoop_GameLaunched_InitializeGameStateQueries(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            GSQ_IsTimePassing.Enable(Helper);
            GSQ_SMAPIContexts.Enable(Helper);
        }

        /// <summary>Initializes custom trigger actions and related features.</summary>
        private void GameLoop_InitializeTriggerActions(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            //actions
            TriggerAction_LogMessage.Enable(Monitor);

            //triggers
            Trigger_GameLaunched.Enable(Helper);
            Trigger_ReturnedToTitle.Enable(Helper);
            Trigger_OneSecondUpdateTicked.Enable(Helper);
            Trigger_SaveLoaded.Enable(Helper);
            Trigger_TimeChanged.Enable(Helper);
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
