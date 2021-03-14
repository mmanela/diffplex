# DiffPlex WinForms Demo

This is an example of WinForms application that targets .NET Framework 4.8 (developed by C#).
It shows how to use both `InlineDiffBuilder` and `SideBySideDiffBuilder` with a standard `System.Windows.Forms.RichTextBox`.

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
