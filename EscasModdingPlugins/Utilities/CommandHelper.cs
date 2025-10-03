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
        /// <summary>The set of registered sub-commands. Keys are sub-command names. Values are callback methods that perform the sub-command.</summary>
        private static Dictionary<string, Action<string, string[]>> SubCommands { get; set; } = new(StringComparer.OrdinalIgnoreCase);

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
        /// <param name="name">The name of the sub-command. Case-insensitive.</param>
        /// <param name="callback">The method that runs this command. Parameters are the sub-command's name and arguments.</param>
        /// <remarks>"Help" is a reserved term, and shouldn't be used as a sub-command's name or accepted as a first argument. This is case-insensitive.</remarks>
        public static void AddSubCommand(string name, Action<string, string[]> callback)
            => SubCommands[name] = callback;

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
                string output = Helper.Translation.Get("Commands.EMP.AvailableCommands");
                foreach (string subCommand in SubCommands.Keys)
                {
                    output += $"\n\nEMP {subCommand}";
                    foreach (string line in GetMultilineTranslation($"Commands.EMP.{subCommand}.Description.", Helper.Translation))
                        output += $"\n   {line}"; //add each doc line with indentation
                }
                Monitor.Log(output, LogLevel.Info);
            }
            else if (args.Length >= 2 && string.Equals(args[0], "help", StringComparison.OrdinalIgnoreCase)) //format: "command help sub-command"
            {
                string subCommand = args[1];
                if (SubCommands.ContainsKey(subCommand)) //if the sub-command is valid
                {
                    string output = $"EMP {subCommand}";
                    foreach (string line in GetMultilineTranslation($"Commands.EMP.{subCommand}.Description.", Helper.Translation))
                        output += $"\n   {line}"; //add each doc line with indentation
                    Monitor.Log(output, LogLevel.Info);
                }
                else //if the subcommand is invalid
                    Monitor.Log(Helper.Translation.Get("Commands.EMP.UnrecognizedCommand", new { COMMAND = subCommand }), LogLevel.Info);

            }
            else //format: "command sub-command arguments"
            {
                string subCommand = args[0];
                if (SubCommands.TryGetValue(subCommand, out var methods)) //if a valid sub-command was provided
                    methods.Invoke(subCommand, args.Skip(1).ToArray()); //run the sub-command with any args after its name
                else //if an invalid sub-command was provided
                    Monitor.Log(Helper.Translation.Get("Commands.EMP.UnrecognizedCommand", new { COMMAND = subCommand }), LogLevel.Info);
            }
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