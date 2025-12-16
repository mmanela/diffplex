using System;
using System.Collections.Generic;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.DiffBuilder;

public class InlineDiffBuilder : IInlineDiffBuilder
{
    private readonly IDiffer differ;

    /// <summary>
    /// Gets the default singleton instance of the inline diff builder.
    /// </summary>
    public static InlineDiffBuilder Instance { get; } = new();

    public InlineDiffBuilder(IDiffer differ = null)
    {
        this.differ = differ ?? Differ.Instance;
    }

    public DiffPaneModel BuildDiffModel(string oldText, string newText)
        => BuildDiffModel(oldText, newText, ignoreWhitespace: true);

    public DiffPaneModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace)
    {
        LineChunker chunker = new();
        return BuildDiffModel(oldText, newText, ignoreWhitespace, false, chunker);
    }

    public DiffPaneModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase, IChunker chunker)
    {
        if (oldText == null) throw new ArgumentNullException(nameof(oldText));
        if (newText == null) throw new ArgumentNullException(nameof(newText));

        DiffPaneModel model = new();
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

        DiffPaneModel model = new();
        var diffResult = (differ ?? Differ.Instance).CreateDiffs(oldText, newText, ignoreWhiteSpace, ignoreCase, chunker ?? LineChunker.Instance);
        BuildDiffPieces(diffResult, model.Lines);
        
        return model;
    }

    private static void BuildDiffPieces(DiffResult diffResult, List<DiffPiece> pieces)
    {
        int bPos = 0;

        foreach (var diffBlock in diffResult.DiffBlocks)
        {
            for (; bPos < diffBlock.InsertStartB; bPos++)
                pieces.Add(new(diffResult.PiecesNew[bPos], ChangeType.Unchanged, bPos + 1));

            int i = 0;
            for (; i < Math.Min(diffBlock.DeleteCountA, diffBlock.InsertCountB); i++)
                pieces.Add(new(diffResult.PiecesOld[i + diffBlock.DeleteStartA], ChangeType.Deleted));

            i = 0;
            for (; i < Math.Min(diffBlock.DeleteCountA, diffBlock.InsertCountB); i++)
            {
                pieces.Add(new(diffResult.PiecesNew[i + diffBlock.InsertStartB], ChangeType.Inserted, bPos + 1));
                bPos++;
            }

            if (diffBlock.DeleteCountA > diffBlock.InsertCountB)
            {
                for (; i < diffBlock.DeleteCountA; i++)
                    pieces.Add(new(diffResult.PiecesOld[i + diffBlock.DeleteStartA], ChangeType.Deleted));
            }
            else
            {
                for (; i < diffBlock.InsertCountB; i++)
                {
                    pieces.Add(new(diffResult.PiecesNew[i + diffBlock.InsertStartB], ChangeType.Inserted, bPos + 1));
                    bPos++;
                }
            }
        }

        for (; bPos < diffResult.PiecesNew.Count; bPos++)
            pieces.Add(new(diffResult.PiecesNew[bPos], ChangeType.Unchanged, bPos + 1));
    }
}
