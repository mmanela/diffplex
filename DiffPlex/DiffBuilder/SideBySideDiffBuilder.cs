using System;
using System.Collections.Generic;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.DiffBuilder
{
    public class SideBySideDiffBuilder : ISideBySideDiffBuilder
    {
        private readonly IDiffer differ;
        private readonly IChunker lineChunker;
        private readonly IChunker wordChunker;

        private delegate void PieceBuilder(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces);
        
        /// <summary>
        /// Gets the default singleton instance.
        /// </summary>
        public static SideBySideDiffBuilder Instance { get; } = new SideBySideDiffBuilder();

        public SideBySideDiffBuilder(IDiffer differ, IChunker lineChunker, IChunker wordChunker)
        {
            this.differ = differ ?? Differ.Instance;
            this.lineChunker = lineChunker ?? throw new ArgumentNullException(nameof(lineChunker));
            this.wordChunker = wordChunker ?? throw new ArgumentNullException(nameof(wordChunker));
        }

        public SideBySideDiffBuilder(IDiffer differ = null) : 
            this(differ, new LineChunker(), new WordChunker())
        {
        }

        public SideBySideDiffBuilder(IDiffer differ, char[] wordSeparators) 
            : this(differ, new LineChunker(), new DelimiterChunker(wordSeparators))
        {
        }

        public SideBySideDiffModel BuildDiffModel(string oldText, string newText)
            => BuildDiffModel(oldText, newText, ignoreWhitespace: true);

        public SideBySideDiffModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace)
        {
            return BuildLineDiff(
                oldText ?? throw new ArgumentNullException(nameof(oldText)),
                newText ?? throw new ArgumentNullException(nameof(newText)),
                ignoreWhitespace);
        }

        /// <summary>
        /// Gets the side-by-side textual diffs.
        /// </summary>
        /// <param name="oldText">The old text to diff.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; othewise, false.</param>
        /// <param name="ignoreCase">true if case-insensitive; otherwise, false.</param>
        /// <returns>The diffs result.</returns>
        public static SideBySideDiffModel Diff(string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false)
        {
            if (oldText == null) throw new ArgumentNullException(nameof(oldText));
            if (newText == null) throw new ArgumentNullException(nameof(newText));

            var model = new SideBySideDiffModel();
            var diffResult = Differ.Instance.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, LineChunker.Instance);
            BuildDiffPieces(diffResult, model.OldText.Lines, model.NewText.Lines, BuildWordDiffPiecesInternal);

            return model;
        }

        /// <summary>
        /// Gets the side-by-side textual diffs.
        /// </summary>
        /// <param name="differ">The differ instance.</param>
        /// <param name="oldText">The old text to diff.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhiteSpace">true if ignore the white space; othewise, false.</param>
        /// <param name="ignoreCase">true if case-insensitive; otherwise, false.</param>
        /// <param name="lineChunker">The line chunker.</param>
        /// <param name="wordChunker">The word chunker.</param>
        /// <returns>The diffs result.</returns>
        public static SideBySideDiffModel Diff(IDiffer differ, string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false, IChunker lineChunker = null, IChunker wordChunker = null)
        {
            if (oldText == null) throw new ArgumentNullException(nameof(oldText));
            if (newText == null) throw new ArgumentNullException(nameof(newText));

            if (differ == null) return Diff(oldText, newText, ignoreWhiteSpace, ignoreCase);
            var model = new SideBySideDiffModel();
            var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, lineChunker ?? LineChunker.Instance);
            BuildDiffPieces(diffResult, model.OldText.Lines, model.NewText.Lines, (ot, nt, op, np) =>
            {
                var r = differ.CreateDiffs(oldText, newText, false, false, wordChunker ?? WordChunker.Instance);
                BuildDiffPieces(r, op, np, null);
            });
            
            return model;
        }

        private static void BuildWordDiffPiecesInternal(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces)
        {
            var diffResult = Differ.Instance.CreateDiffs(oldText, newText, false, false, WordChunker.Instance);
            BuildDiffPieces(diffResult, oldPieces, newPieces, null);
        }

        private SideBySideDiffModel BuildLineDiff(string oldText, string newText, bool ignoreWhitespace)
        {
            var model = new SideBySideDiffModel();
            var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhitespace, false, lineChunker);
            BuildDiffPieces(diffResult, model.OldText.Lines, model.NewText.Lines, BuildWordDiffPieces);
            
            return model;
        }

        private void BuildWordDiffPieces(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces)
        {
            var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhiteSpace: false, false, wordChunker);
            BuildDiffPieces(diffResult, oldPieces, newPieces, subPieceBuilder: null);
        }

        private static void BuildDiffPieces(DiffResult diffResult, List<DiffPiece> oldPieces, List<DiffPiece> newPieces, PieceBuilder subPieceBuilder)
        {
            int aPos = 0;
            int bPos = 0;

            foreach (var diffBlock in diffResult.DiffBlocks)
            {
                while (bPos < diffBlock.InsertStartB && aPos < diffBlock.DeleteStartA)
                {
                    oldPieces.Add(new DiffPiece(diffResult.PiecesOld[aPos], ChangeType.Unchanged, aPos + 1));
                    newPieces.Add(new DiffPiece(diffResult.PiecesNew[bPos], ChangeType.Unchanged, bPos + 1));
                    aPos++;
                    bPos++;
                }

                int i = 0;
                for (; i < Math.Min(diffBlock.DeleteCountA, diffBlock.InsertCountB); i++)
                {
                    var oldPiece = new DiffPiece(diffResult.PiecesOld[i + diffBlock.DeleteStartA], ChangeType.Deleted, aPos + 1);
                    var newPiece = new DiffPiece(diffResult.PiecesNew[i + diffBlock.InsertStartB], ChangeType.Inserted, bPos + 1);

                    if (subPieceBuilder != null)
                    {
                        subPieceBuilder(diffResult.PiecesOld[aPos], diffResult.PiecesNew[bPos], oldPiece.SubPieces, newPiece.SubPieces);
                        newPiece.Type = oldPiece.Type = ChangeType.Modified;
                    }

                    oldPieces.Add(oldPiece);
                    newPieces.Add(newPiece);
                    aPos++;
                    bPos++;
                }

                if (diffBlock.DeleteCountA > diffBlock.InsertCountB)
                {
                    for (; i < diffBlock.DeleteCountA; i++)
                    {
                        oldPieces.Add(new DiffPiece(diffResult.PiecesOld[i + diffBlock.DeleteStartA], ChangeType.Deleted, aPos + 1));
                        newPieces.Add(new DiffPiece());
                        aPos++;
                    }
                }
                else
                {
                    for (; i < diffBlock.InsertCountB; i++)
                    {
                        newPieces.Add(new DiffPiece(diffResult.PiecesNew[i + diffBlock.InsertStartB], ChangeType.Inserted, bPos + 1));
                        oldPieces.Add(new DiffPiece());
                        bPos++;
                    }
                }
            }

            while (bPos < diffResult.PiecesNew.Length && aPos < diffResult.PiecesOld.Length)
            {
                oldPieces.Add(new DiffPiece(diffResult.PiecesOld[aPos], ChangeType.Unchanged, aPos + 1));
                newPieces.Add(new DiffPiece(diffResult.PiecesNew[bPos], ChangeType.Unchanged, bPos + 1));
                aPos++;
                bPos++;
            }
        }
    }
}