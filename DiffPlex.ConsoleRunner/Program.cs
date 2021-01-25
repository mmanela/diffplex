using System;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.ConsoleRunner
{
    internal static class Program
    {
        private static void Main()
        {
            #region sample-usage

            var diffBuilder = new InlineDiffBuilder(new Differ());
            var diff = diffBuilder.BuildDiffModel(OldText, NewText);

            var savedColor = Console.ForegroundColor;
            foreach (var line in diff.Lines)
            {
                switch (line.Type)
                {
                    case ChangeType.Inserted:
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("+ ");
                        break;
                    case ChangeType.Deleted:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("- ");
                        break;
                    default:
                        // compromise for dark or light background
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("  ");
                        break;
                }

                Console.WriteLine(line.Text);
            }

            Console.ForegroundColor = savedColor;

            #endregion
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