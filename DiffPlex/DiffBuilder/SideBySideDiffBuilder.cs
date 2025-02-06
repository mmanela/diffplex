using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.DiffBuilder;

public class SideBySideDiffBuilder : ISideBySideDiffBuilder
{
    private readonly IDiffer differ;
    private readonly IChunker lineChunker;
    private readonly IChunker wordChunker;

    private delegate ChangeType PieceBuilder(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces, bool ignoreWhitespace, bool ignoreCase);

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

    public SideBySideDiffModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace) => BuildDiffModel(
            oldText,
            newText,
            ignoreWhitespace,
            false);

    public SideBySideDiffModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase)
    {
        return BuildLineDiff(
            oldText ?? throw new ArgumentNullException(nameof(oldText)),
            newText ?? throw new ArgumentNullException(nameof(newText)),
            ignoreWhitespace,
            ignoreCase);
    }

    /// <summary>
    /// Gets the side-by-side textual diffs.
    /// </summary>
    /// <param name="oldText">The old text to diff.</param>
    /// <param name="newText">The new text.</param>
    /// <param name="ignoreWhiteSpace"><see langword="true"/> if ignore the white space; otherwise, <see langword="false"/>.</param>
    /// <param name="ignoreCase"><see langword="true"/> if case-insensitive; otherwise, <see langword="false"/>.</param>
    /// <returns>The diffs result.</returns>
    public static SideBySideDiffModel Diff(string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false)
    {
        if (oldText == null) throw new ArgumentNullException(nameof(oldText));
        if (newText == null) throw new ArgumentNullException(nameof(newText));
        var diffResult = Differ.Instance.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, LineChunker.Instance);
        return BuildDiffModel(diffResult, BuildWordDiffPiecesInternal, ignoreWhiteSpace, ignoreCase);
    }

#if !NET_TOO_OLD_VER
    /// <summary>
    /// Gets the side-by-side textual diffs.
    /// </summary>
    /// <param name="oldText">The old text to diff.</param>
    /// <param name="newText">The new text.</param>
    /// <param name="ignoreWhiteSpace"><see langword="true"/> if ignore the white space; otherwise, <see langword="false"/>.</param>
    /// <param name="ignoreCase"><see langword="true"/> if case-insensitive; otherwise, <see langword="false"/>.</param>
    /// <returns>The diffs result.</returns>
    public static SideBySideDiffModel Diff(ReadOnlySpan<char> oldText, ReadOnlySpan<char> newText, bool ignoreWhiteSpace = true, bool ignoreCase = false)
    {
        if (oldText == null) throw new ArgumentNullException(nameof(oldText));
        if (newText == null) throw new ArgumentNullException(nameof(newText));
        var diffResult = Differ.Instance.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, LineChunker.Instance);
        return BuildDiffModel(diffResult, BuildWordDiffPiecesInternal, ignoreWhiteSpace, ignoreCase);
    }

    /// <summary>
    /// Gets the side-by-side textual diffs.
    /// </summary>
    /// <param name="differ">The differ instance.</param>
    /// <param name="oldText">The old text to diff.</param>
    /// <param name="newText">The new text.</param>
    /// <param name="ignoreWhiteSpace"><see langword="true"/> if ignore the white space; otherwise, <see langword="false"/>.</param>
    /// <param name="ignoreCase"><see langword="true"/> if case-insensitive; otherwise, <see langword="false"/>.</param>
    /// <param name="lineChunker">The line chunker.</param>
    /// <param name="wordChunker">The word chunker.</param>
    /// <returns>The diffs result.</returns>
    public static SideBySideDiffModel Diff(IDiffer differ, ReadOnlySpan<char> oldText, ReadOnlySpan<char> newText, bool ignoreWhiteSpace = true, bool ignoreCase = false, IChunker lineChunker = null, IChunker wordChunker = null)
    {
        if (differ == null) return Diff(oldText, newText, ignoreWhiteSpace, ignoreCase);
        var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, lineChunker ?? LineChunker.Instance);
        return BuildDiffModel(diffResult, (ot, nt, op, np, iw, ic) =>
        {
            var r = differ.CreateDiffs(ot, nt, iw, ic, wordChunker ?? WordChunker.Instance);
            return BuildDiffPieces(r, op, np, null, iw, ic);
        }, ignoreWhiteSpace, ignoreCase);
    }
