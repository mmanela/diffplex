using DiffPlex.DiffBuilder.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace DiffPlex.WindowsForms.Controls;

/// <summary>
/// The diff control for text.
/// </summary>
public partial class DiffViewer : UserControl
{
    /// <summary>
    /// Initializes a new instance of the DiffViewer class.
    /// </summary>
    public DiffViewer()
    {
        InitializeComponent();
        Controls.Add(new ElementHost
        {
            Dock = DockStyle.Fill,
            Child = Core = new Wpf.Controls.DiffViewer()
        });
    }

    /// <summary>
    /// Gets the core control.
    /// </summary>
    public Wpf.Controls.DiffViewer Core { get; }

    /// <summary>
    /// Occurs when the view mode is changed.
    /// </summary>
    public event EventHandler<Wpf.Controls.DiffViewer.ViewModeChangedEventArgs> ViewModeChanged
    {
        add => Core.ViewModeChanged += value;
        remove => Core.ViewModeChanged -= value;
    }

    /// <summary>
    /// Occurs when the grid splitter loses mouse capture.
    /// </summary>
    public event DragCompletedEventHandler SplitterDragCompleted
    {
        add => Core.SplitterDragCompleted += value;
        remove => Core.SplitterDragCompleted -= value;
    }

    /// <summary>
    /// Occurs one or more times as the mouse changes position when the grid splitter has logical focus and mouse capture.
    /// </summary>
    public event DragDeltaEventHandler SplitterDragDelta
    {
        add => Core.SplitterDragDelta += value;
        remove => Core.SplitterDragDelta -= value;
    }

    /// <summary>
    /// Occurs when the grid splitter receives logical focus and mouse capture.
    /// </summary>
    public event DragStartedEventHandler SplitterDragStarted
    {
        add => Core.SplitterDragStarted += value;
        remove => Core.SplitterDragStarted -= value;
    }

