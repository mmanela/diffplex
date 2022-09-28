using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
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
using Trivial.IO;
using Trivial.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace DiffPlex.UI;

/// <summary>
/// The window for diff text.
/// </summary>
public sealed partial class DiffTextWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the DiffTextWindow class.
    /// </summary>
    public DiffTextWindow()
    {
        InitializeComponent();
        Title = "DiffPlex";
        var osVer = Environment.OSVersion.Version;
        if (osVer.Major < 10 || (osVer.Major == 10 && osVer.Minor == 0 && osVer.Build < 22000)) return;
        MainElement.OpenFileToReadText = GetFileTextAsync;
    }

    /// <summary>
    /// Gets the root element.
    /// </summary>
    public DiffTextView RootElement => MainElement;

    /// <summary>
    /// Sets the text.
    /// </summary>
    /// <param name="left">The old text.</param>
    /// <param name="right">The new text.</param>
    public void SetText(string oldText, string newText)
        => MainElement.SetText(oldText, newText);

    /// <summary>
    /// Sets the text.
    /// </summary>
    /// <param name="left">The old text.</param>
    /// <param name="right">The new text.</param>
    public void SetText(CharsReader oldText, CharsReader newText)
        => MainElement.SetText(oldText, newText);

    /// <summary>
    /// Sets the text.
    /// </summary>
    /// <param name="left">The old text.</param>
    /// <param name="right">The new text.</param>
    public void SetText(JsonObjectNode oldText, JsonObjectNode newText)
        => MainElement.SetText(oldText, newText);

    /// <summary>
    /// Sets the text.
    /// </summary>
    /// <param name="left">The old text.</param>
    /// <param name="right">The new text.</param>
    public void SetText(FileInfo oldText, FileInfo newText)
        => MainElement.SetText(oldText, newText);

    /// <summary>
    /// Refreshes.
    /// </summary>
    public void Refresh()
        => MainElement.Refresh();

    /// <summary>
    /// Clears.
    /// </summary>
    public void Clear()
        => MainElement.Clear();

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
