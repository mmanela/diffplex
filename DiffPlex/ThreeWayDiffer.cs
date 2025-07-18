using System;
using System.Collections.Generic;
using System.Linq;
using DiffPlex.Model;

namespace DiffPlex
{
    public class ThreeWayDiffer : IThreeWayDiffer
    {
        /// <summary>
        /// Gets the default singleton instance of three-way differ.
        /// </summary>
        public static ThreeWayDiffer Instance { get; } = new ThreeWayDiffer();

        private readonly IDiffer _differ = Differ.Instance;

        public ThreeWayDiffResult CreateDiffs(string baseText, string oldText, string newText,
        bool ignoreWhiteSpace, bool ignoreCase, IChunker chunker)
        {
            if (baseText == null) throw new ArgumentNullException(nameof(baseText));
            if (oldText == null) throw new ArgumentNullException(nameof(oldText));
            if (newText == null) throw new ArgumentNullException(nameof(newText));
            if (chunker == null) throw new ArgumentNullException(nameof(chunker));

            var basePieces = chunker.Chunk(baseText);
            var oldPieces = chunker.Chunk(oldText);
            var newPieces = chunker.Chunk(newText);

            // Create two-way diffs: base->old and base->new
            var baseToOld = _differ.CreateDiffs(baseText, oldText, ignoreWhiteSpace, ignoreCase, chunker);
            var baseToNew = _differ.CreateDiffs(baseText, newText, ignoreWhiteSpace, ignoreCase, chunker);

            var threeWayBlocks = CreateThreeWayDiffBlocks(basePieces, oldPieces, newPieces,
            baseToOld, baseToNew, ignoreWhiteSpace, ignoreCase);

            return new ThreeWayDiffResult(basePieces, oldPieces, newPieces, threeWayBlocks);
        }

        public ThreeWayMergeResult CreateMerge(string baseText, string oldText, string newText,
        bool ignoreWhiteSpace, bool ignoreCase, IChunker chunker)
        {
            var diffResult = CreateDiffs(baseText, oldText, newText, ignoreWhiteSpace, ignoreCase, chunker);

            var mergedPieces = new List<string>();
            var conflictBlocks = new List<ThreeWayConflictBlock>();
            var isSuccessful = true;

            var baseIndex = 0;
            var oldIndex = 0;
            var newIndex = 0;

            foreach (var block in diffResult.DiffBlocks)
            {
                // Add unchanged content before this block
                while (baseIndex < block.BaseStart)
                {
                    mergedPieces.Add(diffResult.PiecesBase[baseIndex]);
                    baseIndex++;
                    oldIndex++;
                    newIndex++;
                }

                switch (block.ChangeType)
                {
                    case ThreeWayChangeType.Unchanged:
                        // Add base content (all are the same)
                        for (int i = 0; i < block.BaseCount; i++)
                        {
                            mergedPieces.Add(diffResult.PiecesBase[baseIndex + i]);
                        }
                        break;

                    case ThreeWayChangeType.OldOnly:
                        // Take old version
                        for (int i = 0; i < block.OldCount; i++)
                        {
                            mergedPieces.Add(diffResult.PiecesOld[oldIndex + i]);
                        }
                        break;

                    case ThreeWayChangeType.NewOnly:
                        // Take new version
                        for (int i = 0; i < block.NewCount; i++)
                        {
                            mergedPieces.Add(diffResult.PiecesNew[newIndex + i]);
                        }
                        break;

                    case ThreeWayChangeType.BothSame:
                        // Both made the same change, take either (we'll take old)
                        for (int i = 0; i < block.OldCount; i++)
                        {
                            mergedPieces.Add(diffResult.PiecesOld[oldIndex + i]);
                        }
                        break;

                    case ThreeWayChangeType.Conflict:
                        // Create conflict block
                        var basePieces = diffResult.PiecesBase.Skip(baseIndex).Take(block.BaseCount).ToList();
                        var oldPieces = diffResult.PiecesOld.Skip(oldIndex).Take(block.OldCount).ToList();
                        var newPieces = diffResult.PiecesNew.Skip(newIndex).Take(block.NewCount).ToList();

                        var conflictBlock = new ThreeWayConflictBlock(mergedPieces.Count, basePieces,
                        oldPieces, newPieces, block);
                        conflictBlocks.Add(conflictBlock);

                        // Add conflict markers
                        mergedPieces.Add("<<<<<<< old");
                        mergedPieces.AddRange(oldPieces);
                        mergedPieces.Add("||||||| base");
                        mergedPieces.AddRange(basePieces);
                        mergedPieces.Add("=======");
                        mergedPieces.AddRange(newPieces);
                        mergedPieces.Add(">>>>>>> new");

                        isSuccessful = false;
                        break;
                }

                baseIndex += block.BaseCount;
                oldIndex += block.OldCount;
                newIndex += block.NewCount;
            }

            // Add remaining unchanged content
            while (baseIndex < diffResult.PiecesBase.Count)
            {
                mergedPieces.Add(diffResult.PiecesBase[baseIndex]);
                baseIndex++;
            }

            return new ThreeWayMergeResult(mergedPieces, isSuccessful, conflictBlocks, diffResult);
        }

