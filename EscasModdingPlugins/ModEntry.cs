using ContentPatcher;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;

namespace EscasModdingPlugins
{
    public class ModEntry : Mod
    {
        /**********/
        /* Fields */
        /**********/

        /// <summary>The beginning of each asset name implemented by this mod.</summary>
        public static readonly string AssetPrefix = "Mods/Esca.EMP/";
        /// <summary>The beginning of each each map/tile property name implemented by this mod.</summary>
        public static readonly string PropertyPrefix = "Esca.EMP/";
        /// <summary>The beginning of each action, query, etc implemented by this mod. Complies with the "unique string ID" format recommended for SDV 1.6.</summary>
        public static readonly string OtherPrefix = "Esca.EMP_";

        /******************/
        /* Public methods */
        /******************/

        /// <summary>Runs once after all mods are loaded by SMAPI. Initializes file data, events, and Harmony patches.</summary>
        public override void Entry(IModHelper helper)
        {
            /*********************************/
            /* Initialize internal utilities */
            /*********************************/

            Harmony harmony = new Harmony(ModManifest.UniqueID);

            ModConfig.Initialize(helper, Monitor);
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched_InitializeGMCM;

            AssetHelper.Initialize(helper);
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked_InitializeCommands;
            TileData.Monitor = Monitor;

            /****************************************/
            /* Initialize features - Game mechanics */
            /****************************************/

            //bed placement
            HarmonyPatch_BedPlacement.ApplyPatch(harmony, Monitor);
            HarmonyPatch_PassOutSafely.ApplyPatch(harmony, Monitor);

            //custom order boards
            HarmonyPatch_CustomOrderBoards.ApplyPatch(harmony, Monitor);
            DisplayNewOrderExclamationPoint.Enable(helper, Monitor);
            Command_CustomBoard.Enable(helper, Monitor);

            //destroyable bushes
            HarmonyPatch_DestroyableBushes.ApplyPatch(harmony, Monitor);

            //fish locations
            HarmonyPatch_FishLocations.ApplyPatch(harmony, Monitor);

            //kitchen features
            ActionKitchen.Enable(Monitor);
            HarmonyPatch_AllowMiniFridges.ApplyPatch(harmony, Monitor);

            //water color
            WaterColor.Enable(helper, Monitor);

            /*******************************************/
            /* Initialize features - Modding utilities */
            /*******************************************/

            //Content Patcher tokens
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched_InitializeCPTokens;

            //game state queries
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched_InitializeGameStateQueries;

            //trigger actions
            Helper.Events.GameLoop.GameLaunched += GameLoop_InitializeTriggerActions;
        }

        /// <summary>Generates an API instance for another SMAPI mod.</summary>
        /// <remarks>See <see cref="IEmpApi"/> for documentation.</remarks>
        /// <returns>A new API instance.</returns>
        public override object GetApi() => new EmpApi();

        /*******************/
        /* Private methods */
        /*******************/

        /// <summary>Initializes this mod's shared console command.</summary>
        private void GameLoop_UpdateTicked_InitializeCommands(object sender, UpdateTickedEventArgs e)
        {
            if (e.Ticks >= 1) //wait for 1 tick (NOTE: if used before this, Helper.Translation (i18n) only appears to use the default language)
            {
                CommandHelper.Initialize(Helper, Monitor);
                Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked_InitializeCommands; //disable this event after running once
            }
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
    }
}
