﻿using System;
using System.Collections.Generic;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.DiffBuilder
{
    public class SideBySideDiffBuilder : ISideBySideDiffBuilder
    {
        private readonly IDiffer differ;

        private delegate void PieceBuilder(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces);

        public char[] WordSeparaters { get; } = { ' ', '\t', '.', '(', ')', '{', '}', ',', '!' };

        public SideBySideDiffBuilder(IDiffer differ)
        {
            this.differ = differ ?? throw new ArgumentNullException(nameof(differ));
        }

        public SideBySideDiffBuilder(IDiffer differ, char[] wordSeparators) : this(differ)
        {
            if (wordSeparators is null || wordSeparators.Length == 0)
            {
                throw new ArgumentException("wordSeparators cannot be null or empty.", nameof(wordSeparators));
            }
            WordSeparaters = wordSeparators;
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

        private SideBySideDiffModel BuildLineDiff(string oldText, string newText, bool ignoreWhitespace)
        {
            var model = new SideBySideDiffModel();
            var diffResult = differ.CreateLineDiffs(oldText, newText, ignoreWhitespace);
            BuildDiffPieces(diffResult, model.OldText.Lines, model.NewText.Lines, BuildWordDiffPieces);
            return model;
        }

        private void BuildWordDiffPieces(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces)
        {
            var diffResult = differ.CreateWordDiffs(oldText, newText, ignoreWhitespace: false, WordSeparaters);
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