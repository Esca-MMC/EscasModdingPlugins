using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

namespace EscasModdingPlugins
{
    /// <summary>Adds a SMAPI console command that opens a special orders board for a specified order type.</summary>
    public static class Command_OrderBoard
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

        /// <summary>Initialize's this class's SMAPI console command.</summary>
        /// <param name="helper">The helper instance to use for API access.</param>
        /// <param name="monitor">The monitor instance to use for console/log messages.</param>
        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            if (Initialized)
                return;

            //store args
            Helper = helper;
            Monitor = monitor;

            //initialize commands
            CommandHelper.AddSubCommand("OrderBoard", OrderBoard);

            Initialized = true;
        }

        /// <summary>Opens a special orders board for a specified order type.</summary>
        /// <param name="command">The console command used when calling this method.</param>
        /// <param name="args">The arguments provided after the console command, split around spaces.</param>
        private static void OrderBoard(string command, string[] args)
        {
            try
            {
                if (!Context.IsPlayerFree)
                {
                    Monitor.Log(Helper.Translation.Get("Commands.EMP.PlayerIsBusy"), LogLevel.Info);
                    return;
                }

                string orderType = args?.Length > 0 ? args[0] : ""; //use the default order type ("") if none was provided 
                Monitor.Log(Helper.Translation.Get("Commands.EMP.OrderBoard.OpeningBoard", new { ORDERTYPE = orderType }), LogLevel.Info);
                Game1.activeClickableMenu = new SpecialOrdersBoard(orderType);
            }
            catch (Exception ex)
            {
                Monitor.Log($"This console command encountered an error and couldn't run. Full error message:\n{ex.ToString()}", LogLevel.Error);
            }
        }
    }
}
