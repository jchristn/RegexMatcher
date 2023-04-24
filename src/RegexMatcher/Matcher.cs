using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

namespace RegexMatcher
{
    /// <summary>
    /// Library to store regular expressions with a supplied object, and return that object when evaluating an input and a matching regular expression is found.
    /// </summary>
    public class Matcher
    {
        #region Public-Members

        /// <summary>
        /// Specify which sorting mode should be used when evaluating for a match.
        /// </summary>
        public MatchPreferenceType MatchPreference = MatchPreferenceType.First;

        #endregion

        #region Private-Members

        private Dictionary<Regex, object> _RegexDict = new Dictionary<Regex, object>();
        private readonly object _RegexDictLock = new object();

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
        /// Add a regular expression and return value to the evaluation dictionary.
        /// </summary>
        /// <param name="regex">Regular expression.</param>
        /// <param name="val">Value to return when a match is found.</param>
        public void Add(Regex regex, object val)
        {
            if (regex == null) throw new ArgumentNullException(nameof(regex));

            lock (_RegexDictLock)
            {
                _RegexDict.Add(regex, val);
            }
        }

        /// <summary>
        /// Remove a regular expression from the evaluation dictionary.
        /// </summary>
        /// <param name="regex">Regular expression.</param>
        public void Remove(Regex regex)
        {
            if (regex == null) throw new ArgumentNullException(nameof(regex));

            lock (_RegexDictLock)
            {
                if (_RegexDict.ContainsKey(regex)) _RegexDict.Remove(regex);
            }
        }

        /// <summary>
        /// Retrieve the evaluation dictionary.
        /// </summary>
        /// <returns>Dictionary containing regular expression and return value that is returned upon match.</returns>
        public Dictionary<Regex, object> Get()
        {
            lock (_RegexDictLock)
            {
                return _RegexDict;
            }
        }

        /// <summary>
        /// Check if a regular expression exists in the evaluation dictionary.
        /// </summary>
        /// <param name="regex">Regular expression.</param>
        /// <returns>True if found.</returns>
        public bool Exists(Regex regex)
        {
            if (regex == null) return false;

            lock (_RegexDictLock)
            {
                if (_RegexDict.ContainsKey(regex)) return true;
            }

            return false;
        }

        /// <summary>
        /// Extract all matches for a given input.
        /// </summary>
        /// <returns>List of values.</returns>
        public List<object> AllMatches(string val)
        {
            if (String.IsNullOrEmpty(val)) throw new ArgumentNullException(nameof(val));

            var vals = new List<object>();

            lock (_RegexDictLock)
            {

                foreach (KeyValuePair<Regex, object> curr in _RegexDict)
                {
                    Match match = curr.Key.Match(val);
                    if (match.Success)
                    {
                        vals.Add(curr.Value);
                    }
                }

                return vals;
            }
        }

        /// <summary>
        /// Check if a value exists in the evaluation dictionary.  Returns true on the first match of the value.
        /// </summary>
        /// <param name="val">Object to match.</param>
        /// <returns>True if found.</returns>
        public bool ValueExists(object val)
        {
            lock (_RegexDictLock)
            {
                foreach (KeyValuePair<Regex, object> curr in _RegexDict)
                {
                    if (curr.Value == val) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Evaluate the supplied string against the evaluation dictionary.
        /// </summary>
        /// <param name="inVal">The string to evaluate.</param>
        /// <param name="val">The object value mapped to the regular expression in the evaluation dictionary.</param>
        /// <returns>True if a match was found.</returns>
        public bool Match(string inVal, out object val)
        {
            if (String.IsNullOrEmpty(inVal)) throw new ArgumentNullException(nameof(inVal));
            val = null;

            lock (_RegexDictLock)
            {
                Regex bestMatch = null;

                foreach (KeyValuePair<Regex, object> curr in _RegexDict)
                {
                    Match match = curr.Key.Match(inVal);
                    if (match.Success)
                    {
                        if (MatchPreference == MatchPreferenceType.First)
                        {
                            val = curr.Value;
                            return true;
                        }

                        if (bestMatch == null)
                        {
                            bestMatch = curr.Key;
                            val = curr.Value;
                        }
                        else
                        {
                            string regex = curr.Key.ToString();

                            if (MatchPreference == MatchPreferenceType.LongestFirst)
                            {
                                if (curr.Key.ToString().Length > bestMatch.ToString().Length)
                                {
                                    bestMatch = curr.Key;
                                    val = curr.Value;
                                }
                            }
                            else if (MatchPreference == MatchPreferenceType.ShortestFirst)
                            {
                                if (curr.Key.ToString().Length < bestMatch.ToString().Length)
                                {
                                    bestMatch = curr.Key;
                                    val = curr.Value;
                                }
                            }
                        }
                    }
                }

                if (bestMatch != null) return true;
            }

            return false;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
