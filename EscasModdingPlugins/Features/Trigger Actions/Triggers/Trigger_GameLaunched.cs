using StardewModdingAPI;
using StardewValley.Triggers;
using StardewModdingAPI.Events;

namespace EscasModdingPlugins
{
    /// <summary>A trigger raised when the game has launched and all mods have loaded.</summary>
    public class Trigger_GameLaunched
    {
        /// <summary>The name of the trigger action added by this class.</summary>
        public static string TriggerName { get; set; } = null;
        /// <summary>True if this class's behavior is currently enabled.</summary>
		public static bool Enabled { get; private set; } = false;
        /// <summary>The helper instance to use to register events, etc.</summary>
        private static IModHelper Helper { get; set; } = null;

        /// <summary>Initializes this class and enables its features.</summary>
        /// <param name="helper">The helper instance to use during initialization, e.g. to register events.</param>
        public static void Enable(IModHelper helper)
        {
            if (Enabled)
                return;

            Helper = helper;

            TriggerName = ModEntry.TriggerActionPrefix + "GameLaunched";

            TriggerActionManager.RegisterTrigger(TriggerName);
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            Enabled = true;
        }

        /// <summary>Raises this trigger after the game's first update tick is completed.</summary>
        /// <remarks>
        /// Raising this trigger during an actual GameLaunched event would cause timing issues, e.g. it might fire before Content Patcher has edited data assets.
        /// 
        /// Instead, this trigger is raised after the game's first update tick is completed, which should be acceptable for most use cases.
        /// </remarks>
        [EventPriority(EventPriority.Low)]
        private static void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            TriggerActionManager.Raise(TriggerName);
            Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked; //unregister this event after it happens once
        }
    }
}