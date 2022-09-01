using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Trivial.Collection;
using Trivial.Data;
using Trivial.IO;
using Trivial.Text;
using Trivial.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace DiffPlex.UI;

using static System.Net.Mime.MediaTypeNames;
using DependencyObjectProxy = DependencyObjectProxy<DiffTextView>;

public sealed partial class DiffTextView : UserControl
{
    /// <summary>
    /// The dependency property of selection mode.
    /// </summary>
    public static readonly DependencyProperty SelectionModeProperty = DependencyObjectProxy.RegisterProperty(nameof(SelectionMode), ListViewSelectionMode.None);

    /// <summary>
    /// The dependency property of item container style.
    /// </summary>
    public static readonly DependencyProperty ItemContainerStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(ItemContainerStyle));

    /// <summary>
    /// The dependency property of command bar height.
    /// </summary>
    public static readonly DependencyProperty CommandBarHeightProperty = DependencyObjectProxy.RegisterProperty(nameof(CommandBarHeight), new GridLength(50));

    /// <summary>
    /// The dependency property of text style.
    /// </summary>
    public static readonly DependencyProperty TextStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(TextStyle));

    /// <summary>
    /// The dependency property of change type style.
    /// </summary>
    public static readonly DependencyProperty ChangeTypeStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(ChangeTypeStyle));

    /// <summary>
    /// The dependency property of change type width.
    /// </summary>
    public static readonly DependencyProperty ChangeTypeWidthProperty = DependencyObjectProxy.RegisterProperty(nameof(ChangeTypeWidth), new GridLength(20));

    /// <summary>
    /// The dependency property of line number style.
    /// </summary>
    public static readonly DependencyProperty LineNumberStyleProperty = DependencyObjectProxy.RegisterProperty<Style>(nameof(LineNumberStyle));

    /// <summary>
    /// The dependency property of line number width.
    /// </summary>
    public static readonly DependencyProperty LineNumberWidthProperty = DependencyObjectProxy.RegisterProperty(nameof(LineNumberWidth), new GridLength(50));

    /// <summary>
    /// The dependency property of text selection state.
    /// </summary>
    public static readonly DependencyProperty IsTextSelectionEnabledProperty = DependencyObjectProxy.RegisterProperty(nameof(IsTextSelectionEnabled), true);

    /// <summary>
    /// The dependency property of split view toggle.
    /// </summary>
    public static readonly DependencyProperty IsSplitViewProperty = DependencyObjectProxy.RegisterProperty(nameof(IsSplitView), SetSplitView, true);

    /// <summary>
    /// The dependency property of unchanged section collapse state.
    /// </summary>
    public static readonly DependencyProperty IsUnchangedSectionCollapsedProperty = DependencyObjectProxy.RegisterProperty(nameof(IsUnchangedSectionCollapsed), UpdateCollapseState, false);

    /// <summary>
    /// The dependency property of line count for context.
    /// </summary>
    public static readonly DependencyProperty LineCountForContextProperty = DependencyObjectProxy.RegisterProperty(nameof(LineCountForContext), UpdateCollapseState, 2);

    /// <summary>
    /// The dependency property of ignore white space.
    /// </summary>
    public static readonly DependencyProperty IgnoreWhiteSpaceProperty = DependencyObjectProxy.RegisterProperty(nameof(IgnoreWhiteSpace), Refresh, true);

    /// <summary>
    /// The dependency property of case sensitive.
    /// </summary>
    public static readonly DependencyProperty IsCaseSensitiveProperty = DependencyObjectProxy.RegisterProperty(nameof(IsCaseSensitive), Refresh, true);

    /// <summary>
    /// The dependency property of flag for file menu button.
    /// </summary>
    public static readonly DependencyProperty IsFileMenuEnabledProperty = DependencyObjectProxy.RegisterProperty(nameof(IsFileMenuEnabled), Refresh, true);

    /// <summary>
    /// The dependency property of old text.
    /// </summary>
    public static readonly DependencyProperty OldTextProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(OldText), Refresh);

    /// <summary>
    /// The dependency property of old text.
    /// </summary>
    public static readonly DependencyProperty NewTextProperty = DependencyObjectProxy.RegisterProperty<string>(nameof(NewText), Refresh);

    /// <summary>
    /// The dependency property of old text.
    /// </summary>
    public static readonly DependencyProperty CommandLabelPositionProperty = DependencyObjectProxy.RegisterProperty(nameof(CommandLabelPosition), CommandBarDefaultLabelPosition.Right);

    private List<DiffTextViewModel> sideBySide;
    private List<DiffPiece> inlines;
    private bool skipRefresh = true;
    private DiffTextViewReference reference;

    /// <summary>
    /// Initializes a new instance of the DiffTextView class.
    /// </summary>
    public DiffTextView()
    {
        InitializeComponent();
        SplitViewMenuItem.IsChecked = true;
        reference = new(this);
    }

    /// <summary>
    /// Adds or removes the change event on item.
    /// </summary>
    public event SelectionChangedEventHandler SelectionChanged;

    /// <summary>
    /// Gets or sets the handler to open file to read text.
    /// </summary>
    public Func<Task<string>> OpenFileToReadText { get; set; }

    /// <summary>
    /// Gets or sets the selection behavior for the element.
    /// </summary>
    public ListViewSelectionMode SelectionMode
    {
        get => (ListViewSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets the item container style.
    /// </summary>
    public Style ItemContainerStyle
    {
        get => (Style)GetValue(ItemContainerStyleProperty);
        set => SetValue(ItemContainerStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the line number style.
    /// </summary>
    public GridLength CommandBarHeight
    {
        get => (GridLength)GetValue(CommandBarHeightProperty);
        set => SetValue(CommandBarHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the text style.
    /// </summary>
    public Style TextStyle
    {
        get => (Style)GetValue(TextStyleProperty);
        set => SetValue(TextStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the change type style.
    /// </summary>
    public Style ChangeTypeStyle
    {
        get => (Style)GetValue(ChangeTypeStyleProperty);
        set => SetValue(ChangeTypeStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of change type text.
    /// </summary>
    public GridLength ChangeTypeWidth
    {
        get => (GridLength)GetValue(ChangeTypeWidthProperty);
        set => SetValue(ChangeTypeWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the line number style.
    /// </summary>
    public Style LineNumberStyle
    {
        get => (Style)GetValue(LineNumberStyleProperty);
        set => SetValue(LineNumberStyleProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of line number.
    /// </summary>
    public GridLength LineNumberWidth
    {
        get => (GridLength)GetValue(LineNumberWidthProperty);
        set => SetValue(LineNumberWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether text selection is enabled in each line.
    /// </summary>
    public bool IsTextSelectionEnabled
    {
        get => (bool)GetValue(IsTextSelectionEnabledProperty);
        set => SetValue(IsTextSelectionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether it is split view.
    /// </summary>
    public bool IsSplitView
    {
        get => (bool)GetValue(IsSplitViewProperty);
        set => SetValue(IsSplitViewProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether it is unified view.
    /// </summary>
    public bool IsUnifiedView
    {
        get => !IsSplitView;
        set => IsSplitView = !value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether it is unchanged section.
    /// </summary>
    public bool IsUnchangedSectionCollapsed
    {
        get => (bool)GetValue(IsUnchangedSectionCollapsedProperty);
        set => SetValue(IsUnchangedSectionCollapsedProperty, value);
    }

    /// <summary>
    /// Gets or sets the line count for context.
    /// </summary>
    public int LineCountForContext
    {
        get => (int)GetValue(LineCountForContextProperty);
        set => SetValue(LineCountForContextProperty, value);
    }

    /// <summary>
    /// Gets or sets the old text.
    /// </summary>
    public string OldText
    {
        get => (string)GetValue(OldTextProperty);
        set => SetValue(OldTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the new text.
    /// </summary>
    public string NewText
    {
        get => (string)GetValue(NewTextProperty);
        set => SetValue(NewTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the default label position of command bar.
    /// </summary>
    public CommandBarDefaultLabelPosition CommandLabelPosition
    {
        get => (CommandBarDefaultLabelPosition)GetValue(CommandLabelPositionProperty);
        set => SetValue(CommandLabelPositionProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether ignore white space.
    /// </summary>
    public bool IgnoreWhiteSpace
    {
        get => (bool)GetValue(IgnoreWhiteSpaceProperty);
        set => SetValue(IgnoreWhiteSpaceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the text is case sensitive.
    /// </summary>
    public bool IsCaseSensitive
    {
        get => (bool)GetValue(IsCaseSensitiveProperty);
        set => SetValue(IsCaseSensitiveProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the file menu button is enabled.
    /// </summary>
    public bool IsFileMenuEnabled
    {
        get => (bool)GetValue(IsFileMenuEnabledProperty);
        set => SetValue(IsFileMenuEnabledProperty, value);
    }

    /// <summary>
    /// Gets the collection of secondary command elements for the command bar.
    /// </summary>
    public IObservableVector<ICommandBarElement> SecondaryCommands => TopCommandBar.SecondaryCommands;

    /// <summary>
    /// Sets the text.
    /// </summary>
    /// <param name="left">The old text.</param>
    /// <param name="right">The new text.</param>
    public void SetText(string oldText, string newText)
    {
        skipRefresh = true;
        OldText = oldText;
        NewText = newText;
    }

    /// <summary>
    /// Sets the text.
    /// </summary>
    /// <param name="left">The old text.</param>
    /// <param name="right">The new text.</param>
    public void SetText(CharsReader oldText, CharsReader newText)
    {
        skipRefresh = true;
        OldText = oldText?.ReadToEnd();
        NewText = newText?.ReadToEnd();
    }

    /// <summary>
    /// Refreshes.
    /// </summary>
    public void Refresh()
    {
        if (skipRefresh)
        {
            skipRefresh = false;
            return;
        }

        SplitElement.ItemsSource = null;
        UnifiedElement.ItemsSource = null;
        if (IsSplitView) RefreshSplitView();
        else RefreshUnifiedView();
    }

    /// <summary>
    /// Clears.
    /// </summary>
    public void Clear()
    {
        sideBySide = null;
        inlines = null;
        SplitElement.ItemsSource = null;
        UnifiedElement.ItemsSource = null;
    }

    /// <summary>
    /// Scrolls to previous diff line.
    /// </summary>
    public void ScrollPreviousDiffIntoView()
    {
        if (IsSplitView) ScrollPreviousDiffIntoView<DiffTextViewModel>(SplitElement, GetDiffPiece);
        else ScrollPreviousDiffIntoView<InlineDiffTextViewModel>(UnifiedElement, GetDiffPiece);
    }

    /// <summary>
    /// Scrolls to previous diff line.
    /// </summary>
    private static void ScrollPreviousDiffIntoView<T>(ListView list, Func<T, DiffPiece> resolver) where T : class
    {
        var elements = new List<(T, double)>();
        foreach (var item in list.Items)
        {
            var container = list.ContainerFromItem(item) as ListViewItem;
            if (container == null || item is not T model) continue;
            var piece = resolver(model);
            if (piece == null) continue;
            var transform = container.TransformToVisual(list) as MatrixTransform;
            if (transform == null) continue;
            var pos = transform.Matrix.OffsetY;
            var isUnchanged = piece.Type == ChangeType.Unchanged;
            if (pos >= 0) break;
            if (!isUnchanged) elements.Add((model, pos));
        }

        var height = -Math.Max(list.ActualHeight - 50, 10);
        for (var i = 0; i < elements.Count; i++)
        {
            var (model, pos) = elements[i];
            if (pos < height) continue;
            list.ScrollIntoView(model);
            return;
        }

        var first = list.Items.OfType<T>().FirstOrDefault();
        if (first != null) list.ScrollIntoView(first);
    }
    private static void ScrollPreviousDiffIntoView2<T>(ListView list, Func<T, DiffPiece> resolver) where T : class
    {
        T target = null;
        foreach (var item in list.Items)
        {
            var container = list.ContainerFromItem(item) as ListViewItem;
            if (container == null || item is not T model) continue;
            var piece = resolver(model);
            if (piece == null) continue;
            var transform = container.TransformToVisual(list) as MatrixTransform;
            if (transform == null) continue;
            var pos = transform.Matrix.OffsetY;
            var isUnchanged = piece.Type == ChangeType.Unchanged;
            if (pos < 0)
            {
                if (!isUnchanged) target = model;
                continue;
            }

            if (target == null) target = list.Items.OfType<T>().FirstOrDefault();
            if (target != null) list.ScrollIntoView(target);
            break;
        }
    }

    /// <summary>
    /// Scrolls to next diff line.
    /// </summary>
    public void ScrollNextDiffIntoView()
    {
        if (IsSplitView) ScrollNextDiffIntoView<DiffTextViewModel>(SplitElement, GetDiffPiece);
        else ScrollNextDiffIntoView<InlineDiffTextViewModel>(UnifiedElement, GetDiffPiece);
    }

    /// <summary>
    /// Scrolls to previous diff line.
    /// </summary>
    private static void ScrollNextDiffIntoView<T>(ListView list, Func<T, DiffPiece> resolver) where T : class
    {
        var isIn = false;
        var height = Math.Max(list.ActualHeight - 50, 10);
        foreach (var item in list.Items)
        {
            var container = list.ContainerFromItem(item) as ListViewItem;
            if (container == null || item is not T model) continue;
            var piece = resolver(model);
            if (piece == null) continue;
            var transform = container.TransformToVisual(list) as MatrixTransform;
            if (transform == null) continue;
            var pos = transform.Matrix.OffsetY;
            if (pos < height)
            {
                if (pos > 0) isIn = true;
                continue;
            }

            if (!isIn || piece.Type == ChangeType.Unchanged) continue;
            isIn = true;
            list.ScrollIntoView(model, ScrollIntoViewAlignment.Leading);
            break;
        }
    }

    /// <summary>
    /// Scrolls the list to bring the specified line number into view with the specified alignment.
    /// </summary>
    /// <param name="lineNumber">The line number.</param>
    /// <param name="alignment">An enumeration value that specifies whether the item uses default or leading alignment.</param>
    public void ScrollIntoView(int lineNumber, ScrollIntoViewAlignment alignment = ScrollIntoViewAlignment.Default)
    {
        if (IsSplitView)
        {
            var line = sideBySide.FirstOrDefault(ele => ele?.Right?.Position == lineNumber);
            SplitElement.ScrollIntoView(line, alignment);
        }
        else
        {
            var line = inlines.FirstOrDefault(ele => ele?.Position == lineNumber);
            UnifiedElement.ScrollIntoView(line, alignment);
        }
    }

    /// <summary>
    /// Collapses unchanged sections.
    /// </summary>
    /// <param name="linesForContext">The count of line for context.</param>
    public void CollapseUnchangedSections(int linesForContext)
    {
        LineCountForContext = linesForContext;
        IsUnchangedSectionCollapsed = true;
    }

    /// <summary>
    /// Sets old text.
    /// </summary>
    /// <param name="value">The input value.</param>
    public void SetOldText(CharsReader value)
        => OldText = value?.ReadToEnd();

    /// <summary>
    /// Sets old text.
    /// </summary>
    /// <param name="value">The input value.</param>
    public void SetNewText(CharsReader value)
        => NewText = value?.ReadToEnd();

    public void ShowOldFileSelectDialog()
    {
        var h = OpenFileToReadText;
        if (h != null)
        {
            _ = OnFileSelectAsync(h, false);
            return;
        }

        FilePathContainer.Visibility = Visibility.Visible;
        FilePathContainer.Tag = false;
        _ = FocusFilePathTextBoxAsync();
    }

    public void ShowNewFileSelectDialog()
    {
        var h = OpenFileToReadText;
        if (h != null)
        {
            _ = OnFileSelectAsync(h, true);
            return;
        }

        FilePathContainer.Visibility = Visibility.Visible;
        FilePathContainer.Tag = true;
        _ = FocusFilePathTextBoxAsync();
    }

    private List<T> Filter<T>(List<T> col, Func<T, DiffPiece> piece)
    {
        if (!IsUnchangedSectionCollapsed) return col;
        var list = new List<T>();
        var lines = LineCountForContext;
        var isUnchanged = -1;
        for (var i = 0; i < col.Count; i++)
        {
            try
            {
                var row = col[i];
                var item = piece(row);
                if (item == null) continue;
                var j = list.Count;
                list.Add(row);
                if (item.Type == ChangeType.Unchanged)
                {
                    if (isUnchanged < 0) isUnchanged = j;
                    continue;
                }

                if (isUnchanged < 0) continue;
                var begin = isUnchanged + lines;
                var len = j - lines - begin;
                isUnchanged = -1;
                if (len >= 0) list.RemoveRange(begin, len);
            }
            catch (ArgumentException)
            {
                return col;
            }
        }

        return list;
    }

    private void RefreshSplitView(bool forceToUpdate = false)
    {
        if (SplitElement.ItemsSource != null && !forceToUpdate) return;
        sideBySide = null;
        var diff = SideBySideDiffBuilder.Diff(OldText ?? string.Empty, NewText ?? string.Empty, IgnoreWhiteSpace, !IsCaseSensitive);
        var left = diff?.OldText?.Lines ?? new();
        var right = diff?.NewText?.Lines ?? new();
        var count = Math.Max(left.Count, right.Count);
        var col = new List<DiffTextViewModel>();
        for (var i = 0; i < count; i++)
        {
            var r = i < right.Count ? right[i] : new(null, ChangeType.Imaginary);
            var l = i < left.Count ? left[i] : new(null, ChangeType.Imaginary);
            col.Add(new(i, l, r, reference));
        }

        sideBySide = col;
        SplitElement.ItemsSource = Filter(col, GetDiffPiece);
    }

    private void RefreshUnifiedView(bool forceToUpdate = false)
    {
        if (UnifiedElement.ItemsSource != null && !forceToUpdate) return;
        var lines = InlineDiffBuilder.Diff(OldText ?? string.Empty, NewText ?? string.Empty, IgnoreWhiteSpace, !IsCaseSensitive)?.Lines;
        inlines = lines;
        var col = new List<InlineDiffTextViewModel>();
        if (lines == null)
        {
            UnifiedElement.ItemsSource = col;
            return;
        }

        var i = 0;
        foreach (var item in lines)
        {
            if (item == null) continue;
            col.Add(new(i, item, reference));
            i++;
        }

        UnifiedElement.ItemsSource = Filter(col, GetDiffPiece);
    }

    private void UpdateCollapseState()
    {
        var isCollapsed = IsUnchangedSectionCollapsed;
        var i = LineCountForContext;
        if (i < 0) i = 0;
        else if (i > 100) i = 100;
        if (CollapseUnchangedMenuItem.IsChecked != isCollapsed) CollapseUnchangedMenuItem.IsChecked = isCollapsed;
        LinesContextMenuItem.Visibility = isCollapsed ? Visibility.Visible : Visibility.Collapsed;
        foreach (var m in LinesContextMenuItem.Items)
        {
            if (m is not RadioMenuFlyoutItem radio) continue;
            var s = radio.Text?.Trim();
            if (string.IsNullOrEmpty(s))
            {
                if (radio.IsChecked) radio.IsChecked = false;
                continue;
            }

            if (!int.TryParse(s, out var j)) continue;
            var isChecked = i == j;
            if (radio.IsChecked != isChecked) radio.IsChecked = isChecked;
        }

        if (IsSplitView)
        {
            RefreshSplitView(true);
            UnifiedElement.ItemsSource = null;
        }
        else
        {
            RefreshUnifiedView(true);
            SplitElement.ItemsSource = null;
        }
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        => SelectionChanged?.Invoke(this, e);

    private void OnGoToClick(object sender, RoutedEventArgs e)
    {
        if (GoToMenuButton.IsChecked == false)
        {
            GoToNumberBox.Value = double.NaN;
            GoToNumberContainer.Visibility = Visibility.Collapsed;
            if (IsSplitView) SplitElement.Focus(FocusState.Programmatic);
            else UnifiedElement.Focus(FocusState.Programmatic);
            return;
        }

        GoToNumberContainer.Visibility = Visibility.Visible;
        _ = FocusNumberBoxAsync();
    }

    private async Task FocusNumberBoxAsync()
    {
        await Task.Delay(200);
        GoToNumberBox.Focus(FocusState.Keyboard);
    }

    private void OnGoToChange(NumberBox sender, NumberBoxValueChangedEventArgs args)
    {
        try
        {
            ScrollIntoView((int)Math.Round(GoToNumberBox.Value), ScrollIntoViewAlignment.Leading);
        }
        catch (ArithmeticException)
        {
        }
        catch (InvalidCastException)
        {
        }

        GoToMenuButton.IsChecked = false;
        GoToNumberContainer.Visibility = Visibility.Collapsed;
        GoToNumberBox.Value = double.NaN;
        if (IsSplitView) SplitElement.Focus(FocusState.Programmatic);
        else UnifiedElement.Focus(FocusState.Programmatic);
    }

    private void OnSplitViewClick(object sender, RoutedEventArgs e)
        => IsSplitView = true;

    private void OnUnifiedViewClick(object sender, RoutedEventArgs e)
        => IsUnifiedView = true;

    private void OnLeftFileClick(object sender, RoutedEventArgs e)
        => ShowOldFileSelectDialog();

    private void OnRightFileClick(object sender, RoutedEventArgs e)
        => ShowNewFileSelectDialog();

    private async Task FocusFilePathTextBoxAsync()
    {
        await Task.Delay(200);
        FilePathTextBox.Focus(FocusState.Keyboard);
    }

    private void OnLeftTextClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnRightTextClick(object sender, RoutedEventArgs e)
    {
    }

    private void OnSwitchClick(object sender, RoutedEventArgs e)
    {
        var old = OldText;
        skipRefresh = true;
        OldText = NewText;
        NewText = old;
    }

    private void OnPreviousDiffClick(object sender, RoutedEventArgs e)
        => ScrollPreviousDiffIntoView();

    private void OnNextDiffClick(object sender, RoutedEventArgs e)
        => ScrollNextDiffIntoView();

    private void OnCollaspeUnchangedClick(object sender, RoutedEventArgs e)
        => IsUnchangedSectionCollapsed = CollapseUnchangedMenuItem.IsChecked;

    private void OnLinesContextClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem menu || string.IsNullOrWhiteSpace(menu.Text) || !int.TryParse(menu.Text.Trim(), out var i) || i < 0) return;
        CollapseUnchangedSections(i);
    }

    private void OnGoToBoxLostFocus(object sender, RoutedEventArgs e)
    {
        GoToNumberBox.Value = double.NaN;
        GoToNumberContainer.Visibility = Visibility.Collapsed;
        GoToMenuButton.IsChecked = false;
    }

    private void OnFileSelectClick(object sender, RoutedEventArgs e)
    {
        if (FilePathContainer.Tag is not bool b) return;
        FilePathContainer.Tag = null;
        var file = FileSystemInfoUtility.TryGetFileInfo(FilePathTextBox.Text);
        FilePathContainer.Visibility = Visibility.Collapsed;
        FilePathTextBox.Text = null;
        if (file == null || !file.Exists) return;
        try
        {
            var text = File.ReadAllText(file.FullName);
            skipRefresh = false;
            if (b) NewText = text;
            else OldText = text;
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (NotSupportedException)
        {
        }
    }

    private async Task OnFileSelectAsync(Func<Task<string>> task, bool isNew)
    {
        var s = await task();
        skipRefresh = false;
        if (isNew) NewText = s;
        else OldText = s;
    }

    private void OnFileCancelClick(object sender, RoutedEventArgs e)
    {
        FilePathTextBox.Text = null;
        FilePathContainer.Visibility = Visibility.Collapsed;
    }

    private static void Refresh<T>(DiffTextView c, ChangeEventArgs<T> e, DependencyProperty d)
        => c?.Refresh();

    private static void SetSplitView(DiffTextView c, bool state)
    {
        if (c == null) return;
        if (c.SplitViewMenuItem.IsChecked != state) c.SplitViewMenuItem.IsChecked = state;
        if (c.UnifiedViewMenuItem.IsChecked == state) c.UnifiedViewMenuItem.IsChecked = !state;
        var visibility = state ? Visibility.Visible : Visibility.Collapsed;
        if (c.SplitElement.Visibility != visibility) c.SplitElement.Visibility = visibility;
        if (c.UnifiedElement.Visibility == visibility) c.UnifiedElement.Visibility = state ? Visibility.Collapsed : Visibility.Visible;
        if (state) c.RefreshSplitView();
        else c.RefreshUnifiedView();
    }

    private static void SetSplitView(DiffTextView c, ChangeEventArgs<bool> e, DependencyProperty d)
    {
        if (c == null || e == null) return;
        SetSplitView(c, e.NewValue);
    }

    private static void UpdateCollapseState<T>(DiffTextView c, ChangeEventArgs<T> e, DependencyProperty d)
        => c?.UpdateCollapseState();

    private static DiffPiece GetDiffPiece(DiffTextViewModel m)
        => m.Right;

    private static DiffPiece GetDiffPiece(InlineDiffTextViewModel m)
        => m.Line;
}
