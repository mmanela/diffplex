using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;

namespace Perf.DiffPlex
{
    internal class DiffPerfTester
    {
        private static readonly Random random = new Random();
        private readonly SideBySideDiffBuilder sideBySideDiffer;
        private const int MaxLineLength = 150;
        private const double DifferenceAmount = 0.2;
        private const int MaxLines = 8000;

        public DiffPerfTester()
        {
            Console.WriteLine("Max number of lines: {0}", MaxLines);
            Console.WriteLine("Max length of lines: {0}", MaxLineLength);
            Console.WriteLine("Max difference amount: {0}", DifferenceAmount);
            Console.WriteLine();
            sideBySideDiffer = new SideBySideDiffBuilder(new Differ());
        }

        public void Run()
        {
            var oldLines = GenerateLines(MaxLines);
            var newLines = MakeDifferent(oldLines, DifferenceAmount);
            var oldText = Implode(oldLines, Environment.NewLine);
            var newText = Implode(newLines, Environment.NewLine);
            new PerfTester().Run(() => sideBySideDiffer.BuildDiffModel(oldText, newText));
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
            else
                return String.Empty;
        }

        private static IList<string> MakeDifferent(IList<string> lines, double differenceAmount)
        {
            var newLines = new List<string>();
            foreach (var i in Enumerable.Range(0, lines.Count))
            {
                if(random.NextDouble() <= differenceAmount)
                {
                    // Either delete line or add different one
                    if(random.Next(2) % 2 == 1)
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
            foreach (var i in Enumerable.Range(0, random.Next(0, maxLength)))
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            if (random.Next(2) % 2 == 0)
                return builder.ToString().ToLower();

            return builder.ToString();
        }

    }
}