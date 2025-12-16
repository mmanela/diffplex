namespace DiffPlex.Model;

/// <summary>
/// A block of consecutive edits from A and/or B
/// </summary>
public class DiffBlock(int deleteStartA, int deleteCountA, int insertStartB, int insertCountB)
{
    /// <summary>
    /// Position where deletions in A begin
    /// </summary>
    public int DeleteStartA { get; } = deleteStartA;

    /// <summary>
    /// The number of deletions in A
    /// </summary>
    public int DeleteCountA { get; } = deleteCountA;

    /// <summary>
    /// Position where insertion in B begin
    /// </summary>
    public int InsertStartB { get; } = insertStartB;

    /// <summary>
    /// The number of insertions in B
    /// </summary>
    public int InsertCountB { get; } = insertCountB;
}