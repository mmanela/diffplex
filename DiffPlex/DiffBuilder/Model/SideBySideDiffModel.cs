using System.Collections.Generic;

namespace DiffPlex.DiffBuilder.Model;

/// <summary>
/// A model which represents differences between to texts to be shown side by side.
/// </summary>
/// <param name="oldText">The old text information in diff.</param>
/// <param name="newText">The new text information in diff.</param>
#if !NET_TOO_OLD_VER
[System.Text.Json.Serialization.JsonConverter(typeof(JsonSideBySideDiffConverter))]
#endif
public class SideBySideDiffModel(DiffPaneModel oldText, DiffPaneModel newText)
{
    /// <summary>
    /// Initializes a new instance of the SideBySideDiffModel class.
    /// </summary>
    /// <param name="oldText">The old text information in diff.</param>
    /// <param name="newText">The new text information in diff.</param>
    public SideBySideDiffModel(IReadOnlyList<DiffPiece> oldText, IReadOnlyList<DiffPiece> newText)
        : this(new DiffPaneModel(oldText), new(newText))
    {
    }

    /// <summary>
    /// Gets the old text model.
    /// </summary>
    public DiffPaneModel OldText { get; } = oldText ?? new(null);

    /// <summary>
    /// Gets the new text model.
    /// </summary>
    public DiffPaneModel NewText { get; } = newText ?? new(null);
}