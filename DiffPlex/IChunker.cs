using System;
using System.Collections.Generic;

namespace DiffPlex;

/// <summary>
/// Responsible for how to turn the document into pieces
/// </summary>
public interface IChunker
{
    /// <summary>
    /// Divide text into sub-parts
    /// </summary>
    IReadOnlyList<string> Chunk(string text);
}

#if !NET_TOO_OLD_VER
/// <summary>
/// Responsible for how to turn the document into pieces
/// </summary>
public interface ISpanChunker : IChunker
{
    /// <summary>
    /// Divide text into sub-parts
    /// </summary>
    IReadOnlyList<string> Chunk(ReadOnlySpan<char> text);
}
#endif