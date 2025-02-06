using System;
using System.Collections.Generic;

namespace DiffPlex.Chunkers;

public class CustomFunctionChunker : IChunker
#if !NET_TOO_OLD_VER
    , ISpanChunker
#endif
{
    private readonly Func<string, IReadOnlyList<string>> customChunkerFunc;

    public CustomFunctionChunker(Func<string, IReadOnlyList<string>> customChunkerFunc)
    {
        if (customChunkerFunc == null) throw new ArgumentNullException(nameof(customChunkerFunc));
        this.customChunkerFunc = customChunkerFunc;
    }

    public IReadOnlyList<string> Chunk(string text)
    {
        return customChunkerFunc(text);
    }

#if !NET_TOO_OLD_VER
    public IReadOnlyList<string> Chunk(ReadOnlySpan<char> text)
    {
        return customChunkerFunc(text.ToString());
    }
#endif
}