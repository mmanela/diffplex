using DiffPlex.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Threading.Tasks;
using Trivial.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace DiffPlex.UI;

/// <summary>
/// The main window.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "DiffPlex";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(HeaderBar);
        MainElement.OpenFileToReadText = GetFileTextAsync;
#if DEBUG
        LoadData();
#else
        MainElement.ShowNewFileSelectDialog();
#endif
        var appWindow = VisualUtility.TryGetAppWindow(this);
        try
        {
            appWindow?.SetIcon("DiffPlex.ico");
        }
        catch (ArgumentException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (NullReferenceException)
        {
        }
        catch (ApplicationException)
        {
        }
        catch (ExternalException)
        {
        }

        try
        {
            var v1 = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "Demo";
            var v2 = typeof(DiffTextView).Assembly?.GetName()?.Version?.ToString() ?? "Demo";
            var v3 = typeof(Differ).Assembly?.GetName()?.Version?.ToString() ?? "Demo";
            VersionText.Text = $"App \t{v1}{Environment.NewLine}UI \t{v2}{Environment.NewLine}Core \t{v3}";
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }
    }

    private void LoadData()
    {
        MainElement.OldText = TestData.DuplicateText(TestData.OldText, 200);
        MainElement.NewText = TestData.DuplicateText(TestData.NewText, 200);
    }

    private Task<string> GetFileTextAsync()
        => InternalUtilities.TryGetFileTextAsync(this);

    private void OnAboutClick(object sender, RoutedEventArgs e)
    {
        AboutPanel.Visibility = Visibility.Visible;
        _ = FocusOkButtonAsync();
    }

    private async Task FocusOkButtonAsync()
    {
        await Task.Delay(200);
        if (AboutPanel.Visibility == Visibility.Visible) OkButton.Focus(FocusState.Programmatic);
    }

    private void OnExitAboutClick(object sender, RoutedEventArgs e)
        => AboutPanel.Visibility = Visibility.Collapsed;
}
