using System;
using System.Collections.Generic;
using System.Linq;

namespace DiffPlex.DiffBuilder.Model;

/// <summary>
/// The model of diff lines.
/// </summary>
#if !NET_TOO_OLD_VER
[System.Text.Json.Serialization.JsonConverter(typeof(JsonDiffPaneConverter))]
#endif
public class DiffPaneModel
{
    /// <summary>
    /// Initializes a new instance of the DiffPaneModel class.
    /// </summary>
    /// <param name="lines">The lines.</param>
    public DiffPaneModel(IEnumerable<DiffPiece> lines)
    {
#if NETSTANDARD1_0
        if (lines is null)
            Lines = new List<DiffPiece>();
        else if (lines is IReadOnlyList<DiffPiece> col)
            Lines = col;
        else
            Lines = lines.ToList();
#else
        if (lines is List<DiffPiece> list)
            Lines = list.AsReadOnly();
        else if (lines is IReadOnlyList<DiffPiece> col)
            Lines = col;
        else if (lines is null)
            Lines = new List<DiffPiece>();
        else
            Lines = lines.ToList().AsReadOnly();
#endif
    }

    /// <summary>
    /// Gets all the lines.
    /// </summary>
    public IReadOnlyList<DiffPiece> Lines { get; }

    /// <summary>
    /// Gets the count of lines.
    /// </summary>
    public int Count => Lines.Count;

    /// <summary>
    /// Gets a value indicating whether it contains any difference.
    /// </summary>
    public bool HasDifferences => Lines.Any(x => x.Type != ChangeType.Unchanged);

    /// <summary>
    /// Concatenates the members of a constructed this collection of type string, using the specified separator between each line.
    /// </summary>
    /// <param name="separator">The line separator which is included in the returned string only if values has multiple lines.</param>
    /// <param name="lineGenerator">The handler to format each line.</param>
    /// <param name="skipNull">true if skip null returned by the lineGenerator; otherwise, false.</param>
    /// <returns>A string that consists of the elements of values delimited by the separator string; or empty if values has zero elements.</returns>
    public string Join(string separator, Func<DiffPiece, string> lineGenerator, bool skipNull = false)
    {
        var col = Lines.Select(lineGenerator);
        if (skipNull) col = col.Where(ele => ele != null);
        return string.Join(separator, col);
    }

    /// <summary>
    /// Concatenates the members of a constructed this collection of type string in each line.
    /// </summary>
    /// <param name="lineGenerator">The handler to format each line.</param>
    /// <param name="skipNull">true if skip null returned by the lineGenerator; otherwise, false.</param>
    /// <returns>A string that consists of the elements of values delimited by the separator string; or empty if values has zero elements.</returns>
    public string Join(Func<DiffPiece, string> lineGenerator, bool skipNull = false)
        => Join(Environment.NewLine, lineGenerator, skipNull);

    /// <summary>
    /// Concatenates the members of a constructed this collection of type string, using the specified separator between each line.
    /// </summary>
    /// <param name="separator">The line separator which is included in the returned string only if values has multiple lines.</param>
    /// <param name="lineGenerator">The handler to format each line.</param>
    /// <param name="skipNull">true if skip null returned by the lineGenerator; otherwise, false.</param>
    /// <returns>A string that consists of the elements of values delimited by the separator string; or empty if values has zero elements.</returns>
    public string Join(string separator, Func<DiffPiece, int, string> lineGenerator, bool skipNull = false)
    {
        var col = Lines.Select(lineGenerator);
        if (skipNull) col = col.Where(ele => ele != null);
        return string.Join(separator, col);
    }

    /// <summary>
    /// Concatenates the members of a constructed this collection of type string in each line.
    /// </summary>
    /// <param name="lineGenerator">The handler to format each line.</param>
    /// <param name="skipNull">true if skip null returned by the lineGenerator; otherwise, false.</param>
    /// <returns>A string that consists of the elements of values delimited by the separator string; or empty if values has zero elements.</returns>
    public string Join(Func<DiffPiece, int, string> lineGenerator, bool skipNull = false)
        => Join(Environment.NewLine, lineGenerator, skipNull);

    /// <summary>
    /// Concatenates the members of a constructed this collection of type string in each line.
    /// </summary>
    /// <returns>A string that consists of the elements of values delimited by the separator string; or empty if values has zero elements.</returns>
    public string Join()
        => Join(Environment.NewLine, FormatToString, true);

    private static string FormatToString(DiffPiece value)
        => value?.ToString();
}
