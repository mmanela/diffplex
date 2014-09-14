using System;

namespace Perf.DiffPlex
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"DiffPlex Perf Tester");

            new DiffPerfTester().Run();
            Console.WriteLine();
        }
    }
}
