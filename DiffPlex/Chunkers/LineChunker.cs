using System;
using System.Collections.Generic;

namespace DiffPlex.Chunkers;

public class LineChunker : IChunker
{
    private readonly string[] lineSeparators = ["\r\n", "\r", "\n"];

    /// <summary>
    /// Gets the default singleton instance of the chunker.
    /// </summary>
    public static LineChunker Instance { get; } = new();

    public IReadOnlyList<string> Chunk(string text)
    {
        return text.Split(lineSeparators, StringSplitOptions.None);
    }
}