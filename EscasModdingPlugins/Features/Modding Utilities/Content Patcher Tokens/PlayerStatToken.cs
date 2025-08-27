using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EscasModdingPlugins
{
    /// <summary>A Content Patcher token that accepts a player stat key and outputs the stat's current value.</summary>
    /// <remarks>This token targets the current local player, i.e. <see cref="Game1.player"/>.</remarks>
    public class PlayerStatToken
    {
        /* Private fields */

        /// <summary>A set of token inputs and outputs for the most recent context update.</summary>
        private PerScreen<Dictionary<string, uint>> InputOutputCache { get; set; } = new PerScreen<Dictionary<string, uint>>(() => new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase));

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

        /** State methods **/

        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public bool UpdateContext()
        {
            bool anyResultsChanged = false;

            foreach (string key in InputOutputCache.Value.Keys.ToList()) //loop through a separate list to allow value changes
            {
                uint newValue = Game1.player.stats.Get(key);
                if (InputOutputCache.Value[key] != newValue)
                {
                    anyResultsChanged = true;
                    InputOutputCache.Value[key] = newValue;
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
                yield return "0";
            else
            {
                if (InputOutputCache.Value.ContainsKey(input) == false)
                    InputOutputCache.Value[input] = Game1.player.stats.Get(input); //note: the underlying stats dictionary should be case-insensitive

                yield return InputOutputCache.Value[input].ToString();
            }

        }
    }
}