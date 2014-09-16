using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;

namespace Perf.DiffPlex
{
    class DiffPerfTester
    {
        private readonly IDiffer differ;
        private SideBySideDiffBuilder sideBySideDiffer;
        const int MaxLineLength = 150;
        const double DifferenceAmount = 0.2;
        const int MaxLines = 8000;

        public DiffPerfTester()
        {
            differ = new Differ();
            Console.WriteLine("Max number of lines: {0}", MaxLines);
            Console.WriteLine("Max length of lines: {0}", MaxLineLength);
            Console.WriteLine("Max difference amount: {0}", DifferenceAmount);
            Console.WriteLine();
            sideBySideDiffer = new SideBySideDiffBuilder(differ);
        }

        public void Run()
        {
            var oldLines = GenerateLines(MaxLines);
            var newLines = MakeDifferent(oldLines, DifferenceAmount);
            var oldText = Implode(oldLines, Environment.NewLine);
            var newText = Implode(newLines, Environment.NewLine);
            new PerfTester().Run(() => sideBySideDiffer.BuildDiffModel(oldText, newText));
        }


        private string Implode<T>(IEnumerable<T> enumerable, string delim)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");
            if (delim == null) throw new ArgumentNullException("delim");

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

        private IList<string> MakeDifferent(IList<string> lines, double differenceAmount)
        {
            var random = new Random();
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

        private IList<string> GenerateLines(int lines)
        {
            return Enumerable.Range(0, lines).Select(i => RandomString(MaxLineLength)).ToList();
        }

        private string RandomString(int maxLength)
        {
            var builder = new StringBuilder();
            var random = new Random();
            char ch;
            foreach (var i in Enumerable.Range(0, random.Next(0, maxLength)))
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            if (random.Next(2) % 2 == 0)
                return builder.ToString().ToLower();

            return builder.ToString();
        }

    }
}