using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace DiffPlex.UI;

internal class InternalUtilities
{
    public const string FontFamily = "Cascadia Code, Consolas, Courier New, monospace, Microsoft Yahei, Microsoft Jhenghei, Meiryo, Segoe UI, Segoe UI Emoji, Segoe UI Symbol";

    public static readonly SolidColorBrush InsertBackground = new(Color.FromArgb(64, 96, 216, 32));

    public static readonly SolidColorBrush DeleteBackground = new(Color.FromArgb(64, 216, 32, 32));

    public static readonly SolidColorBrush GrayBackground = new(Color.FromArgb(32, 128, 128, 128));

    public static List<TextHighlighter> GetTextHighlighter(List<DiffPiece> sub, ChangeType modify, Brush foreground)
    {
        if (sub == null) return null;
        var insert = new TextHighlighter
        {
            Foreground = foreground,
            Background = InsertBackground
        };
        var delete = new TextHighlighter
        {
            Foreground = foreground,
            Background = DeleteBackground
        };
        var i = 0;
        foreach (var piece in sub)
        {
            var s = piece.Text;
            if (string.IsNullOrEmpty(s)) continue;
            var pt = piece.Type;
            if (pt == ChangeType.Modified) pt = modify;
            switch (piece.Type)
            {
                case ChangeType.Inserted:
                    Add(insert, i, piece.Text.Length);
                    break;
                case ChangeType.Deleted:
                    Add(delete, i, piece.Text.Length);
                    break;
            }

            i += piece.Text.Length;
        }

        return new List<TextHighlighter>
        {
            insert,
            delete
        };
    }

    internal static void Add(TextHighlighter highlighter, int start, int length)
    {
        if (highlighter.Ranges.Count > 0)
        {
            var last = highlighter.Ranges.Last();
            var end = last.StartIndex + last.Length;
            if (start == end)
            {
                start = last.StartIndex;
                length += last.Length;
                highlighter.Ranges.Remove(last);
            }
        }

        highlighter.Ranges.Add(new TextRange(start, length));
    }

    public static async Task<FileInfo> SelectFileAsync(Window window)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
        };
        try
        {
            if (window != null) WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(window));
            picker.FileTypeFilter.Add("*");
            var file = await picker.PickSingleFileAsync();
            if (file != null) return Trivial.IO.FileSystemInfoUtility.TryGetFileInfo(file.Path);
        }
        catch (ArgumentException)
        {
        }
        catch (IOException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (SecurityException)
        {
        }
        catch (NotSupportedException)
        {
        }
        catch (ExternalException)
        {
        }

        return null;
    }

    public static async Task<string> TryGetFileTextAsync(Window window, Action<Exception> errorHandler = null)
    {
        try
        {
            var file = await SelectFileAsync(window);
            if (file == null || !file.Exists) return null;
            return await File.ReadAllTextAsync(file.FullName);
        }
        catch (ArgumentException ex)
        {
            errorHandler(ex);
        }
        catch (IOException ex)
        {
            errorHandler(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            errorHandler(ex);
        }
        catch (SecurityException ex)
        {
            errorHandler(ex);
        }
        catch (InvalidOperationException ex )
        {
            errorHandler(ex);
        }
        catch (NotSupportedException ex)
        {
            errorHandler(ex);
        }
        catch (ExternalException ex)
        {
            errorHandler(ex);
        }
        catch (OutOfMemoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            errorHandler(ex);
            throw;
        }

        return null;
    }
}
