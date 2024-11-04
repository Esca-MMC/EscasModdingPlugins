using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Triggers;

namespace EscasModdingPlugins
{
    /// <summary>A trigger raised when a saved game is loaded.</summary>
    public class Trigger_SaveLoaded
    {
        /// <summary>The name of the trigger action added by this class.</summary>
        public static string TriggerName { get; set; } = null;
        /// <summary>True if this class's behavior is currently enabled.</summary>
		public static bool Enabled { get; private set; } = false;

        /// <summary>Initializes this class and enables its features.</summary>
        /// <param name="helper">The helper instance to use during initialization, e.g. to register events.</param>
        public static void Enable(IModHelper helper)
        {
            if (Enabled)
                return;

            TriggerName = ModEntry.OtherPrefix + "SaveLoaded";

            TriggerActionManager.RegisterTrigger(TriggerName);
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            Enabled = true;
        }

        private static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            TriggerActionManager.Raise(TriggerName);
        }
    }
}