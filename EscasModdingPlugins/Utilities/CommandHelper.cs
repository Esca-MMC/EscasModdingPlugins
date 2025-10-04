using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EscasModdingPlugins
{
    /// <summary>Manages a group of console commands inside a single shared command.</summary>
    internal static class CommandHelper
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
        /// <summary>The set of registered sub-commands. Keys are sub-command names (case-insensitive). Values are callback methods that perform the sub-command.</summary>
        private static Dictionary<string, Action<string, string[]>> SubCommands { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>The set of registered aliases for sub-commands. Keys are aliases (case-insensitive). Values are the sub-command that each alias represents.</summary>
        private static Dictionary<string, string> SubCommandAliases { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /******************/
        /* Public methods */
        /******************/

        /// <summary>Initializes this shared console command.</summary>
        /// <param name="helper">The helper instance to use for API access.</param>
        /// <param name="monitor">The monitor instance to use for console/log messages.</param>
        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            if (Initialized)
                return;

            Helper = helper;
            Monitor = monitor;

            helper.ConsoleCommands.Add("EMP", helper.Translation.Get("Commands.EMP.Description"), RunCommand); //NOTE: the translation used here won't change until the game is restarted
            Initialized = true;
        }

        /// <summary>Adds or replaces a sub-command within this shared console command.</summary>
        /// <param name="name">The name of the sub-command. Case-insensitive, but the default case is displayed in "help" documentation.</param>
        /// <param name="callback">The method that runs this command. Parameters are the sub-command's name and arguments.</param>
        /// <remarks>"Help" is a reserved term, and shouldn't be used as a sub-command's name or accepted as a first argument. This is case-insensitive.</remarks>
        public static void AddSubCommand(string name, Action<string, string[]> callback)
            => SubCommands[name] = callback;

        /// <summary>Adds or replaces an alias for a sub-command, which can be used instead of the sub-command's primary name.</summary>
        /// <param name="alias">The alias to use. Case-insensitive, but the default case is displayed in "help" documentation.</param>
        /// <param name="subCommandName">The sub-command that this alias represents.</param>
        public static void AddSubCommandAlias(string alias, string subCommandName)
            => SubCommandAliases[alias] = subCommandName;

        /*******************/
        /* Private methods */
        /*******************/

        /// <summary>Runs this class's shared command, which runs a sub-command or displays documentation.</summary>
        /// <param name="command">The name of the shared command.</param>
        /// <param name="args">The arguments provided after the shared command, split around spaces.</param>
        private static void RunCommand(string command, string[] args)
        {
            if (args == null || args.Length == 0 || (args.Length == 1 && string.Equals(args[0], "help", StringComparison.OrdinalIgnoreCase))) //format: "command" or "command help"
            {
                string output = Helper.Translation.Get("Commands.EMP.CommandInfo");
                foreach (string subCommand in SubCommands.Keys)
                {
                    output += $"\n\nEMP {subCommand}"; //display command and sub-command

                    var aliases = GetAliases(subCommand);
                    if (aliases.Count > 0)
                        output += $"\n   {Helper.Translation.Get("Commands.EMP.Aliases", new { ALIASES = string.Join(", ", aliases) })}"; //display aliases for this sub-command, if any

                    foreach (string line in GetMultilineTranslation($"Commands.EMP.{subCommand}.Description.", Helper.Translation))
                        output += $"\n   {line}"; //display each line of documentation
                }
                Monitor.Log(output, LogLevel.Info);
            }
            else if (args.Length >= 2 && string.Equals(args[0], "help", StringComparison.OrdinalIgnoreCase)) //format: "command help sub-command"
            {
                string subCommand = args[1];
                if (SubCommands.ContainsKey(subCommand) //if the sub-command is valid
                || (SubCommandAliases.TryGetValue(subCommand, out subCommand) && SubCommands.ContainsKey(subCommand))) //OR if it's an alias of a valid command (overwrite it with the sub-command name)
                {
                    string output = Helper.Translation.Get("Commands.EMP.CommandInfo");

                    subCommand = SubCommands.Keys.FirstOrDefault(key => string.Equals(subCommand, key, StringComparison.OrdinalIgnoreCase)) ?? subCommand; //get the default capitalization of this sub-command
                    output += $"\n\nEMP {subCommand}"; //display command and sub-command

                    var aliases = GetAliases(subCommand);
                    if (aliases.Count > 0)
                        output += $"\n   {Helper.Translation.Get("Commands.EMP.Aliases", new { ALIASES = string.Join(", ", aliases) })}"; //display aliases for this sub-command, if any

                    foreach (string line in GetMultilineTranslation($"Commands.EMP.{subCommand}.Description.", Helper.Translation))
                        output += $"\n   {line}"; //display each line of documentation
                    Monitor.Log(output, LogLevel.Info);
                }
                else //if the subcommand is invalid
                    Monitor.Log(Helper.Translation.Get("Commands.EMP.UnrecognizedCommand", new { COMMAND = args[1] }), LogLevel.Info); //NOTE: use args here; subCommand may be overwritten

            }
            else //format: "command sub-command arguments"
            {
                string subCommand = args[0];
                if (SubCommands.TryGetValue(subCommand, out var methods) //if the sub-command is valid
                || SubCommandAliases.TryGetValue(subCommand, out subCommand) && SubCommands.TryGetValue(subCommand, out methods)) //OR if it's an alias of a valid command (overwrite it with the sub-command name)
                    methods.Invoke(subCommand, args.Skip(1).ToArray()); //run the sub-command with any args after its name
                else //if the sub-command is invalid
                    Monitor.Log(Helper.Translation.Get("Commands.EMP.UnrecognizedCommand", new { COMMAND = args[0] }), LogLevel.Info); //NOTE: use args here; subCommand may be overwritten
            }
        }

        /// <summary>Gets all registered aliases for a sub-command.</summary>
        /// <param name="subCommandName">The sub-command that the aliases represent.</param>
        /// <returns></returns>
        private static List<string> GetAliases(string subCommandName)
        {
            List<string> aliases = new();
            foreach (var entry in SubCommandAliases)
                if (string.Equals(subCommandName, entry.Value, StringComparison.OrdinalIgnoreCase)) //if the sub-command matches entry.Value, then entry.Key is an alias for the sub-command
                    aliases.Add(entry.Key);
            return aliases;
        }

        /// <summary>Gets each translated line of a multi-line i18n key, in the nearest applicable language to the current setting.</summary>
        /// <param name="key">The key used for this i18n file(s). This should NOT include any trailing line numbers.</param>
        /// <param name="translationHelper">The SMAPI translation/i18n helper to use.</param>
        /// <returns>Each translated line for this key in ascending order. If no i18n file has a matching key for the first line, this returns an empty set.</returns>
        /// <remarks>
        /// <para>This method retrieves text from the i18n file that best matches the user's current language setting, and supports variable line counts.</para>
        /// <para>The expected key format in an i18n file is "KEY#", where "KEY" is this method's "key" argument, and # is an integer.</para>
        /// <para>Line 1 (e.g. "KEY1") must exist in an i18n file, or that file will not be used. All lines must be in numerical order without gaps.</para>
        /// <para>All lines matching the format will be returned in numerical order, from low to high, starting with line 1 and ending when the next line is absent.</para>
        /// </remarks>
        private static IEnumerable<string> GetMultilineTranslation(string key, ITranslationHelper translationHelper)
        {
            string line1key = $"{key}1";
            if (!translationHelper.ContainsKey(line1key)) //if no applicable i18n has this key
                yield break; //return an empty set

            var translationsForLine1 = translationHelper.GetInAllLocales(line1key, false);

            string localeToUse = translationHelper.Locale ?? "default";
            if (!translationsForLine1.ContainsKey(localeToUse)) //if the full locale doesn't have an i18n
            {
                localeToUse = localeToUse.Split('-', 2)[0]; //trim off any sub-locale information, e.g. "pt-BR" -> "pt"
                if (!translationsForLine1.ContainsKey(localeToUse)) //if the shorter locale doesn't exist either
                {
                    localeToUse = "default";
                    if (!translationsForLine1.ContainsKey(localeToUse)) //if default doesn't exist (TODO: test whether this can happen and why)
                        yield break; //return an empty set
                }
            }

            int lineNumber = 1;
            while (true)
            {
                if (translationHelper.GetInAllLocales($"{key}{lineNumber}").TryGetValue(localeToUse, out Translation line)) //if this line has a translation in the same i18n
                    yield return line.UsePlaceholder(false); //use the line (and suppress placeholders if it's blank)
                else
                    yield break; //stop here
                lineNumber++;
            }
        }
    }
}