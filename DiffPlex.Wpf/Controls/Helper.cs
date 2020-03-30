using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.Wpf.Controls
{
    internal static class Helper
    {
        private const int MaxCount = 3000;
        public const string FontFamily = "Cascadia Code, Consolas, Courier New, monospace, Microsoft Yahei, Segoe UI Emoji, Segoe UI Symbol";

        /// <summary>
        /// Updates the inline diffs view.
        /// </summary>
        internal static void RenderInlineDiffs(InternalLinesViewer viewer, DiffPaneModel m, UIElement source)
        {
            viewer.Clear();
            if (m?.Lines == null) return;
            var disableSubPieces = m.Lines.Count > MaxCount;    // For performance.
            foreach (var line in m.Lines)
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
