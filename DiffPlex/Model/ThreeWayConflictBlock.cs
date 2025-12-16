using System.Collections.Generic;

namespace DiffPlex.Model;

/// <summary>
/// Represents a conflict block in a three-way merge where automatic resolution was not possible
/// </summary>
public class ThreeWayConflictBlock(
    int mergedStart,
    IReadOnlyList<string> basePieces,
    IReadOnlyList<string> oldPieces,
    IReadOnlyList<string> newPieces,
    ThreeWayDiffBlock originalBlock)
{
    /// <summary>
    /// Position in the merged result where this conflict starts
    /// </summary>
    public int MergedStart { get; } = mergedStart;

    /// <summary>
    /// The base text pieces for this conflict
    /// </summary>
    public IReadOnlyList<string> BasePieces { get; } = basePieces;

    /// <summary>
    /// Old text pieces for this conflict
    /// </summary>
    public IReadOnlyList<string> OldPieces { get; } = oldPieces;

    /// <summary>
    /// New text pieces for this conflict
    /// </summary>
    public IReadOnlyList<string> NewPieces { get; } = newPieces;

    /// <summary>
    /// The original three-way diff block that caused this conflict
    /// </summary>
    public ThreeWayDiffBlock OriginalBlock { get; } = originalBlock;
}
