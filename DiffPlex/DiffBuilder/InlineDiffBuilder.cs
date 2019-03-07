﻿using System;
using System.Collections.Generic;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.DiffBuilder
{
    public class InlineDiffBuilder : IInlineDiffBuilder
    {
        private readonly IDiffer differ;

        public InlineDiffBuilder(IDiffer differ)
        {
            this.differ = differ ?? throw new ArgumentNullException(nameof(differ));
        }

        public DiffPaneModel BuildDiffModel(string oldText, string newText)
            => BuildDiffModel(oldText, newText, ignoreWhitespace: true);

        public DiffPaneModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace)
        {
            if (oldText == null) throw new ArgumentNullException(nameof(oldText));
            if (newText == null) throw new ArgumentNullException(nameof(newText));

            var model = new DiffPaneModel();
            var diffResult = differ.CreateLineDiffs(oldText, newText, ignoreWhitespace);
            BuildDiffPieces(diffResult, model.Lines);
            return model;
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

            for (; bPos < diffResult.PiecesNew.Length; bPos++)
                pieces.Add(new DiffPiece(diffResult.PiecesNew[bPos], ChangeType.Unchanged, bPos + 1));
        }
    }
}
