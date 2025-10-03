using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace EscasModdingPlugins
{
    /// <summary>Adds a SMAPI console command that opens a special orders board for a specified order type.</summary>
    public static class Command_CustomBoard
    {
        /**************/
        /* Properties */
        /**************/

        /// <summary>The helper instance to use for API access.</summary>
        private static IModHelper Helper { get; set; } = null;
        /// <summary>The monitor instance to use for console/log messages.</summary>
        private static IMonitor Monitor { get; set; } = null;

        /// <summary>True if this class is enabled and ready to use.</summary>
        private static bool Enabled { get; set; } = false;

        /***********/
        /* Methods */
        /***********/

        /// <summary>Enables this class's SMAPI console commands.</summary>
        /// <param name="helper">The helper instance to use for API access.</param>
        /// <param name="monitor">The monitor instance to use for console/log messages.</param>
        public static void Enable(IModHelper helper, IMonitor monitor)
        {
            if (Enabled)
                return; //do nothing

            //store args
            Helper = helper;
            Monitor = monitor;

            //initialize commands
            CommandHelper.AddSubCommand("CustomBoard", CustomBoard);

            Enabled = true;
        }

        /// <summary>Opens a special orders board for a custom order type.</summary>
        /// <param name="command">The console command used when calling this method (e.g. "EMP").</param>
        /// <param name="args">The arguments provided after the console command (e.g. "CustomBoard" "MyCustomOrders").</param>
        private static void CustomBoard(string command, string[] args)
        {
            if (!Context.IsPlayerFree)
            {
                Monitor.Log(Helper.Translation.Get("Commands.EMP.PlayerIsBusy"), LogLevel.Info);
                return;
            }

            string orderType = args?.Length > 0 ? args[0] : ""; //use the default order type ("") if none was provided 
            Monitor.Log(Helper.Translation.Get("Commands.EMP.CustomBoard.OpeningBoard", new { ORDERTYPE = orderType }), LogLevel.Info);
            Game1.activeClickableMenu = new SpecialOrdersBoard(orderType);
        }
    }
}
