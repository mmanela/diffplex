using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;
using DiffPlex.Renderer;
using DiffPlex.Chunkers;

namespace DiffPlex.ConsoleRunner;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            PrintUsage();
            return 1;
        }

        try
        {
            var options = ParseCommandLineOptions(args);
            
            return options.Command switch
            {
                "file" => HandleFileCommand(options),
                "text" => HandleTextCommand(options),
                "3way-file" => Handle3WayFileCommand(options),
                "3way-text" => Handle3WayTextCommand(options),
                "merge-file" => HandleMergeFileCommand(options),
                "merge-text" => HandleMergeTextCommand(options),
                _ => HandleUnknownCommand(options.Command)
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static CommandLineOptions ParseCommandLineOptions(string[] args)
    {
        var options = new CommandLineOptions
        {
            Command = args[0].ToLowerInvariant()
        };

        var positionalArgs = new List<string>();
        
        for (int i = 1; i < args.Length; i++)
        {
            string arg = args[i];
            
            if (arg == "-i" || arg == "--ignore-case")
            {
                options.IgnoreCase = true;
            }
            else if (arg == "-w" || arg == "--ignore-whitespace")
            {
                options.IgnoreWhitespace = true;
            }
            else if (arg.StartsWith("-"))
            {
                throw new ArgumentException($"Unknown option: {arg}");
            }
            else
            {
                positionalArgs.Add(arg);
            }
        }

        options.Arguments = positionalArgs.ToArray();
        return options;
    }

    private static int HandleFileCommand(CommandLineOptions options)
    {
        if (options.Arguments.Length < 2)
        {
            Console.Error.WriteLine("Error: file command requires old and new file paths");
            PrintUsage();
            return 1;
        }

        string oldFilePath = NormalizePath(options.Arguments[0]);
        string newFilePath = NormalizePath(options.Arguments[1]);
        
        if (!File.Exists(oldFilePath))
        {
            Console.Error.WriteLine($"Error: File not found: {oldFilePath}");
            return 1;
        }

        if (!File.Exists(newFilePath))
        {
            Console.Error.WriteLine($"Error: File not found: {newFilePath}");
            return 1;
        }

        string oldText = File.ReadAllText(oldFilePath);
        string newText = File.ReadAllText(newFilePath);
        string result = UnidiffRenderer.GenerateUnidiff(oldText, newText, oldFilePath, newFilePath);
        
        Console.WriteLine(result);
        return 0;
    }

    private static int HandleTextCommand(CommandLineOptions options)
    {
        if (options.Arguments.Length < 2)
        {
            Console.Error.WriteLine("Error: text command requires old and new text");
            PrintUsage();
            return 1;
        }

        string oldText = options.Arguments[0].Replace("\\n", "\n");
        string newText = options.Arguments[1].Replace("\\n", "\n");
        string result = UnidiffRenderer.GenerateUnidiff(oldText, newText);
        
        Console.WriteLine(result);
        return 0;
    }

    private static int Handle3WayFileCommand(CommandLineOptions options)
    {
        if (options.Arguments.Length < 3)
        {
            Console.Error.WriteLine("Error: 3way-file requires base, old, and new file paths");
            PrintUsage();
            return 1;
        }

        string result = Handle3WayFileDiff(options.Arguments[0], options.Arguments[1], options.Arguments[2], options);
        Console.WriteLine(result);
        return 0;
    }

    private static int Handle3WayTextCommand(CommandLineOptions options)
    {
        if (options.Arguments.Length < 3)
        {
            Console.Error.WriteLine("Error: 3way-text requires base, old, and new text");
            PrintUsage();
            return 1;
        }

        string baseText = options.Arguments[0].Replace("\\n", "\n");
        string oldText = options.Arguments[1].Replace("\\n", "\n");
        string newText = options.Arguments[2].Replace("\\n", "\n");
        string result = Handle3WayTextDiff(baseText, oldText, newText, options);
        
        Console.WriteLine(result);
        return 0;
    }

    private static int HandleMergeFileCommand(CommandLineOptions options)
    {
        if (options.Arguments.Length < 3)
        {
            Console.Error.WriteLine("Error: merge-file requires base, old, and new file paths");
            PrintUsage();
            return 1;
        }

        var result = Handle3WayFileMerge(options.Arguments[0], options.Arguments[1], options.Arguments[2], options);
        Console.WriteLine(result.Output);
        return result.ExitCode;
    }

    private static int HandleMergeTextCommand(CommandLineOptions options)
    {
        if (options.Arguments.Length < 3)
        {
            Console.Error.WriteLine("Error: merge-text requires base, old, and new text");
            PrintUsage();
            return 1;
        }

        string baseText = options.Arguments[0].Replace("\\n", "\n");
        string oldText = options.Arguments[1].Replace("\\n", "\n");
        string newText = options.Arguments[2].Replace("\\n", "\n");
        var result = Handle3WayTextMerge(baseText, oldText, newText, options);
        
        Console.WriteLine(result.Output);
        return result.ExitCode;
    }

    private static int HandleUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("DiffPlex.ConsoleRunner - Generate unified diff (unidiff) output and three-way diffs/merges");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  DiffPlex.ConsoleRunner file <old-file> <new-file> [options]");
        Console.WriteLine("  DiffPlex.ConsoleRunner text <old-string> <new-string> [options]");
        Console.WriteLine("  DiffPlex.ConsoleRunner 3way-file <base-file> <old-file> <new-file> [options]");
        Console.WriteLine("  DiffPlex.ConsoleRunner 3way-text <base-string> <old-string> <new-string> [options]");
        Console.WriteLine("  DiffPlex.ConsoleRunner merge-file <base-file> <old-file> <new-file> [options]");
        Console.WriteLine("  DiffPlex.ConsoleRunner merge-text <base-string> <old-string> <new-string> [options]");
        Console.WriteLine();
        Console.WriteLine("Commands:");
        Console.WriteLine("  file        Compare two files and generate a unidiff");
        Console.WriteLine("  text        Compare two strings and generate a unidiff");
        Console.WriteLine("  3way-file   Compare three files and generate three-way diff");
        Console.WriteLine("  3way-text   Compare three strings and generate three-way diff");
        Console.WriteLine("  merge-file  Merge three files and generate merged result");
        Console.WriteLine("  merge-text  Merge three strings and generate merged result");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -i, --ignore-case        Ignore case differences");
        Console.WriteLine("  -w, --ignore-whitespace  Ignore whitespace differences");
        Console.WriteLine();
        Console.WriteLine("Exit codes:");
        Console.WriteLine("  0    Success (no conflicts for merge operations)");
        Console.WriteLine("  1    Error or conflicts found");
        Console.WriteLine();
        Console.WriteLine("File paths work with both Windows and Unix-style paths.");
    }
    
    private static string Handle3WayFileDiff(string basePath, string oldPath, string newPath, CommandLineOptions options)
    {
        basePath = NormalizePath(basePath);
        oldPath = NormalizePath(oldPath);
        newPath = NormalizePath(newPath);

        if (!File.Exists(basePath))
        {
            throw new FileNotFoundException($"Base file not found: {basePath}");
        }
        if (!File.Exists(oldPath))
        {
            throw new FileNotFoundException($"Old file not found: {oldPath}");
        }
        if (!File.Exists(newPath))
        {
            throw new FileNotFoundException($"New file not found: {newPath}");
        }

        string baseText = File.ReadAllText(basePath);
        string oldText = File.ReadAllText(oldPath);
        string newText = File.ReadAllText(newPath);

        return Handle3WayTextDiff(baseText, oldText, newText, options);
    }

    private static string Handle3WayTextDiff(string baseText, string oldText, string newText, CommandLineOptions options)
    {
        var differ = ThreeWayDiffer.Instance;
        var result = differ.CreateDiffs(baseText, oldText, newText, options.IgnoreWhitespace, options.IgnoreCase, LineChunker.Instance);

        return ThreeWayDiffRenderer.RenderDiff(result);
    }

    private static MergeResult Handle3WayFileMerge(string basePath, string oldPath, string newPath, CommandLineOptions options)
    {
        basePath = NormalizePath(basePath);
        oldPath = NormalizePath(oldPath);
        newPath = NormalizePath(newPath);

        if (!File.Exists(basePath))
        {
            throw new FileNotFoundException($"Base file not found: {basePath}");
        }
        if (!File.Exists(oldPath))
        {
            throw new FileNotFoundException($"Old file not found: {oldPath}");
        }
        if (!File.Exists(newPath))
        {
            throw new FileNotFoundException($"New file not found: {newPath}");
        }

        string baseText = File.ReadAllText(basePath);
        string oldText = File.ReadAllText(oldPath);
        string newText = File.ReadAllText(newPath);

        return Handle3WayTextMerge(baseText, oldText, newText, options);
    }

    private static MergeResult Handle3WayTextMerge(string baseText, string oldText, string newText, CommandLineOptions options)
    {
        var differ = ThreeWayDiffer.Instance;
        var result = differ.CreateMerge(baseText, oldText, newText, options.IgnoreWhitespace, options.IgnoreCase, LineChunker.Instance);

        return new MergeResult
        {
            Output = result.IsSuccessful 
                ? string.Join(Environment.NewLine, result.MergedPieces)
                : ThreeWayMergeRenderer.RenderMergeWithConflicts(result),
            ExitCode = result.IsSuccessful ? 0 : 1
        };
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

    private class CommandLineOptions
    {
        public string Command { get; set; } = string.Empty;
        public string[] Arguments { get; set; } = Array.Empty<string>();
        public bool IgnoreCase { get; set; }
        public bool IgnoreWhitespace { get; set; }
    }

    private class MergeResult
    {
        public string Output { get; set; } = string.Empty;
        public int ExitCode { get; set; }
    }
}

public static class ThreeWayDiffRenderer
{
    public static string RenderDiff(ThreeWayDiffResult result)
    {
        var output = new System.Text.StringBuilder();
        output.AppendLine("=== Three-Way Diff ===");
        output.AppendLine($"Base: {result.PiecesBase.Count} lines");
        output.AppendLine($"Old:  {result.PiecesOld.Count} lines");
        output.AppendLine($"New:  {result.PiecesNew.Count} lines");
        output.AppendLine($"Blocks: {result.DiffBlocks.Count}");
        output.AppendLine();

        foreach (var block in result.DiffBlocks)
        {
            output.AppendLine($"@@@ {block.ChangeType} @@@");
            output.AppendLine($"Base[{block.BaseStart}..{block.BaseStart + block.BaseCount - 1}] ({block.BaseCount})");
            output.AppendLine($"Old [{block.OldStart}..{block.OldStart + block.OldCount - 1}] ({block.OldCount})");
            output.AppendLine($"New [{block.NewStart}..{block.NewStart + block.NewCount - 1}] ({block.NewCount})");

            // Show content for each section
            if (block.BaseCount > 0)
            {
                output.AppendLine("Base content:");
                for (int i = 0; i < block.BaseCount; i++)
                {
                    output.AppendLine($"  = {result.PiecesBase[block.BaseStart + i]}");
                }
            }

            if (block.OldCount > 0)
            {
                output.AppendLine("Old content:");
                for (int i = 0; i < block.OldCount; i++)
                {
                    output.AppendLine($"  < {result.PiecesOld[block.OldStart + i]}");
                }
            }

            if (block.NewCount > 0)
            {
                output.AppendLine("New content:");
                for (int i = 0; i < block.NewCount; i++)
                {
                    output.AppendLine($"  > {result.PiecesNew[block.NewStart + i]}");
                }
            }

            output.AppendLine();
        }

        return output.ToString();
    }
}

public static class ThreeWayMergeRenderer
{
    public static string RenderMergeWithConflicts(ThreeWayMergeResult result)
    {
        var output = new System.Text.StringBuilder();
        output.AppendLine($"=== Merge Result (with {result.ConflictBlocks.Count} conflicts) ===");
        output.AppendLine();
        output.AppendLine(string.Join(Environment.NewLine, result.MergedPieces));
        
        if (result.ConflictBlocks.Count > 0)
        {
            output.AppendLine();
            output.AppendLine("=== Conflict Summary ===");
            foreach (var conflict in result.ConflictBlocks)
            {
                output.AppendLine($"Conflict at merged line {conflict.MergedStart}:");
                output.AppendLine($"  Base: {conflict.BasePieces.Count} lines");
                output.AppendLine($"  Old:  {conflict.OldPieces.Count} lines");
                output.AppendLine($"  New:  {conflict.NewPieces.Count} lines");
            }
        }

        return output.ToString();
    }
}
