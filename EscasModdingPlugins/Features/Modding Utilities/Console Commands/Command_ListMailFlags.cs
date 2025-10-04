using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace EscasModdingPlugins
{
    /// <summary>Adds a SMAPI console command that lists a player's current mail flags.</summary>
    public static class Command_ListMailFlags
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
            CommandHelper.AddSubCommand("ListMailFlags", ListMailFlags);
            CommandHelper.AddSubCommandAlias("Flag", "ListMailFlags");
            CommandHelper.AddSubCommandAlias("Flags", "ListMailFlags");
            CommandHelper.AddSubCommandAlias("ListFlags", "ListMailFlags");
            CommandHelper.AddSubCommandAlias("ListMail", "ListMailFlags");
            CommandHelper.AddSubCommandAlias("Mail", "ListMailFlags");
            CommandHelper.AddSubCommandAlias("MailFlags", "ListMailFlags");

            Initialized = true;
        }

        /// <summary>Lists a player's current mail flags.</summary>
        /// <param name="command">The console command used when calling this method.</param>
        /// <param name="args">The arguments provided after the command, split around spaces.</param>
        private static void ListMailFlags(string command, string[] args)
        {
            try
            {
                string mailTypeString = args.Length > 0 ? args[0] : "Any"; //if no mail type was specified, show all types

                if (!Enum.TryParse(typeof(MailType), mailTypeString, true, out object mailTypeRaw) || mailTypeRaw is not MailType mailType) //try to parse the arg into a known mail type
                {
                    Monitor.Log(Helper.Translation.Get("Commands.EMP.ListMailFlags.UnrecognizedMailType", new { MAILTYPE = mailTypeString }), LogLevel.Info);
                    return;
                }

                string output = "";

                if (mailType is MailType.Mailbox or MailType.Any)
                {
                    if (output != "")
                        output += "\n\n";
                    output += $"{Helper.Translation.Get("Commands.EMP.ListMailFlags.Mailbox")}";

                    List<string> flags = new(Game1.player.mailbox);
                    flags.Sort();
                    foreach (string flag in flags)
                        output += $"\n  {flag}";
                }

                if (mailType is MailType.Received or MailType.Any)
                {
                    if (output != "")
                        output += "\n\n";
                    output += $"{Helper.Translation.Get("Commands.EMP.ListMailFlags.Received")}";

                    List<string> flags = new(Game1.player.mailReceived);
                    flags.Sort();
                    foreach (string flag in flags)
                        output += $"\n  {flag}";
                }

                if (mailType is MailType.Tomorrow or MailType.Any)
                {
                    if (output != "")
                        output += "\n\n";
                    output += $"{Helper.Translation.Get("Commands.EMP.ListMailFlags.Tomorrow")}";

                    List<string> flags = new(Game1.player.mailForTomorrow);
                    flags.Sort();
                    foreach (string flag in flags)
                        output += $"\n  {flag}";
                }

                Monitor.Log(output, LogLevel.Info);
            }
            catch (Exception ex)
            {
                Monitor.Log($"This console command encountered an error and couldn't run. Full error message:\n{ex.ToString()}", LogLevel.Error);
            }
        }

        /// <summary>The support mail types this command can check.</summary>
        private enum MailType
        {
            Any,

            Mailbox,
            M = Mailbox,

            Received,
            R = Received,

            Tomorrow,
            T = Tomorrow
        }
    }
}
