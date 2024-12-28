using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace EscasModdingPlugins
{
    /// <summary>Allows retrieval of game assets' most recent versions. Uses caching to minimize load frequency.</summary>
    /// <remarks>
    /// This class's events clear the cache whenever Content Patcher might update assets with new changes.
    /// This will not account for changes made with IAssetEditor at different times; edits are currently passive and can't be actively detected.
    /// 
    /// Note that an overhaul of the SMAPI content API is planned, which will likely change this process and/or obsolete this class.
    /// </remarks>
    internal static class AssetHelper
    {
        /**********/
        /* Fields */
        /**********/

        /// <summary>True if this class is initialized and ready to use.</summary>
        private static bool Initialized = false;
        /// <summary>A set of asset names and constructors for their default instances.</summary>
        private static Dictionary<string, Func<object>> Defaults = new Dictionary<string, Func<object>>();
        /// <summary>A set of asset names and their most recently updated instances.</summary>
        private static Dictionary<string, object> Cache = new Dictionary<string, object>();
        /// <summary>This mod's SMAPI helper instance.</summary>
        private static IModHelper Helper = null;
        /// <summary>A lock used to prevent multiple threads simultaneously loading data.</summary>
        private static object LoadLock = new();

        /******************/
        /* Public methods */
        /******************/

        /// <summary>Performs required setup tasks for this class.</summary>
        /// <param name="helper">This mod's SMAPI helper instance.</param>
        internal static void Initialize(IModHelper helper)
        {
            if (Initialized)
                return;

            //store args
            Helper = helper;

            //enable SMAPI events
            helper.Events.Content.AssetRequested += AssetRequested_LoadDefaults;
            helper.Events.GameLoop.DayStarted += DayStarted_ClearCache;
            helper.Events.GameLoop.TimeChanged += TimeChanged_ClearCache;
            helper.Events.Player.Warped += Warped_ClearCache;

            Initialized = true;
        }

        /// <summary>Get the most recent version of a game asset.</summary>
        /// <typeparam name="T">The asset's type.</typeparam>
        /// <param name="assetName">The asset's name, e.g. "Characters/Abigail".</param>
        /// <returns>The latest available version of the asset.</returns>
        internal static T GetAsset<T>(string assetName)
        {
            if (Cache.TryGetValue(assetName, out object asset)) //if this asset has a cached version
                return (T)asset; //return the cached asset as the given type
            else //if this asset does NOT have a cached version
            {
                T loadedAsset;

                lock (LoadLock)
                {
                    loadedAsset = Helper.GameContent.Load<T>(assetName); //load the asset's most recent version
                    Cache[assetName] = loadedAsset; //cache it
                }

                return loadedAsset; //return it
            }
        }

        /// <summary>Gets the default instance of the named asset if one is available.</summary>
        /// <typeparam name="T">The asset's type.</typeparam>
        /// <param name="normalizedAssetName">The asset's normalized name. See <see cref="IAssetInfo.AssetName"/> or <see cref="IContentHelper.NormalizeAssetName(string)"/>.</param>
        /// <param name="defaultAsset">A default instance of the asset.</param>
        /// <returns>True if a default instance exists for this asset. False otherwise.</returns>
        internal static bool TryGetDefault<T>(string normalizedAssetName, out T defaultAsset)
        {
            if (Defaults.TryGetValue(normalizedAssetName, out Func<object> getNewDefaultAsset)) //if this asset has a default to load
            {
                defaultAsset = (T)getNewDefaultAsset.Invoke(); //generate a new default instance of this asset, cast it as the given type, and return it
                return true; //success
            }
            else //if this asset does NOT have a default to load
            {
                defaultAsset = default(T); //return the given type's default value (e.g. null)
                return false; //failure
            }
        }

        /// <summary>Sets a default instance generator for the named asset, which allows this class to create and manage the asset.</summary>
        /// <param name="assetName">The asset name, e.g. "Characters/Abigail".</param>
        /// <param name="getNewDefaultAsset">A method that returns a new default instance for this asset, e.g. a blank dictionary with the appropriate key/value types.</param>
        internal static void SetDefault(string assetName, Func<object> getNewDefaultAsset)
        {
            Defaults[Helper.GameContent.ParseAssetName(assetName).Name] = getNewDefaultAsset; //normalize the asset name and store the default instance
        }

        /// <summary>Checks whether this asset name has a default instance to load.</summary>
        /// <param name="normalizedAssetName">The asset's normalized name. See <see cref="IAssetInfo.AssetName"/> or <see cref="IContentHelper.NormalizeAssetName(string)"/>.</param>
        /// <returns>True if a default instance exists for this asset. False otherwise.</returns>
        internal static bool HasDefault(string normalizedAssetName)
        {
            return Defaults.ContainsKey(normalizedAssetName); //return true if the asset name has a default
        }

        /// <summary>Removes an asset from the cache, allowing a more recent version to be loaded when needed.</summary>
        /// <param name="assetName">The asset's name, e.g. "Characters/Abigail".</param>
        internal static void Invalidate(string assetName)
        {
            Cache.Remove(assetName); //remove this asset's cached version if it exists
        }

        /****************/
        /* SMAPI events */
        /****************/

        /// <summary>Clears all cached assets at the start of each in-game day.</summary>
        private static void DayStarted_ClearCache(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Cache.Clear();
        }

        /// <summary>Clears all cached assets when the local player changes location.</summary>
        private static void Warped_ClearCache(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            Cache.Clear();
        }

        /// <summary>Clears all cached assets every 10 in-game minutes.</summary>
        private static void TimeChanged_ClearCache(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            Cache.Clear();
        }

        /// <summary>Loads default instances of any new assets created by this mod.</summary>
        private static void AssetRequested_LoadDefaults(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (TryGetDefault(e.Name.Name, out object defaultAsset)) //if a default instance exists for this asset
            {
                e.LoadFrom(() => defaultAsset, StardewModdingAPI.Events.AssetLoadPriority.Medium, null);
            }
        }
    }
}