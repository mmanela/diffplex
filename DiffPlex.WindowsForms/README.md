# DiffPlex WinForms Demo

This is an example of WinForms application that targets .NET Framework 4.8 (developed by C#).

## WPF element in WinForms

By using `ElementHost` [control](https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.integration.elementhost)
(in `System.Windows.Forms.Integration` namespace and can be found in Toolbox of Visual Studio)
and setting its `Child` property as one of `DiffViewer`, `SideBySideDiffViewer` and `InlineDiffViewer`
(in `DiffPlex.Wpf.Controls` namespace of `DiffPlex.Wpf.dll` assembly) instance,
you can add the visual element into the control or window of your WinForms project.

See `Form2.cs` to get details.

## Classic

This shows how to use both `InlineDiffBuilder` and `SideBySideDiffBuilder` with a standard `System.Windows.Forms.RichTextBox`.

![Screenshot of the application showing an inline and side-by-side example.](screenshot.png)

See `Form1.cs` to get details.

To run this sample, please update file `Program.cs` as following.

```csharp
[STAThread]
static void Main()
{
	Application.EnableVisualStyles();
	Application.SetCompatibleTextRenderingDefault(false);
+	Application.Run(new Form1());
-	Application.Run(new Form2());
}
```
