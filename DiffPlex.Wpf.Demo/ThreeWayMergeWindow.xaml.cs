using System.Windows;
using System.Windows.Media;

namespace DiffPlex.Wpf.Demo;

/// <summary>
/// Interaction logic for ThreeWayMergeWindow.xaml
/// </summary>
public partial class ThreeWayMergeWindow : Window
{
    public ThreeWayMergeWindow()
    {
        InitializeComponent();
        LoadExample1();
        UpdateTheme();
        UpdateConflictStatus();
        
        // Update conflict status when merge changes
        MergeViewer.SizeChanged += (s, e) => UpdateConflictStatus();
    }

    private void UpdateTheme()
    {
        var isDark = System.DateTime.Now.Hour < 6 || System.DateTime.Now.Hour >= 18;
        MergeViewer.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(240, 240, 240) : Color.FromRgb(32, 32, 32));
        Background = new SolidColorBrush(isDark ? Color.FromRgb(32, 32, 32) : Color.FromRgb(251, 251, 251));
    }

    private void UpdateConflictStatus()
    {
        if (MergeViewer.HasConflicts)
        {
            ConflictStatusText.Text = "Yes";
            ConflictStatusText.Foreground = Brushes.Red;
        }
        else
        {
            ConflictStatusText.Text = "None";
            ConflictStatusText.Foreground = Brushes.Green;
        }
    }

    private void LoadExample1Button_Click(object sender, RoutedEventArgs e)
    {
        LoadExample1();
    }

    private void LoadExample2Button_Click(object sender, RoutedEventArgs e)
    {
        LoadExample2();
    }

    private void LoadExample3Button_Click(object sender, RoutedEventArgs e)
    {
        LoadExample3();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        LoadExample1();
    }

    private void LoadExample1()
    {
        // Simple conflict example
        var baseText = @"Line 1
Line 2
Line 3
Line 4
Line 5";

        var yoursText = @"Line 1
Line 2 - Your Change
Line 3
Line 4
Line 5";

        var theirsText = @"Line 1
Line 2 - Their Change
Line 3
Line 4
Line 5";

        MergeViewer.SetTexts(baseText, yoursText, theirsText);
        MergeViewer.Refresh();
        UpdateConflictStatus();
    }

    private void LoadExample2()
    {
        // Multiple conflicts example
        var baseText = @"Header
Section 1
Content A
Content B
Section 2
Footer";

        var yoursText = @"Header - Updated
Section 1
Content A - Your Version
Content B
Section 2 - Your Section
Footer";

        var theirsText = @"Header - Different Update
Section 1
Content A - Their Version
Content B
Section 2 - Their Section
Footer";

        MergeViewer.SetTexts(baseText, yoursText, theirsText);
        MergeViewer.Refresh();
        UpdateConflictStatus();
    }

    private void LoadExample3()
    {
        // Non-conflicting changes example
        var baseText = @"Line 1
Line 2
Line 3
Line 4
Line 5
Line 6";

        var yoursText = @"Line 1 - Your Change
Line 2
Line 3
Line 4
Line 5
Line 6";

        var theirsText = @"Line 1
Line 2
Line 3
Line 4
Line 5
Line 6 - Their Change";

        MergeViewer.SetTexts(baseText, yoursText, theirsText);
        MergeViewer.Refresh();
        UpdateConflictStatus();
    }
}
