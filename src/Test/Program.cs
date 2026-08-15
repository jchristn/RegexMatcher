using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RegexMatcher;

namespace Test
{
    /// <summary>
    /// Automated test harness for RegexMatcher. Exits with code 0 when all tests
    /// pass and a non-zero code when any test fails.
    /// </summary>
    internal static class Program
    {
        private static int _Passed = 0;
        private static int _Failed = 0;

        private static int Main()
        {
            Console.WriteLine("RegexMatcher test harness");
            Console.WriteLine("=========================");

            // Positive cases
            TestAddAndMatch();
            TestMatchPreferenceFirst();
            TestMatchPreferenceLongestFirst();
            TestMatchPreferenceShortestFirst();
            TestExists();
            TestValueExists();
            TestRemove();
            TestGet();
            TestAllMatches();
            TestNullValueStored();

            // Negative cases
            TestNoMatch();
            TestExistsMissing();
            TestValueExistsMissing();
            TestAllMatchesNoMatches();
            TestRemoveMissingIsNoOp();
            TestAddNullRegexThrows();
            TestRemoveNullRegexThrows();
            TestMatchNullInputThrows();
            TestMatchEmptyInputThrows();
            TestAllMatchesNullThrows();
            TestAllMatchesEmptyThrows();
            TestExistsNullReturnsFalse();

            Console.WriteLine();
            Console.WriteLine("=========================");
            Console.WriteLine($"Passed: {_Passed}, Failed: {_Failed}");

            if (_Failed > 0)
            {
                Console.WriteLine("RESULT: FAILURE");
                return 1;
            }

            Console.WriteLine("RESULT: SUCCESS");
            return 0;
        }

        #region Positive-Tests

        private static void TestAddAndMatch()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo/\\d+$"), "foo with id");

            bool found = matcher.Match("/foo/42", out object val);
            Assert("Add and Match returns true on match", found);
            Assert("Add and Match returns mapped value", (string)val == "foo with id");
        }

        private static void TestMatchPreferenceFirst()
        {
            Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.First };
            matcher.Add(new Regex("^/bar/.*$"), "first");
            matcher.Add(new Regex("^/bar/\\d+$"), "second");

