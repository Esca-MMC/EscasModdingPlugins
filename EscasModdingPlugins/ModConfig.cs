using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscasModdingPlugins
{
    /// <summary>The available user configuration settings.</summary>
    public class ModConfig
    {
        /// <summary>The currently active user configuration settings.</summary>
        public static ModConfig Instance { get; set; } = null;

        /// <summary>Loads <see cref="Instance"/> from the user's config.json file.</summary>
        /// <summary>The helper instance to use for API access.</summary>
        /// <summary>The monitor instance to use for console/log messages.</summary>
        public static void Initialize(IModHelper helper, IMonitor monitor)
        {
            try
            {
                Instance = helper.ReadConfig<ModConfig>();
            }
            catch (Exception ex)
            {
                Instance = new ModConfig(); //use default config
                monitor.Log($"Error while reading config.json. Using default settings instead. Full error message: \n{ex}", LogLevel.Warn);
            }
        }

        /// <summary>True if bed placement should be enabled at all locations.</summary>
        public bool AllowBedPlacementEverywhere { get; set; } = false;
        /// <summary>True if the "passing out" penalty should be removed from all locations.</summary>
        public bool PassOutSafelyEverywhere { get; set; } = false;
        /// <summary>True if Mini-Fridge placement should be enabled at all locations.</summary>
        public bool AllowMiniFridgesEverywhere { get; set; } = false;
    }
}
