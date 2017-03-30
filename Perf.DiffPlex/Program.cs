using System;

namespace Perf.DiffPlex
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine(@"DiffPlex Perf Tester");

            new DiffPerfTester().Run();
            Console.WriteLine();
        }
    }
}
