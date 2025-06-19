using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.Wpf.Controls;

/// <summary>
/// A control for three-way merge visualization and conflict resolution
/// </summary>
public partial class ThreeWayMergeViewer : UserControl
{
    /// <summary>
    /// The property of base text.
    /// </summary>
    public static readonly DependencyProperty BaseTextProperty = RegisterRefreshDependencyProperty<string>(nameof(BaseText), null);

    /// <summary>
    /// The property of yours text.
    /// </summary>
    public static readonly DependencyProperty YoursTextProperty = RegisterRefreshDependencyProperty<string>(nameof(YoursText), null);

    /// <summary>
    /// The property of theirs text.
    /// </summary>
    public static readonly DependencyProperty TheirsTextProperty = RegisterRefreshDependencyProperty<string>(nameof(TheirsText), null);

    /// <summary>
    /// The property of a flag to ignore white space.
    /// </summary>
    public static readonly DependencyProperty IgnoreWhiteSpaceProperty = RegisterRefreshDependencyProperty(nameof(IgnoreWhiteSpace), true);

    /// <summary>
    /// The property of a flag to ignore case.
    /// </summary>
    public static readonly DependencyProperty IgnoreCaseProperty = RegisterRefreshDependencyProperty(nameof(IgnoreCase), false);

    /// <summary>
    /// The property of line number width.
    /// </summary>
    public static readonly DependencyProperty LineNumberWidthProperty = RegisterDependencyProperty(nameof(LineNumberWidth), 40, (d, e) =>
    {
        if (d is not ThreeWayMergeViewer c || e.OldValue == e.NewValue || e.NewValue is not int n) return;
        c.BaseContentPanel.LineNumberWidth = c.YoursContentPanel.LineNumberWidth = c.TheirsContentPanel.LineNumberWidth = c.ResultContentPanel.LineNumberWidth = n;
    });

    /// <summary>
    /// The property of line number foreground brush.
    /// </summary>
    public static readonly DependencyProperty LineNumberForegroundProperty = RegisterDependencyProperty<Brush>(nameof(LineNumberForeground), new SolidColorBrush(Color.FromArgb(255, 64, 128, 160)));

    /// <summary>
    /// The property of change type symbol foreground brush.
    /// </summary>
    public static readonly DependencyProperty ChangeTypeForegroundProperty = RegisterDependencyProperty<Brush>(nameof(ChangeTypeForeground), new SolidColorBrush(Color.FromArgb(255, 128, 128, 128)));

    /// <summary>
    /// The property of unchanged text background brush.
    /// </summary>
    public static readonly DependencyProperty UnchangedBackgroundProperty = RegisterDependencyProperty<Brush>(nameof(UnchangedBackground), null);

    /// <summary>
    /// The property of base-only change background brush.
    /// </summary>
    public static readonly DependencyProperty BaseOnlyBackgroundProperty = RegisterDependencyProperty<Brush>(nameof(BaseOnlyBackground), new SolidColorBrush(Color.FromArgb(64, 128, 128, 128)));

    /// <summary>
    /// The property of yours-only change background brush.
    /// </summary>
    public static readonly DependencyProperty YoursOnlyBackgroundProperty = RegisterDependencyProperty<Brush>(nameof(YoursOnlyBackground), new SolidColorBrush(Color.FromArgb(64, 96, 216, 32)));

    /// <summary>
    /// The property of theirs-only change background brush.
    /// </summary>
    public static readonly DependencyProperty TheirsOnlyBackgroundProperty = RegisterDependencyProperty<Brush>(nameof(TheirsOnlyBackground), new SolidColorBrush(Color.FromArgb(64, 32, 96, 216)));

    /// <summary>
    /// The property of both-same change background brush.
    /// </summary>
    public static readonly DependencyProperty BothSameBackgroundProperty = RegisterDependencyProperty<Brush>(nameof(BothSameBackground), new SolidColorBrush(Color.FromArgb(64, 216, 216, 32)));

