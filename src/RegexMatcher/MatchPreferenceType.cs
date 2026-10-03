namespace RegexMatcher
{
    /// <summary>
    /// Specify how evaluation for a match should behave when multiple regular expressions match the input.
    /// </summary>
    public enum MatchPreferenceType
    {
        /// <summary>
        /// Return the value for the first matching regular expression, in the order in which entries were added.
        /// </summary>
        First,
        /// <summary>
        /// Return the value for the matching regular expression whose pattern string is longest, which typically indicates the most specific pattern.
        /// Ties are resolved in favor of the entry added first.
        /// </summary>
        LongestFirst,
        /// <summary>
        /// Return the value for the matching regular expression whose pattern string is shortest, which typically indicates the least specific pattern.
        /// Ties are resolved in favor of the entry added first.
        /// </summary>
        ShortestFirst
    }
}