#endif

    /// <summary>
    /// Gets the side-by-side textual diffs.
    /// </summary>
    /// <param name="differ">The differ instance.</param>
    /// <param name="oldText">The old text to diff.</param>
    /// <param name="newText">The new text.</param>
    /// <param name="ignoreWhiteSpace"><see langword="true"/> if ignore the white space; otherwise, <see langword="false"/>.</param>
    /// <param name="ignoreCase"><see langword="true"/> if case-insensitive; otherwise, <see langword="false"/>.</param>
    /// <param name="lineChunker">The line chunker.</param>
    /// <param name="wordChunker">The word chunker.</param>
    /// <returns>The diffs result.</returns>
    public static SideBySideDiffModel Diff(IDiffer differ, string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false, IChunker lineChunker = null, IChunker wordChunker = null)
    {
        if (oldText == null) throw new ArgumentNullException(nameof(oldText));
        if (newText == null) throw new ArgumentNullException(nameof(newText));
        if (differ == null) return Diff(oldText, newText, ignoreWhiteSpace, ignoreCase);
        var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, lineChunker ?? LineChunker.Instance);
        return BuildDiffModel(diffResult, (ot, nt, op, np, iw, ic) =>
        {
            var r = differ.CreateDiffs(ot, nt, iw, ic, wordChunker ?? WordChunker.Instance);
            return BuildDiffPieces(r, op, np, null, iw, ic);
        }, ignoreWhiteSpace, ignoreCase);
    }

    private static ChangeType BuildWordDiffPiecesInternal(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces, bool ignoreWhiteSpace, bool ignoreCase)
    {
        var diffResult = Differ.Instance.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, WordChunker.Instance);
        return BuildDiffPieces(diffResult, oldPieces, newPieces, null, ignoreWhiteSpace, ignoreCase);
    }

    private SideBySideDiffModel BuildLineDiff(string oldText, string newText, bool ignoreWhiteSpace, bool ignoreCase)
    {
        var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, lineChunker);
        return BuildDiffModel(diffResult, BuildWordDiffPieces, ignoreWhiteSpace, ignoreCase);
    }

    private ChangeType BuildWordDiffPieces(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces, bool ignoreWhiteSpace, bool ignoreCase)
    {
        var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhiteSpace: ignoreWhiteSpace, ignoreCase, wordChunker);
        return BuildDiffPieces(diffResult, oldPieces, newPieces, subPieceBuilder: null, ignoreWhiteSpace, ignoreCase);
    }

    private static SideBySideDiffModel BuildDiffModel(DiffResult diffResult, PieceBuilder subPieceBuilder, bool ignoreWhiteSpace, bool ignoreCase)
    {
        var oldLines = new List<DiffPiece>();
        var newLines = new List<DiffPiece>();
        BuildDiffPieces(diffResult, oldLines, newLines, subPieceBuilder, ignoreWhiteSpace, ignoreCase);
        return new(oldLines, newLines);
    }

    private static ChangeType BuildDiffPieces(DiffResult diffResult, List<DiffPiece> oldPieces, List<DiffPiece> newPieces, PieceBuilder subPieceBuilder, bool ignoreWhiteSpace, bool ignoreCase)
    {
        var aPos = 0;
        var bPos = 0;

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
                var oldPieceType = ChangeType.Deleted;
                var newPieceType = ChangeType.Inserted;
                var oldSubPieces = new List<DiffPiece>();
                var newSubPieces = new List<DiffPiece>();
                if (subPieceBuilder != null)
                {
                    var subChangeSummary = subPieceBuilder(diffResult.PiecesOld[aPos], diffResult.PiecesNew[bPos], oldSubPieces, newSubPieces, ignoreWhiteSpace, ignoreCase);
                    oldPieceType = newPieceType = subChangeSummary;
                }

                oldPieces.Add(new(diffResult.PiecesOld[i + diffBlock.DeleteStartA], oldPieceType, aPos + 1, oldSubPieces));
                newPieces.Add(new(diffResult.PiecesNew[i + diffBlock.InsertStartB], newPieceType, bPos + 1, newSubPieces));
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
        if(oldPieces.Any(x => x.Type is ChangeType.Modified or ChangeType.Inserted or ChangeType.Deleted))
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