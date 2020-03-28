# DiffPlex controls for WPF

[![NuGet](https://img.shields.io/nuget/v/DiffPlex.Wpf.svg)](https://www.nuget.org/packages/DiffPlex.Wpf/)

This library contains WPF controls for textual diffs.

### Import

```csharp
using DiffPlex.Wpf.Controls;
```

And insert following code into the root node of your xaml file.

```
xmlns:diffplex="clr-namespace:DiffPlex.Wpf.Controls;assembly=DiffPlex.Wpf"
```

## Diff viewer

Class `DiffViewer` is used to render textual diffs by setting new text and old text.

```xaml
<diffplex:DiffViewer x:Name="DiffView" OldTextHeader="Old" NewTextHeader="New" />
```

```csharp
DiffView.OldText = OldText;
DiffView.NewText = NewText;
```

![WPF sample](../images/wpf_side_light.jpg)

And you can switch to side-by-side view mode by call `ShowSideBySide` member method or switch to inline view mode by `ShowInline` member method.

## Side-by-side diff viewer

Class `SideBySideDiffViewer` is used to render side-by-side textual diffs by setting diff model.

```xaml
<diffplex:SideBySideDiffViewer x:Name="DiffView" />
```

```csharp
DiffView.SetDiffModel(OldText, NewText);
```

## Inline diff viewer

Class `InlineDiffViewer` is used to render inline textual diffs by setting diff model.

```xaml
<diffplex:InlineDiffViewer x:Name="DiffView" />
```

```csharp
DiffView.SetDiffModel(OldText, NewText);
```
