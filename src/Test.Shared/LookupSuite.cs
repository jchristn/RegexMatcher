namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using RegexMatcher;
    using Touchstone.Core;

    /// <summary>
    /// Tests for Matcher.Exists and Matcher.ValueExists.
    /// </summary>
    public static class LookupSuite
    {
        private const string _SuiteId = "Lookup";

        /// <summary>
        /// Create the suite descriptor.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Exists and ValueExists",
                cases: new List<TestCaseDescriptor>
                {
                    // Positive
                    TestHelpers.Case(_SuiteId, "ExistsTrue", "Exists returns true for an added regex instance", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex regex = new Regex("^/foo$");
                        matcher.Add(regex, "foo");
                        TestHelpers.True(matcher.Exists(regex), "Exists returned false");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsTrue", "ValueExists returns true for a stored value", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "the-value");
                        TestHelpers.True(matcher.ValueExists("the-value"), "ValueExists returned false");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsSameReference", "ValueExists returns true for the same object reference", () =>
                    {
                        Matcher matcher = new Matcher();
                        object handler = new object();
                        matcher.Add(new Regex("^/foo$"), handler);
                        TestHelpers.True(matcher.ValueExists(handler), "ValueExists returned false");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsNullStored", "ValueExists returns true for a stored null value", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), null!);
                        TestHelpers.True(matcher.ValueExists(null!), "ValueExists(null) returned false");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsAmongMany", "ValueExists finds a value that is not the first entry", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/a$"), "a");
                        matcher.Add(new Regex("^/b$"), "b");
                        matcher.Add(new Regex("^/c$"), "c");
                        TestHelpers.True(matcher.ValueExists("c"), "ValueExists returned false");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsEqualString", "ValueExists finds an equal string that is a different instance", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "the-value");
                        string probe = new string("the-value".ToCharArray());
                        TestHelpers.False(Object.ReferenceEquals(probe, "the-value"), "Probe should be a distinct instance");
                        TestHelpers.True(matcher.ValueExists(probe), "ValueExists returned false for an equal string");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsBoxedValueType", "ValueExists finds an equal boxed value type", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/answer$"), 42);
                        TestHelpers.True(matcher.ValueExists(42), "ValueExists returned false for boxed int");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsEqualsOverride", "ValueExists honors a type's Equals override", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/site$"), new Uri("https://example.com/a"));
                        TestHelpers.True(matcher.ValueExists(new Uri("https://example.com/a")), "ValueExists returned false for an equal Uri");
                    }),

                    // Negative
                    TestHelpers.Case(_SuiteId, "ExistsDifferentInstance", "Exists returns false for a different instance with the same pattern", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        TestHelpers.False(matcher.Exists(new Regex("^/foo$")), "Exists returned true");
                    }),

                    TestHelpers.Case(_SuiteId, "ExistsNullReturnsFalse", "Exists with null returns false", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        TestHelpers.False(matcher.Exists(null!), "Exists(null) returned true");
                    }),

                    TestHelpers.Case(_SuiteId, "ExistsEmptyMatcher", "Exists returns false on an empty matcher", () =>
                    {
                        Matcher matcher = new Matcher();
                        TestHelpers.False(matcher.Exists(new Regex("^/foo$")), "Exists returned true");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsMissing", "ValueExists returns false for an absent value", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        TestHelpers.False(matcher.ValueExists("absent"), "ValueExists returned true");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsNullNotStored", "ValueExists(null) returns false when no null value is stored", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "foo");
                        TestHelpers.False(matcher.ValueExists(null!), "ValueExists(null) returned true");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsEmptyMatcher", "ValueExists returns false on an empty matcher", () =>
                    {
                        Matcher matcher = new Matcher();
                        TestHelpers.False(matcher.ValueExists("foo"), "ValueExists returned true");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsDifferentValueType", "ValueExists does not match a different value of the same type", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/answer$"), 42);
                        TestHelpers.False(matcher.ValueExists(43), "ValueExists returned true for 43");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsDifferentType", "ValueExists does not match a value of a different type with the same text", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/answer$"), 42);
                        TestHelpers.False(matcher.ValueExists("42"), "ValueExists returned true for string \"42\"");
                        TestHelpers.False(matcher.ValueExists(42L), "ValueExists returned true for long 42");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsCaseSensitive", "ValueExists string comparison is case-sensitive", () =>
                    {
                        Matcher matcher = new Matcher();
                        matcher.Add(new Regex("^/foo$"), "Value");
                        TestHelpers.False(matcher.ValueExists("value"), "ValueExists returned true for different casing");
                    }),

                    TestHelpers.Case(_SuiteId, "ValueExistsAfterRemove", "ValueExists returns false after its regex is removed", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex regex = new Regex("^/foo$");
                        matcher.Add(regex, "foo");
                        matcher.Remove(regex);
                        TestHelpers.False(matcher.ValueExists("foo"), "ValueExists returned true");
                    })
                });
        }
    }
}
