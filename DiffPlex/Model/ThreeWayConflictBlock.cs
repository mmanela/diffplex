using System.Collections.Generic;

namespace DiffPlex.Model
{
    /// <summary>
    /// Represents a conflict block in a three-way merge where automatic resolution was not possible
    /// </summary>
    public class ThreeWayConflictBlock
    {
        /// <summary>
        /// Position in the merged result where this conflict starts
        /// </summary>
        public int MergedStart { get; }

        /// <summary>
        /// The base text pieces for this conflict
        /// </summary>
        public IReadOnlyList<string> BasePieces { get; }

        /// <summary>
        /// Old text pieces for this conflict
        /// </summary>
        public IReadOnlyList<string> OldPieces { get; }

        /// <summary>
        /// New text pieces for this conflict
        /// </summary>
        public IReadOnlyList<string> NewPieces { get; }

        /// <summary>
        /// The original three-way diff block that caused this conflict
        /// </summary>
        public ThreeWayDiffBlock OriginalBlock { get; }

        public ThreeWayConflictBlock(int mergedStart, IReadOnlyList<string> basePieces, 
            IReadOnlyList<string> oldPieces, IReadOnlyList<string> newPieces, 
            ThreeWayDiffBlock originalBlock)
        {
            MergedStart = mergedStart;
            BasePieces = basePieces;
            OldPieces = oldPieces;
            NewPieces = newPieces;
            OriginalBlock = originalBlock;
        }
    }
}
