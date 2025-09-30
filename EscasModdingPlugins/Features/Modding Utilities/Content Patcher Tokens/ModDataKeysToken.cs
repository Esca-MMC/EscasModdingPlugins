using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EscasModdingPlugins
{
    /// <summary>A Content Patcher token that checks which keys exist in a target's modData.</summary>
    public class ModDataKeysToken
    {
        /******************/
        /* Private fields */
        /******************/

        /// <summary>A set of token inputs and outputs for the most recent context update.</summary>
        private PerScreen<Dictionary<string, HashSet<string>>> InputOutputCache { get; set; } = new(() => new());

        /******************/
        /* Public methods */
        /******************/

        /// <summary>Get whether the token allows input arguments (e.g. an NPC name for a relationship token).</summary>
        public bool AllowsInput()
        {
            return true;
        }

        /// <summary>Whether the token requires input arguments to work, and does not provide values without it (see <see cref="AllowsInput"/>).</summary>
        public bool RequiresInput()
        {
            return true;
        }

        /// <summary>Whether the token may return multiple values for the given input.</summary>
        /// <param name="input">The input arguments, if applicable.</param>
        public bool CanHaveMultipleValues(string input = null)
        {
            return true;
        }

        /// <summary>Validate that the provided input arguments are valid.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="error">The validation error, if any.</param>
        /// <returns>Returns whether validation succeeded.</returns>
        /// <remarks>Default true.</remarks>
        public bool TryValidateInput(string input, out string error)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                error = "ModDataKeys input was null or blank.";
                return false;
            }

            if (InputOutputCache.Value.ContainsKey(input)) //if this input was already validated
            {
                error = null;
                return true;
            }

            if (!TryParseInput(input, out string _, out error)) //if this input is invalid
            {
                return false;
            }

            InputOutputCache.Value.Add(input, null); //add the parsed input to the cache (its value will be updated elsewhere)

            error = null;
            return true;
        }

        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public bool UpdateContext()
        {
            bool anyResultsChanged = false;

            foreach (var cachedInput in InputOutputCache.Value.Keys)
            {
                TryParseInput(cachedInput, out string modDataTarget, out _); //parse cached input into arguments; ignore errors, because it's been validated before

                //get the target instance to check its mod data
                IHaveModData targetInstance = null;
                if (modDataTarget == "farm")
                {
                    if (Context.IsWorldReady)
                        targetInstance = Game1.getFarm();
                    else
                        targetInstance = SaveGame.loaded?.locations?.FirstOrDefault((loc) => loc?.Name == "Farm"); //try to get the farm while the game is still loading (null if not found)
                }
                else if (modDataTarget == "player")
                {
                    if (Context.IsWorldReady)
                        targetInstance = Game1.player;
                    else
                        targetInstance = SaveGame.loaded?.player;
                }

                HashSet<string> newValue = targetInstance?.modData?.Keys.ToHashSet() ?? null; //get the new output value for this input (the target's modData keys) or null if unavailable
                HashSet<string> oldValue = InputOutputCache.Value[cachedInput];

                if (newValue != null && oldValue != null && !newValue.SetEquals(oldValue)) //if both values exist, but their contents do NOT match
                {
                    anyResultsChanged = true;
                    InputOutputCache.Value[cachedInput] = newValue;
                }
                else if (newValue != oldValue) //if only one of the values is null
                {
                    anyResultsChanged = true;
                    InputOutputCache.Value[cachedInput] = newValue;
                }
            }

            return anyResultsChanged;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public bool IsReady()
        {
            if (Context.IsWorldReady)
                return true;

            //if the world isn't ready yet, but all modData targets are available, return true
            if (SaveGame.loaded?.player?.modData != null && SaveGame.loaded?.locations != null) //if the player and locations exist
            {
                foreach (GameLocation location in SaveGame.loaded.locations)
                {
                    if (location?.Name == "Farm" && location.modData != null) //if the farm and its modData exist
                        return true;
                }
            }

            return false;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if any.</param>
        public IEnumerable<string> GetValues(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                yield break; //return nothing if the input is invalid
            }
            else
            {
                if (InputOutputCache.Value.TryGetValue(input, out HashSet<string> modDataKeys) && modDataKeys != null) //if a non-null value exists for this input
                {
                    foreach (string key in modDataKeys.OrderBy((key) => key, StringComparer.Ordinal)) //for each modData key in alphabetical order
                        yield return key;
                }
                else
                    yield break; //return nothing if the input hasn't been cached for some reason
            }
        }

        /*******************/
        /* Private methods */
        /*******************/

        /// <summary>Tries to validate and parse this token's input.</summary>
        /// <param name="input">The token's input arguments as a space-separated string.</param>
        /// <param name="target">The target instance to check for modData keys. Null if parsing failed.</param>
        /// <param name="error">An error message about invalid input. Null if parsing succeeded.</param>
        /// <returns>True if parsing succeeded.</returns>
        private bool TryParseInput(string input, out string target, out string error)
        {
            target = null;
            error = null;

            string cleanInput = input?.Trim();

            //validate "target" arg
            if (string.Equals(cleanInput, "farm", StringComparison.OrdinalIgnoreCase))
            {
                target = "farm"; //output in expected format, e.g. lower case
                return true;
            }
            else if (string.Equals(cleanInput, "player", StringComparison.OrdinalIgnoreCase))
            {
                target = "player";
                return true;
            }
            else
            {
                error = "ModData field 'Target' was not a recognized value. Expected values: Farm, Player";
                return false;
            }
        }
    }
}