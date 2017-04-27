using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RegexMatcher
{
    /// <summary>
    /// Library to store regular expressions with a supplied object, and return that object when evaluating an input and a matching regular expression is found.
    /// </summary>
    public class Matcher
    {
        #region Constructors-and-Factories

        /// <summary>
        /// Instantiates the object.
        /// </summary>
        public Matcher()
        {
            regexDict = new Dictionary<Regex, object>();
            dictLock = new object();
        }
         
        #endregion

        #region Public-Members

        #endregion

        #region Private-Members

        private Dictionary<Regex, object> regexDict;
        private readonly object dictLock;

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
            
            lock (dictLock)
            {
                regexDict.Add(regex, val);
            }
        }

        /// <summary>
        /// Remove a regular expression from the evaluation dictionary.
        /// </summary>
        /// <param name="regex">Regular expression.</param>
        public void Remove(Regex regex)
        {
            if (regex == null) throw new ArgumentNullException(nameof(regex));

            lock (dictLock)
            {
                if (regexDict.ContainsKey(regex)) regexDict.Remove(regex);
            }
        }

        /// <summary>
        /// Retrieve the evaluation dictionary.
        /// </summary>
        /// <returns>Dictionary containing regular expression and return value that is returned upon match.</returns>
        public Dictionary<Regex, object> Get()
        {
            lock (dictLock)
            {
                return regexDict;
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

            lock (dictLock)
            {
                if (regexDict.ContainsKey(regex)) return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a value exists in the evaluation dictionary.  Returns true on the first match of the value.
        /// </summary>
        /// <param name="val">Object to match.</param>
        /// <returns>True if found.</returns>
        public bool ValueExists(object val)
        {
            lock (dictLock)
            {
                foreach (KeyValuePair<Regex, object> curr in regexDict)
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

            lock (dictLock)
            {
                foreach (KeyValuePair<Regex, object> curr in regexDict)
                {
                    Match match = curr.Key.Match(inVal);
                    if (match.Success)
                    {
                        val = curr.Value;
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
