using DiffPlex.TextDiffer.Model;

namespace DiffPlex.TextDiffer
{
    public interface ITextDiffBuilder
    {
        DiffModel BuildDiffModel(string oldText, string newText);
    }
}