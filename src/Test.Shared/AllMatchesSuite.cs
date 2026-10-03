namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using RegexMatcher;
    using Touchstone.Core;

    /// <summary>
    /// Tests for Matcher.AllMatches.
    /// </summary>
    public static class AllMatchesSuite
    {
        private const string _SuiteId = "AllMatches";

        /// <summary>
        /// Create the suite descriptor.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "AllMatches",
                cases: new List<TestCaseDescriptor>
                {
                    // Positive
                    TestHelpers.Case(_SuiteId, "ReturnsEveryMatch", "AllMatches returns every matching value and excludes non-matches", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo/\\d+$"), "id");
                        matcher.Add(new Regex("^/foo/.*$"), "wildcard");
                        matcher.Add(new Regex("^/bar$"), "bar");
                        List<object> matches = matcher.AllMatches("/foo/42");
                        TestHelpers.Equal(2, matches.Count, "Match count");
                        TestHelpers.True(matches.Contains("id"), "Missing id");
                        TestHelpers.True(matches.Contains("wildcard"), "Missing wildcard");
                        TestHelpers.False(matches.Contains("bar"), "Contains bar");
                    }),

                    TestHelpers.Case(_SuiteId, "PreservesInsertionOrder", "AllMatches returns values in insertion order", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("a"), "1");
                        matcher.Add(new Regex("b"), "2");
                        matcher.Add(new Regex("c"), "3");
                        List<object> matches = matcher.AllMatches("abc");
                        TestHelpers.Equal(3, matches.Count, "Match count");
                        TestHelpers.Equal("1", matches[0], "Index 0");
                        TestHelpers.Equal("2", matches[1], "Index 1");
                        TestHelpers.Equal("3", matches[2], "Index 2");
                    }),

                    TestHelpers.Case(_SuiteId, "IncludesNullValues", "AllMatches includes null stored values", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), null!);
                        matcher.Add(new Regex("foo"), "foo");
                        List<object> matches = matcher.AllMatches("/foo");
                        TestHelpers.Equal(2, matches.Count, "Match count");
                        TestHelpers.True(matches.Contains(null!), "Missing null value");
                    }),

                    TestHelpers.Case(_SuiteId, "IncludesDuplicateValues", "AllMatches returns a shared value once per matching regex", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "shared");
                        matcher.Add(new Regex("foo"), "shared");
                        List<object> matches = matcher.AllMatches("/foo");
                        TestHelpers.Equal(2, matches.Count, "Match count");
                    }),

                    TestHelpers.Case(_SuiteId, "IgnoresMatchPreference", "AllMatches returns every match regardless of MatchPreference", () =>
                    {
                        foreach (MatchPreferenceType pref in (MatchPreferenceType[])Enum.GetValues(typeof(MatchPreferenceType)))
                        {
                            Matcher matcher = new Matcher { MatchPreference = pref };
                            matcher.Add(new Regex("^/x/.*$"), "shorter");
                            matcher.Add(new Regex("^/x/[a-z]+/?$"), "longer");
                            TestHelpers.Equal(2, matcher.AllMatches("/x/abc").Count, "Match count for " + pref);
                        }
                    }),

                    TestHelpers.Case(_SuiteId, "ReturnsNewListEachCall", "AllMatches returns an independent list on each call", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        List<object> first = matcher.AllMatches("/foo");
                        first.Clear();
                        List<object> second = matcher.AllMatches("/foo");
                        TestHelpers.False(Object.ReferenceEquals(first, second), "Same list instance returned");
                        TestHelpers.Equal(1, second.Count, "Second call match count");
                    }),

                    TestHelpers.Case(_SuiteId, "ReflectsRemove", "AllMatches excludes removed regexes", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex foo = new Regex("foo");
                        matcher.Add(foo, "foo");
                        matcher.Add(new Regex("^/foo$"), "exact");
                        matcher.Remove(foo);
                        List<object> matches = matcher.AllMatches("/foo");
                        TestHelpers.Equal(1, matches.Count, "Match count");
                        TestHelpers.Equal("exact", matches[0], "Remaining value");
                    }),

                    // Negative
                    TestHelpers.Case(_SuiteId, "NoMatchesReturnsEmpty", "AllMatches returns an empty list when nothing matches", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        List<object> matches = matcher.AllMatches("/bar");
                        TestHelpers.True(matches != null, "Returned null");
                        TestHelpers.Equal(0, matches!.Count, "Match count");
                    }),

                    TestHelpers.Case(_SuiteId, "EmptyMatcherReturnsEmpty", "AllMatches on an empty matcher returns an empty list", () =>
                    {
                        Matcher matcher = new Matcher();
                        List<object> matches = matcher.AllMatches("/foo");
                        TestHelpers.True(matches != null, "Returned null");
                        TestHelpers.Equal(0, matches!.Count, "Match count");
                    }),

                    TestHelpers.Case(_SuiteId, "NullInputThrows", "AllMatches with null input throws ArgumentNullException", () =>
                    {
                        Matcher matcher = new Matcher();
                        ArgumentNullException ex = TestHelpers.Throws<ArgumentNullException>(
                            () => matcher.AllMatches(null!), "AllMatches(null)");
                        TestHelpers.Equal("val", ex.ParamName, "ParamName");
                    }),

                    TestHelpers.Case(_SuiteId, "EmptyInputThrows", "AllMatches with empty input throws ArgumentNullException", () =>
                    {
                        Matcher matcher = new Matcher();
                        ArgumentNullException ex = TestHelpers.Throws<ArgumentNullException>(
                            () => matcher.AllMatches(""), "AllMatches(\"\")");
                        TestHelpers.Equal("val", ex.ParamName, "ParamName");
                    })
                });
        }
    }
}
