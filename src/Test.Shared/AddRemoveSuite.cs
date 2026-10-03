namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using RegexMatcher;
    using Touchstone.Core;

    /// <summary>
    /// Tests for Matcher.Add, Matcher.Remove, and Matcher.Get.
    /// </summary>
    public static class AddRemoveSuite
    {
        private const string _SuiteId = "AddRemove";

        /// <summary>
        /// Create the suite descriptor.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Add, Remove, and Get",
                cases: new List<TestCaseDescriptor>
                {
                    // Positive
                    TestHelpers.Case(_SuiteId, "NewMatcherIsEmpty", "New matcher has an empty dictionary", () =>
                    {
                        Matcher matcher = new Matcher();
                        TestHelpers.True(matcher.Get() != null, "Get returned null");
                        TestHelpers.Equal(0, matcher.Get().Count, "Entry count");
                    }),

                    TestHelpers.Case(_SuiteId, "AddSingle", "Add stores a regex and value", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex regex = new Regex("^/foo$");
                        matcher.Add(regex, "foo");
                        Dictionary<Regex, object> all = matcher.Get();
                        TestHelpers.Equal(1, all.Count, "Entry count");
                        TestHelpers.Equal("foo", all[regex], "Stored value");
                    }),

                    TestHelpers.Case(_SuiteId, "GetReturnsAllEntries", "Get returns all entries", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        matcher.Add(new Regex("^/bar$"), "bar");
                        Dictionary<Regex, object> all = matcher.Get();
                        TestHelpers.Equal(2, all.Count, "Entry count");
                        TestHelpers.True(all.ContainsValue("foo"), "Missing value foo");
                        TestHelpers.True(all.ContainsValue("bar"), "Missing value bar");
                    }),

                    TestHelpers.Case(_SuiteId, "AddNullValue", "Add accepts a null value", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex regex = new Regex("^/foo$");
                        matcher.Add(regex, null!);
                        TestHelpers.True(matcher.Exists(regex), "Regex with null value not stored");
                        TestHelpers.Equal(null, matcher.Get()[regex], "Stored value");
                    }),

                    TestHelpers.Case(_SuiteId, "AddSamePatternDistinctInstances", "Add accepts distinct Regex instances with identical patterns", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "one");
                        matcher.Add(new Regex("^/foo$"), "two");
                        TestHelpers.Equal(2, matcher.Get().Count, "Entry count");
                    }),

                    TestHelpers.Case(_SuiteId, "AddDuplicateValues", "Add accepts the same value for multiple regexes", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "shared");
                        matcher.Add(new Regex("^/bar$"), "shared");
                        TestHelpers.Equal(2, matcher.Get().Count, "Entry count");
                    }),

                    TestHelpers.Case(_SuiteId, "RemoveExisting", "Remove deletes the entry", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex regex = new Regex("^/foo$");
                        matcher.Add(regex, "foo");
                        matcher.Remove(regex);
                        TestHelpers.False(matcher.Exists(regex), "Regex still exists after Remove");
                        TestHelpers.False(matcher.Match("/foo", out _), "Match succeeded after Remove");
                        TestHelpers.Equal(0, matcher.Get().Count, "Entry count");
                    }),

                    TestHelpers.Case(_SuiteId, "RemoveLeavesOthers", "Remove deletes only the targeted entry", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex foo = new Regex("^/foo$");
                        Regex bar = new Regex("^/bar$");
                        matcher.Add(foo, "foo");
                        matcher.Add(bar, "bar");
                        matcher.Remove(foo);
                        TestHelpers.False(matcher.Exists(foo), "Removed regex still exists");
                        TestHelpers.True(matcher.Exists(bar), "Other regex was removed");
                        TestHelpers.True(matcher.Match("/bar", out object val), "Remaining regex did not match");
                        TestHelpers.Equal("bar", val, "Remaining value");
                    }),

                    TestHelpers.Case(_SuiteId, "RemoveThenReAdd", "A removed regex instance can be added again", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex regex = new Regex("^/foo$");
                        matcher.Add(regex, "first");
                        matcher.Remove(regex);
                        matcher.Add(regex, "second");
                        TestHelpers.True(matcher.Match("/foo", out object val), "Re-added regex did not match");
                        TestHelpers.Equal("second", val, "Re-added value");
                    }),

                    TestHelpers.Case(_SuiteId, "RemoveTwiceIsNoOp", "Removing the same regex twice does not throw", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex regex = new Regex("^/foo$");
                        matcher.Add(regex, "foo");
                        matcher.Remove(regex);
                        matcher.Remove(regex);
                        TestHelpers.Equal(0, matcher.Get().Count, "Entry count");
                    }),

                    // Negative
                    TestHelpers.Case(_SuiteId, "AddNullRegexThrows", "Add with null regex throws ArgumentNullException", () =>
                    {
                        Matcher matcher = new Matcher();
                        ArgumentNullException ex = TestHelpers.Throws<ArgumentNullException>(
                            () => matcher.Add(null!, "x"), "Add(null)");
                        TestHelpers.Equal("regex", ex.ParamName, "ParamName");
                        TestHelpers.Equal(0, matcher.Get().Count, "Entry count after failed Add");
                    }),

                    TestHelpers.Case(_SuiteId, "AddSameInstanceTwiceThrows", "Add of the same Regex instance twice throws ArgumentException", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex regex = new Regex("^/foo$");
                        matcher.Add(regex, "first");
                        TestHelpers.Throws<ArgumentException>(() => matcher.Add(regex, "second"), "Duplicate Add");
                        TestHelpers.Equal(1, matcher.Get().Count, "Entry count");
                        TestHelpers.Equal("first", matcher.Get()[regex], "Original value was overwritten");
                    }),

                    TestHelpers.Case(_SuiteId, "RemoveNullRegexThrows", "Remove with null regex throws ArgumentNullException", () =>
                    {
                        Matcher matcher = new Matcher();
                        ArgumentNullException ex = TestHelpers.Throws<ArgumentNullException>(
                            () => matcher.Remove(null!), "Remove(null)");
                        TestHelpers.Equal("regex", ex.ParamName, "ParamName");
                    }),

                    TestHelpers.Case(_SuiteId, "RemoveMissingIsNoOp", "Remove of a regex that was never added leaves entries intact", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        matcher.Remove(new Regex("^/missing$"));
                        TestHelpers.Equal(1, matcher.Get().Count, "Entry count");
                    }),

                    TestHelpers.Case(_SuiteId, "RemoveEqualPatternDifferentInstanceIsNoOp", "Remove with a different instance of the same pattern does not remove the entry", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex regex = new Regex("^/foo$");
                        matcher.Add(regex, "foo");
                        matcher.Remove(new Regex("^/foo$"));
                        TestHelpers.True(matcher.Exists(regex), "Entry removed by a different instance");
                    }),

                    TestHelpers.Case(_SuiteId, "RemoveFromEmptyIsNoOp", "Remove on an empty matcher does not throw", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Remove(new Regex("^/foo$"));
                        TestHelpers.Equal(0, matcher.Get().Count, "Entry count");
                    })
                });
        }
    }
}
