using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Perf.DiffPlex
{
    public class LoadTester
    {
        private readonly Action action;
        private static bool everyoneDie;
        private int threadCount;
        private int errorCount;
        private readonly Stopwatch overallTimer = new Stopwatch();
        private readonly List<long> times = new List<long>();
        private readonly ReaderWriterLockSlim signal = new ReaderWriterLockSlim();

        public LoadTester(Action action)
        {
            this.action = action;
        }

        public void Run()
        {
            Console.WriteLine();
            Console.Write("Enter number of threads: ");
            var numberOfThreads = int.Parse(Console.ReadLine());

            Console.Write("Enter number of seconds: ");
            var numberOfSeconds = int.Parse(Console.ReadLine());

            Console.WriteLine("Starting {0} threads", numberOfThreads);
            for (var i = 0; i < numberOfThreads; i++)
            {
                var t = new Thread(Worker);
                t.Start();
            }
            Thread.Sleep(10 * numberOfSeconds);
            Console.WriteLine("{0} threads started", threadCount);

            
            Console.WriteLine("Running for {0} seconds", numberOfSeconds);
            overallTimer.Start();
            for (var i = 0; i < numberOfSeconds; i++)
            {
                Console.Write(".");
                Thread.Sleep(1000);
            }
            Console.WriteLine();

            Console.WriteLine("Stopping threads");
            everyoneDie = true;
            signal.EnterReadLock();
            bool sleepMore = threadCount != 0;
            signal.ExitReadLock();
            do
            {
                Thread.Sleep(10);
                signal.EnterReadLock();
                sleepMore = threadCount != 0;
                signal.ExitReadLock();
            } while (sleepMore);
                

            overallTimer.Stop();

            Console.WriteLine();
            Console.WriteLine("Total Elapsed: {0}ms", overallTimer.ElapsedMilliseconds);
            Console.WriteLine("Total Requests: {0}", times.Count());
            Console.WriteLine("Total Errors: {0}", errorCount);
            Console.WriteLine("RPS: {0}", (Convert.ToDouble(times.Count()) / overallTimer.ElapsedMilliseconds) * 1000);
            Console.WriteLine("Average: {0}ms", times.Average());
        }

        private void Worker()
        {

            Interlocked.Increment(ref threadCount);

            var timer = new Stopwatch();
         
            while (!everyoneDie)
            {
                timer.Start();

                try
                {
                    action();
                }
                catch
                {
                    errorCount++;
                }

                timer.Stop();
                times.Add(timer.ElapsedMilliseconds);

                timer.Reset();
            }

            Interlocked.Decrement(ref threadCount);
        }
    }
}
