using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.Wpf.Controls
{
    internal static class Helper
    {
        public const string FontFamily = "Cascadia Code, Consolas, Courier New, monospace, Microsoft Yahei, Segoe UI Emoji, Segoe UI Symbol";

        /// <summary>
        /// Updates the inline diffs view.
        /// </summary>
        internal static void RenderInlineDiffs(InternalLinesViewer viewer, DiffPaneModel m, UIElement source)
        {
            viewer.Clear();
            if (m?.Lines == null) return;
            foreach (var line in m.Lines)
            {
                if (line == null)
                {
                    viewer.Add(null, null, null, ChangeType.Unchanged.ToString(), source);
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

                viewer.Add(line.Position, changeType switch
                {
                    ChangeType.Inserted => "+",
                    ChangeType.Deleted => "-",
                    _ => " "
                }, text, changeType.ToString(), source);
            }

            viewer.AdjustScrollView();
        }

        internal static void InsertLines(InternalLinesViewer panel, List<DiffPiece> lines, bool isOld, UIElement source)
        {
            if (lines == null || panel == null) return;
            foreach (var line in lines)
            {
                if (line == null)
                {
                    panel.Add(null, null, null, ChangeType.Unchanged.ToString(), source);
                    continue;
                }

                var changeType = line.Type;
                var text = line.Text;
                switch (line.Type)
                {
                    case ChangeType.Modified:
                        changeType = isOld ? ChangeType.Deleted : ChangeType.Inserted;
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

                panel.Add(line.Position, changeType switch
                {
                    ChangeType.Inserted => "+",
                    ChangeType.Deleted => "-",
                    _ => " "
                }, text, changeType.ToString(), source);
            }

            panel.AdjustScrollView();
        }

    }
}
