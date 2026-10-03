namespace Test.Xunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.XunitAdapter;
    using global::Xunit;

    public sealed class RegexMatcherFactTests : TouchstoneFactBase
    {
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return RegexMatcherSuites.All; }
        }

        [Fact]
        public async Task RunAll()
        {
            await RunAllAsync();
        }
    }
}
