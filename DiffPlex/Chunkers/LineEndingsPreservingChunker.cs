using System;
using System.Collections.Generic;

namespace DiffPlex.Chunkers;

public class LineEndingsPreservingChunker : IChunker
#if !NET_TOO_OLD_VER
        , ISpanChunker
#endif
{
    /// <summary>
    /// Gets the default singleton instance of the chunker.
    /// </summary>
    public static LineEndingsPreservingChunker Instance { get; } = new LineEndingsPreservingChunker();

    public IReadOnlyList<string> Chunk(string text)
    {
        var output = new List<string>();
        if (string.IsNullOrEmpty(text)) return output;
        var lastCut = 0;
        for (var currentPosition = 0; currentPosition < text.Length; currentPosition++)
        {
            char ch = text[currentPosition];
            switch (ch)
            {
                case '\n':
                case '\r':
                    currentPosition++;
                    if (ch == '\r' && currentPosition < text.Length && text[currentPosition] == '\n')
                    {
                        currentPosition++;
                    }
                    var str = text.Substring(lastCut, currentPosition - lastCut);
                    lastCut = currentPosition;
                    output.Add(str);
                    break;
                default:
                    continue;
            }
        }

        if (lastCut != text.Length)
        {
            var str = text.Substring(lastCut, text.Length - lastCut);
            output.Add(str);
        }

        return output;
    }

#if !NET_TOO_OLD_VER
    public IReadOnlyList<string> Chunk(ReadOnlySpan<char> text)
    {
        var output = new List<string>();
        if (text.Length == 0) return output;
        var lastCut = 0;
        for (var currentPosition = 0; currentPosition < text.Length; currentPosition++)
        {
            char ch = text[currentPosition];
            switch (ch)
            {
                case '\n':
                case '\r':
                    currentPosition++;
                    if (ch == '\r' && currentPosition < text.Length && text[currentPosition] == '\n')
                    {
                        currentPosition++;
                    }
                    var str = text.Slice(lastCut, currentPosition - lastCut);
                    lastCut = currentPosition;
                    output.Add(str.ToString());
                    break;
                default:
                    continue;
            }
        }

        if (lastCut != text.Length)
        {
            var str = text.Slice(lastCut, text.Length - lastCut);
            output.Add(str.ToString());
        }

        return output;
    }
#endif
}