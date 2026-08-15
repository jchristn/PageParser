namespace Test.Xunit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Touchstone.Core;
    using Touchstone.XunitAdapter;

    using Test.Shared;

    using global::Xunit;

    /// <summary>
    /// Fact-style host: every shared descriptor runs inside a single [Fact].
    /// </summary>
    public sealed class PageParserFactTests : TouchstoneFactBase
    {
        /// <inheritdoc />
        protected override IReadOnlyList<TestSuiteDescriptor> Suites
        {
            get { return PageParserSuites.All; }
        }

        /// <summary>
        /// Run all shared descriptors as a single fact.
        /// </summary>
        [Fact]
        public async Task RunAll()
        {
            await RunAllAsync();
        }
    }
}
