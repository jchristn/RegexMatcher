using System;
using System.Collections.Generic;
using System.Text;

namespace RegexMatcher
{
    /// <summary>
    /// Specify how evaluation for a match should behave when multiple matches exist. 
    /// </summary>
    public enum MatchPreferenceType
    {
        /// <summary>
        /// When evaluating for a match and multiple matches are identified, return the first match.
        /// </summary>
        First,
        /// <summary>
        /// When evaluating for a match and multiple matches are identified, return the longest match.
        /// </summary>
        LongestFirst,
        /// <summary>
        /// When evaluating for a match and multiple matches are identified, return the shortest match.
        /// </summary>
        ShortestFirst
    }
}
