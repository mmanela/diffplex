using System.Collections.Generic;

namespace DiffPlex.Model
{
    /// <summary>
    /// The result of a three-way merge operation
    /// </summary>
    public class ThreeWayMergeResult
    {
        /// <summary>
        /// The merged text pieces
        /// </summary>
        public IReadOnlyList<string> MergedPieces { get; }

        /// <summary>
        /// Whether the merge was successful without conflicts
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// List of conflict blocks that could not be automatically merged
        /// </summary>
        public IList<ThreeWayConflictBlock> ConflictBlocks { get; }

        /// <summary>
        /// The three-way diff result that was used to create this merge
        /// </summary>
        public ThreeWayDiffResult DiffResult { get; }

        public ThreeWayMergeResult(IReadOnlyList<string> mergedPieces, bool isSuccessful, 
            IList<ThreeWayConflictBlock> conflictBlocks, ThreeWayDiffResult diffResult)
        {
            MergedPieces = mergedPieces;
            IsSuccessful = isSuccessful;
            ConflictBlocks = conflictBlocks;
            DiffResult = diffResult;
        }
    }
}