    /// <summary>
    /// The property of conflict background brush.
    /// </summary>
    public static readonly DependencyProperty ConflictBackgroundProperty = RegisterDependencyProperty<Brush>(nameof(ConflictBackground), new SolidColorBrush(Color.FromArgb(64, 216, 32, 32)));

    private ThreeWayMergeResult mergeResult;
    private ThreeWayDiffResult diffResult;
    private readonly List<Button> conflictButtons = [];
    private bool isUpdatingScroll;

    /// <summary>
    /// Initializes a new instance of the ThreeWayMergeViewer class.
    /// </summary>
    public ThreeWayMergeViewer()
    {
        InitializeComponent();
        
        BaseContentPanel.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        YoursContentPanel.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        TheirsContentPanel.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        ResultContentPanel.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

        BaseContentPanel.SetBinding(ForegroundProperty, new Binding(nameof(Foreground)) { Source = this, Mode = BindingMode.OneWay });
        YoursContentPanel.SetBinding(ForegroundProperty, new Binding(nameof(Foreground)) { Source = this, Mode = BindingMode.OneWay });
        TheirsContentPanel.SetBinding(ForegroundProperty, new Binding(nameof(Foreground)) { Source = this, Mode = BindingMode.OneWay });
        ResultContentPanel.SetBinding(ForegroundProperty, new Binding(nameof(Foreground)) { Source = this, Mode = BindingMode.OneWay });
    }

