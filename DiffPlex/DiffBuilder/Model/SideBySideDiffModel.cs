namespace DiffPlex.DiffBuilder.Model;

/// <summary>
/// A model which represents differences between to texts to be shown side by side
/// </summary>
public class SideBySideDiffModel
{
    public DiffPaneModel OldText { get; } = new();
    public DiffPaneModel NewText { get; } = new();
}