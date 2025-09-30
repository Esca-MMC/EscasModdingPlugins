using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;

namespace EscasModdingPlugins
{
    /// <summary>A Content Patcher token that accepts a game state query and outputs the result.</summary>
    public class GameStateQueryToken
    {
        /* Subclasses */

        /// <summary>The value type used in <see cref="QueryResultsCache"/>.</summary>
        private class CacheData
        {
            public CacheData(bool dirty, bool result)
            {
                Dirty = dirty;
                Result = result;
            }
            /// <summary>If true, the result has not been updated for the current context, and should be re-checked before use.</summary>
            public bool Dirty { get; set; }
            /// <summary>The most recent result of a game state query (GSQ) when checked.</summary>
            public bool Result { get; set; }
        }

        /* Private fields */

        /// <summary>A set of game state queries and results. If a specific query has already been checked since the last context update, the result will be cached here as the value.</summary>
        private PerScreen<Dictionary<string, CacheData>> QueryResultsCache { get; set; } = new(() => new Dictionary<string, CacheData>());

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

        /// <summary>Get whether the token always chooses from a set of known values for the given input. Mutually exclusive with <see cref="HasBoundedRangeValues"/>.</summary>
        /// <param name="input">The input arguments, if any.</param>
        /// <param name="allowedValues">The possible values for the input.</param>
        /// <remarks>Default unrestricted.</remarks>
        public bool HasBoundedValues(string input, out IEnumerable<string> allowedValues)
        {
            allowedValues = new string[2] { "True", "False" };
            return true; //yes, this has bounded values
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
                error = "GameStateQuery input was null or blank.";
                return false;
            }

            if (QueryResultsCache.Value.ContainsKey(input)) //if this input was already validated
            {
                error = null;
                return true;
            }

            foreach (var parsedQueryComponent in GameStateQuery.Parse(input)) //try to parse the input into an array of queries
            {
                if (parsedQueryComponent.Error != null) //if a parsing error occurred
                {
                    error = $"GameStateQuery input could not be parsed: {parsedQueryComponent.Error}";
                    return false;
                }
            }

            error = null;
            return true;
        }

        /** State methods **/

        /// <summary>Update the values when the context changes.</summary>
        /// <returns>Returns whether the value changed, which may trigger patch updates.</returns>
        public bool UpdateContext()
        {
            bool anyResultsChanged = false;

            foreach (var query in QueryResultsCache.Value.Keys)
            {
                if (anyResultsChanged)
                    QueryResultsCache.Value[query].Dirty = true;
                else
                {
                    bool newResult = GameStateQuery.CheckConditions(query);
                    if (QueryResultsCache.Value[query].Result != newResult)
                    {
                        anyResultsChanged = true;
                        QueryResultsCache.Value[query].Result = newResult;
                    }
                    QueryResultsCache.Value[query].Dirty = false;
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
                yield return "False";
            else
            {
                if (!QueryResultsCache.Value.ContainsKey(input) || QueryResultsCache.Value[input].Dirty)
                {
                    bool newResult = GameStateQuery.CheckConditions(input);
                    QueryResultsCache.Value[input] = new(false, newResult);
                }
                yield return QueryResultsCache.Value[input].Result.ToString(); //return the query result as "True" or "False"
            }
        }
    }
}