using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using System;
using System.Collections.Generic;

namespace EscasModdingPlugins
{
    /// <summary>Adds a SMAPI console command that lists all registered GSQs and their aliases.</summary>
    public static class Command_ListGSQs
    {
        /**************/
        /* Properties */
        /**************/

        /// <summary>The helper instance to use for API access.</summary>
        private static IModHelper Helper { get; set; } = null;
        /// <summary>The monitor instance to use for console/log messages.</summary>
        private static IMonitor Monitor { get; set; } = null;
        /// <summary>True if this class is initialized and ready to use.</summary>
        private static bool Initialized { get; set; } = false;

        /***********/
        /* Methods */
        /***********/

        /// <summary>Initializes this class's SMAPI console command.</summary>
        /// <param name="helper">The helper instance to use for API access.</param>
        /// <param name="monitor">The monitor instance to use for console/log messages.</param>
        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            if (Initialized)
                return;

            //store args
            Helper = helper;
            Monitor = monitor;

            //initialize command and aliases
            CommandHelper.AddSubCommand("ListGSQs", ListGSQs);
            CommandHelper.AddSubCommandAlias("ListGSQ", "ListGSQs");
            CommandHelper.AddSubCommandAlias("GSQ", "ListGSQs");
            CommandHelper.AddSubCommandAlias("GSQs", "ListGSQs");

            Initialized = true;
        }

        /// <summary>Lists all registered GSQs and their aliases.</summary>
        /// <param name="command">The console command used when calling this method.</param>
        /// <param name="args">The arguments provided after the command, split around spaces.</param>
        private static void ListGSQs(string command, string[] args)
        {
            try
            {
                var queryDict = Helper.Reflection.GetField<Dictionary<string, GameStateQueryDelegate>>(typeof(GameStateQuery), "QueryTypeLookup", true)?.GetValue(); //get the non-public GSQ dictionary
                var aliasDict = Helper.Reflection.GetField<Dictionary<string, string>>(typeof(GameStateQuery), "Aliases", true).GetValue(); //get the non-public GSQ alias dictionary

                List<string> queries = new(queryDict.Keys); //create a sortable list of GSQ IDs
                queries.Sort();

                string output = "";
                foreach (var query in queries)
                {
                    output += $"\n{query}";

                    List<string> aliases = new();
                    foreach (var entry in aliasDict)
                        if (entry.Value?.Equals(query, StringComparison.OrdinalIgnoreCase) == true) //if entry.Value matches this query, entry.Key is an alias of the query
                            aliases.Add(entry.Key);
                    aliases.Sort();

                    if (aliases.Count > 0)
                        output += $"\n  {Helper.Translation.Get("Commands.EMP.Aliases", new { ALIASES = string.Join(", ", aliases) })}"; //display the "aliases" line with comma-separated aliases
                }

                Monitor.Log($"{Helper.Translation.Get("Commands.EMP.ListGSQs.StartList")}{output}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"This console command encountered an error and couldn't run. Full error message:\n{ex.ToString()}", LogLevel.Error);
            }
        }
    }
}