        private List<ThreeWayDiffBlock> CreateThreeWayDiffBlocks(IReadOnlyList<string> basePieces,
            IReadOnlyList<string> oldPieces, IReadOnlyList<string> newPieces,
            DiffResult baseToOld, DiffResult baseToNew, bool ignoreWhiteSpace, bool ignoreCase)
        {
            var blocks = new List<ThreeWayDiffBlock>();

            // If no changes, return single unchanged block
            if (baseToOld.DiffBlocks.Count == 0 && baseToNew.DiffBlocks.Count == 0)
            {
                if (basePieces.Count > 0)
                {
                    blocks.Add(new ThreeWayDiffBlock(0, basePieces.Count, 0, basePieces.Count,
                        0, basePieces.Count, ThreeWayChangeType.Unchanged));
                }
                return blocks;
            }

            var baseIndex = 0;
            var oldIndex = 0;
            var newIndex = 0;

            var oldBlockIndex = 0;
            var newBlockIndex = 0;

            while (baseIndex < basePieces.Count)
            {
                var nextOldChange = oldBlockIndex < baseToOld.DiffBlocks.Count
                    ? baseToOld.DiffBlocks[oldBlockIndex].DeleteStartA : int.MaxValue;
                var nextNewChange = newBlockIndex < baseToNew.DiffBlocks.Count
                    ? baseToNew.DiffBlocks[newBlockIndex].DeleteStartA : int.MaxValue;
                var nextChange = Math.Min(nextOldChange, nextNewChange);

                // Add unchanged section before next change
                if (baseIndex < nextChange && nextChange != int.MaxValue)
                {
                    var unchangedCount = nextChange - baseIndex;
                    blocks.Add(new ThreeWayDiffBlock(baseIndex, unchangedCount, oldIndex, unchangedCount,
                        newIndex, unchangedCount, ThreeWayChangeType.Unchanged));

                    baseIndex += unchangedCount;
                    oldIndex += unchangedCount;
                    newIndex += unchangedCount;
                }

                // Process changes at the same base position
                if (baseIndex == nextOldChange && baseIndex == nextNewChange)
                {
                    // Both have changes at same position
                    var oldBlock = baseToOld.DiffBlocks[oldBlockIndex];
                    var newBlock = baseToNew.DiffBlocks[newBlockIndex];

                    var changeType = DetermineChangeType(basePieces, oldPieces, newPieces,
                        oldBlock, newBlock, ignoreWhiteSpace, ignoreCase);

                    blocks.Add(new ThreeWayDiffBlock(baseIndex, oldBlock.DeleteCountA,
                        oldIndex, oldBlock.InsertCountB,
                        newIndex, newBlock.InsertCountB, changeType));

                    baseIndex += oldBlock.DeleteCountA;
                    oldIndex += oldBlock.InsertCountB;
                    newIndex += newBlock.InsertCountB;

                    oldBlockIndex++;
                    newBlockIndex++;
                }
                else if (baseIndex == nextOldChange)
                {
                    // Only old has change
                    var oldBlock = baseToOld.DiffBlocks[oldBlockIndex];
                    blocks.Add(new ThreeWayDiffBlock(baseIndex, oldBlock.DeleteCountA,
                        oldIndex, oldBlock.InsertCountB,
                        newIndex, oldBlock.DeleteCountA, ThreeWayChangeType.OldOnly));

                    baseIndex += oldBlock.DeleteCountA;
                    oldIndex += oldBlock.InsertCountB;
                    newIndex += oldBlock.DeleteCountA;

                    oldBlockIndex++;
                }
                else if (baseIndex == nextNewChange)
                {
                    // Only new has change
                    var newBlock = baseToNew.DiffBlocks[newBlockIndex];
                    blocks.Add(new ThreeWayDiffBlock(baseIndex, newBlock.DeleteCountA,
                        oldIndex, newBlock.DeleteCountA,
                        newIndex, newBlock.InsertCountB, ThreeWayChangeType.NewOnly));

                    baseIndex += newBlock.DeleteCountA;
                    oldIndex += newBlock.DeleteCountA;
                    newIndex += newBlock.InsertCountB;

                    newBlockIndex++;
                }
                else
                {
                    // No more changes, add remaining as unchanged
                    var remainingCount = basePieces.Count - baseIndex;
                    if (remainingCount > 0)
                    {
                        blocks.Add(new ThreeWayDiffBlock(baseIndex, remainingCount, oldIndex, remainingCount,
                            newIndex, remainingCount, ThreeWayChangeType.Unchanged));
                    }
                    break;
                }
            }

            return blocks;
        }

