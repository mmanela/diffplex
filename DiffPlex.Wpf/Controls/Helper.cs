using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.Wpf.Controls;

internal static class Helper
{
    private const int MaxCount = 3000;
    public const string FontFamily = "Cascadia Code, Consolas, Courier New, monospace, Microsoft Yahei, Microsoft Jhenghei, Meiryo, Segoe UI, Segoe UI Emoji, Segoe UI Symbol";

    /// <summary>
    /// Updates the inline diffs view.
    /// </summary>
    internal static void RenderInlineDiffs(InternalLinesViewer viewer, ICollection<DiffPiece> lines, UIElement source, int contextLineCount)
    {
        viewer.Clear();
        if (lines == null) return;
        if (lines.Any() == false) return;
        var disableSubPieces = lines.Count > MaxCount;    // For performance.
        foreach (var line in lines)
        {
            if (line == null)
            {
                var c = viewer.Add(null, null, null as string, ChangeType.Unchanged.ToString(), source);
                c.Tag = line;
                continue;
            }

            var changeType = line.Type;
            var text = line.Text;
            var hasAdded = false;
            switch (line.Type)
            {
                case ChangeType.Modified:
                    changeType = ChangeType.Inserted;
                    break;
                case ChangeType.Inserted:
                    if (line.SubPieces != null && line.SubPieces.Count > 1 && !disableSubPieces)
                    {
                        var details = GetSubPiecesInfo(line, true);
                        var c = viewer.Add(line.Position, "+", details, changeType.ToString(), source);
                        c.Tag = line;
                        hasAdded = true;
                    }

                    break;
                case ChangeType.Deleted:
                    if (line.SubPieces != null && line.SubPieces.Count > 1 && !disableSubPieces)
                    {
                        var details = GetSubPiecesInfo(line, false);
                        var c = viewer.Add(line.Position, "-", details, changeType.ToString(), source);
                        c.Tag = line;
                        hasAdded = true;
                    }

                    break;
                case ChangeType.Unchanged:
                    break;
                default:
                    changeType = ChangeType.Imaginary;
                    text = string.Empty;
                    break;
            }

            if (!hasAdded)
            {
                var c = viewer.Add(line.Position, changeType switch
                {
                    ChangeType.Inserted => "+",
                    ChangeType.Deleted => "-",
                    _ => " "
                }, text, changeType.ToString(), source);
                c.Tag = line;
            }
        }

        if (contextLineCount > -1) CollapseUnchangedSections(viewer, contextLineCount);
        viewer.AdjustScrollView();
    }

    internal static void InsertLines(InternalLinesViewer panel, List<DiffPiece> lines, bool isOld, UIElement source, int contextLineCount)
    {
        if (lines == null || panel == null) return;
        var guid = panel.TrackingId = Guid.NewGuid();
        if (lines.Count < 600)
        {
            InsertLinesInteral(panel, lines, isOld, source);
            if (contextLineCount > -1) CollapseUnchangedSections(panel, contextLineCount);
            return;
        }

        _ = InsertLinesAsync(guid, panel, lines, isOld, source, contextLineCount);
    }

    private static async Task InsertLinesAsync(Guid guid, InternalLinesViewer panel, List<DiffPiece> lines, bool isOld, UIElement source, int contextLineCount)
    {   // For performance.
        if (lines == null || panel == null) return;
        var disablePieces = lines.Count > MaxCount;
        var i = 300;
        if (panel.TrackingId != guid) return;
        InsertLinesInteral(panel, lines.Take(i).ToList(), isOld, source, disablePieces);
        while (i < lines.Count)
        {
            await Task.Delay(i > 5000 ? 1000 : 800);
            if (panel.TrackingId != guid) return;
            panel.Dispatcher.Invoke(() =>
            {
                InsertLinesInteral(panel, lines.Skip(i).Take(500).ToList(), isOld, source, disablePieces);
            });
            i += 500;
        }

        if (contextLineCount > -1) CollapseUnchangedSections(panel, contextLineCount);
    }

