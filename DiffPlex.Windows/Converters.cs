using DiffPlex.DiffBuilder.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace DiffPlex.UI;

/// <summary>
/// The base converter for change type.
/// </summary>
/// <param name="defaultChangeType">The default chagne type.</param>
public class DiffChangeTypeConverter(ChangeType defaultChangeType) : IValueConverter
{
    /// <summary>
    /// Gets or sets the default change type for modify.
    /// </summary>
    public ChangeType ModifyChangeType { get; set; } = defaultChangeType;

    /// <summary>
    /// Gets or sets the default change type for modify.
    /// </summary>
    public Brush Foreground { get; set; }

    /// <summary>
    /// Converts a source to target.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not ChangeType t)
        {
            if (targetType == typeof(IEnumerable<TextHighlighter>))
            {
                if (value is not List<DiffPiece> sub)
                {
                    if (value is not DiffPiece diffPiece) return null;
                    sub = diffPiece.SubPieces;
                }

                return sub == null ? null : InternalUtilities.GetTextHighlighter(sub, ModifyChangeType, Foreground);
            }

            if (value is not DiffPiece p) return null;
            t = p.Type;
        }

        if (t == ChangeType.Modified) t = ModifyChangeType;
        if (targetType == typeof(string))
            return t switch
            {
                ChangeType.Inserted => "+",
                ChangeType.Deleted => "-",
                _ => null
            };
        return t switch
        {
            ChangeType.Inserted => InternalUtilities.InsertBackground,
            ChangeType.Deleted => InternalUtilities.DeleteBackground,
            ChangeType.Imaginary => InternalUtilities.GrayBackground,
            _ => null
        };
    }

    /// <summary>
    /// Converts the source back.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is not Color c)
        {
            if (value is not SolidColorBrush b) return ChangeType.Modified;
            c = b.Color;
        }

        if (SameColor(c, InternalUtilities.InsertBackground.Color)) return ChangeType.Inserted;
        if (SameColor(c, InternalUtilities.DeleteBackground.Color)) return ChangeType.Deleted;
        if (SameColor(c, InternalUtilities.GrayBackground.Color)) return ChangeType.Imaginary;
        return ChangeType.Unchanged;
    }

    private static bool SameColor(Color left, Color right)
    {
        var r = Math.Abs(left.R - right.R) <= 4;
        var g = Math.Abs(left.G - right.G) <= 4;
        var b = Math.Abs(left.B - right.B) <= 4;
        return r && g && b;
    }
}

/// <summary>
/// The diff change type converter for old text.
/// </summary>
public class OldDiffChangeTypeConverter : DiffChangeTypeConverter
{
    /// <summary>
    /// Initializes a new instance of the OldDiffChangeTypeConverter class.
    /// </summary>
    public OldDiffChangeTypeConverter()
        : base(ChangeType.Deleted)
    {
    }
}

/// <summary>
/// The diff change type converter for new text.
/// </summary>
public class NewDiffChangeTypeConverter : DiffChangeTypeConverter
{
    /// <summary>
    /// Initializes a new instance of the NewDiffChangeTypeConverter class.
    /// </summary>
    public NewDiffChangeTypeConverter()
        : base(ChangeType.Inserted)
    {
    }
}

/// <summary>
/// The base converter for change type.
/// </summary>
/// <param name="defaultChangeType">The default chagne type.</param>
public class DiffTextHighlighterConverter(ChangeType defaultChangeType) : IValueConverter
{
    /// <summary>
    /// Gets or sets the default change type for modify.
    /// </summary>
    public ChangeType ModifyChangeType { get; set; } = defaultChangeType;

    /// <summary>
    /// Gets or sets the foreground.
    /// </summary>
    public Brush Foreground { get; set; }

    /// <summary>
    /// Converts a source to target.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is FrameworkElement element) value = element.DataContext;
        if (value is not List<DiffPiece> sub)
        {
            if (value is DiffPiece p) sub = p.SubPieces;
            else return null;
        }

        return InternalUtilities.GetTextHighlighter(sub, ModifyChangeType, Foreground ?? (parameter as Brush));
    }

    /// <summary>
    /// Converts the source back.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return null;
    }
}

/// <summary>
/// The diff change type converter for old text.
/// </summary>
public class DeletedDiffTextHighlighterConverter : DiffTextHighlighterConverter
{
    /// <summary>
    /// Initializes a new instance of the DeletedDiffTextHighlighterConverter class.
    /// </summary>
    public DeletedDiffTextHighlighterConverter()
        : base(ChangeType.Deleted)
    {
    }
}

/// <summary>
/// The diff change type converter for new text.
/// </summary>
public class InsertedDiffTextHighlighterConverter : DiffTextHighlighterConverter
{
    /// <summary>
    /// Initializes a new instance of the InsertedDiffTextHighlighterConverter class.
    /// </summary>
    public InsertedDiffTextHighlighterConverter()
        : base(ChangeType.Inserted)
    {
    }
}
