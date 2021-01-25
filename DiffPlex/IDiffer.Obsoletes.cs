using System;
using DiffPlex.Model;

namespace DiffPlex
{
    /// <summary>
    /// Responsible for generating differences between texts
    /// </summary>
    public partial interface IDiffer
    {
        [Obsolete("Use CreateDiffs method instead", false)]
        DiffResult CreateLineDiffs(string oldText, string newText, bool ignoreWhitespace);

        [Obsolete("Use CreateDiffs method instead", false)]
        DiffResult CreateLineDiffs(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase);

        [Obsolete("Use CreateDiffs method instead", false)]
        DiffResult CreateCharacterDiffs(string oldText, string newText, bool ignoreWhitespace);

        [Obsolete("Use CreateDiffs method instead", false)]
        DiffResult CreateCharacterDiffs(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase);

        [Obsolete("Use CreateDiffs method instead", false)]
        DiffResult CreateWordDiffs(string oldText, string newText, bool ignoreWhitespace, char[] separators);

        [Obsolete("Use CreateDiffs method instead", false)]
        DiffResult CreateWordDiffs(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase, char[] separators);

        [Obsolete("Use CreateDiffs method instead", false)]
        DiffResult CreateCustomDiffs(string oldText, string newText, bool ignoreWhiteSpace, Func<string, string[]> chunker);

        [Obsolete("Use CreateDiffs method instead", false)]
        DiffResult CreateCustomDiffs(string oldText, string newText, bool ignoreWhiteSpace, bool ignoreCase, Func<string, string[]> chunker);
    }
}