    internal static void CollapseUnchangedSections(InternalLinesViewer panel, int contextLineCount)
    {
        var i = -1;
        var was = false;
        var last = 0;
        var max = -1;
        var removing = new List<int>();
        foreach (var ele in panel.GetTagsOfEachLine())
        {
            i++;
            if (ele is not DiffPiece e || e.Type != ChangeType.Unchanged)
            {
                if (!was)
                {
                    was = true;
                    if (contextLineCount > 0)
                    {
                        var first = Math.Max(last, removing.Count - contextLineCount);
                        removing.RemoveRange(first, removing.Count - first);
                    }
                }

                continue;
            }

            if (was)
            {
                was = false;
                last = removing.Count;
                max = i + contextLineCount;
            }

            if (i < max) continue;
            removing.Add(i);
        }

        ExpandUnchangedSections(panel);
        foreach (var j in removing)
        {
            panel.SetLineVisible(j, false);
        }

        if (removing.Count > 0 && panel.Count == removing.Count)
        {
            panel.SetLineVisible(0, true);
            panel.SetLineVisible(panel.Count - 1, true);
        }
    }

    internal static void ExpandUnchangedSections(InternalLinesViewer panel)
    {
        for (var i = 0; i < panel.Count; i++)
        {
            panel.SetLineVisible(i, true);
        }
    }

    /// <summary>
    /// Goes to the specific line.
    /// </summary>
    /// <param name="panel">The content panel.</param>
    /// <param name="lineIndex">The index of the line to go to.</param>
    /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
    internal static bool GoTo(InternalLinesViewer panel, int lineIndex)
    {
        try
        {
            var currentScrollPosition = panel.ValueScrollViewer.VerticalOffset;
            var point = new Point(0, currentScrollPosition);
            if (lineIndex == 0)
            {
                foreach (var item in panel.ValuePanel.Children)
                {
                    if (item is not FrameworkElement ele || ele.Tag is not DiffPiece line) continue;
                    var pos = ele.TransformToVisual(panel.ValueScrollViewer).Transform(point);
                    panel.ValueScrollViewer.ScrollToVerticalOffset(pos.Y);
                    return true;
                }
            }

            foreach (var item in panel.ValuePanel.Children)
            {
                if (item is not FrameworkElement ele || ele.Tag is not DiffPiece line || line?.Position != lineIndex) continue;
                var pos = ele.TransformToVisual(panel.ValueScrollViewer).Transform(point);
                panel.ValueScrollViewer.ScrollToVerticalOffset(pos.Y);
                return true;
            }
        }
        catch (InvalidOperationException)
        {
        }
        
        return false;
    }

    /// <summary>
    /// Goes to the specific line.
    /// </summary>
    /// <param name="panel">The content panel.</param>
    /// <param name="line">The line to go to.</param>
    /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
    internal static bool GoTo(InternalLinesViewer panel, DiffPiece line)
    {
        if (line == null) return false;
        try
        {
            var scrollView = panel.ValueScrollViewer;
            var point = new Point(0, 0);
            foreach (var item in panel.ValuePanel.Children)
            {
                if (item is not FrameworkElement ele || ele.Tag != line) continue;
                var pos = ele.TranslatePoint(point, panel.ValueScrollViewer);
                if (pos.Y >= 0 && pos.Y <= scrollView.ActualHeight - ele.ActualHeight) return true;
                var currentScrollPosition = panel.ValueScrollViewer.VerticalOffset;
                point = new Point(0, currentScrollPosition);
                pos = ele.TransformToVisual(panel.ValueScrollViewer).Transform(point);
                panel.ValueScrollViewer.ScrollToVerticalOffset(pos.Y);
                return true;
            }
        }
        catch (InvalidOperationException)
        {
        }

        return false;
    }

    /// <summary>
    /// Gets the line diff information.
    /// </summary>
    /// <param name="panel">The content panel.</param>
    /// <param name="lineIndex">The index of the line to get information.</param>
    /// <returns>The line diff information instance; or null, if non-exists.</returns>
    internal static DiffPiece GetLine(InternalLinesViewer panel, int lineIndex)
    {
        foreach (var item in panel.ValuePanel.Children)
        {
            if (item is not FrameworkElement ele || ele.Tag is not DiffPiece line || line?.Position != lineIndex) continue;
            return line;
        }

        return null;
    }

