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
        private static readonly Dictionary<string, Func<object>> Defaults = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>A set of asset names and their most recently updated instances.</summary>
        private static readonly Dictionary<string, object> Cache = new(StringComparer.OrdinalIgnoreCase);
        /// <summary>This mod's SMAPI helper instance.</summary>
        private static IModHelper Helper = null;
        /// <summary>A lock used to prevent multiple threads simultaneously loading data.</summary>
        private static readonly object LoadLock = new();

        /******************/
        /* Public methods */
        /******************/

        /// <summary>Perform required setup tasks for this class.</summary>
        /// <param name="helper">This mod's SMAPI helper instance.</param>
        internal static void Initialize(IModHelper helper)
        {
            if (Initialized)
                return;

            //store args
            Helper = helper;

            //enable SMAPI events
            helper.Events.Content.AssetRequested += AssetRequested_LoadDefaults;
            helper.Events.Content.AssetsInvalidated += Content_AssetsInvalidated;

            Initialized = true;
        }

        /// <summary>Get the most recent version of a game asset. Automatically uses a cache system when possible.</summary>
        /// <typeparam name="T">The asset's type.</typeparam>
        /// <param name="assetName">The asset's name, e.g. "Characters/Abigail".</param>
        /// <returns>The most recent version of the asset.</returns>
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

        /// <summary>Get the default instance of the named asset if one is available.</summary>
        /// <typeparam name="T">The asset's type.</typeparam>
        /// <param name="assetName">The asset's name, e.g. "Characters/Abigail".</param>
        /// <param name="defaultAsset">A default instance of the asset.</param>
        /// <returns>True if a default instance exists for this asset. False otherwise.</returns>
        internal static bool TryGetDefault<T>(string assetName, out T defaultAsset)
        {
            if (Defaults.TryGetValue(assetName, out Func<object> getNewDefaultAsset)) //if this asset has a default to load
            {
                defaultAsset = (T)getNewDefaultAsset.Invoke(); //generate a new default instance of this asset, cast it as the given type, and return it
                return true; //success
            }
            else //if this asset does NOT have a default to load
            {
                defaultAsset = default; //return the given type's default value (e.g. null)
                return false; //failure
            }
        }

        /// <summary>Set a default instance generator for the named asset, which allows this class to create and manage the asset.</summary>
        /// <param name="assetName">The asset name, e.g. "Characters/Abigail".</param>
        /// <param name="getNewDefaultAsset">A method that returns a new default instance for this asset, e.g. a blank dictionary with the appropriate key/value types.</param>
        internal static void SetDefault(string assetName, Func<object> getNewDefaultAsset)
            => Defaults[assetName] = getNewDefaultAsset; //normalize the asset name and store the default instance

        /// <summary>Check whether this asset name has a default instance to load.</summary>
        /// <param name="assetName">The asset's name, e.g. "Characters/Abigail".</param>
        /// <returns>True if a default instance exists for this asset. False otherwise.</returns>
        internal static bool HasDefault(string assetName) => Defaults.ContainsKey(assetName);

        /// <summary>Remove an asset from the cache, if applicable, allowing a more recent version to be loaded when needed.</summary>
        /// <param name="assetName">The asset's name, e.g. "Characters/Abigail".</param>
        /// <returns>True if the asset had a cached version that was removed. False if the asset was not currently cached.</returns>
        internal static bool Invalidate(string assetName) => Cache.Remove(assetName);

        /****************/
        /* SMAPI events */
        /****************/

        /// <summary>Load default instances of any new assets created by this mod.</summary>
        private static void AssetRequested_LoadDefaults(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (TryGetDefault(e.Name.BaseName, out object defaultAsset)) //if a default instance exists for this asset
            {
                e.LoadFrom(() => defaultAsset, StardewModdingAPI.Events.AssetLoadPriority.Medium, null);
            }
        }

        /// <summary>Clear cached assets whenever they're invalidated in the game's content system.</summary>
        private static void Content_AssetsInvalidated(object sender, StardewModdingAPI.Events.AssetsInvalidatedEventArgs e)
        {
            foreach (IAssetName name in e.Names)
                Invalidate(name.BaseName);
        }
    }
}