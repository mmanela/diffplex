using System;
using System.Collections.Generic;
using System.Text;

namespace DiffPlex.Wpf.Controls
{
    internal static class Helper
    {
        public static Differ Instance { get; } = new Differ();

        public const string FontFamily = "Cascadia Code, Consolas, Courier New, monospace, Microsoft Yahei, Segoe UI Emoji, Segoe UI Symbol";
    }
}