    /// <summary>
    /// Gets all line information in viewport.
    /// </summary>
    /// <param name="panel">The content panel.</param>
    /// <param name="level">The optional visibility level.</param>
    /// <returns>All lines.</returns>
    internal static IEnumerable<DiffPiece> GetLinesInViewport(InternalLinesViewer panel, VisibilityLevels level)
    {
        var states = GetLineViewportStates(panel, level);
        var needBreak = false;
        foreach (var item in states)
        {
            if (item.IsCollapsed) continue;
            if (!item.IsInViewport)
            {
                if (needBreak) yield break;
                continue;
            }

            needBreak = true;
            yield return item.Model;
        }
    }

    /// <summary>
    /// Gets all line information before viewport.
    /// </summary>
    /// <param name="panel">The content panel.</param>
    /// <param name="level">The optional visibility level.</param>
    /// <returns>All lines.</returns>
    internal static IEnumerable<DiffPiece> GetLinesBeforeViewport(InternalLinesViewer panel, VisibilityLevels level)
    {
        var states = GetLineViewportStates(panel, level);
        foreach (var item in states)
        {
            if (item.IsCollapsed) continue;
            if (item.IsInViewport) yield break;
            yield return item.Model;
        }
    }

    /// <summary>
    /// Gets all line information after viewport.
    /// </summary>
    /// <param name="panel">The content panel.</param>
    /// <param name="level">The optional visibility level.</param>
    /// <returns>All lines.</returns>
    internal static IEnumerable<DiffPiece> GetLinesAfterViewport(InternalLinesViewer panel, VisibilityLevels level)
    {
        var states = GetLineViewportStates(panel, level);
        var needReturn = false;
        foreach (var item in states)
        {
            if (item.IsCollapsed) continue;
            if (item.IsInViewport)
            {
                needReturn = true;
                continue;
            }

            if (needReturn) yield return item.Model;
        }
    }

    internal static string GetButtonName(string original, string hotkey)
    {
        return original.StartsWith(hotkey, StringComparison.OrdinalIgnoreCase)
            ? ("_" + original)
            : $"{original} (_{hotkey})";
    }

    internal static ContextMenu CreateLineContextMenu(FrameworkElement parentElement)
    {
        var menu = new ContextMenu();
        var copyMenuItem = new MenuItem
        {
            Header = GetButtonName(Resource.CopyThisLine ?? "Copy this line", "C")
        };
        menu.Items.Add(copyMenuItem);
        var str = string.Empty;
        parentElement.ContextMenuOpening += (sender, ev) =>
        {
            copyMenuItem.IsEnabled = false;
            var ele = ev.OriginalSource as FrameworkElement;
            while (ele != null && ele != parentElement && ele is not Window)
            {
                if (ele.Tag is DiffPiece piece)
                {
                    str = piece.Text;
                    break;
                }

                ele = ele.Parent as FrameworkElement;
            }

            copyMenuItem.IsEnabled = !string.IsNullOrWhiteSpace(str);
        };
        copyMenuItem.Click += (sender, ev) =>
        {
            if (string.IsNullOrEmpty(str)) return;
            try
            {
                Clipboard.SetText(str);
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
            }
            catch (InvalidOperationException)
            {
            }
            catch (NotSupportedException)
            {
            }
        };
        return menu;
    }

    /// <summary>
    /// Finds all line numbers that the text contains the given string.
    /// </summary>
    /// <param name="panel">The content panel.</param>
    /// <param name="q">The string to seek.</param>
    /// <returns>All lines with the given string.</returns>
    internal static IEnumerable<DiffPiece> Find(InternalLinesViewer panel, string q)
    {
        if (string.IsNullOrEmpty(q)) yield break;
        foreach (var item in panel.ValuePanel.Children)
        {
            if (item is not FrameworkElement ele || ele.Tag is not DiffPiece line || line.Text == null) continue;
            if (line.Text.Contains(q)) yield return line;
        }
    }

