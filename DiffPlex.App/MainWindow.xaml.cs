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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security;
using System.Threading.Tasks;
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
        var osVer = Environment.OSVersion.Version;
        if (osVer.Major < 10 || (osVer.Major == 10 && osVer.Minor == 0 && osVer.Build < 22000)) return;
        MainElement.OpenFileToReadText = GetFileTextAsync;
#if DEBUG
        LoadData();
#else
        MainElement.ShowNewFileSelectDialog();
#endif
    }

    private void LoadData()
    {
        MainElement.OldText = TestData.DuplicateText(TestData.OldText, 200);
        MainElement.NewText = TestData.DuplicateText(TestData.NewText, 200);
    }

    private async Task<string> GetFileTextAsync()
    {
        try
        {
            var file = await InternalUtilities.SelectFileAsync(this);
            if (file == null || !file.Exists) return null;
            return await File.ReadAllTextAsync(file.FullName);
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (NotSupportedException)
        {
        }

        return null;
    }
}
