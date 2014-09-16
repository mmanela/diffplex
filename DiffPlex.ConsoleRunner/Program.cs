using System;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.ConsoleRunner
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var d = new Differ();
            var inlineBuilder = new InlineDiffBuilder(d);
            var result = inlineBuilder.BuildDiffModel(OldText, NewText);
            foreach (var line in result.Lines)
            {
                if(line.Type == ChangeType.Inserted)
                    Console.Write("+ ");
                else if(line.Type == ChangeType.Deleted)
                    Console.Write("- ");
                else
                    Console.Write("  ");

                Console.WriteLine(line.Text);
            }
        }

        private const string OldText =
            @"We the people
of the united states of america
establish justice
ensure domestic tranquility
provide for the common defence
secure the blessing of liberty
to ourselves and our posterity
";

        private const string NewText =
            @"We the people
in order to form a more perfect union
establish justice
ensure domestic tranquility
promote the general welfare and
secure the blessing of liberty
to ourselves and our posterity
do ordain and establish this constitution
for the United States of America




";
    }
}