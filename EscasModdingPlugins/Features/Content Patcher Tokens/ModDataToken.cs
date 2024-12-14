using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace EscasModdingPlugins
{
    /// <summary>A Content Patcher token that reads text from a given target's modData.</summary>
    public class ModDataToken
    {
        /* Private fields */

        /// <summary>A set of token inputs and outputs for the most recent context update.</summary>
        private PerScreen<Dictionary<string, string>> InputOutputCache { get; set; } = new PerScreen<Dictionary<string, string>>(() => new Dictionary<string, string>());

        /* Public methods */

        /** Metadata methods **/

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
            return false;
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
                error = "ModData input was null or blank.";
                return false;
            }

            if (InputOutputCache.Value.ContainsKey(input)) //if this input was already validated
            {
                //stop early, since it's already been checked
                error = null;
                return true;
            }

            if (!TryParseInput(input, out _, out _, out error)) //if the input was invalid
            {
                return false;
            }

            InputOutputCache.Value.Add(input, null); //add this input to the cache (its value will be updated elsewhere)

            error = null;
            return true;
        }

        /** State methods **/

        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public bool UpdateContext()
        {
            bool anyResultsChanged = false;

            foreach (var cachedInput in InputOutputCache.Value.Keys.ToList()) //for each key in the cache
            {
                TryParseInput(cachedInput, out string modDataTarget, out string modDataKey, out _); //parse cached input into arguments; ignore errors, because it's been parsed before

                string newValue = null;
                if (modDataTarget == "farm")
                {
                    Game1.getFarm()?.modData.TryGetValue(modDataKey, out newValue); //get modData from the Farm location
                }
                else if (modDataTarget == "player")
                {
                    Game1.player?.modData.TryGetValue(modDataKey, out newValue); //get modData from the current player
                }

                if (InputOutputCache.Value[cachedInput] != newValue) //if the cached value doesn't match the new value
                {
                    anyResultsChanged = true;
                    InputOutputCache.Value[cachedInput] = newValue; //update the cache
                }
            }

            return anyResultsChanged;
        }

        /// <summary>Get whether the token is available for use.</summary>
        public bool IsReady()
        {
            return Context.IsWorldReady;
        }

        /// <summary>Get the current values.</summary>
        /// <param name="input">The input arguments, if any.</param>
        public IEnumerable<string> GetValues(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                yield return ""; //return blank if null
            }
            else
            {
                if (InputOutputCache.Value.TryGetValue(input, out string modDataValue))
                    yield return modDataValue ?? ""; //return the data, or blank if it's null
                else
                    yield return ""; //return blank if the input hasn't been cached for some reason
            }
        }

        /* Private methods */

        /// <summary>Tries to validate and parse this token's input.</summary>
        /// <param name="input">The token's input arguments as a space-separated string.</param>
        /// <param name="target">The target instance from which to read modData. Null if parsing failed.</param>
        /// <param name="key">The modData key to read. Null if parsing failed.</param>
        /// <param name="error">An error message about invalid input. Null if parsing succeeded.</param>
        /// <returns>True if parsing succeeded.</returns>
        private bool TryParseInput(string input, out string target, out string key, out string error)
        {
            target = null;
            key = null;
            error = null;

            //try to parse args from input
            string[] splitInput = ArgUtility.SplitBySpace(input, 2);
            if (!ArgUtility.TryGet(splitInput, 0, out target, out error, false, "Target Instance") || !ArgUtility.TryGet(splitInput, 1, out key, out error, false, "ModData Key"))
            {
                return false;
            }

            //validate "target" arg
            if (string.Equals(target, "farm", System.StringComparison.OrdinalIgnoreCase))
            {
                target = "farm"; //output in expected format, e.g. not upper-case
                return true;
            }
            else if (string.Equals(target, "player", System.StringComparison.OrdinalIgnoreCase))
            {
                target = "player";
                return true;
            }
            else
            {
                error = "ModData field 'Key' was not a recognized value. Expected values: Farm, Player";
                return false;
            }
        }
    }
}