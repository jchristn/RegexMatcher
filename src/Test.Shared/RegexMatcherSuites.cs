namespace Test.Shared
{
    using System.Collections.Generic;
    using Touchstone.Core;

    /// <summary>
    /// Central source of truth for all RegexMatcher test suites.  Every runner consumes this list.
    /// </summary>
    public static class RegexMatcherSuites
    {
        /// <summary>
        /// All test suites.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All
        {
            get
            {
                return new List<TestSuiteDescriptor>
                {
                    AddRemoveSuite.Create(),
                    LookupSuite.Create(),
                    MatchSuite.Create(),
                    MatchPreferenceSuite.Create(),
                    AllMatchesSuite.Create(),
                    ConcurrencySuite.Create()
                };
            }
        }
    }
}
