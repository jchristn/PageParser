namespace Test.Nunit
{
    using System.Collections;
    using System.Threading;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Touchstone.Core;
    using Touchstone.NunitAdapter;

    using Test.Shared;

    /// <summary>
    /// TestCaseSource-style host: each non-skipped shared descriptor becomes its
    /// own NUnit test case, giving per-test reporting in the IDE and CI.
    /// </summary>
    [TestFixture]
    public sealed class PageParserNunitTests
    {
        private static IEnumerable TestCases()
        {
            return new TouchstoneTestCaseSource(PageParserSuites.All);
        }

        /// <summary>
        /// Execute a single descriptor.
        /// </summary>
        /// <param name="testCase">The descriptor to run.</param>
        [Test]
        [TestCaseSource(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