        private ThreeWayChangeType DetermineChangeType(IReadOnlyList<string> basePieces,
            IReadOnlyList<string> oldPieces, IReadOnlyList<string> newPieces,
            DiffBlock oldBlock, DiffBlock newBlock, bool ignoreWhiteSpace, bool ignoreCase)
        {
            if (oldBlock == null || newBlock == null)
            {
                return ThreeWayChangeType.Conflict;
            }

            // Extract the changed content
            var oldContent = oldPieces.Skip(oldBlock.InsertStartB).Take(oldBlock.InsertCountB).ToList();
            var newContent = newPieces.Skip(newBlock.InsertStartB).Take(newBlock.InsertCountB).ToList();

            // Compare the changes
            var comparer = CreateStringComparer(ignoreWhiteSpace, ignoreCase);

            if (oldContent.SequenceEqual(newContent, comparer))
            {
                return ThreeWayChangeType.BothSame;
            }
            else
            {
                return ThreeWayChangeType.Conflict;
            }
        }

        private IEqualityComparer<string> CreateStringComparer(bool ignoreWhiteSpace, bool ignoreCase)
        {
            return new StringComparer(ignoreWhiteSpace, ignoreCase);
        }

        private class StringComparer : IEqualityComparer<string>
        {
            private readonly bool _ignoreWhiteSpace;
            private readonly bool _ignoreCase;

            public StringComparer(bool ignoreWhiteSpace, bool ignoreCase)
            {
                _ignoreWhiteSpace = ignoreWhiteSpace;
                _ignoreCase = ignoreCase;
            }

            public bool Equals(string x, string y)
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;

                var stringX = _ignoreWhiteSpace ? x.Trim() : x;
                var stringY = _ignoreWhiteSpace ? y.Trim() : y;

                return string.Equals(stringX, stringY,
                    _ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
            }

            public int GetHashCode(string obj)
            {
                if (obj == null) return 0;
                var str = _ignoreWhiteSpace ? obj.Trim() : obj;
                return _ignoreCase ? str.ToUpperInvariant().GetHashCode() : str.GetHashCode();
            }
        }
    }
}
