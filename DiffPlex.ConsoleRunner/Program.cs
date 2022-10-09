using System;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.ConsoleRunner;

internal static class Program
{
    private static void Main()
    {
        var diffBuilder = new InlineDiffBuilder(new Differ());
        var diff = diffBuilder.BuildDiffModel(TestData.OldText, TestData.NewText);

        foreach (var line in diff.Lines)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            if (line.Position.HasValue) Console.Write(line.Position.Value);
            Console.Write('\t');
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
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("  ");
                    break;
            }

            Console.WriteLine(line.Text);
        }
    }
}
