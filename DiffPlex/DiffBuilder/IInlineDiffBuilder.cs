using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.DiffBuilder
{
    public interface IInlineDiffBuilder
    {
        DiffPaneModel BuildDiffModel(string oldText, string newText);
    }
}
