using System;
using System.Collections.Generic;

namespace DiffPlex.Chunkers;

public class LineChunker : IChunker
#if !NET_TOO_OLD_VER
    , ISpanChunker
#endif
{
    private readonly string[] lineSeparators = new[] {"\r\n", "\r", "\n"};

    /// <summary>
    /// Gets the default singleton instance of the chunker.
    /// </summary>
    public static LineChunker Instance { get; } = new LineChunker();

    public IReadOnlyList<string> Chunk(string text)
    {
        return text.Split(lineSeparators, StringSplitOptions.None);
    }

#if !NET_TOO_OLD_VER
    public IReadOnlyList<string> Chunk(ReadOnlySpan<char> text)
    {
        // MemoryExtensions.Split(text, lineSeparators, StringSplitOptions.None);
        return text.ToString().Split(lineSeparators, StringSplitOptions.None);
    }
#endif
}