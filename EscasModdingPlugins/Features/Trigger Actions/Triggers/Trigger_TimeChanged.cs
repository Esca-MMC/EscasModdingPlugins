using StardewModdingAPI;
using StardewValley.Triggers;

namespace EscasModdingPlugins
{
    /// <summary>A trigger raised when the in-game time changes.</summary>
    public class Trigger_TimeChanged
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

            TriggerName = ModEntry.TriggerActionPrefix + "TimeChanged";

            TriggerActionManager.RegisterTrigger(TriggerName);
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;

            Enabled = true;
        }

        private static void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            TriggerActionManager.Raise(TriggerName);
        }
    }
}