using DiffPlex.DiffBuilder.Model;
using Microsoft.UI.Xaml.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiffPlex.UI;

/// <summary>
/// The view types of the diff text.
/// </summary>
public enum DiffTextViewType : byte
{
    /// <summary>
    /// Unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Inline item in unified view.
    /// </summary>
    Inline = 1,

    /// <summary>
    /// Left (old) item in split view.
    /// </summary>
    Left = 2,

    /// <summary>
    /// Right (new) item in split view.
    /// </summary>
    Right = 3,
}

public struct DiffTextViewInfo
{
    private readonly object token;

    /// <summary>
    /// Initializes a new instance of the DiffTextViewInfo class.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="viewType">The view type.</param>
    /// <param name="model">The diff piece instance.</param>
    internal DiffTextViewInfo(object token, DiffTextViewType viewType, DiffPiece model)
    {
        this.token = token ?? new();
        ViewType = viewType;
        model ??= new();
        ChangeType = model.Type;
        Position = model.Position;
        Text = model.Text;
    }

    /// <summary>
    /// Gets the view type.
    /// </summary>
    public DiffTextViewType ViewType { get; }

    /// <summary>
    /// Gets the change type.
    /// </summary>
    public ChangeType ChangeType { get; }

    /// <summary>
    /// Gets the line position.
    /// </summary>
    public int? Position { get; }

    /// <summary>
    /// Gets the content text.
    /// </summary>
    public string Text { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        if (Position.HasValue)
        {
            sb.Append(Position.Value);
            sb.Append(' ');
        }

        switch (ChangeType)
        {
            case ChangeType.Inserted:
                sb.Append("+ ");
                break;
            case ChangeType.Deleted:
                sb.Append("- ");
                break;
        }

        sb.Append('\t');
        sb.Append(Text);
        return sb.ToString();
    }

    /// <summary>
    /// Tests if the token is the current one.
    /// </summary>
    /// <param name="token">The token to test.</param>
    /// <returns>true if the same; otherwise, false.</returns>
    internal bool IsToken(object token)
        => token == this.token;
}

/// <summary>
/// The base view model for diff text.
/// </summary>
internal abstract class BaseDiffTextViewModel : IEquatable<DiffTextViewInfo>
{
    private readonly object token = new();

    /// <summary>
    /// Get or set the line index.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Gets a value indicating whether the line is unchanged.
    /// </summary>
    public abstract bool IsUnchanged { get; }

    /// <summary>
    /// Gets a value indicating whether the text is null.
    /// </summary>
    public bool IsNullLine => SourceText is null;

    /// <summary>
    /// Gets the text.
    /// </summary>
    public abstract string SourceText { get; }

    /// <summary>
    /// Gets the position.
    /// </summary>
    public abstract int? Position { get; }

    /// <summary>
    /// Returns a value indicating whether a specified substring occurs within the text in this view model.
    /// </summary>
    /// <param name="q">The string to seek.</param>
    /// <returns>true if the value parameter occurs within the text in this view model; otherwise, false.</returns>
    public abstract bool Contains(string q);

    /// <summary>
    /// Converts to the view info of diff piece.
    /// </summary>
    public abstract DiffTextViewInfo ToInfo();

    /// <summary>
    /// Converts to the view info of diff piece.
    /// </summary>
    protected DiffTextViewInfo ToInfo(DiffTextViewType viewType, DiffPiece diffPiece)
    {
        return new(token, viewType, diffPiece);
    }

    public bool Equals(DiffTextViewInfo other)
    {
        return other.IsToken(token);
    }
}

