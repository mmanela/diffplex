using System;
using System.IO;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;
using DiffPlex.Renderer;

namespace DiffPlex.ConsoleRunner;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            PrintUsage();
            return;
        }

        try
        {
            string command = args[0].ToLowerInvariant();
            string result;

            if (command == "file")
            {
                // File comparison mode
                string oldFilePath = NormalizePath(args[1]);
                string newFilePath = NormalizePath(args[2]);
                
                if (!File.Exists(oldFilePath))
                {
                    Console.Error.WriteLine($"Error: File not found: {oldFilePath}");
                    return;
                }

                if (!File.Exists(newFilePath))
                {
                    Console.Error.WriteLine($"Error: File not found: {newFilePath}");
                    return;
                }

                string oldText = File.ReadAllText(oldFilePath);
                string newText = File.ReadAllText(newFilePath);
                result = UnidiffRenderer.GenerateUnidiff(oldText, newText, oldFilePath, newFilePath);
            }
            else if (command == "text")
            {
                // Text comparison mode
                // Process escape sequences like \n in the input text
                string oldText = args[1].Replace("\\n", "\n");
                string newText = args[2].Replace("\\n", "\n");
                result = UnidiffRenderer.GenerateUnidiff(oldText, newText);
            }
            else
            {
                Console.Error.WriteLine($"Unknown command: {command}");
                PrintUsage();
                return;
            }

            Console.WriteLine(result);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("DiffPlex.ConsoleRunner - Generate unified diff (unidiff) output");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  DiffPlex.ConsoleRunner file <old-file> <new-file>");
        Console.WriteLine("  DiffPlex.ConsoleRunner text <old-string> <new-string>");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  file    Compare two files and generate a unidiff");
        Console.WriteLine("  text    Compare two strings and generate a unidiff");
        Console.WriteLine();
        Console.WriteLine("File paths work with both Windows and Unix-style paths.");
    }
    
    /// <summary>
    /// Normalizes a file path to work correctly on both Windows and Unix-based systems.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>A normalized path that works on the current operating system.</returns>
    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;
            
        // Replace any forward slashes with the platform-specific directory separator
        return path.Replace('/', Path.DirectorySeparatorChar)
                  .Replace('\\', Path.DirectorySeparatorChar);
    }
}
