using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.DiffBuilder
{
    public interface ISideBySideDiffBuilder
    {
        SideBySideDiffModel BuildDiffModel(string oldText, string newText);
    }
}