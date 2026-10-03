namespace RegexMatcher
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Library to store regular expressions with a supplied object, and return that object when evaluating an input and a matching regular expression is found.
    /// Entries are evaluated in the order in which they were added.
    /// All methods are thread-safe.
    /// </summary>
    public class Matcher
    {
        #region Public-Members

        /// <summary>
        /// Specify how a match is selected when multiple regular expressions match the input.
        /// Default is MatchPreferenceType.First.
        /// </summary>
        public MatchPreferenceType MatchPreference = MatchPreferenceType.First;

        #endregion

        #region Private-Members

        private readonly List<KeyValuePair<Regex, object>> _Entries = new List<KeyValuePair<Regex, object>>();
        private readonly HashSet<Regex> _Keys = new HashSet<Regex>();
        private readonly object _Lock = new object();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public Matcher()
        {
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Add a regular expression and return value to the end of the evaluation list.
        /// Regular expressions are identified by instance; two distinct Regex instances with the same pattern are separate entries.
        /// </summary>
        /// <param name="regex">Regular expression.  Must not be null.</param>
        /// <param name="val">Value to return when a match is found.  May be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when regex is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the same Regex instance has already been added.</exception>
        public void Add(Regex regex, object val)
        {
            if (regex == null) throw new ArgumentNullException(nameof(regex));

            lock (_Lock)
            {
                if (!_Keys.Add(regex))
                    throw new ArgumentException("The supplied Regex instance with pattern '" + regex.ToString() + "' has already been added.", nameof(regex));

                _Entries.Add(new KeyValuePair<Regex, object>(regex, val));
            }
        }

        /// <summary>
        /// Remove a regular expression from the evaluation list.  No action is taken if the Regex instance is not present.
        /// </summary>
        /// <param name="regex">Regular expression.  Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when regex is null.</exception>
        public void Remove(Regex regex)
        {
            if (regex == null) throw new ArgumentNullException(nameof(regex));

            lock (_Lock)
            {
                if (!_Keys.Remove(regex)) return;

                for (int i = 0; i < _Entries.Count; i++)
                {
                    if (Object.ReferenceEquals(_Entries[i].Key, regex))
                    {
                        _Entries.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve a snapshot copy of the evaluation dictionary.
        /// Changes made to the returned dictionary do not affect the matcher, and the snapshot is safe to enumerate while other threads modify the matcher.
        /// </summary>
        /// <returns>Dictionary containing each regular expression and the value returned upon match.  Never null.</returns>
        public Dictionary<Regex, object> Get()
        {
            lock (_Lock)
            {
                Dictionary<Regex, object> ret = new Dictionary<Regex, object>(_Entries.Count);
                foreach (KeyValuePair<Regex, object> entry in _Entries) ret.Add(entry.Key, entry.Value);
                return ret;
            }
        }

        /// <summary>
        /// Check if a regular expression instance exists in the evaluation list.
        /// </summary>
        /// <param name="regex">Regular expression.  Null returns false.</param>
        /// <returns>True if found.</returns>
        public bool Exists(Regex regex)
        {
            if (regex == null) return false;

            lock (_Lock)
            {
                return _Keys.Contains(regex);
            }
        }

        /// <summary>
        /// Retrieve the values for every regular expression that matches the input, in the order in which they were added.
        /// MatchPreference does not affect this method.
        /// </summary>
        /// <param name="val">The string to evaluate.  Must not be null or empty.</param>
        /// <returns>List of values.  Empty if no regular expression matches.  Never null.</returns>
        /// <exception cref="ArgumentNullException">Thrown when val is null or empty.</exception>
        /// <exception cref="RegexMatchTimeoutException">Thrown when a regular expression with a timeout exceeds it.</exception>
        public List<object> AllMatches(string val)
        {
            if (String.IsNullOrEmpty(val)) throw new ArgumentNullException(nameof(val));

            List<object> vals = new List<object>();

            lock (_Lock)
            {
                foreach (KeyValuePair<Regex, object> entry in _Entries)
                {
                    if (entry.Key.IsMatch(val)) vals.Add(entry.Value);
                }
            }

            return vals;
        }

        /// <summary>
        /// Check if a value exists in the evaluation list.  Values are compared using Object.Equals, so equal strings and boxed value types are found.
        /// </summary>
        /// <param name="val">Object to match.  May be null, in which case true is returned if any entry has a null value.</param>
        /// <returns>True if found.</returns>
        public bool ValueExists(object val)
        {
            lock (_Lock)
            {
                foreach (KeyValuePair<Regex, object> entry in _Entries)
                {
                    if (Object.Equals(entry.Value, val)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Evaluate the supplied string against the evaluation list.
        /// When multiple regular expressions match, MatchPreference determines which value is returned.
        /// </summary>
        /// <param name="inVal">The string to evaluate.  Must not be null or empty.</param>
        /// <param name="val">The object value mapped to the selected regular expression, or null if no match was found.</param>
        /// <returns>True if a match was found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when inVal is null or empty.</exception>
        /// <exception cref="RegexMatchTimeoutException">Thrown when a regular expression with a timeout exceeds it.</exception>
        public bool Match(string inVal, out object val)
        {
            if (String.IsNullOrEmpty(inVal)) throw new ArgumentNullException(nameof(inVal));
            val = null;

            lock (_Lock)
            {
                MatchPreferenceType pref = MatchPreference;
                Regex bestMatch = null;

                foreach (KeyValuePair<Regex, object> entry in _Entries)
                {
                    if (!entry.Key.IsMatch(inVal)) continue;

                    if (pref == MatchPreferenceType.First)
                    {
                        val = entry.Value;
                        return true;
                    }

                    if (bestMatch == null
                        || (pref == MatchPreferenceType.LongestFirst && entry.Key.ToString().Length > bestMatch.ToString().Length)
                        || (pref == MatchPreferenceType.ShortestFirst && entry.Key.ToString().Length < bestMatch.ToString().Length))
                    {
                        bestMatch = entry.Key;
                        val = entry.Value;
                    }
                }

                return bestMatch != null;
            }
        }

        #endregion
    }
}
