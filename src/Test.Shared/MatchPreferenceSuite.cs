namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using RegexMatcher;
    using Touchstone.Core;

    /// <summary>
    /// Tests for Matcher.MatchPreference behavior when multiple regexes match.
    /// Note: LongestFirst and ShortestFirst compare the length of the regex pattern string, not the matched text.
    /// </summary>
    public static class MatchPreferenceSuite
    {
        private const string _SuiteId = "MatchPreference";

        /// <summary>
        /// Create the suite descriptor.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Match Preference",
                cases: new List<TestCaseDescriptor>
                {
                    // Positive
                    TestHelpers.Case(_SuiteId, "DefaultIsFirst", "Default MatchPreference is First", () =>
                    {
                        Matcher matcher = new Matcher();
                        TestHelpers.Equal(MatchPreferenceType.First, matcher.MatchPreference, "Default preference");
                    }),

                    TestHelpers.Case(_SuiteId, "EnumValues", "MatchPreferenceType defines First, LongestFirst, and ShortestFirst", () =>
                    {
                        MatchPreferenceType[] values = (MatchPreferenceType[])Enum.GetValues(typeof(MatchPreferenceType));
                        TestHelpers.Equal(3, values.Length, "Enum value count");
                        TestHelpers.Equal(0, (int)MatchPreferenceType.First, "First ordinal");
                        TestHelpers.Equal(1, (int)MatchPreferenceType.LongestFirst, "LongestFirst ordinal");
                        TestHelpers.Equal(2, (int)MatchPreferenceType.ShortestFirst, "ShortestFirst ordinal");
                    }),

                    TestHelpers.Case(_SuiteId, "FirstReturnsFirstInserted", "First returns the first inserted matching regex", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.First };
                        matcher.Add(new Regex("^/bar/.*$"), "first");
                        matcher.Add(new Regex("^/bar/\\d+$"), "second");
                        TestHelpers.True(matcher.Match("/bar/1", out object val), "Match returned false");
                        TestHelpers.Equal("first", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "FirstSkipsNonMatching", "First skips earlier non-matching regexes", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.First };
                        matcher.Add(new Regex("^/foo$"), "foo");
                        matcher.Add(new Regex("^/bar$"), "bar");
                        TestHelpers.True(matcher.Match("/bar", out object val), "Match returned false");
                        TestHelpers.Equal("bar", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "LongestPicksLonger", "LongestFirst picks the longer of two matching patterns", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.LongestFirst };
                        matcher.Add(new Regex("^/x/.*$"), "shorter");
                        matcher.Add(new Regex("^/x/[a-z]+/?$"), "longer");
                        TestHelpers.True(matcher.Match("/x/abc", out object val), "Match returned false");
                        TestHelpers.Equal("longer", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "LongestPicksLongerRegardlessOfOrder", "LongestFirst picks the longer pattern when it is inserted first", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.LongestFirst };
                        matcher.Add(new Regex("^/x/[a-z]+/?$"), "longer");
                        matcher.Add(new Regex("^/x/.*$"), "shorter");
                        TestHelpers.True(matcher.Match("/x/abc", out object val), "Match returned false");
                        TestHelpers.Equal("longer", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "LongestIgnoresNonMatching", "LongestFirst ignores longer patterns that do not match", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.LongestFirst };
                        matcher.Add(new Regex("^/a$"), "short");
                        matcher.Add(new Regex("^/a.*extra$"), "unmatched");
                        matcher.Add(new Regex("^/aaaaaaaaaa$"), "long");
                        TestHelpers.True(matcher.Match("/a", out object val), "Match returned false");
                        TestHelpers.Equal("short", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "LongestOfThree", "LongestFirst picks the longest of three matching patterns", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.LongestFirst };
                        matcher.Add(new Regex("^/x/.*$"), "medium");
                        matcher.Add(new Regex("/x"), "shortest");
                        matcher.Add(new Regex("^/x/[a-z]{1,10}/?$"), "longest");
                        TestHelpers.True(matcher.Match("/x/abc", out object val), "Match returned false");
                        TestHelpers.Equal("longest", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "LongestTieKeepsFirst", "LongestFirst keeps the first inserted pattern on a length tie", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.LongestFirst };
                        matcher.Add(new Regex("^/x/a.*$"), "first");
                        matcher.Add(new Regex("^/x/.*c$"), "second");
                        TestHelpers.True(matcher.Match("/x/abc", out object val), "Match returned false");
                        TestHelpers.Equal("first", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "ShortestPicksShorter", "ShortestFirst picks the shorter of two matching patterns", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.ShortestFirst };
                        matcher.Add(new Regex("^/x/.*$"), "shorter");
                        matcher.Add(new Regex("^/x/[a-z]+/?$"), "longer");
                        TestHelpers.True(matcher.Match("/x/abc", out object val), "Match returned false");
                        TestHelpers.Equal("shorter", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "ShortestPicksShorterRegardlessOfOrder", "ShortestFirst picks the shorter pattern when it is inserted last", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.ShortestFirst };
                        matcher.Add(new Regex("^/x/[a-z]+/?$"), "longer");
                        matcher.Add(new Regex("^/x/.*$"), "shorter");
                        TestHelpers.True(matcher.Match("/x/abc", out object val), "Match returned false");
                        TestHelpers.Equal("shorter", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "ShortestIgnoresNonMatching", "ShortestFirst ignores shorter patterns that do not match", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.ShortestFirst };
                        matcher.Add(new Regex("^/b$"), "unmatched");
                        matcher.Add(new Regex("^/a/\\d+$"), "matched");
                        TestHelpers.True(matcher.Match("/a/1", out object val), "Match returned false");
                        TestHelpers.Equal("matched", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "ShortestTieKeepsFirst", "ShortestFirst keeps the first inserted pattern on a length tie", () =>
                    {
                        Matcher matcher = new Matcher { MatchPreference = MatchPreferenceType.ShortestFirst };
                        matcher.Add(new Regex("^/x/a.*$"), "first");
                        matcher.Add(new Regex("^/x/.*c$"), "second");
                        TestHelpers.True(matcher.Match("/x/abc", out object val), "Match returned false");
                        TestHelpers.Equal("first", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "SingleMatchAllModes", "Every preference returns the only matching value", () =>
                    {
                        foreach (MatchPreferenceType pref in (MatchPreferenceType[])Enum.GetValues(typeof(MatchPreferenceType)))
                        {
                            Matcher matcher = new Matcher { MatchPreference = pref };
                            matcher.Add(new Regex("^/foo$"), "foo");
                            matcher.Add(new Regex("^/bar$"), "bar");
                            TestHelpers.True(matcher.Match("/bar", out object val), "Match returned false for " + pref);
                            TestHelpers.Equal("bar", val, "Matched value for " + pref);
                        }
                    }),

                    TestHelpers.Case(_SuiteId, "ChangePreferenceAtRuntime", "Changing MatchPreference affects subsequent matches", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/x/.*$"), "shorter");
                        matcher.Add(new Regex("^/x/[a-z]+/?$"), "longer");

                        matcher.MatchPreference = MatchPreferenceType.LongestFirst;
                        TestHelpers.True(matcher.Match("/x/abc", out object val1), "LongestFirst match returned false");
                        TestHelpers.Equal("longer", val1, "LongestFirst value");

                        matcher.MatchPreference = MatchPreferenceType.ShortestFirst;
                        TestHelpers.True(matcher.Match("/x/abc", out object val2), "ShortestFirst match returned false");
                        TestHelpers.Equal("shorter", val2, "ShortestFirst value");

                        matcher.MatchPreference = MatchPreferenceType.First;
                        TestHelpers.True(matcher.Match("/x/abc", out object val3), "First match returned false");
                        TestHelpers.Equal("shorter", val3, "First value");
                    }),

                    // Negative
                    TestHelpers.Case(_SuiteId, "NoMatchAllModes", "Every preference returns false and null when nothing matches", () =>
                    {
                        foreach (MatchPreferenceType pref in (MatchPreferenceType[])Enum.GetValues(typeof(MatchPreferenceType)))
                        {
                            Matcher matcher = new Matcher { MatchPreference = pref };
                            matcher.Add(new Regex("^/foo$"), "foo");
                            matcher.Add(new Regex("^/foo/.*$"), "foo child");
                            TestHelpers.False(matcher.Match("/bar", out object val), "Match returned true for " + pref);
                            TestHelpers.Equal(null, val, "Out value for " + pref);
                        }
                    }),

                    TestHelpers.Case(_SuiteId, "EmptyMatcherAllModes", "Every preference returns false on an empty matcher", () =>
                    {
                        foreach (MatchPreferenceType pref in (MatchPreferenceType[])Enum.GetValues(typeof(MatchPreferenceType)))
                        {
                            Matcher matcher = new Matcher { MatchPreference = pref };
                            TestHelpers.False(matcher.Match("/foo", out object val), "Match returned true for " + pref);
                            TestHelpers.Equal(null, val, "Out value for " + pref);
                        }
                    }),

                    TestHelpers.Case(_SuiteId, "InvalidInputAllModes", "Every preference rejects null and empty input", () =>
                    {
                        foreach (MatchPreferenceType pref in (MatchPreferenceType[])Enum.GetValues(typeof(MatchPreferenceType)))
                        {
                            Matcher matcher = new Matcher { MatchPreference = pref };
                            matcher.Add(new Regex(".*"), "any");
                            TestHelpers.Throws<ArgumentNullException>(() => matcher.Match(null!, out _), "Match(null) for " + pref);
                            TestHelpers.Throws<ArgumentNullException>(() => matcher.Match("", out _), "Match(\"\") for " + pref);
                        }
                    })
                });
        }
    }
}