/// <summary>
/// The diff text view model of split mode.
/// </summary>
internal class DiffTextViewModel : BaseDiffTextViewModel
{
    private object token = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffTextViewModel"/> class.
    /// </summary>
    public DiffTextViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffTextViewModel"/> class.
    /// </summary>
    /// <param name="index">The line index.</param>
    /// <param name="left">The left diff piece instance.</param>
    /// <param name="right">The right diff piece instance.</param>
    public DiffTextViewModel(int index, DiffPiece left, DiffPiece right)
        : this(index, left, right, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffTextViewModel"/> class.
    /// </summary>
    /// <param name="index">The line index.</param>
    /// <param name="left">The left diff piece instance.</param>
    /// <param name="right">The right diff piece instance.</param>
    /// <param name="reference">The view reference.</param>
    public DiffTextViewModel(int index, DiffPiece left, DiffPiece right, DiffTextViewReference reference)
    {
        Index = index;
        Left = left;
        Right = right;
        Reference = reference;
    }

    public DiffPiece Left { get; private set; }

    public DiffPiece Right { get; private set; }

    /// <summary>
    /// Gets the view reference.
    /// </summary>
    public DiffTextViewReference Reference { get; private set; }

    /// <summary>
    /// Gets the left text.
    /// </summary>
    public string LeftText => Left?.Text;

    /// <summary>
    /// Gets the right text.
    /// </summary>
    public string RightText => Right?.Text;

    /// <inheritdoc />
    public override bool IsUnchanged => Right?.Type == ChangeType.Unchanged;

    /// <inheritdoc />
    public override string SourceText => Right?.Text;

    /// <inheritdoc />
    public override int? Position => Right?.Position;

    public IEnumerable<TextHighlighter> GetLeftHighlighter()
        => InternalUtilities.GetTextHighlighter(Left?.SubPieces, ChangeType.Deleted, Reference?.Element?.Foreground);

    public IEnumerable<TextHighlighter> GetRightHighlighter()
        => InternalUtilities.GetTextHighlighter(Right?.SubPieces, ChangeType.Inserted, Reference?.Element?.Foreground);

    /// <inheritdoc />
    public override bool Contains(string q)
    {
        if (string.IsNullOrEmpty(q)) return false;
        var v = Right?.Text;
        if (v != null && v.Contains(q)) return true;
        v = Left?.Text;
        if (v != null && v.Contains(q)) return true;
        return false;
    }

    /// <inheritdoc />
    public override DiffTextViewInfo ToInfo()
        => ToInfo(DiffTextViewType.Right, Right);
}

/// <summary>
/// The diff text view model of unified mode.
/// </summary>
internal class InlineDiffTextViewModel : BaseDiffTextViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiffTextViewModel"/> class.
    /// </summary>
    public InlineDiffTextViewModel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffTextViewModel"/> class.
    /// </summary>
    /// <param name="index">The line index.</param>
    /// <param name="line">The inline diff piece instance.</param>
    public InlineDiffTextViewModel(int index, DiffPiece line)
        : this(index, line, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiffTextViewModel"/> class.
    /// </summary>
    /// <param name="index">The line index.</param>
    /// <param name="line">The inline diff piece instance.</param>
    /// <param name="reference">The view reference.</param>
    public InlineDiffTextViewModel(int index, DiffPiece line, DiffTextViewReference reference)
    {
        Index = index;
        Line = line;
        Reference = reference;
    }

    public DiffPiece Line { get; private set; }

    /// <summary>
    /// Gets the text.
    /// </summary>
    public string Text => Line?.Text;

    /// <inheritdoc />
    public override int? Position => Line?.Position;

    /// <summary>
    /// Gets the view reference.
    /// </summary>
    public DiffTextViewReference Reference { get; private set; }

    /// <inheritdoc />
    public override bool IsUnchanged => Line?.Type == ChangeType.Unchanged;

    /// <inheritdoc />
    public override string SourceText => Line?.Text;

    public IEnumerable<TextHighlighter> GetTextHighlighter()
        => InternalUtilities.GetTextHighlighter(Line?.SubPieces, ChangeType.Deleted, Reference?.Element?.Foreground);

    /// <inheritdoc />
    public override bool Contains(string q)
    {
        if (string.IsNullOrEmpty(q)) return false;
        var v = Line?.Text;
        return v != null && v.Contains(q);
    }

    /// <inheritdoc />
    public override DiffTextViewInfo ToInfo()
        => ToInfo(DiffTextViewType.Inline, Line);
}

internal class DiffTextViewReference(DiffTextView element)
{
    public DiffTextView Element { get; set; } = element;
}
