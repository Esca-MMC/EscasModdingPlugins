using StardewModdingAPI;
using StardewValley;

namespace EscasModdingPlugins
{
    /// <summary>A game state query initializer for the "should time pass" query.</summary>
    /// <remarks>
    /// Format: Esca.EMP_SHOULD_TIME_PASS
    /// </remarks>
    public class GSQ_IsTimePassing
    {
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

            GameStateQuery.Register(ModEntry.OtherPrefix + "IS_TIME_PASSING", (_, _) => IsTimePassing());

            Enabled = true;
        }

        private static bool IsTimePassing()
        {
            if (Game1.HostPaused || Game1.NetTimePaused || !Game1.shouldTimePass(false)) //check common pause reasons like festivals, events, chat commands, etc
                return false;

            //check whether the game is paused because it's not the active window in single-player mode
            //NOTE: this is based on a section of Game1._update from the Windows build of the game; it may need adjustment for some operating systems and/or game versions

            if ((Game1.paused || (!Game1.game1.IsActiveNoOverlay && Program.releaseBuild)) && (Game1.options == null || Game1.options.pauseWhenOutOfFocus || Game1.paused) && Game1.multiplayerMode == 0)
                return false;

            return true;
        }
    }
}