    /// <summary>
    /// Gets or sets the open type weight or thickness of the specified font.
    /// </summary>
    public int FontWeight
    {
        get => Core.FontWeight.ToOpenTypeWeight();
        set => Core.FontWeight = System.Windows.FontWeight.FromOpenTypeWeight(value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the font is italic.
    /// </summary>
    public bool IsFontItalic
    {
        get => Core.FontStyle == System.Windows.FontStyles.Italic || Core.FontStyle == System.Windows.FontStyles.Oblique;
        set => Core.FontStyle = value ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal;
    }

    /// <summary>
    /// Gets or sets the degree to which a font is condensed or expanded on the screen.
    /// </summary>
    public System.Windows.FontStretch FontStretch
    {
        get => Core.FontStretch;
        set => Core.FontStretch = value;
    }


    /// <summary>
    /// Gets or sets the font size.
    /// </summary>
    public double FontSize
    {
        get => Core.FontSize;
        set => Core.FontSize = value;
    }

    /// <summary>
    /// Gets or sets the font family names.
    /// </summary>
    public string FontFamilyNames
    {
        get => Core.FontFamily?.Source ?? string.Empty;
        set => Core.FontFamily = new System.Windows.Media.FontFamily(value ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the border width of a control.
    /// </summary>
    public Padding BorderWidth
    {
        get => ToPadding(Core.BorderThickness);
        set => Core.BorderThickness = ToThickness(value);
    }

    /// <summary>
    /// Gets or sets the border background color of a control.
    /// </summary>
    public Color BorderColor
    {
        get => GetColor(Core.Background);
        set => Core.Background = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the foreground color.
    /// </summary>
    public override Color ForeColor
    {
        get
        {
            return GetColor(Core.Foreground);
        }

        set
        {
            base.ForeColor = value;
            Core.Foreground = ToBrush(value);
        }
    }

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public override Color BackColor
    {
        get
        {
            return GetColor(Core.Background);
        }

        set
        {
            base.BackColor = value;
            Core.Background = ToBrush(value);
        }
    }

    /// <summary>
    /// Gets or sets the old text.
    /// </summary>
    public string OldText
    {
        get => Core.OldText;
        set => Core.OldText = value;
    }

    /// <summary>
    /// Gets or sets the new text.
    /// </summary>
    public string NewText
    {
        get => Core.NewText;
        set => Core.NewText = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether ignore the white space.
    /// </summary>
    public bool IgnoreWhiteSpace
    {
        get => Core.IgnoreWhiteSpace;
        set => Core.IgnoreWhiteSpace = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether ignore case.
    /// </summary>
    public bool IgnoreCase
    {
        get => Core.IgnoreCase;
        set => Core.IgnoreCase = value;
    }

    /// <summary>
    /// Gets or sets the foreground color of the line number.
    /// </summary>
    public Color LineNumberForeColor
    {
        get => GetColor(Core.LineNumberForeground);
        set => Core.LineNumberForeground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the line number width.
    /// </summary>
    public int LineNumberWidth
    {
        get => Core.LineNumberWidth;
        set => Core.LineNumberWidth = value;
    }

    /// <summary>
    /// Gets or sets the foreground color of the change type symbol.
    /// </summary>
    public Color ChangeTypeForeColor
    {
        get => GetColor(Core.ChangeTypeForeground);
        set => Core.ChangeTypeForeground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the header of the old text.
    /// </summary>
    public string OldTextHeader
    {
        get => Core.OldTextHeader;
        set => Core.OldTextHeader = value;
    }

    /// <summary>
    /// Gets or sets the header of the new text.
    /// </summary>
    public string NewTextHeader
    {
        get => Core.NewTextHeader;
        set => Core.NewTextHeader = value;
    }

    /// <summary>
    /// Gets or sets the foreground color of the line added.
    /// </summary>
    public double HeaderHeight
    {
        get => Core.HeaderHeight;
        set => Core.HeaderHeight = value;
    }

    /// <summary>
    /// Gets or sets the foreground color of the line added.
    /// </summary>
    public Color HeaderForeColor
    {
        get => GetColor(Core.HeaderForeground);
        set => Core.HeaderForeground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the background color of the line added.
    /// </summary>
    public Color HeaderBackColor
    {
        get => GetColor(Core.HeaderBackground);
        set => Core.HeaderBackground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the foreground color of the line added.
    /// </summary>
    public Color InsertedForeColor
    {
        get => GetColor(Core.InsertedForeground);
        set => Core.InsertedForeground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the background color of the line added.
    /// </summary>
    public Color InsertedBackColor
    {
        get => GetColor(Core.InsertedBackground);
        set => Core.InsertedBackground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the foreground color of the line deleted.
    /// </summary>
    public Color DeletedForeColor
    {
        get => GetColor(Core.DeletedForeground);
        set => Core.DeletedForeground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the background color of the line deleted.
    /// </summary>
    public Color DeletedBackColor
    {
        get => GetColor(Core.DeletedBackground);
        set => Core.DeletedBackground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the foreground color of the line unchanged.
    /// </summary>
    public Color UnchangedForeColor
    {
        get => GetColor(Core.UnchangedForeground);
        set => Core.UnchangedForeground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the background color of the line unchanged.
    /// </summary>
    public Color UnchangedBackColor
    {
        get => GetColor(Core.UnchangedBackground);
        set => Core.UnchangedBackground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the background color of the line imaginary.
    /// </summary>
    public Color ImaginaryBackColor
    {
        get => GetColor(Core.ImaginaryBackground);
        set => Core.ImaginaryBackground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the background color of the grid splitter.
    /// </summary>
    public Color SplitterBackColor
    {
        get => GetColor(Core.SplitterBackground);
        set => Core.SplitterBackground = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the border color of the grid splitter.
    /// </summary>
    public Color SplitterBorderColor
    {
        get => GetColor(Core.SplitterBorderBrush);
        set => Core.SplitterBorderBrush = ToBrush(value);
    }

    /// <summary>
    /// Gets or sets the border width of the grid splitter.
    /// </summary>
    public Padding SplitterBorderWidth
    {
        get => ToPadding(Core.SplitterBorderThickness);
        set => Core.SplitterBorderThickness = ToThickness(value);
    }

    /// <summary>
    /// Gets or sets the width of the grid splitter.
    /// </summary>
    public double SplitterWidth
    {
        get => Core.SplitterWidth;
        set => Core.SplitterWidth = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether it is in side-by-side (split) view mode to diff.
    /// </summary>
    public bool IsSideBySide
    {
        get => Core.IsSideBySide;
        set => Core.IsSideBySide = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether need collapse unchanged sections.
    /// </summary>
    public bool IgnoreUnchanged
    {
        get => Core.IgnoreUnchanged;
        set => Core.IgnoreUnchanged = value;
    }

    /// <summary>
    /// Gets or sets the count of context line.
    /// The context line is the one unchanged arround others as their margin.
    /// </summary>
    public int LinesContext
    {
        get => Core.LinesContext;
        set => Core.LinesContext = value;
    }

    /// <summary>
    /// Gets or sets the display name of inline mode toggle.
    /// </summary>
    public object InlineModeToggleTitle
    {
        get => Core.InlineModeToggleTitle;
        set => Core.InlineModeToggleTitle = value;
    }

    /// <summary>
    /// Gets or sets the display name of side by side mode toggle.
    /// </summary>
    public object SideBySideModeToggleTitle
    {
        get => Core.SideBySideModeToggleTitle;
        set => Core.SideBySideModeToggleTitle = value;
    }

    /// <summary>
    /// Gets or sets the display name of skip unchanged lines toggle.
    /// </summary>
    public object CollapseUnchangedSectionsToggleTitle
    {
        get => Core.CollapseUnchangedSectionsToggleTitle;
        set => Core.CollapseUnchangedSectionsToggleTitle = value;
    }

    /// <summary>
    /// Gets or sets the display name of context lines count.
    /// </summary>
    public object ContextLinesMenuItemsTitle
    {
        get => Core.ContextLinesMenuItemsTitle;
        set => Core.ContextLinesMenuItemsTitle = value;
    }

    /// <summary>
    /// Gets a value indicating whether the grid splitter has logical focus and mouse capture and the left mouse button is pressed.
    /// </summary>
    public bool IsSplitterDragging => Core.IsSplitterDragging;

    /// <summary>
    /// Gets a value that represents the actual calculated width of the left side panel.
    /// </summary>
    public double LeftSideActualWidth => Core.LeftSideActualWidth;

    /// <summary>
    /// Gets a value that represents the actual calculated width of the right side panel.
    /// </summary>
    public double RightSideActualWidth => Core.RightSideActualWidth;

    /// <summary>
    /// Gets a value indicating whether it is side-by-side view mode.
    /// </summary>
    public bool IsSideBySideViewMode => Core.IsSideBySideViewMode;

    /// <summary>
    /// Gets a value indicating whether it is inline view mode.
    /// </summary>
    public bool IsInlineViewMode => Core.IsInlineViewMode;

    /// <summary>
    /// Gets the side-by-side diffs result.
    /// </summary>
    public SideBySideDiffModel GetSideBySideDiffModel() => Core.GetSideBySideDiffModel();

    /// <summary>
    /// Gets the inline diffs result.
    /// </summary>
    public DiffPaneModel GetInlineDiffModel() => Core.GetInlineDiffModel();

    /// <summary>
    /// Refreshes.
    /// </summary>
    public void RefreshCore() => Core.Refresh();

    /// <inheritdoc />
    public override void Refresh()
    {
        base.Refresh();
        Core.Refresh();
    }

    /// <summary>
    /// Switches to the view of side-by-side diff mode.
    /// </summary>
    public void ShowSideBySide() => Core.ShowSideBySide();

    /// <summary>
    /// Switches to the view of inline diff mode.
    /// </summary>
    public void ShowInline() => Core.ShowInline();

    /// <summary>
    /// Goes to a specific line.
    /// </summary>
    /// <param name="lineIndex">The index of line.</param>
    /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
    /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
    public bool GoTo(int lineIndex, bool isLeftLine = false) => Core.GoTo(lineIndex, isLeftLine);

    /// <summary>
    /// Goes to a specific line.
    /// </summary>
    /// <param name="line">The line to go to.</param>
    /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
    /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
    public bool GoTo(DiffPiece line, bool isLeftLine = false) => Core.GoTo(line);

    /// <summary>
    /// Gets the line diff information.
    /// </summary>
    /// <param name="lineIndex">The zero-based index of line to go to.</param>
    /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
    /// <returns>The line diff information instance; or null, if non-exists.</returns>
    public DiffPiece GetLine(int lineIndex, bool isLeftLine = false) => Core.GetLine(lineIndex, isLeftLine);

    /// <summary>
    /// Gets all line information in viewport.
    /// </summary>
    /// <param name="isLeftLine">true if goes to the line of the left panel for side-by-side (splitted) view; otherwise, false. This will be ignored when it is in inline view.</param>
    /// <param name="level">The optional visibility level.</param>
    /// <returns>All lines.</returns>
    public IEnumerable<DiffPiece> GetLinesInViewport(bool isLeftLine = false, Wpf.Controls.VisibilityLevels level = Wpf.Controls.VisibilityLevels.Any) => Core.GetLinesInViewport(isLeftLine, level);

    /// <summary>
    /// Gets all line information in viewport.
    /// </summary>
    /// <param name="level">The optional visibility level.</param>
    /// <returns>All lines.</returns>
    public IEnumerable<DiffPiece> GetLinesInViewport(Wpf.Controls.VisibilityLevels level) => Core.GetLinesInViewport(level);

    /// <summary>
    /// Opens the context menu for view mode selection.
    /// </summary>
    public void OpenViewModeContextMenu() => Core.OpenViewModeContextMenu();

    /// <summary>
    /// Collapses unchanged sections.
    /// </summary>
    /// <param name="contextLineCount">The optional context line count to set.</param>
    /// <exception cref="ArgumentOutOfRangeException">contextLineCount was less than 0.</exception>
    public void CollapseUnchangedSections(int? contextLineCount = null) => Core.CollapseUnchangedSections(contextLineCount);

    /// <summary>
    /// Expands unchanged sections.
    /// </summary>
    public void ExpandUnchangedSections() => Core.ExpandUnchangedSections();

    /// <summary>
    /// Sets header as old and new.
    /// </summary>
    public void SetHeaderAsOldToNew() => Core.SetHeaderAsOldToNew();

    /// <summary>
    /// Sets header as left and right.
    /// </summary>
    public void SetHeaderAsLeftToRight() => Core.SetHeaderAsLeftToRight();

    private static System.Windows.Media.SolidColorBrush ToBrush(Color? color)
    {
        var c = color.HasValue
            ? System.Windows.Media.Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B)
            : System.Windows.Media.Color.FromArgb(0, 127, 127, 127);
        return new System.Windows.Media.SolidColorBrush(c);
    }

    private static Color GetColor(System.Windows.Media.Brush brush)
    {
        if (!(brush is System.Windows.Media.SolidColorBrush b)) return Color.Empty;
        return Color.FromArgb(b.Color.A, b.Color.R, b.Color.G, b.Color.B);
    }

    private static Padding ToPadding(System.Windows.Thickness thickness)
    {
        return new Padding((int)thickness.Left, (int)thickness.Top, (int)thickness.Right, (int)thickness.Bottom);
    }

    private static System.Windows.Thickness ToThickness(Padding padding)
    {
        return new System.Windows.Thickness(padding.Left, padding.Top, padding.Right, padding.Bottom);
    }
}
