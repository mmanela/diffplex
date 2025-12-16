using System;
using System.Collections.Generic;

namespace DiffPlex.Chunkers;

public class CustomFunctionChunker(Func<string, IReadOnlyList<string>> customChunkerFunc) : IChunker
{
    private readonly Func<string, IReadOnlyList<string>> customChunkerFunc = customChunkerFunc ?? throw new ArgumentNullException(nameof(customChunkerFunc));

    public IReadOnlyList<string> Chunk(string text)
    {
        return customChunkerFunc(text);
    }
}