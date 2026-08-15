namespace Test.Automated
{
    using System.Threading.Tasks;

    using Touchstone.Cli;

    using Test.Shared;

    /// <summary>
    /// Touchstone CLI runner. Executes every descriptor defined in Test.Shared
    /// and returns a process exit code of 0 when all tests pass, 1 otherwise.
    ///
    /// Usage:
    ///   Test.Automated [--results &lt;path-to-json&gt;]
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Entry point.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>Process exit code.</returns>
        public static async Task<int> Main(string[] args)
        {
            string resultsPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--results" && i + 1 < args.Length)
                {
                    resultsPath = args[i + 1];
                    i++;
                }
            }

            return await ConsoleRunner.RunAsync(PageParserSuites.All, resultsPath: resultsPath);
        }
    }
}
