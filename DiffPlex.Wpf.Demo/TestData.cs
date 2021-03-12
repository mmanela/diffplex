using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace DiffPlex.Wpf.Demo
{
    static class TestData
    {
        internal static bool FillDiffViewer(Controls.DiffViewer viewer, int duplicateTimes = 50)
        {
            var now = DateTime.Now;
            var isDark = now.Hour < 6 || now.Hour >= 18;
            viewer.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(240, 240, 240) : Color.FromRgb(32, 32, 32));
            viewer.OldText = DuplicateText(OldText, duplicateTimes);
            viewer.NewText = DuplicateText(NewText, duplicateTimes);
            viewer.SetHeaderAsOldToNew();
            return isDark;
        }

        internal static object RemoveHotkey(object content)
        {
            if (!(content is string s)) return content;
            s = s.Trim();
            if (s.StartsWith("_")) return s.Substring(1);
            var i = s.Length - 5;
            return s.IndexOf(" (_") == i ? s.Substring(0, i) : s;

        }

        internal static string DuplicateText(string text, int repeat = 2)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < repeat; i++)
            {
                sb.Append(text);
            }

            return sb.ToString();
        }

        internal const string OldText =
            @"ABCDEFG hijklmn 01234567 _!# 98?

We the people
of the united states of america
establish justice
ensure domestic tranquility
provide for the common defence
secure the blessing of liberty
to ourselves and our posterity

Lorem ipsum dolor sit amet, consectetur adipiscing elit, 
sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris 
nisi ut aliquip ex ea commodo consequat. 
Duis aute irure dolor in reprehenderit in 
voluptate velit esse cillum dolore eu fugiat nulla pariatur. 
Excepteur sint occaecat cupidatat non proident, 
sunt in culpa qui officia deserunt mollit 
anim id est laborum

=======
";

        internal const string NewText =
            @"ABCDEFG opq rst uvw xyz 01234567 _&^ 98.

We the people
in order to form a more perfect union
establish justice
ensure domestic tranquility
promote the general welfare and
secure the blessing of liberty
to ourselves and our posterity
do ordain and establish this constitution
for the United States of America


Lorem ipsum dolor sit amet, consectetur adipiscing elit, 
sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. 
Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris 
nisi ut aliquip ex ea commodo consequat. 
Duis aute irure dolor in reprehenderit in 
gdhdgfg velit esse cillum dolore eu fugiat nulla pariatur. 
Excepteur sint occaecat cupidatat non proident, 
sunt in culpa qui officia deserunt mollit 
anim id est laborum

中文测试
=======
";
    }
}
