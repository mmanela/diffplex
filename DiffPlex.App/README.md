# DiffPlex App

This is a simple demo about how to add the diff element into the app on Windows App SDK framework.

In `MainWindow.xaml` and its `MainWindow.xaml.cs` file, we initialize an instance of the `DiffTextView` class to render text diffs.

## Build

Please ensure it runs on Package mode since the project configuration file is applied for MSIX package.

The CPU arch platform should be one of `x64`, `x86` and `arm64`. It will be fail to run on `Any CPU` because of Windows App SDK.

It will show a demo diff text on Debug mode, so please make sure to select Debug;
and a file selector on Release mode because it is designed to be available to publish to Microsoft Store.
