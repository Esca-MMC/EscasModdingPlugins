using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;

namespace EscasModdingPlugins
{
    /// <summary>A set of game state queries that check SMAPI's Context class.</summary>
    /// <remarks>
    /// These queries are true or false based on the associated SMAPI context value. Refer to SMAPI or EMP's documentation for info about individual contexts.
    /// <para />
    /// Formats:
    /// <para />
    /// Esca.EMP_CAN_PLAYER_MOVE<br />
    /// Esca.EMP_IS_PLAYER_FREE<br />
    /// Esca.EMP_IS_SPLIT_SCREEN<br />
    /// Esca.EMP_IS_WORLD_READY
    /// </remarks>
    public class GSQ_SMAPI_Contexts
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

            GameStateQuery.Register(ModEntry.OtherPrefix + "CAN_PLAYER_MOVE", (_, _) => Context.CanPlayerMove);
            GameStateQuery.Register(ModEntry.OtherPrefix + "IS_PLAYER_FREE", (_, _) => Context.IsPlayerFree);
            GameStateQuery.Register(ModEntry.OtherPrefix + "IS_SPLIT_SCREEN", (_, _) => Context.IsSplitScreen);
            GameStateQuery.Register(ModEntry.OtherPrefix + "IS_WORLD_READY", (_, _) => Context.IsWorldReady);

            Enabled = true;
        }
    }
}