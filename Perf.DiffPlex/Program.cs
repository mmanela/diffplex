using System;
using BenchmarkDotNet.Running;

namespace Perf.DiffPlex
{
    internal static class Program
    {
        public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
