using DiffPlex.Model;

namespace DiffPlex
{
    /// <summary>
    /// Responsible for generating differences between texts
    /// </summary>
    public partial interface IDiffer
    {
        /// <summary>
        /// Creates a diff by comparing text line by line.
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