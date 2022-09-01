# DiffPlex App

This is a simple demo about how to add the diff element into the app on Windows App SDK framework.

In `MainWindow.xaml` and its `MainWindow.xaml.cs` file, we initialize an instance of the `DiffTextView` class to render text diffs.

## Build

Please ensure it runs on Package mode since the project configuration file is applied for MSIX package.

The CPU architecture platform should be one of `x64`, `x86` and `arm64`.

It will output test data of texture diffs on `Debug` mode and empty on Release mode.
