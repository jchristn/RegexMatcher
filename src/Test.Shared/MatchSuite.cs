namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using RegexMatcher;
    using Touchstone.Core;

    /// <summary>
    /// Tests for Matcher.Match independent of match preference.
    /// </summary>
    public static class MatchSuite
    {
        private const string _SuiteId = "Match";

        /// <summary>
        /// Create the suite descriptor.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Match",
                cases: new List<TestCaseDescriptor>
                {
                    // Positive
                    TestHelpers.Case(_SuiteId, "AddAndMatch", "Match returns true and the mapped value", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo/\\d+$"), "foo with id");
                        TestHelpers.True(matcher.Match("/foo/42", out object val), "Match returned false");
                        TestHelpers.Equal("foo with id", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "NullValueStored", "Match returns true and null when the stored value is null", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), null!);
                        TestHelpers.True(matcher.Match("/foo", out object val), "Match returned false");
                        TestHelpers.Equal(null, val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "ReturnsSameReference", "Match returns the exact stored object reference", () =>
                    {
                        Matcher matcher = new Matcher();
                        List<string> handler = new List<string> { "x" };
                        matcher.Add(new Regex("^/foo$"), handler);
                        TestHelpers.True(matcher.Match("/foo", out object val), "Match returned false");
                        TestHelpers.True(Object.ReferenceEquals(handler, val), "Returned a different reference");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueTypeStored", "Match returns boxed value types intact", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/answer$"), 42);
                        TestHelpers.True(matcher.Match("/answer", out object val), "Match returned false");
                        TestHelpers.Equal(42, val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "UnanchoredPartialMatch", "An unanchored pattern matches a substring of the input", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("foo"), "contains foo");
                        TestHelpers.True(matcher.Match("/api/foo/bar", out object val), "Match returned false");
                        TestHelpers.Equal("contains foo", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "IgnoreCaseOption", "RegexOptions.IgnoreCase is honored", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$", RegexOptions.IgnoreCase), "foo");
                        TestHelpers.True(matcher.Match("/FOO", out object val), "Match returned false");
                        TestHelpers.Equal("foo", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "WhitespaceInput", "Whitespace-only input is evaluated rather than rejected", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^\\s+$"), "whitespace");
                        TestHelpers.True(matcher.Match("   ", out object val), "Match returned false");
                        TestHelpers.Equal("whitespace", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "UnicodeInput", "Unicode input is matched", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/café/\\p{L}+$"), "unicode");
                        TestHelpers.True(matcher.Match("/café/über", out object val), "Match returned false");
                        TestHelpers.Equal("unicode", val, "Matched value");
                    }),

                    TestHelpers.Case(_SuiteId, "RepeatedMatchIsStable", "Repeated Match calls return the same result", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        for (int i = 0; i < 100; i++)
                        {
                            TestHelpers.True(matcher.Match("/foo", out object val), "Match returned false on iteration " + i);
                            TestHelpers.Equal("foo", val, "Matched value on iteration " + i);
                        }
                    }),

                    TestHelpers.Case(_SuiteId, "ReadmeExample", "README example routes resolve as documented", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo/\\d+$"), "foo with id");
                        matcher.Add(new Regex("^/foo/?$"), "foo with optional slash");
                        matcher.Add(new Regex("^/foo$"), "foo alone");
                        matcher.Add(new Regex("^/bar/(.*?)/(.*?)/?$"), "bar with two children");
                        matcher.Add(new Regex("^/bar/(.*?)/?$"), "bar with one child");
                        matcher.Add(new Regex("^/bar/\\d+$"), "bar with id");
                        matcher.Add(new Regex("^/bar/?$"), "bar with optional slash");
                        matcher.Add(new Regex("^/bar$"), "bar alone");

                        TestHelpers.True(matcher.Match("/bar/child/foo", out object val1), "/bar/child/foo did not match");
                        TestHelpers.Equal("bar with two children", val1, "/bar/child/foo value");
                        TestHelpers.True(matcher.Match("/foo/36", out object val2), "/foo/36 did not match");
                        TestHelpers.Equal("foo with id", val2, "/foo/36 value");
                        TestHelpers.True(matcher.Match("/foo", out object val3), "/foo did not match");
                        TestHelpers.Equal("foo with optional slash", val3, "/foo value");
                        TestHelpers.False(matcher.Match("/unknown", out object val4), "/unknown matched");
                        TestHelpers.Equal(null, val4, "/unknown value");
                    }),

                    // Negative
                    TestHelpers.Case(_SuiteId, "NoMatch", "Match returns false and null when nothing matches", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        TestHelpers.False(matcher.Match("/nope", out object val), "Match returned true");
                        TestHelpers.Equal(null, val, "Out value");
                    }),

                    TestHelpers.Case(_SuiteId, "EmptyMatcher", "Match on an empty matcher returns false", () =>
                    {
                        Matcher matcher = new Matcher();
                        TestHelpers.False(matcher.Match("/foo", out object val), "Match returned true");
                        TestHelpers.Equal(null, val, "Out value");
                    }),

                    TestHelpers.Case(_SuiteId, "CaseSensitiveByDefault", "Match is case-sensitive without RegexOptions.IgnoreCase", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        TestHelpers.False(matcher.Match("/FOO", out _), "Match returned true");
                    }),

                    TestHelpers.Case(_SuiteId, "AnchoredRejectsSuperstring", "An anchored pattern rejects input with extra characters", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        TestHelpers.False(matcher.Match("/foo/bar", out _), "Match returned true for trailing segment");
                        TestHelpers.False(matcher.Match("x/foo", out _), "Match returned true for leading prefix");
                    }),

                    TestHelpers.Case(_SuiteId, "NullInputThrows", "Match with null input throws ArgumentNullException", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex(".*"), "any");
                        ArgumentNullException ex = TestHelpers.Throws<ArgumentNullException>(
                            () => matcher.Match(null!, out _), "Match(null)");
                        TestHelpers.Equal("inVal", ex.ParamName, "ParamName");
                    }),

                    TestHelpers.Case(_SuiteId, "EmptyInputThrows", "Match with empty input throws ArgumentNullException", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex(".*"), "any");
                        ArgumentNullException ex = TestHelpers.Throws<ArgumentNullException>(
                            () => matcher.Match("", out _), "Match(\"\")");
                        TestHelpers.Equal("inVal", ex.ParamName, "ParamName");
                    }),

                    TestHelpers.Case(_SuiteId, "RegexTimeoutPropagates", "A regex match timeout propagates and does not leave the matcher locked", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex slow = new Regex("^(a+)+$", RegexOptions.None, TimeSpan.FromMilliseconds(1));
                        matcher.Add(slow, "slow");
                        string input = new string('a', 40) + "!";
                        TestHelpers.Throws<RegexMatchTimeoutException>(() => matcher.Match(input, out _), "Catastrophic backtracking");

                        matcher.Remove(slow);
                        matcher.Add(new Regex("^/foo$"), "foo");
                        TestHelpers.True(matcher.Match("/foo", out object val), "Matcher unusable after timeout");
                        TestHelpers.Equal("foo", val, "Matched value after timeout");
                    })
                });
        }
    }
}