    /// <summary>
    /// Gets or sets the base text.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public string BaseText
    {
        get => (string)GetValue(BaseTextProperty);
        set => SetValue(BaseTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the yours text.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public string YoursText
    {
        get => (string)GetValue(YoursTextProperty);
        set => SetValue(YoursTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the theirs text.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public string TheirsText
    {
        get => (string)GetValue(TheirsTextProperty);
        set => SetValue(TheirsTextProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether ignore the white space.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public bool IgnoreWhiteSpace
    {
        get => (bool)GetValue(IgnoreWhiteSpaceProperty);
        set => SetValue(IgnoreWhiteSpaceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether ignore case.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public bool IgnoreCase
    {
        get => (bool)GetValue(IgnoreCaseProperty);
        set => SetValue(IgnoreCaseProperty, value);
    }

    /// <summary>
    /// Gets or sets the line number width.
    /// </summary>
    [Bindable(true)]
    [Category("Appearance")]
    public int LineNumberWidth
    {
        get => (int)GetValue(LineNumberWidthProperty);
        set => SetValue(LineNumberWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush of the line number.
    /// </summary>
    [Bindable(true)]
    public Brush LineNumberForeground
    {
        get => (Brush)GetValue(LineNumberForegroundProperty);
        set => SetValue(LineNumberForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush of the change type symbol.
    /// </summary>
    [Bindable(true)]
    public Brush ChangeTypeForeground
    {
        get => (Brush)GetValue(ChangeTypeForegroundProperty);
        set => SetValue(ChangeTypeForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush of unchanged lines.
    /// </summary>
    [Bindable(true)]
    public Brush UnchangedBackground
    {
        get => (Brush)GetValue(UnchangedBackgroundProperty);
        set => SetValue(UnchangedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush of base-only changes.
    /// </summary>
    [Bindable(true)]
    public Brush BaseOnlyBackground
    {
        get => (Brush)GetValue(BaseOnlyBackgroundProperty);
        set => SetValue(BaseOnlyBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush of yours-only changes.
    /// </summary>
    [Bindable(true)]
    public Brush YoursOnlyBackground
    {
        get => (Brush)GetValue(YoursOnlyBackgroundProperty);
        set => SetValue(YoursOnlyBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush of theirs-only changes.
    /// </summary>
    [Bindable(true)]
    public Brush TheirsOnlyBackground
    {
        get => (Brush)GetValue(TheirsOnlyBackgroundProperty);
        set => SetValue(TheirsOnlyBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush of both-same changes.
    /// </summary>
    [Bindable(true)]
    public Brush BothSameBackground
    {
        get => (Brush)GetValue(BothSameBackgroundProperty);
        set => SetValue(BothSameBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush of conflicts.
    /// </summary>
    [Bindable(true)]
    public Brush ConflictBackground
    {
        get => (Brush)GetValue(ConflictBackgroundProperty);
        set => SetValue(ConflictBackgroundProperty, value);
    }

    /// <summary>
    /// Gets the merged text result.
    /// </summary>
    public string MergedText => mergeResult != null ? string.Join("\n", mergeResult.MergedPieces) : string.Empty;

    /// <summary>
    /// Gets a value indicating whether the merge has conflicts.
    /// </summary>
    public bool HasConflicts => mergeResult != null && !mergeResult.IsSuccessful;

    /// <summary>
    /// Sets the three texts for comparison.
    /// </summary>
    /// <param name="baseText">The base text.</param>
    /// <param name="yoursText">Your version of the text.</param>
    /// <param name="theirsText">Their version of the text.</param>
    public void SetTexts(string baseText, string yoursText, string theirsText)
    {
        BaseText = baseText;
        YoursText = yoursText;
        TheirsText = theirsText;
    }

    /// <summary>
    /// Refreshes the three-way merge view.
    /// </summary>
    public void Refresh()
    {
        if (BaseText == null || YoursText == null || TheirsText == null) return;

        var differ = ThreeWayDiffer.Instance;
        var chunker = new LineChunker();
        
        diffResult = differ.CreateDiffs(BaseText, YoursText, TheirsText, IgnoreWhiteSpace, IgnoreCase, chunker);
        mergeResult = differ.CreateMerge(BaseText, YoursText, TheirsText, IgnoreWhiteSpace, IgnoreCase, chunker);

        RenderThreeWayDiff();
        RenderMergeResult();
        UpdateConflictButtons();
    }

    private void RenderThreeWayDiff()
    {
        if (diffResult == null) return;

        var baseDiffLines = CreateDiffPaneLines(diffResult.PiecesBase, diffResult.DiffBlocks, DiffPaneType.Base);
        var yoursDiffLines = CreateDiffPaneLines(diffResult.PiecesOld, diffResult.DiffBlocks, DiffPaneType.Yours);
        var theirsDiffLines = CreateDiffPaneLines(diffResult.PiecesNew, diffResult.DiffBlocks, DiffPaneType.Theirs);

        Helper.RenderDiffPaneLines(BaseContentPanel, baseDiffLines, LineNumberForeground, this);
        Helper.RenderDiffPaneLines(YoursContentPanel, yoursDiffLines, LineNumberForeground, this);
        Helper.RenderDiffPaneLines(TheirsContentPanel, theirsDiffLines, LineNumberForeground, this);
    }

    private void RenderMergeResult()
    {
        if (mergeResult == null) return;

        var resultLines = CreateMergeResultLines();
        Helper.RenderDiffPaneLines(ResultContentPanel, resultLines, LineNumberForeground, this);
    }

    private List<DiffPiece> CreateDiffPaneLines(IReadOnlyList<string> pieces, 
        IList<ThreeWayDiffBlock> blocks, DiffPaneType paneType)
    {
        var lines = new List<DiffPiece>();
        var lineIndex = 0;

        foreach (var block in blocks)
        {
            int start, count;
            switch (paneType)
            {
                case DiffPaneType.Base:
                    start = block.BaseStart;
                    count = block.BaseCount;
                    break;
                case DiffPaneType.Yours:
                    start = block.OldStart;
                    count = block.OldCount;
                    break;
                case DiffPaneType.Theirs:
                    start = block.NewStart;
                    count = block.NewCount;
                    break;
                default:
                    start = 0;
                    count = 0;
                    break;
            }

            for (var i = 0; i < count; i++)
            {
                var pieceIndex = start + i;
                if (pieceIndex < pieces.Count)
                {
                    var changeType = GetChangeTypeForPane(block.ChangeType, paneType);
                    var piece = new DiffPiece(pieces[pieceIndex], changeType, lineIndex + 1);
                    lines.Add(piece);
                }
                lineIndex++;
            }
        }

        return lines;
    }

    private List<DiffPiece> CreateMergeResultLines()
    {
        var lines = new List<DiffPiece>();
        var lineIndex = 0;

        foreach (var piece in mergeResult.MergedPieces)
        {
            var changeType = ChangeType.Unchanged;
            
            // Check if this line is part of a conflict
            var isConflictLine = piece.StartsWith("<<<<<<< ") || piece.StartsWith("||||||| ") || 
                               piece.StartsWith("======= ") || piece.StartsWith(">>>>>>> ");
            
            if (isConflictLine)
            {
                changeType = ChangeType.Deleted; // Use deleted style for conflict markers
            }
            else
            {
                // Check if this line is part of conflict content
                foreach (var conflict in mergeResult.ConflictBlocks)
                {
                    if (lineIndex >= conflict.MergedStart && 
                        lineIndex < conflict.MergedStart + GetConflictBlockLineCount(conflict))
                    {
                        changeType = ChangeType.Modified;
                        break;
                    }
                }
            }

            var diffPiece = new DiffPiece(piece, changeType, lineIndex + 1);
            lines.Add(diffPiece);
            lineIndex++;
        }

        return lines;
    }

    private int GetConflictBlockLineCount(ThreeWayConflictBlock conflict)
    {
        // Count: <<<<<<< line + yours lines + ||||||| line + base lines + ======= line + theirs lines + >>>>>>> line
        return 1 + conflict.OldPieces.Count + 1 + conflict.BasePieces.Count + 1 + conflict.NewPieces.Count + 1;
    }

    private ChangeType GetChangeTypeForPane(ThreeWayChangeType threeWayChangeType, DiffPaneType paneType)
    {
        return threeWayChangeType switch
        {
            ThreeWayChangeType.Unchanged => ChangeType.Unchanged,
            ThreeWayChangeType.OldOnly => paneType == DiffPaneType.Yours ? ChangeType.Inserted : 
                                          paneType == DiffPaneType.Base ? ChangeType.Deleted : ChangeType.Unchanged,
            ThreeWayChangeType.NewOnly => paneType == DiffPaneType.Theirs ? ChangeType.Inserted : 
                                           paneType == DiffPaneType.Base ? ChangeType.Deleted : ChangeType.Unchanged,
            ThreeWayChangeType.BothSame => paneType == DiffPaneType.Base ? ChangeType.Deleted : ChangeType.Inserted,
            ThreeWayChangeType.Conflict => ChangeType.Modified,
            _ => ChangeType.Unchanged
        };
    }

    private void UpdateConflictButtons()
    {
        ConflictButtonsPanel.Children.Clear();
        conflictButtons.Clear();

        if (mergeResult?.ConflictBlocks == null || mergeResult.ConflictBlocks.Count == 0)
        {
            ConflictPanel.Visibility = Visibility.Collapsed;
            return;
        }

        ConflictPanel.Visibility = Visibility.Visible;

        for (int i = 0; i < mergeResult.ConflictBlocks.Count; i++)
        {
            var conflict = mergeResult.ConflictBlocks[i];
            var conflictIndex = i;

            var acceptYoursButton = new Button
            {
                Content = $"Accept Yours ({i + 1})",
                Style = (Style)FindResource("ConflictButtonStyle")
            };
            acceptYoursButton.Click += (s, e) => ResolveConflict(conflictIndex, ConflictResolution.AcceptYours);

            var acceptTheirsButton = new Button
            {
                Content = $"Accept Theirs ({i + 1})",
                Style = (Style)FindResource("ConflictButtonStyle")
            };
            acceptTheirsButton.Click += (s, e) => ResolveConflict(conflictIndex, ConflictResolution.AcceptTheirs);

            var acceptBaseButton = new Button
            {
                Content = $"Accept Base ({i + 1})",
                Style = (Style)FindResource("ConflictButtonStyle")
            };
            acceptBaseButton.Click += (s, e) => ResolveConflict(conflictIndex, ConflictResolution.AcceptBase);

            ConflictButtonsPanel.Children.Add(acceptYoursButton);
            ConflictButtonsPanel.Children.Add(acceptTheirsButton);
            ConflictButtonsPanel.Children.Add(acceptBaseButton);

            conflictButtons.Add(acceptYoursButton);
            conflictButtons.Add(acceptTheirsButton);
            conflictButtons.Add(acceptBaseButton);
        }
    }

    private void ResolveConflict(int conflictIndex, ConflictResolution resolution)
    {
        if (mergeResult?.ConflictBlocks == null || conflictIndex >= mergeResult.ConflictBlocks.Count) return;

        var conflict = mergeResult.ConflictBlocks[conflictIndex];
        var conflictStartLine = conflict.MergedStart;
        var conflictEndLine = conflictStartLine + GetConflictBlockLineCount(conflict);

        var newMergedPieces = new List<string>(mergeResult.MergedPieces);
        
        // Remove conflict block
        for (int i = conflictEndLine - 1; i >= conflictStartLine; i--)
        {
            if (i < newMergedPieces.Count)
                newMergedPieces.RemoveAt(i);
        }

        // Insert resolved content
        var resolvedContent = resolution switch
        {
            ConflictResolution.AcceptYours => conflict.OldPieces,
            ConflictResolution.AcceptTheirs => conflict.NewPieces,
            ConflictResolution.AcceptBase => conflict.BasePieces,
            _ => conflict.OldPieces
        };

        foreach (var line in resolvedContent.Reverse())
        {
            newMergedPieces.Insert(conflictStartLine, line);
        }

        // Update merge result
        var updatedConflictBlocks = mergeResult.ConflictBlocks.Where((c, i) => i != conflictIndex).ToList();
        mergeResult = new ThreeWayMergeResult(newMergedPieces, updatedConflictBlocks.Count == 0, updatedConflictBlocks, diffResult);

        RenderMergeResult();
        UpdateConflictButtons();
    }

    private void ContentPanel_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (isUpdatingScroll) return;

        isUpdatingScroll = true;
        try
        {
            var source = sender as InternalLinesViewer;
            if (source == null) return;

            // Sync all panels
            if (source != BaseContentPanel)
                BaseContentPanel.ScrollToVerticalOffset(e.VerticalOffset);
            if (source != YoursContentPanel)
                YoursContentPanel.ScrollToVerticalOffset(e.VerticalOffset);
            if (source != TheirsContentPanel)
                TheirsContentPanel.ScrollToVerticalOffset(e.VerticalOffset);
            if (source != ResultContentPanel)
                ResultContentPanel.ScrollToVerticalOffset(e.VerticalOffset);
        }
        finally
        {
            isUpdatingScroll = false;
        }
    }

    private static DependencyProperty RegisterDependencyProperty<T>(string name, T defaultValue, Action<DependencyObject, DependencyPropertyChangedEventArgs> onChanged = null)
    {
        var callback = onChanged != null ? new PropertyChangedCallback(onChanged) : null;
        return DependencyProperty.Register(name, typeof(T), typeof(ThreeWayMergeViewer), new PropertyMetadata(defaultValue, callback));
    }

    private static DependencyProperty RegisterRefreshDependencyProperty<T>(string name, T defaultValue)
    {
        return DependencyProperty.Register(name, typeof(T), typeof(ThreeWayMergeViewer), 
            new PropertyMetadata(defaultValue, (d, e) =>
            {
                if (d is ThreeWayMergeViewer viewer && e.OldValue != e.NewValue)
                    viewer.Refresh();
            }));
    }

    private enum DiffPaneType
    {
        Base,
        Yours,
        Theirs
    }

    private enum ConflictResolution
    {
        AcceptYours,
        AcceptTheirs,
        AcceptBase
    }
}
