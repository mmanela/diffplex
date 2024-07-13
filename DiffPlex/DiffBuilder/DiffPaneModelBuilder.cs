using System;

internal static class DiffPaneModelBuilder
{
    internal static SideBySideDiffModel BuildDiffModel(
        IDiffer differ, 
        string oldText, 
        string newText, 
        bool ignoreWhiteSpace = true, 
        bool ignoreCase = false, 
        IChunker lineChunker = null,
        IChunker wordChunker = null)
    {
        if (oldText == null) throw new ArgumentNullException(nameof(oldText));
        if (newText == null) throw new ArgumentNullException(nameof(newText));

        if (differ == null) return Diff(oldText, newText, ignoreWhiteSpace, ignoreCase);

        var model = new SideBySideDiffModel();
        var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, lineChunker ?? LineChunker.Instance);
        BuildDiffPieces(diffResult, model.OldText.Lines, model.NewText.Lines, (ot, nt, op, np, iw, ic) =>
        {
            var r = differ.CreateDiffs(ot, nt, iw, ic, wordChunker ?? WordChunker.Instance);
            return BuildDiffPieces(r, op, np, null, iw, ic);
        }, ignoreWhiteSpace, ignoreCase);

        return model;
    }

    private static ChangeType BuildWordDiffPiecesInternal(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces, bool ignoreWhiteSpace, bool ignoreCase)
    {
        var diffResult = Differ.Instance.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, WordChunker.Instance);
        return BuildDiffPieces(diffResult, oldPieces, newPieces, null, ignoreWhiteSpace, ignoreCase);
    }


    private static ChangeType BuildDiffPieces(DiffResult diffResult, List<DiffPiece> oldPieces, List<DiffPiece> newPieces, PieceBuilder subPieceBuilder, bool ignoreWhiteSpace, bool ignoreCase)
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
                    var subChangeSummary = subPieceBuilder(diffResult.PiecesOld[aPos], diffResult.PiecesNew[bPos], oldPiece.SubPieces, newPiece.SubPieces, ignoreWhiteSpace, ignoreCase);
                    newPiece.Type = oldPiece.Type = subChangeSummary;
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

        while (bPos < diffResult.PiecesNew.Count && aPos < diffResult.PiecesOld.Count)
        {
            oldPieces.Add(new DiffPiece(diffResult.PiecesOld[aPos], ChangeType.Unchanged, aPos + 1));
            newPieces.Add(new DiffPiece(diffResult.PiecesNew[bPos], ChangeType.Unchanged, bPos + 1));
            aPos++;
            bPos++;
        }

        // Consider the whole diff as "modified" if we found any change, otherwise we consider it unchanged
        if (oldPieces.Any(x => x.Type is ChangeType.Modified or ChangeType.Inserted or ChangeType.Deleted))
        {
            return ChangeType.Modified;
        }

        if (newPieces.Any(x => x.Type is ChangeType.Modified or ChangeType.Inserted or ChangeType.Deleted))
        {
            return ChangeType.Modified;
        }

        return ChangeType.Unchanged;
    }
}
