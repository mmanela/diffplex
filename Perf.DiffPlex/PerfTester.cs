using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Perf.DiffPlex
{
    public class PerfTester
    {
        public void Run(Action action)
        {
            const double count = 5;
            var times = new List<double>();
            var timer = new Stopwatch();
            var totalTime = new Stopwatch();
            double maxTime = 0;
            double minTime = double.MaxValue;
            Console.WriteLine("Ensuring code is Jitted");
            action();
            Console.WriteLine();
            Console.WriteLine("Time before run: {0}", DateTime.Now);
            Console.WriteLine("Running {0} times.", count);
            totalTime.Start();
            for (var i = 0; i < count; i++)
            {
                timer.Start();
                action();
                timer.Stop();
                maxTime = Math.Max(maxTime, timer.ElapsedMilliseconds);
                minTime = Math.Min(minTime, timer.ElapsedMilliseconds);
                times.Add(timer.ElapsedMilliseconds);
                timer.Reset();
            }
            totalTime.Stop();
            Console.WriteLine();
            Console.WriteLine("Time after run: {0}", DateTime.Now);
            Console.WriteLine("Elapsed: {0}ms", totalTime.ElapsedMilliseconds);
            Console.WriteLine("Diffs Per Second: {0}", (count / totalTime.ElapsedMilliseconds) * 1000);
            Console.WriteLine("Average: {0}ms", times.Average());
            Console.WriteLine("Max time for a call: {0}ms", maxTime);
            Console.WriteLine("Min time for a call: {0}ms", minTime);
        }
    }
}