    /// <summary>
    /// Gets all line information in viewport.
    /// </summary>
    /// <param name="panel">The content panel.</param>
    /// <param name="level">The optional visibility level.</param>
    /// <returns>All lines.</returns>
    private static IEnumerable<LineViewportStateInfo> GetLineViewportStates(InternalLinesViewer panel, VisibilityLevels level)
    {
        var scrollView = panel.ValueScrollViewer;
        var point = new Point(0, 0);
        switch (level)
        {
            case VisibilityLevels.All:
                foreach (var item in panel.ValuePanel.Children)
                {
                    if (item is not FrameworkElement ele || ele.Tag is not DiffPiece line) continue;
                    var pos = ele.TranslatePoint(point, panel.ValueScrollViewer);
                    var isIn = ele.ActualHeight > 0 && pos.Y >= 0 && pos.Y <= scrollView.ActualHeight - ele.ActualHeight;
                    yield return new(line, isIn, ele.Visibility != Visibility.Visible);
                }

                break;
            case VisibilityLevels.Half:
                foreach (var item in panel.ValuePanel.Children)
                {
                    if (item is not FrameworkElement ele || ele.Tag is not DiffPiece line) continue;
                    var pos = ele.TranslatePoint(point, panel.ValueScrollViewer);
                    var halfHeight = ele.ActualHeight / 2;
                    var isIn = halfHeight > 0 && pos.Y >= -halfHeight && pos.Y <= scrollView.ActualHeight - halfHeight;
                    yield return new(line, isIn, ele.Visibility != Visibility.Visible);
                }

                break;
            default:
                foreach (var item in panel.ValuePanel.Children)
                {
                    if (item is not FrameworkElement ele || ele.Tag is not DiffPiece line) continue;
                    var pos = ele.TranslatePoint(point, panel.ValueScrollViewer);
                    var isIn = ele.ActualHeight > 0 && pos.Y > -ele.ActualHeight && pos.Y < scrollView.ActualHeight;
                    yield return new(line, isIn, ele.Visibility != Visibility.Visible);
                }

                break;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0056:", Justification = "Not supported.")]
    private static List<KeyValuePair<string, string>> GetSubPiecesInfo(DiffPiece line, bool isOld)
    {
        var details = new List<KeyValuePair<string, string>>();
        foreach (var ele in line.SubPieces)
        {
            if (string.IsNullOrEmpty(ele?.Text)) continue;
            var subType = ele.Type switch
            {
                ChangeType.Modified => isOld ? ChangeType.Deleted : ChangeType.Inserted,
                ChangeType.Inserted => ChangeType.Inserted,
                ChangeType.Deleted => ChangeType.Deleted,
                ChangeType.Unchanged => ChangeType.Unchanged,
                _ => ChangeType.Imaginary
            };
            var subTypeStr = subType != ChangeType.Imaginary ? subType.ToString() : null;
            if (details.Count > 0)
            {
                var last = details[details.Count - 1];
                if (string.Equals(last.Value, subTypeStr, StringComparison.InvariantCulture))
                {
                    details[details.Count - 1] = new KeyValuePair<string, string>(last.Key + ele.Text, subTypeStr);
                    continue;
                }
            }

            details.Add(new KeyValuePair<string, string>(ele.Text, subTypeStr));
        }

        return details;
    }

    private static void InsertLinesInteral(InternalLinesViewer panel, List<DiffPiece> lines, bool isOld, UIElement source, bool disableSubPieces = false)
    {
        foreach (var line in lines)
        {
            if (line == null)
            {
                var c = panel.Add(null, null, null as string, ChangeType.Unchanged.ToString(), source);
                c.Tag = line;
                continue;
            }

            var changeType = line.Type;
            var text = line.Text;
            var hasAdded = false;
            switch (line.Type)
            {
                case ChangeType.Modified:
                    changeType = isOld ? ChangeType.Deleted : ChangeType.Inserted;
                    if (line.SubPieces != null && line.SubPieces.Count > 1 && !disableSubPieces)
                    {
                        var details = GetSubPiecesInfo(line, isOld);
                        var c = panel.Add(line.Position, isOld ? "-" : "+", details, changeType.ToString(), source);
                        c.Tag = line;
                        hasAdded = true;
                    }

                    break;
                case ChangeType.Inserted:
                case ChangeType.Deleted:
                case ChangeType.Unchanged:
                    break;
                default:
                    changeType = ChangeType.Imaginary;
                    text = string.Empty;
                    break;
            }

            if (!hasAdded)
            {
                var c = panel.Add(line.Position, changeType switch
                {
                    ChangeType.Inserted => "+",
                    ChangeType.Deleted => "-",
                    _ => " "
                }, text, changeType.ToString(), source);
                c.Tag = line;
            }
        }

        panel.AdjustScrollView();
    }
}
