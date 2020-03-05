# DiffPlex WPF demo

This is a simple demo for `DiffPlex.Wpf` library.

In `MainWindow.xaml` file you can see there are a `SideBySideDiffViewer` control and a `InlineDiffViewer` control.
A button below controls which control displays.

In `MainWindow.xaml.cs` file, we just set the `DiffModel` property of these controls above to get the textual diffs in UI.
