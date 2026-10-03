namespace Test.Nunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::NUnit.Framework;
    using Test.Shared;
    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    [TestFixture]
    public sealed class RegexMatcherNunitFactTests : TouchstoneNunitBase
    {
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return RegexMatcherSuites.All; }
        }

        [Test]
        public async Task RunAll()
        {
            await RunAllAsync().ConfigureAwait(false);
        }
    }
}
