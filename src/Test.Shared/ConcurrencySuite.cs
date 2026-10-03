namespace Test.Shared
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using RegexMatcher;
    using Touchstone.Core;

    /// <summary>
    /// Tests for thread safety of Matcher mutation and evaluation methods.
    /// </summary>
    public static class ConcurrencySuite
    {
        private const string _SuiteId = "Concurrency";

        /// <summary>
        /// Create the suite descriptor.
        /// </summary>
        /// <returns>Suite descriptor.</returns>
        public static TestSuiteDescriptor Create()
        {
            return new TestSuiteDescriptor(
                suiteId: _SuiteId,
                displayName: "Concurrency",
                cases: new List<TestCaseDescriptor>
                {
                    TestHelpers.Case(_SuiteId, "ParallelAdd", "Parallel Add from many threads stores every entry", () =>
                    {
                        Matcher matcher = new Matcher();
                        int count = 1000;
                        Parallel.For(0, count, i => matcher.Add(new Regex("^/item/" + i + "$"), i));
                        TestHelpers.Equal(count, matcher.Get().Count, "Entry count");
                        for (int i = 0; i < count; i += 97)
                        {
                            TestHelpers.True(matcher.Match("/item/" + i, out object val), "Missing /item/" + i);
                            TestHelpers.Equal(i, val, "Value for /item/" + i);
                        }
                    }),

                    TestHelpers.Case(_SuiteId, "EnumerateGetDuringWrites", "Enumerating a Get snapshot while other threads modify the matcher does not throw", () =>
                    {
                        Matcher matcher = new Matcher();
                        for (int i = 0; i < 50; i++) matcher.Add(new Regex("^/seed/" + i + "$"), i);
                        int iterations = 500;
                        int errors = 0;

                        Task writer = Task.Run(() =>
                        {
                            for (int i = 0; i < iterations; i++)
                            {
                                Regex regex = new Regex("^/temp/" + i + "$");
                                matcher.Add(regex, i);
                                matcher.Remove(regex);
                            }
                        });

                        Task reader = Task.Run(() =>
                        {
                            for (int i = 0; i < iterations; i++)
                            {
                                try
                                {
                                    int seen = 0;
                                    foreach (KeyValuePair<Regex, object> entry in matcher.Get()) seen++;
                                    if (seen < 50) Interlocked.Increment(ref errors);
                                }
                                catch (InvalidOperationException)
                                {
                                    Interlocked.Increment(ref errors);
                                }
                            }
                        });

                        Task.WaitAll(writer, reader);
                        TestHelpers.Equal(0, errors, "Errors while enumerating snapshots");
                    }),

                    TestHelpers.Case(_SuiteId, "ParallelAddRemoveMatch", "Concurrent Add, Remove, Match, AllMatches, Exists, and ValueExists do not throw", () =>
                    {
                        Matcher matcher = new Matcher();
                        Regex stable = new Regex("^/stable$");
                        matcher.Add(stable, "stable");
                        int iterations = 500;
                        int failures = 0;

                        Task writer = Task.Run(() =>
                        {
                            for (int i = 0; i < iterations; i++)
                            {
                                Regex regex = new Regex("^/temp/" + i + "$");
                                matcher.Add(regex, i);
                                matcher.Remove(regex);
                            }
                        });

                        Task reader = Task.Run(() =>
                        {
                            for (int i = 0; i < iterations; i++)
                            {
                                if (!matcher.Match("/stable", out object val) || !"stable".Equals(val))
                                    Interlocked.Increment(ref failures);
                                if (!matcher.AllMatches("/stable").Contains("stable"))
                                    Interlocked.Increment(ref failures);
                                if (!matcher.Exists(stable))
                                    Interlocked.Increment(ref failures);
                                if (!matcher.ValueExists("stable"))
                                    Interlocked.Increment(ref failures);
                            }
                        });

                        Task.WaitAll(writer, reader);
                        TestHelpers.Equal(0, failures, "Reader observed inconsistent results");
                        TestHelpers.Equal(1, matcher.Get().Count, "Entry count after churn");
                    })
                });
        }
    }
}
