namespace Test.Nunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    using Test.Shared;

    /// <summary>
    /// Fact-style host: every shared descriptor runs inside a single [Test].
    /// </summary>
    [TestFixture]
    public sealed class PageParserNunitFactTests : TouchstoneNunitBase
    {
        /// <inheritdoc />
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return PageParserSuites.All; }
        }

        /// <summary>
        /// Run all shared descriptors as a single test.
        /// </summary>
        [Test]
        public async Task RunAll()
        {
            await RunAllAsync();
        }
    }
}
