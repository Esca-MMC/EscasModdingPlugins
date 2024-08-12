using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;

namespace EscasModdingPlugins
{
    /// <summary>A trigger raised whenever an update tick is completed (~60 times per second, generally NOT affected by frame rate).</summary>
    public class Trigger_UpdateTicked
    {
        /// <summary>The name of the trigger action added by this class.</summary>
        public static string TriggerName { get; set; } = null;
        /// <summary>True if this class's behavior is currently enabled.</summary>
		public static bool Enabled { get; private set; } = false;
        /// <summary>The monitor instance to use for console/log messages.</summary>
        private static IMonitor Monitor { get; set; } = null;

        /// <summary>Initializes this class and enables its features.</summary>
        /// <param name="monitor">The monitor instance to use for console/log messages.</param>
        /// <param name="helper">The helper instance to use during initialization, e.g. to register events.</param>
        public static void Enable(IMonitor monitor, IModHelper helper)
        {
            if (Enabled)
                return;

            Monitor = monitor;
            TriggerName = ModEntry.TriggerActionPrefix + "UpdateTicked";

            TriggerActionManager.RegisterTrigger(TriggerName);
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            Enabled = true;
        }

        private static void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            TriggerActionManager.Raise(TriggerName);
        }
    }
}