using System;
using System.Collections.Generic;
using System.Text;

namespace DiffPlex.DiffBuilder.Model;

public enum ChangeType : byte
{
    Unchanged,
    Deleted,
    Inserted,
    Imaginary,
    Modified
}

/// <summary>
/// The diff piece model.
/// </summary>
#if !NET_TOO_OLD_VER
[System.Text.Json.Serialization.JsonConverter(typeof(JsonDiffPieceConverter))]
#endif
public class DiffPiece : IEquatable<DiffPiece>
{
    /// <summary>
    /// Gets the change type.
    /// </summary>
    public ChangeType Type { get; }

    /// <summary>
    /// Gets the nullable zero-based position.
    /// </summary>
    public int? Position { get; }

    /// <summary>
    /// Gets the content text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the sub pieces.
    /// </summary>
    public IReadOnlyList<DiffPiece> SubPieces { get; }

    /// <summary>
    /// Initializes a new instance of the DiffPiece class.
    /// </summary>
    public DiffPiece()
        : this(null, ChangeType.Imaginary)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DiffPiece class.
    /// </summary>
    /// <param name="text">The content text.</param>
    /// <param name="type">The change type.</param>
    /// <param name="position">The nullable zero-based position</param>
    public DiffPiece(string text, ChangeType type, int? position = null)
        : this(text, type, position, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the DiffPiece class.
    /// </summary>
    /// <param name="text">The content text.</param>
    /// <param name="type">The change type.</param>
    /// <param name="position">The nullable zero-based position</param>
    /// <param name="subPieces">The sub pieces.</param>
    public DiffPiece(string text, ChangeType type, int? position, IReadOnlyList<DiffPiece> subPieces)
    {
        Text = text;
        Position = position;
        Type = type;
        SubPieces = subPieces ?? new List<DiffPiece>();
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as DiffPiece);
    }

    public bool Equals(DiffPiece other)
    {
        return other != null
            && Type == other.Type
            && EqualityComparer<int?>.Default.Equals(Position, other.Position)
            && Text == other.Text
            && SubPiecesEqual(other);
    }

    public override int GetHashCode()
    {
        var hashCode = 1688038063;
        hashCode = hashCode * -1521134295 + Type.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(Position);
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Text);
        hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(SubPieces?.Count);
        return hashCode;
    }

#if !NET_TOO_OLD_VER
    /// <summary>
    /// Writes current diff piece into UTF-8 JSON stream.
    /// </summary>
    /// <param name="writer">The UTF-8 JSON stream writer.</param>
    /// <param name="options">The JSON srialization options.</param>
    public void Write(System.Text.Json.Utf8JsonWriter writer, System.Text.Json.JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", Type.ToString());
        if (Position.HasValue) writer.WriteNumber("position", Position.Value);
        writer.WriteString("text", Text);
        if (SubPieces.Count > 0)
        {
            writer.WriteStartArray("sub");
            JsonDiffPieceConverter.Write(SubPieces, writer, options);
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
#endif

    /// <summary>
    /// Returns a string that represents this diff piece.
    /// </summary>
    /// <returns>A string that represents this diff piece.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (Position.HasValue) sb.Append(Position.Value);
        sb.Append('\t');
        switch (Type)
        {
            case ChangeType.Inserted:
                sb.Append("+ ");
                break;
            case ChangeType.Deleted:
                sb.Append("- ");
                break;
            case ChangeType.Modified:
                sb.Append("M ");
                break;
            default:
                sb.Append("  ");
                break;
        }

        sb.Append(Text);
        return sb.ToString();
    }

    private bool SubPiecesEqual(DiffPiece other)
    {
        if (SubPieces is null)
            return other.SubPieces is null;
        else if (other.SubPieces is null)
            return false;

        if (SubPieces.Count != other.SubPieces.Count)
            return false;

        for (int i = 0; i < SubPieces.Count; i++)
        {
            if (!Equals(SubPieces[i], other.SubPieces[i]))
                return false;
        }

        return true;
    }
}