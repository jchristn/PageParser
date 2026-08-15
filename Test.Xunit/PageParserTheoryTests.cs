namespace Test.Xunit
{
    using System.Threading;
    using System.Threading.Tasks;

    using Touchstone.Core;

    using Test.Shared;

    using global::Xunit;

    /// <summary>
    /// Theory-style host: each non-skipped shared descriptor becomes its own
    /// xUnit theory row, giving per-test reporting in the IDE and CI.
    /// </summary>
    public sealed class PageParserTheoryTests
    {
        /// <summary>
        /// Provides every non-skipped descriptor as a theory row.
        /// </summary>
        /// <returns>Theory data.</returns>
        public static TheoryData<TestCaseDescriptor> TestCases()
        {
            TheoryData<TestCaseDescriptor> data = new TheoryData<TestCaseDescriptor>();

            foreach (TestSuiteDescriptor suite in PageParserSuites.All)
            {
                foreach (TestCaseDescriptor testCase in suite.Cases)
                {
                    if (!testCase.Skip)
                        data.Add(testCase);
                }
            }

            return data;
        }

        /// <summary>
        /// Execute a single descriptor.
        /// </summary>
        /// <param name="testCase">The descriptor to run.</param>
        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task RunTest(TestCaseDescriptor testCase)
        {
            await testCase.ExecuteAsync(CancellationToken.None);
        }
    }
}
