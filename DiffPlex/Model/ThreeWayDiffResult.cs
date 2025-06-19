using System.Collections.Generic;

namespace DiffPlex.Model
{
    /// <summary>
    /// The result of diffing three pieces of text (base, old, new)
    /// </summary>
    public class ThreeWayDiffResult
    {
        /// <summary>
        /// The chunked pieces of the base text
        /// </summary>
        public IReadOnlyList<string> PiecesBase { get; }

        /// <summary>
        /// The chunked pieces of the old text
        /// </summary>
        public IReadOnlyList<string> PiecesOld { get; }

        /// <summary>
        /// The chunked pieces of the new text
        /// </summary>
        public IReadOnlyList<string> PiecesNew { get; }

        /// <summary>
        /// A collection of ThreeWayDiffBlocks which details the differences between the three texts
        /// </summary>
        public IList<ThreeWayDiffBlock> DiffBlocks { get; }

        public ThreeWayDiffResult(IReadOnlyList<string> piecesBase, IReadOnlyList<string> piecesOld, 
            IReadOnlyList<string> piecesNew, IList<ThreeWayDiffBlock> blocks)
        {
            PiecesBase = piecesBase;
            PiecesOld = piecesOld;
            PiecesNew = piecesNew;
            DiffBlocks = blocks;
        }
    }
}