            bool found = matcher.Match("/bar/1", out object val);
            Assert("First preference returns first inserted match", found && (string)val == "first");
        }

        private static void TestMatchPreferenceLongestFirst()
        {
            Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.LongestFirst };
            matcher.Add(new Regex("^/a$"), "short");            // pattern length 4
            matcher.Add(new Regex("^/a.*extra$"), "unmatched"); // does not match input
            matcher.Add(new Regex("^/aaaaaaaaaa$"), "long");    // longest matching pattern

            bool found = matcher.Match("/a", out object val);
            Assert("LongestFirst returns value for longest matching pattern",
                found && (string)val == "short");

            // Two matching patterns of differing length; the longer pattern wins.
            Matcher m2 = new Matcher { MatchPreference = MatchPreferenceType.LongestFirst };
            m2.Add(new Regex("^/x/.*$"), "shorter");
            m2.Add(new Regex("^/x/[a-z]+/?$"), "longer");
            bool found2 = m2.Match("/x/abc", out object val2);
            Assert("LongestFirst picks the longer of two matching patterns",
                found2 && (string)val2 == "longer");
        }

        private static void TestMatchPreferenceShortestFirst()
        {
            Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.ShortestFirst };
            matcher.Add(new Regex("^/x/.*$"), "shorter");
            matcher.Add(new Regex("^/x/[a-z]+/?$"), "longer");

            bool found = matcher.Match("/x/abc", out object val);
            Assert("ShortestFirst picks the shorter of two matching patterns",
                found && (string)val == "shorter");
        }

        private static void TestExists()
        {
            Matcher matcher = new Matcher();
            Regex regex = new Regex("^/foo$");
            matcher.Add(regex, "foo");
            Assert("Exists returns true for added regex instance", matcher.Exists(regex));
        }

        private static void TestValueExists()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo$"), "the-value");
            Assert("ValueExists returns true for stored value", matcher.ValueExists("the-value"));
        }

        private static void TestRemove()
        {
            Matcher matcher = new Matcher();
            Regex regex = new Regex("^/foo$");
            matcher.Add(regex, "foo");
            matcher.Remove(regex);
            Assert("Remove deletes the entry", !matcher.Exists(regex));
            Assert("Match returns false after Remove", !matcher.Match("/foo", out _));
        }

        private static void TestGet()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo$"), "foo");
            matcher.Add(new Regex("^/bar$"), "bar");

            Dictionary<Regex, object> all = matcher.Get();
            Assert("Get returns all entries", all != null && all.Count == 2);
        }

        private static void TestAllMatches()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo/\\d+$"), "id");
            matcher.Add(new Regex("^/foo/.*$"), "wildcard");
            matcher.Add(new Regex("^/bar$"), "bar");

            List<object> matches = matcher.AllMatches("/foo/42");
            Assert("AllMatches returns every matching value", matches.Count == 2);
            Assert("AllMatches includes the id match", matches.Contains("id"));
            Assert("AllMatches includes the wildcard match", matches.Contains("wildcard"));
            Assert("AllMatches excludes non-matching value", !matches.Contains("bar"));
        }

        private static void TestNullValueStored()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo$"), null);

            bool found = matcher.Match("/foo", out object val);
            Assert("Match returns true when stored value is null", found);
            Assert("Match yields null value when stored value is null", val == null);
            Assert("ValueExists returns true for stored null value", matcher.ValueExists(null));
        }

        #endregion

        #region Negative-Tests

        private static void TestNoMatch()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo$"), "foo");

            bool found = matcher.Match("/nope", out object val);
            Assert("Match returns false when nothing matches", !found);
            Assert("Match yields null value when nothing matches", val == null);
        }

        private static void TestExistsMissing()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo$"), "foo");
            Assert("Exists returns false for a different regex instance",
                !matcher.Exists(new Regex("^/foo$")));
        }

        private static void TestValueExistsMissing()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo$"), "foo");
            Assert("ValueExists returns false for absent value", !matcher.ValueExists("absent"));
        }

        private static void TestAllMatchesNoMatches()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo$"), "foo");

            List<object> matches = matcher.AllMatches("/bar");
            Assert("AllMatches returns empty list when nothing matches",
                matches != null && matches.Count == 0);
        }

        private static void TestRemoveMissingIsNoOp()
        {
            Matcher matcher = new Matcher();
            matcher.Add(new Regex("^/foo$"), "foo");
            matcher.Remove(new Regex("^/missing$")); // different instance, not present
            Assert("Remove of missing regex leaves existing entries intact",
                matcher.Get().Count == 1);
        }

        private static void TestAddNullRegexThrows()
        {
            Matcher matcher = new Matcher();
            AssertThrows<ArgumentNullException>("Add(null) throws ArgumentNullException",
                () => matcher.Add(null, "x"));
        }

        private static void TestRemoveNullRegexThrows()
        {
            Matcher matcher = new Matcher();
            AssertThrows<ArgumentNullException>("Remove(null) throws ArgumentNullException",
                () => matcher.Remove(null));
        }

        private static void TestMatchNullInputThrows()
        {
            Matcher matcher = new Matcher();
            AssertThrows<ArgumentNullException>("Match(null) throws ArgumentNullException",
                () => matcher.Match(null, out _));
        }

        private static void TestMatchEmptyInputThrows()
        {
            Matcher matcher = new Matcher();
            AssertThrows<ArgumentNullException>("Match(\"\") throws ArgumentNullException",
                () => matcher.Match("", out _));
        }

        private static void TestAllMatchesNullThrows()
        {
            Matcher matcher = new Matcher();
            AssertThrows<ArgumentNullException>("AllMatches(null) throws ArgumentNullException",
                () => matcher.AllMatches(null));
        }

        private static void TestAllMatchesEmptyThrows()
        {
            Matcher matcher = new Matcher();
            AssertThrows<ArgumentNullException>("AllMatches(\"\") throws ArgumentNullException",
                () => matcher.AllMatches(""));
        }

        private static void TestExistsNullReturnsFalse()
        {
            Matcher matcher = new Matcher();
            Assert("Exists(null) returns false", !matcher.Exists(null));
        }

        #endregion

        #region Helpers

        private static void Assert(string description, bool condition)
        {
            if (condition)
            {
                _Passed++;
                Console.WriteLine($"[PASS] {description}");
            }
            else
            {
                _Failed++;
                Console.WriteLine($"[FAIL] {description}");
            }
        }

        private static void AssertThrows<TException>(string description, Action action)
            where TException : Exception
        {
            try
            {
                action();
                _Failed++;
                Console.WriteLine($"[FAIL] {description} (no exception thrown)");
            }
            catch (TException)
            {
                _Passed++;
                Console.WriteLine($"[PASS] {description}");
            }
            catch (Exception ex)
            {
                _Failed++;
                Console.WriteLine($"[FAIL] {description} (threw {ex.GetType().Name})");
            }
        }

        #endregion
    }
}
