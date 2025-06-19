using DiffPlex.Model;

namespace DiffPlex
{
    /// <summary>
    /// Responsible for generating three-way differences and merges between texts
    /// </summary>
    public interface IThreeWayDiffer
    {
        /// <summary>
        /// Creates a three-way diff by comparing base, old, and new text line by line.
        /// </summary>
        /// <param name="baseText">The common base text.</param>
        /// <param name="oldText">The old version of the text.</param>
        /// <param name="newText">The new version of the text.</param>
        /// <param name="ignoreWhiteSpace">If set to <see langword="true"/> will ignore white space when determining if lines are the same.</param>
        /// <param name="ignoreCase">Determine if the text comparison is case sensitive or not</param>
        /// <param name="chunker">Component responsible for tokenizing the compared texts</param>
        /// <returns>A <see cref="ThreeWayDiffResult"/> object which details the differences</returns>
        ThreeWayDiffResult CreateDiffs(string baseText, string oldText, string newText, bool ignoreWhiteSpace, bool ignoreCase, IChunker chunker);

        /// <summary>
        /// Creates a three-way merge by comparing base, old, and new text line by line.
        /// </summary>
        /// <param name="baseText">The common base text.</param>
        /// <param name="oldText">The old version of the text.</param>
        /// <param name="newText">The new version of the text.</param>
        /// <param name="ignoreWhiteSpace">If set to <see langword="true"/> will ignore white space when determining if lines are the same.</param>
        /// <param name="ignoreCase">Determine if the text comparison is case sensitive or not</param>
        /// <param name="chunker">Component responsible for tokenizing the compared texts</param>
        /// <returns>A <see cref="ThreeWayMergeResult"/> object which contains the merged result and conflict information</returns>
        ThreeWayMergeResult CreateMerge(string baseText, string oldText, string newText, bool ignoreWhiteSpace, bool ignoreCase, IChunker chunker);
    }
}
