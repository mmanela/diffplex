using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;
using DiffPlex.Wpf.Controls;

namespace DiffPlex.Wpf.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        LoadData();
    }

    private void LoadData()
    {
        var now = DateTime.Now;
        var isDark = now.Hour < 6 || now.Hour >= 18;
        DiffView.Foreground = new SolidColorBrush(isDark ? Color.FromRgb(240, 240, 240) : Color.FromRgb(32, 32, 32));
        DiffView.OldText = TestData.DuplicateText(TestData.OldText, 100);
        DiffView.NewText = TestData.DuplicateText(TestData.NewText, 100);
        DiffView.SetHeaderAsOldToNew();
        Background = new SolidColorBrush(isDark ? Color.FromRgb(32, 32, 32) : Color.FromRgb(251, 251, 251));
        DiffButton.Background = FutherActionsButton.Background = WindowButton.Background = new SolidColorBrush(isDark ? Color.FromRgb(80, 160, 240) : Color.FromRgb(160, 216, 240));
        IgnoreUnchangedCheckBox.Content = TestData.RemoveHotkey(DiffView.CollapseUnchangedSectionsToggleTitle);
        MarginLineCountLabel.Content = TestData.RemoveHotkey(DiffView.ContextLinesMenuItemsTitle);
    }

    private void DiffButton_Click(object sender, RoutedEventArgs e)
    {
        if (DiffView.IsInlineViewMode)
        {
            DiffView.ShowSideBySide();
            return;
        }

        DiffView.ShowInline();
    }

    private void TextWrapButton_Click(object sender, RoutedEventArgs e)
    {
        DiffView.EnableTextWrapping();
    }

    private void FutherActionsButton_Click(object sender, RoutedEventArgs e)
    {
        DiffView.OpenViewModeContextMenu();
    }

    private void WindowButton_Click(object sender, RoutedEventArgs e)
    {
        var has = false;
        foreach (var w in Application.Current.Windows)
        {
            if (w is DiffWindow dw)
            {
                dw.Activate();
                has = true;
                break;
            }
        }

        if (has) return;
        var win = new DiffWindow();
        win.OpenFileOnBoth();
        win.Show();
    }
}
