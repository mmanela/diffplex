using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Attributes;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Perf.DiffPlex
{
    [MemoryDiagnoser]
    public class SideBySideDiffBuilderBenchmark
    {
        private static readonly Random Random = new Random();
        private readonly SideBySideDiffBuilder sideBySideDiffer = new SideBySideDiffBuilder(new Differ());
        private string oldText;
        private string newText;
        private const int MaxLineLength = 150;
        private const double DifferenceAmount = 0.2;
        private const int MaxLines = 8000;

        [GlobalSetup]
        public void SetUp()
        {
            var oldLines = GenerateLines(MaxLines);
            var newLines = MakeDifferent(oldLines, DifferenceAmount);
            oldText = Implode(oldLines, Environment.NewLine);
            newText = Implode(newLines, Environment.NewLine);
        }

        [Benchmark]
        public SideBySideDiffModel BuildDiffModel()
        {
            return sideBySideDiffer.BuildDiffModel(oldText, newText);
        }
        
        private static string Implode<T>(IEnumerable<T> enumerable, string delim)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (delim == null) throw new ArgumentNullException(nameof(delim));

            bool loopedAtLeaseOnce = false;
            StringBuilder result = new StringBuilder();
            foreach (var item in enumerable)
            {
                loopedAtLeaseOnce = true;
                result.Append(item + delim);
            }

            if (loopedAtLeaseOnce)
                return result.Remove(result.Length - delim.Length, delim.Length).ToString();
            
            return String.Empty;
        }

        private static IList<string> MakeDifferent(IList<string> lines, double differenceAmount)
        {
            var newLines = new List<string>();
            foreach (var i in Enumerable.Range(0, lines.Count))
            {
                if(Random.NextDouble() <= differenceAmount)
                {
                    // Either delete line or add different one
                    if(Random.Next(2) % 2 == 1)
                    {
                        newLines.Add(RandomString(MaxLineLength));
                    }
                }
                else
                {
                    newLines.Add(lines[i]);
                }
            }

            return newLines;

        }

        private static IList<string> GenerateLines(int lines)
        {
            return Enumerable.Range(0, lines).Select(i => RandomString(MaxLineLength)).ToList();
        }

        private static string RandomString(int maxLength)
        {
            var builder = new StringBuilder();
            foreach (var i in Enumerable.Range(0, Random.Next(0, maxLength)))
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
                builder.Append(ch);
            }

            if (Random.Next(2) % 2 == 0)
                return builder.ToString().ToLower();

            return builder.ToString();
        }
    }
}