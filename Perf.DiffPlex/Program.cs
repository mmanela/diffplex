using System;

namespace Perf.DiffPlex
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(@"DiffPlex Perf Tester");

            new DiffPerfTester().Run();
            Console.WriteLine();
        }
    }
}
