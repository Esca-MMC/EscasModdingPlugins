using StardewModdingAPI;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Triggers;

namespace EscasModdingPlugins
{
    /// <summary>A trigger action that logs a message to the SMAPI console and log file.</summary>
    public class TriggerAction_LogMessage
    {
        /// <summary>The name of the trigger action added by this class.</summary>
        public static string TriggerActionName { get; set; } = null;
        /// <summary>True if this class's behavior is currently enabled.</summary>
		public static bool Enabled { get; private set; } = false;
        /// <summary>The monitor instance to use for console/log messages.</summary>
        private static IMonitor Monitor { get; set; } = null;

        /// <summary>Initializes this class and enables its features.</summary>
        /// <param name="monitor">The monitor instance to use for console/log messages.</param>
        public static void Enable(IMonitor monitor)
        {
            if (Enabled)
                return;

            Monitor = monitor;
            TriggerActionName = ModEntry.TriggerActionPrefix + "LogMessage";

            TriggerActionManager.RegisterAction(TriggerActionName, LogMessage);

            Enabled = true;
        }

        /// <summary>Log a provided message to the SMAPI console.</summary>
        /// <inheritdoc cref="TriggerActionDelegate" />
        private static bool LogMessage(string[] args, TriggerActionContext context, out string error)
        {
            //validate and parse arguments

            if (!ArgUtility.TryGetEnum<LogLevel>(args, 1, out LogLevel logLevel, out error)) //a LogLevel name, e.g. LogLevel.Trace
                return false;

            if (!ArgUtility.TryGetRemainder(args, 2, out string message, out error)) //arg 2: all remaining text
                return false;

            //do action

            if (Monitor.IsVerbose)
                Monitor.Log($"Logging a message for another mod. Trigger action ID: {context.Data?.Id ?? "None (not caused by Data/TriggerActions)"}", LogLevel.Trace);

            Monitor.Log(message, logLevel);

            error = null;
            return true;
        }
    }
}