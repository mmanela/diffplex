# DiffPlex WPF demo

This is a simple demo for `DiffPlex.Wpf` [library](../DiffPlex.Wpf).

In `MainWindow.xaml` file you can see there are a `SideBySideDiffControl` control and a `InlineDiffControl` control.
A button below controls which control displays.

In `MainWindow.xaml.cs` file, we just set the `DiffModel` property of these controls above to get the textual diffs in UI.

You can also customize the style of these controls by setting properties such as following.

```csharp
// The font size.
public double FontSize { get; set; }

// The preferred font family.
public FontFamily FontFamily { get; set; }

// The font weight.
public FontWeight FontWeight { get; set; }

// The font style.
public FontStyle FontStyle { get; set; }

// The font-stretching characteristics.
public FontStretch FontStretch { get; set; }

// The default text color (foreground brush).
public Brush Foreground { get; set; }

// The background brush of the line inserted.
public Brush InsertedBackgroundProperty { get; set; }

// The background brush of the line deleted.
public Brush DeletedBackgroundProperty { get; set; }

// The background brush of the line imaginary, `SideBySideDiffControl` only.
public Brush ImaginaryBackgroundProperty { get; set; }

// The background brush of the grid splitter, `SideBySideDiffControl` only.
public Brush SplitterBackgroundProperty { get; set; }

// The width of the grid splitter, `SideBySideDiffControl` only.
public Brush SplitterWidthProperty { get; set; }

// The text color (foreground brush) of the line number.
public Brush LineNumberForegroundProperty { get; set; }

// The width of the line number and change type symbol.
public int LineNumberWidthProperty { get; set; }

// The text color (foreground brush) of the change type symbol.
public Brush ChangeTypeForeground { get; set; }
```
