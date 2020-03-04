using System;
using System.Collections.Generic;
using System.Text;

namespace DiffPlex.Wpf.Controls
{
    internal static class Helper
    {
        public static Differ Instance { get; } = new Differ();
    }
}
