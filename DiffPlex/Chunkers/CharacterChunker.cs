using System;
using System.Collections.Generic;

namespace DiffPlex.Chunkers;

public class CharacterChunker : IChunker
#if !NET_TOO_OLD_VER
    , ISpanChunker
#endif
{
    /// <summary>
    /// Gets the default singleton instance of the chunker.
    /// </summary>
    public static CharacterChunker Instance { get; } = new CharacterChunker();

    public IReadOnlyList<string> Chunk(string text)
    {
        var s = new string[text.Length];
        for (int i = 0; i < text.Length; i++) s[i] = text[i].ToString();
        return s;
    }

#if !NET_TOO_OLD_VER
    public IReadOnlyList<string> Chunk(ReadOnlySpan<char> text)
    {
        var list = new List<string>();
        for (int i = 0; i < text.Length; i++) list.Add(text[i].ToString());
        return list;
    }
#endif
}