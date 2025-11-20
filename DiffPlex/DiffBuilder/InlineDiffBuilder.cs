using System;
using System.Collections.Generic;
using System.Linq;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.DiffBuilder
{
    public class InlineDiffBuilder : IInlineDiffBuilder
    {
        private readonly IDiffer differ;

        private delegate ChangeType PieceBuilder(string oldText, string newText, List<DiffPiece> pieces, bool ignoreWhitespace, bool ignoreCase);

        /// <summary>
        /// Gets the default singleton instance of the inline diff builder.
        /// </summary>
        public static InlineDiffBuilder Instance { get; } = new InlineDiffBuilder();

        public InlineDiffBuilder(IDiffer differ = null)
        {
            this.differ = differ ?? Differ.Instance;
        }

        public DiffPaneModel BuildDiffModel(string oldText, string newText)
            => BuildDiffModel(oldText, newText, ignoreWhitespace: true);

        public DiffPaneModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace)
        {
            var chunker = new LineChunker();
            return BuildDiffModel(oldText, newText, ignoreWhitespace, false, chunker);
        }

        public DiffPaneModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase, IChunker chunker)
        {
            if (oldText == null) throw new ArgumentNullException(nameof(oldText));
            if (newText == null) throw new ArgumentNullException(nameof(newText));

            var model = new DiffPaneModel();
            var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhitespace, ignoreCase: ignoreCase, chunker);
            BuildDiffPieces(diffResult, model.Lines);
            
            return model;
        }

        /// <summary>
        /// Gets the inline textual diffs.
        /// </summary>
        /// <param name="oldText">The old text to diff.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhiteSpace"><see langword="true"/> if ignore the white space; otherwise, <see langword="false"/>.</param>
        /// <param name="ignoreCase"><see langword="true"/> if case-insensitive; otherwise, <see langword="false"/>.</param>
        /// <param name="chunker">The chunker.</param>
        /// <returns>The diffs result.</returns>
        public static DiffPaneModel Diff(string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false, IChunker chunker = null)
        {
            return Diff(Differ.Instance, oldText, newText, ignoreWhiteSpace, ignoreCase, chunker);
        }

        /// <summary>
        /// Gets the inline textual diffs.
        /// </summary>
        /// <param name="differ">The differ instance.</param>
        /// <param name="oldText">The old text to diff.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhiteSpace"><see langword="true"/> if ignore the white space; otherwise, <see langword="false"/>.</param>
        /// <param name="ignoreCase"><see langword="true"/> if case-insensitive; otherwise, <see langword="false"/>.</param>
        /// <param name="chunker">The chunker.</param>
        /// <returns>The diffs result.</returns>
        public static DiffPaneModel Diff(IDiffer differ, string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false, IChunker chunker = null)
        {
            if (oldText == null) throw new ArgumentNullException(nameof(oldText));
            if (newText == null) throw new ArgumentNullException(nameof(newText));

            var model = new DiffPaneModel();
            var diffResult = (differ ?? Differ.Instance).CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, chunker ?? LineChunker.Instance);
            BuildDiffPieces(diffResult, model.Lines);
            
            return model;
        }

        public static DiffPaneModel Diff(
            IDiffer differ, 
            string oldText, string newText,
            List<IChunker> detailsPack,
            bool ignoreWhiteSpace = true, bool ignoreCase = false)
        {
            if (oldText == null) throw new ArgumentNullException(nameof(oldText));
            if (newText == null) throw new ArgumentNullException(nameof(newText));

            if (differ == null) return Diff(oldText, newText, ignoreWhiteSpace, ignoreCase);

            LinkedList<IChunker> chunkers;
            if (detailsPack == null || !detailsPack.Any())
            {
                chunkers = new LinkedList<IChunker>();
                chunkers.AddLast(DiffPlex.Chunkers.LineChunker.Instance);
                chunkers.AddLast(DiffPlex.Chunkers.WordChunker.Instance);
                chunkers.AddLast(DiffPlex.Chunkers.CharacterChunker.Instance);
            }
            else
            {
                chunkers = new LinkedList<IChunker>(detailsPack);
            }

            var model = new DiffPaneModel();
            var cnode = chunkers.First;
            var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, cnode.Value);
            BuildDiffPieces(diffResult, model.Lines, NextPieceBuilderInternal(differ, cnode.Next), ignoreWhiteSpace, ignoreCase);

            return model;
        }


        private static PieceBuilder NextPieceBuilderInternal(
            IDiffer differ,
            LinkedListNode<IChunker> chunkerNode)
        {
            if (chunkerNode == null)
            {
                return null;
            }
            else
            {
                return (ot, nt, p, iw, ic) =>
                {
                    var r = differ.CreateDiffs(ot, nt, iw, ic, chunkerNode.Value);
                    return BuildDiffPieces(r, p, NextPieceBuilderInternal(differ, chunkerNode.Next), iw, ic);
                };
            }
        }

        private static ChangeType BuildDiffPieces(
            DiffResult diffResult, 
            List<DiffPiece> pieces, PieceBuilder subPieceBuilder, 
            bool ignoreWhiteSpace, bool ignoreCase)
        {
            int aPos = 0;
            int bPos = 0;

            ChangeType changeSummary = ChangeType.Unchanged;

            foreach (var diffBlock in diffResult.DiffBlocks)
            {
                while (bPos < diffBlock.InsertStartB && aPos < diffBlock.DeleteStartA)
                {
                    pieces.Add(new DiffPiece(diffResult.PiecesOld[aPos], ChangeType.Unchanged, aPos + 1));
                    aPos++;
                    bPos++;
                }

                int i = 0;
                for (; i < Math.Min(diffBlock.DeleteCountA, diffBlock.InsertCountB); i++)
                {
                    var piece = new DiffPiece(diffResult.PiecesOld[i + diffBlock.DeleteStartA], ChangeType.Deleted, aPos + 1);
                    //var newPiece = new DiffPiece(diffResult.PiecesNew[i + diffBlock.InsertStartB], ChangeType.Inserted, bPos + 1);

                    if (subPieceBuilder != null)
                    {
                        var subChangeSummary = subPieceBuilder(diffResult.PiecesOld[aPos], diffResult.PiecesNew[bPos], piece.SubPieces, ignoreWhiteSpace, ignoreCase);
                        piece.Type = subChangeSummary;
                    }

                    pieces.Add(piece);
                    aPos++;
                    bPos++;
                }

                if (diffBlock.DeleteCountA > diffBlock.InsertCountB)
                {
                    for (; i < diffBlock.DeleteCountA; i++)
                    {
                        pieces.Add(new DiffPiece(diffResult.PiecesOld[i + diffBlock.DeleteStartA], ChangeType.Deleted, aPos + 1));

                        aPos++;
                    }
                }
                else
                {
                    for (; i < diffBlock.InsertCountB; i++)
                    {
                        pieces.Add(new DiffPiece(diffResult.PiecesNew[i + diffBlock.InsertStartB], ChangeType.Inserted, bPos + 1));

                        bPos++;
                    }
                }
            }

            while (bPos < diffResult.PiecesNew.Length && aPos < diffResult.PiecesOld.Length)
            {
                pieces.Add(new DiffPiece(diffResult.PiecesOld[aPos], ChangeType.Unchanged, aPos + 1));
                aPos++;
                bPos++;
            }

            // Consider the whole diff as "modified" if we found any change, otherwise we consider it unchanged
            if (pieces.Any(x => x.Type == ChangeType.Modified || x.Type == ChangeType.Inserted || x.Type == ChangeType.Deleted))
            {
                changeSummary = ChangeType.Modified;
            }

            return changeSummary;
        }


        private static void BuildDiffPieces(DiffResult diffResult, List<DiffPiece> pieces)
        {
            int bPos = 0;

            foreach (var diffBlock in diffResult.DiffBlocks)
            {
                for (; bPos < diffBlock.InsertStartB; bPos++)
                    pieces.Add(new DiffPiece(diffResult.PiecesNew[bPos], ChangeType.Unchanged, bPos + 1));

                int i = 0;
                for (; i < Math.Min(diffBlock.DeleteCountA, diffBlock.InsertCountB); i++)
                    pieces.Add(new DiffPiece(diffResult.PiecesOld[i + diffBlock.DeleteStartA], ChangeType.Deleted));

                i = 0;
                for (; i < Math.Min(diffBlock.DeleteCountA, diffBlock.InsertCountB); i++)
                {
                    pieces.Add(new DiffPiece(diffResult.PiecesNew[i + diffBlock.InsertStartB], ChangeType.Inserted, bPos + 1));
                    bPos++;
                }

                if (diffBlock.DeleteCountA > diffBlock.InsertCountB)
                {
                    for (; i < diffBlock.DeleteCountA; i++)
                        pieces.Add(new DiffPiece(diffResult.PiecesOld[i + diffBlock.DeleteStartA], ChangeType.Deleted));
                }
                else
                {
                    for (; i < diffBlock.InsertCountB; i++)
                    {
                        pieces.Add(new DiffPiece(diffResult.PiecesNew[i + diffBlock.InsertStartB], ChangeType.Inserted, bPos + 1));
                        bPos++;
                    }
                }
            }

            for (; bPos < diffResult.PiecesNew.Count; bPos++)
                pieces.Add(new DiffPiece(diffResult.PiecesNew[bPos], ChangeType.Unchanged, bPos + 1));
        }
    }
}
