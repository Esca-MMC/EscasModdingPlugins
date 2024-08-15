using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;

namespace EscasModdingPlugins
{
    /// <summary>A game state query that checks whether certain mods are installed.</summary>
    /// <remarks>
    /// This query is true if *any* of the listed mod IDs are installed, or false if none are installed.
    /// 
    /// Format: Esca.EMP_HAS_MOD {mod ID}+
    /// </remarks>
    public class GSQ_Has_Mod
    {
        /// <summary>The name of the GSQ added by this class.</summary>
        public static string QueryName { get; set; } = null;
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
            QueryName = ModEntry.OtherPrefix + "HAS_MOD";

            GameStateQuery.Register(QueryName, HasMod);

            Enabled = true;
        }

        /// <inheritdoc cref="T:StardewValley.Delegates.GameStateQueryDelegate" />
        private static bool HasMod(string[] query, GameStateQueryContext context)
        {
            if (!ArgUtility.TryGet(query, 1, out var _, out var error)) //if no arguments were provided
            {
                return GameStateQuery.Helpers.ErrorResult(query, error);
            }

            //return true if any of the arguments matches a loaded mod ID, or false if none match
            return GameStateQuery.Helpers.AnyArgMatches(query, 1, (string modID) => Helper.ModRegistry.IsLoaded(modID));
        }
    }
}