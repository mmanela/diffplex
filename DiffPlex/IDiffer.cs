﻿using System;
using DiffPlex.Model;

namespace DiffPlex
{
    /// <summary>
    /// Responsible for generating differences between texts
    /// </summary>
    public interface IDiffer
    {
        DiffResult CreateLineDiffs(string oldText, string newText, bool ignoreWhitespace);
        DiffResult CreateLineDiffs(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase);
        DiffResult CreateCharacterDiffs(string oldText, string newText, bool ignoreWhitespace);
        DiffResult CreateCharacterDiffs(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase);
        DiffResult CreateWordDiffs(string oldText, string newText, bool ignoreWhitespace, char[] separators);
        DiffResult CreateWordDiffs(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase, char[] separators);
        DiffResult CreateCustomDiffs(string oldText, string newText, bool ignoreWhiteSpace, Func<string, string[]> chunker);
        DiffResult CreateCustomDiffs(string oldText, string newText, bool ignoreWhiteSpace, bool ignoreCase, Func<string, string[]> chunker);

        /// <summary>
        /// Create a diff by comparing text line by line
        /// </summary>
        /// <param name="oldText">The old text.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhiteSpace">if set to <c>true</c> will ignore white space when determining if lines are the same.</param>
        /// <param name="ignoreCase">Determine if the text comparision is case sensitive or not</param>
        /// <param name="chunker">Component responsible for tokenizing the compared texts</param>
        /// <returns>A DiffResult object which details the differences</returns>
        DiffResult CreateDiffs(string oldText, string newText, bool ignoreWhiteSpace, bool ignoreCase, IChunker chunker);
    }
}