namespace DiffPlex.Model;

/// <summary>
/// Represents the type of change in a three-way diff
/// </summary>
public enum ThreeWayChangeType
{
    /// <summary>
    /// No change from base in either old or new
    /// </summary>
    Unchanged,
    
    /// <summary>
    /// Change only in old, new unchanged from base
    /// </summary>
    OldOnly,
    
    /// <summary>
    /// Change only in new, old unchanged from base
    /// </summary>
    NewOnly,
    
    /// <summary>
    /// Same change in both old and new
    /// </summary>
    BothSame,
    
    /// <summary>
    /// Different changes in old and new (conflict)
    /// </summary>
    Conflict
}

/// <summary>
/// A block representing changes in a three-way diff
/// </summary>
public class ThreeWayDiffBlock(
    int baseStart,
    int baseCount,
    int oldStart,
    int oldCount,
    int newStart,
    int newCount,
    ThreeWayChangeType changeType)
{
    /// <summary>
    /// Position where the block starts in base
    /// </summary>
    public int BaseStart { get; } = baseStart;

    /// <summary>
    /// The number of pieces in base for this block
    /// </summary>
    public int BaseCount { get; } = baseCount;

    /// <summary>
    /// Position where the block starts in old
    /// </summary>
    public int OldStart { get; } = oldStart;

    /// <summary>
    /// The number of pieces in old for this block
    /// </summary>
    public int OldCount { get; } = oldCount;

    /// <summary>
    /// Position where the block starts in new
    /// </summary>
    public int NewStart { get; } = newStart;

    /// <summary>
    /// The number of pieces in new for this block
    /// </summary>
    public int NewCount { get; } = newCount;

    /// <summary>
    /// The type of change this block represents
    /// </summary>
    public ThreeWayChangeType ChangeType { get; } = changeType;
}
