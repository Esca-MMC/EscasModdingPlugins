using StardewModdingAPI;
using StardewValley.Triggers;

namespace EscasModdingPlugins
{
    /// <summary>A trigger raised when a loaded game session is exited, returning the the game's title screen.</summary>
    public class Trigger_ReturnedToTitle
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

            TriggerName = ModEntry.OtherPrefix + "ReturnedToTitle";

            TriggerActionManager.RegisterTrigger(TriggerName);
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            Enabled = true;
        }

        private static void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            TriggerActionManager.Raise(TriggerName);
        }
    }
}