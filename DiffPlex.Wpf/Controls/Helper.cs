using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.Wpf.Controls
{
    /// <summary>
    /// Visibility levels.
    /// </summary>
    public enum VisibilityLevels
    {
        /// <summary>
        /// Any part of visual in viewport.
        /// </summary>
        Any = 0,

        /// <summary>
        /// Half at least in viewport.
        /// </summary>
        Half = 1,

        /// <summary>
        /// All visual in viewport.
        /// </summary>
        All = 2
    }

    internal static class Helper
    {
        private const int MaxCount = 3000000;
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
                    viewer.Add(null, null as string, ChangeType.Unchanged, source);
                    continue;
                }

                var changeType = line.Type;
                var text = line.Text;

                switch (line.Type)
                {
                    case ChangeType.Modified:
                        changeType = ChangeType.Inserted;
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

                if ((line.Type == ChangeType.Inserted || line.Type == ChangeType.Deleted) &&
                    line.SubPieces?.Count > 1 && !disableSubPieces)
                {
                    bool isOld = line.Type == ChangeType.Inserted ? true : false;
                    var parts = GetSubPiecesInfo(line, isOld, source);
                    viewer.Add(line, parts, changeType, source);
                }
                else
                {
                    viewer.Add(line, text, changeType, source);
                }
            }

            if (contextLineCount > -1) CollapseUnchangedSections(viewer, contextLineCount);
            viewer.AdjustScrollView();
        }

        internal static void InsertLines(InternalLinesViewer panel, IEnumerable<DiffPiece> lines, bool isOld, UIElement source, int contextLineCount)
        {
            if (lines == null || panel == null) return;

            InsertLinesInteral(panel, lines, isOld, source);
            if (contextLineCount > -1) CollapseUnchangedSections(panel, contextLineCount);
        }

        internal static void CollapseUnchangedSections(InternalLinesViewer panel, int contextLineCount)
        {
            var i = -1;
            var was = false;
            var last = 0;
            var max = -1;
            var removing = new List<int>();
            foreach (var ele in panel.LineDetails)
            {
                i++;
                if (ele.Piece.Type != ChangeType.Unchanged)
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
                var currentScrollPosition = panel._ValueScrollViewer.VerticalOffset;
                var point = new Point(0, currentScrollPosition);

                foreach (var item in panel.LineDetails)
                {
                    if(item.Number == lineIndex || lineIndex == 0)
                    {
                        var container = panel.ValuePanel.ItemContainerGenerator.ContainerFromItem(item) as UIElement;

                        if (container == null)
                            continue;

                        var pos = container.TransformToVisual(panel._ValueScrollViewer).Transform(point);
                        panel._ValueScrollViewer.ScrollToVerticalOffset(pos.Y);
                        return true;
                    }
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
                var lineItem = panel.LineDetails.FirstOrDefault(l => l.Piece == line);

                if (lineItem == null)
                    return false;

                var container = panel.ValuePanel.ItemContainerGenerator.ContainerFromItem(lineItem) as UIElement;

                if (container == null)
                    return false;

                var scrollView = panel._ValueScrollViewer;
                var point = new Point(0, 0);

                var pos = container.TranslatePoint(point, panel._ValueScrollViewer);
                if (pos.Y >= 0 && pos.Y <= scrollView.ActualHeight - container.DesiredSize.Height) return true;
                var currentScrollPosition = panel._ValueScrollViewer.VerticalOffset;
                point = new Point(0, currentScrollPosition);
                pos = container.TransformToVisual(panel._ValueScrollViewer).Transform(point);
                panel._ValueScrollViewer.ScrollToVerticalOffset(pos.Y);
                return true;
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
            return panel.LineDetails.FirstOrDefault(ld => ld.Number == lineIndex)?.Piece;
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
                if (!item.Item2)
                {
                    if (needBreak) yield break;
                    continue;
                }

                needBreak = true;
                yield return item.Item1;
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
                if (item.Item2) yield break;
                yield return item.Item1;
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
                if (item.Item2)
                {
                    needReturn = true;
                    continue;
                }

                if (needReturn) yield return item.Item1;
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
                while (ele != null && ele != parentElement && !(ele is Window))
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
        /// Gets all line information in viewport.
        /// </summary>
        /// <param name="panel">The content panel.</param>
        /// <param name="level">The optional visibility level.</param>
        /// <returns>All lines.</returns>
        private static IEnumerable<Tuple<DiffPiece, bool>> GetLineViewportStates(InternalLinesViewer panel, VisibilityLevels level)
        {
            var scrollView = panel._ValueScrollViewer;
            var point = new Point(0, 0);
            foreach (var item in panel.LineDetails.Where(ld => ld != null))
            {
                var container = panel.ValuePanel.ItemContainerGenerator.ContainerFromItem(item) as UIElement;

                if (container == null)
                    continue;

                var pos = container.TranslatePoint(point, scrollView);

                switch (level)
                {
                    case VisibilityLevels.All:
                        var isAllIn = pos.Y >= 0 && pos.Y <= scrollView.ActualHeight - container.DesiredSize.Height;
                        yield return new Tuple<DiffPiece, bool>(item.Piece, isAllIn);
                        break;
                    case VisibilityLevels.Half:
                        var halfHeight = container.DesiredSize.Height / 2;
                        var isHalfIn = pos.Y >= -halfHeight && pos.Y <= scrollView.ActualHeight - halfHeight;
                        yield return new Tuple<DiffPiece, bool>(item.Piece, isHalfIn);
                        break;
                    default:
                        var isAnyIn = pos.Y > -container.DesiredSize.Height && pos.Y < scrollView.ActualHeight;
                        yield return new Tuple<DiffPiece, bool>(item.Piece, isAnyIn);
                        break;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0056:", Justification = "Not supported.")]
        private static List<LineViewerSegment> GetSubPiecesInfo(DiffPiece line, bool isOld, UIElement source)
        {
            var details = new List<LineViewerSegment>();
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

                details.Add(new LineViewerSegment()
                {
                    TextChunk = ele.Text,
                    ChunkChange = subType,
                    Source = source
                });
            }

            return details;
        }

        private static void InsertLinesInteral(InternalLinesViewer panel, IEnumerable<DiffPiece> lines, bool isOld, UIElement source, bool disableSubPieces = false)
        {
            foreach (var line in lines)
            {
                if (line == null)
                {
                    panel.Add(null, null as string, ChangeType.Unchanged, source);
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
                            var details = GetSubPiecesInfo(line, isOld, source);
                            panel.Add(line, details, changeType, source);
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
                    panel.Add(line, text, changeType, source);
                }
            }

            panel.AdjustScrollView();
        }
    }
}
