using System;
using DiffPlex.Model;

namespace DiffPlex
{
    /// <summary>
    /// Provides methods for generate differences between texts
    /// </summary>
    public interface IDiffer
    {
        /// <summary>
        /// Create a diff by comparing text line by line
        /// </summary>
        /// <param name="oldText">The old text.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhiteSpace">if set to <c>true</c> will ignore white space when determining if lines are the same.</param>
        /// <returns>A DiffResult object which details the differences</returns>
        DiffResult CreateLineDiffs(string oldText, string newText, bool ignoreWhiteSpace);

        /// <summary>
        /// Create a diff by comparing text character by character
        /// </summary>
        /// <param name="oldText">The old text.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhitespace">if set to <c>true</c> will treat all whitespace characters are empty strings.</param>
        /// <returns>A DiffResult object which details the differences</returns>
        DiffResult CreateCharacterDiffs(string oldText, string newText, bool ignoreWhitespace);

        /// <summary>
        /// Create a diff by comparing text word by word
        /// </summary>
        /// <param name="oldText">The old text.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhitespace">if set to <c>true</c> will ignore white space when determining if words are the same.</param>
        /// <param name="separators">The list of characters which define word separators.</param>
        /// <returns>A DiffResult object which details the differences</returns>
        DiffResult CreateWordDiffs(string oldText, string newText, bool ignoreWhitespace, char[] separators);

        /// <summary>
        /// Create a diff by comparing text in chunks determined by the supplied chunker function.
        /// </summary>
        /// <param name="oldText">The old text.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhiteSpace">if set to <c>true</c> will ignore white space when determining if chunks are the same.</param>
        /// <param name="chunker">A function that will break the text into chunks.</param>
        /// <returns>A DiffResult object which details the differences</returns>
        DiffResult CreateCustomDiffs(string oldText, string newText, bool ignoreWhiteSpace, Func<string, string[]> chunker);
    }
}