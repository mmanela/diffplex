using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.Benchmarks;

internal static class Program
{
    public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, launchCount: 1, warmupCount: 1, iterationCount: 3)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CoreDiffBenchmarks
{
    private static readonly char[] WordSeparators = { ' ', '\t', '.', '(', ')', '{', '}', ',', '!', '?', ';' };

    private readonly Differ differ = new();
    private readonly InlineDiffBuilder inlineDiffBuilder;
    private readonly SideBySideDiffBuilder sideBySideDiffBuilder;
    private BenchmarkInput input = null!;

    public CoreDiffBenchmarks()
    {
        inlineDiffBuilder = new InlineDiffBuilder(differ);
        sideBySideDiffBuilder = new SideBySideDiffBuilder(differ);
    }

    [Params(InputScenario.Identical, InputScenario.ManySmallChanges, InputScenario.FewLargeChanges, InputScenario.CompletelyDifferent, InputScenario.LongCommonSubsequence)]
    public InputScenario Scenario { get; set; }

    [Params(InputSize.Small, InputSize.Large)]
    public InputSize Size { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        input = BenchmarkInput.Create(Scenario, Size);
    }

    [Benchmark(Baseline = true)]
    public DiffResult CreateDiffs_LineChunker()
    {
        return differ.CreateDiffs(input.OldText, input.NewText, ignoreWhiteSpace: false, ignoreCase: false, LineChunker.Instance);
    }

    [Benchmark]
    public DiffResult CreateLineDiffs()
    {
        return differ.CreateLineDiffs(input.OldText, input.NewText, ignoreWhitespace: false);
    }

    [Benchmark]
    public DiffResult CreateWordDiffs()
    {
        return differ.CreateWordDiffs(input.OldText, input.NewText, ignoreWhitespace: false, WordSeparators);
    }

    [Benchmark]
    public DiffResult CreateCharacterDiffs()
    {
        return differ.CreateCharacterDiffs(input.CharacterOldText, input.CharacterNewText, ignoreWhitespace: false);
    }

    [Benchmark]
    public DiffPaneModel BuildInlineDiffModel()
    {
        return inlineDiffBuilder.BuildDiffModel(input.OldText, input.NewText, ignoreWhitespace: false, ignoreCase: false, LineChunker.Instance);
    }

    [Benchmark]
    public SideBySideDiffModel BuildSideBySideDiffModel()
    {
        return sideBySideDiffBuilder.BuildDiffModel(input.OldText, input.NewText, ignoreWhitespace: false);
    }
}

public enum InputScenario
{
    Identical,
    ManySmallChanges,
    FewLargeChanges,
    CompletelyDifferent,
    LongCommonSubsequence
}

public enum InputSize
{
    Small,
    Large
}

internal sealed record BenchmarkInput(string OldText, string NewText, string CharacterOldText, string CharacterNewText)
{
    public static BenchmarkInput Create(InputScenario scenario, InputSize size)
    {
        int lineCount = size == InputSize.Small ? 25 : 1_500;
        int characterCount = size == InputSize.Small ? 250 : 1_000;

        var oldLines = CreateLines(lineCount, "old");
        var newLines = scenario switch
        {
            InputScenario.Identical => oldLines.ToArray(),
            InputScenario.ManySmallChanges => ManySmallChanges(oldLines),
            InputScenario.FewLargeChanges => FewLargeChanges(oldLines),
            InputScenario.CompletelyDifferent => CreateLines(lineCount, "new"),
            InputScenario.LongCommonSubsequence => LongCommonSubsequence(oldLines),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
        };

        string oldText = string.Join('\n', oldLines);
        string newText = string.Join('\n', newLines);

        string characterOldText = CreateCharacterText(characterCount, "abcdefghij");
        string characterNewText = scenario switch
        {
            InputScenario.Identical => characterOldText,
            InputScenario.ManySmallChanges => ReplaceEvery(characterOldText, every: 19, 'Z'),
            InputScenario.FewLargeChanges => ReplaceRange(characterOldText, characterCount / 3, characterCount / 5, 'X'),
            InputScenario.CompletelyDifferent => CreateCharacterText(characterCount, "ZYXWVUTSRQ"),
            InputScenario.LongCommonSubsequence => characterOldText.Insert(characterCount / 2, CreateCharacterText(characterCount / 10, "lcs")),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
        };

        return new BenchmarkInput(oldText, newText, characterOldText, characterNewText);
    }

    private static string[] CreateLines(int lineCount, string prefix)
    {
        var lines = new string[lineCount];
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = $"{prefix} line {i:D5}: common-token-{i % 17} value-{(i * 31) % 997} words for diffing";
        }

        return lines;
    }

    private static string[] ManySmallChanges(string[] oldLines)
    {
        var newLines = oldLines.ToArray();
        for (int i = 4; i < newLines.Length; i += 10)
        {
            newLines[i] = newLines[i] + " changed";
        }

        return newLines;
    }

    private static string[] FewLargeChanges(string[] oldLines)
    {
        var newLines = oldLines.ToArray();
        int blockLength = Math.Max(3, oldLines.Length / 5);
        int start = Math.Max(0, (oldLines.Length - blockLength) / 2);

        for (int i = 0; i < blockLength; i++)
        {
            newLines[start + i] = $"replacement block line {i:D5}: updated content with different tokens {i % 11}";
        }

        return newLines;
    }

    private static string[] LongCommonSubsequence(string[] oldLines)
    {
        var newLines = new List<string>(oldLines.Length + Math.Max(1, oldLines.Length / 20));
        for (int i = 0; i < oldLines.Length; i++)
        {
            if (i % 50 == 0)
            {
                newLines.Add($"inserted anchor line {i:D5}: preserves a long common subsequence around edits");
            }

            if (i % 75 != 0)
            {
                newLines.Add(oldLines[i]);
            }
        }

        return newLines.ToArray();
    }

    private static string CreateCharacterText(int characterCount, string pattern)
    {
        var builder = new System.Text.StringBuilder(characterCount);
        while (builder.Length < characterCount)
        {
            builder.Append(pattern);
        }

        return builder.ToString(0, characterCount);
    }

    private static string ReplaceEvery(string text, int every, char replacement)
    {
        var chars = text.ToCharArray();
        for (int i = every - 1; i < chars.Length; i += every)
        {
            chars[i] = replacement;
        }

        return new string(chars);
    }

    private static string ReplaceRange(string text, int start, int length, char replacement)
    {
        var chars = text.ToCharArray();
        for (int i = start; i < Math.Min(chars.Length, start + length); i++)
        {
            chars[i] = replacement;
        }

        return new string(chars);
    }
}
