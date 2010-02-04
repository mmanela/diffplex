using System;
using DiffPlex.Model;

namespace DiffPlex
{
    public interface IDiffer
    {
        DiffResult CreateLineDiffs(string oldText, string newText, bool ignoreWhiteSpace);
        DiffResult CreateCharacterDiffs(string oldText, string newText, bool ignoreWhitespace);
        DiffResult CreateWordDiffs(string oldText, string newText, bool ignoreWhitespace, char[] separators);
        DiffResult CreateCustomDiffs(string oldText, string newText, bool ignoreWhiteSpace, Func<string, string[]> chunker);
    }
}