using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private const int MaxCount = 3000;
        public const string FontFamily = "Cascadia Code, Consolas, Courier New, monospace, Microsoft Yahei, Segoe UI Emoji, Segoe UI Symbol";

        /// <summary>
        /// Updates the inline diffs view.
        /// </summary>
        internal static void RenderInlineDiffs(InternalLinesViewer viewer, ICollection<DiffPiece> lines, UIElement source)
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

            viewer.AdjustScrollView();
        }

        internal static void InsertLines(InternalLinesViewer panel, List<DiffPiece> lines, bool isOld, UIElement source)
        {
            if (lines == null || panel == null) return;
            var guid = panel.TrackingId = Guid.NewGuid();
            if (lines.Count < 500)
            {
                InsertLinesInteral(panel, lines, isOld, source);
                return;
            }

            var disablePieces = lines.Count > MaxCount; // For performance.
            InsertLinesInteral(panel, lines.Take(300).ToList(), isOld, source, disablePieces);
            Task.Delay(800).ContinueWith(t =>   // For performance.
            {
                panel.Dispatcher.Invoke(() =>
                {
                    if (panel.TrackingId != guid) return;
                    InsertLinesInteral(panel, lines.Skip(300).ToList(), isOld, source, disablePieces);
                });
            });
        }

        /// <summary>
        /// Goes to the specific line.
        /// </summary>
        /// <param name="panel">The content panel.</param>
        /// <param name="lineIndex">The zero-based index of line to go to.</param>
        /// <returns>true if it has turned to the specific line; otherwise, false.</returns>
        internal static bool GoTo(InternalLinesViewer panel, int lineIndex)
        {
            var currentScrollPosition = panel.ValueScrollViewer.VerticalOffset;
            var point = new Point(0, currentScrollPosition);
            foreach (var item in panel.ValuePanel.Children)
            {
                if (!(item is FrameworkElement ele) || !(ele.Tag is DiffPiece line) || line?.Position != lineIndex) continue;
                var pos = ele.TransformToVisual(panel.ValueScrollViewer).Transform(point);
                panel.ValueScrollViewer.ScrollToVerticalOffset(pos.Y);
                return true;
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
            var scrollView = panel.ValueScrollViewer;
            var point = new Point(0, 0);
            foreach (var item in panel.ValuePanel.Children)
            {
                if (!(item is FrameworkElement ele) || ele.Tag != line) continue;
                var pos = ele.TranslatePoint(point, panel.ValueScrollViewer);
                if (pos.Y >= 0 && pos.Y <= scrollView.ActualHeight - ele.ActualHeight) return true;
                var currentScrollPosition = panel.ValueScrollViewer.VerticalOffset;
                point = new Point(0, currentScrollPosition);
                pos = ele.TransformToVisual(panel.ValueScrollViewer).Transform(point);
                panel.ValueScrollViewer.ScrollToVerticalOffset(pos.Y);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the line diff information.
        /// </summary>
        /// <param name="panel">The content panel.</param>
        /// <param name="lineIndex">The zero-based index of line to go to.</param>
        /// <returns>The line diff information instance; or null, if non-exists.</returns>
        internal static DiffPiece GetLine(InternalLinesViewer panel, int lineIndex)
        {
            foreach (var item in panel.ValuePanel.Children)
            {
                if (!(item is FrameworkElement ele) || !(ele.Tag is DiffPiece line) || line?.Position != lineIndex) continue;
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
            var scrollView = panel.ValueScrollViewer;
            var currentScrollPosition = scrollView.VerticalOffset;
            var point = new Point(0, currentScrollPosition);
            foreach (var item in panel.ValuePanel.Children)
            {
                if (!(item is FrameworkElement ele) || !(ele.Tag is DiffPiece line)) continue;
                var pos = ele.TranslatePoint(point, panel.ValueScrollViewer);
                switch (level)
                {
                    case VisibilityLevels.All:
                        if (pos.Y >= 0 && pos.Y <= scrollView.ActualHeight - ele.ActualHeight)
                            yield return line;
                        break;
                    case VisibilityLevels.Half:
                        var halfHeight = ele.ActualHeight / 2;
                        if (pos.Y >= -halfHeight && pos.Y <= scrollView.ActualHeight - halfHeight)
                            yield return line;
                        break;
                    default:
                        if (pos.Y > -ele.ActualHeight && pos.Y < scrollView.ActualHeight)
                            yield return line;
                        break;
                }
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

                if (string.IsNullOrWhiteSpace(str)) return;
                copyMenuItem.IsEnabled = true;
                Clipboard.SetText(str);
            };
            copyMenuItem.Click += (sender, ev) =>
            {
                Clipboard.SetText(str);
            };
            return menu;
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
}
