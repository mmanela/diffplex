# DiffPlex controls for WPF

This library contains WPF controls for textual diffs.

### Import

```csharp
using DiffPlex.Wpf.Controls;
```

And insert following code into the root node of your xaml file.

```
xmlns:diffplex="clr-namespace:DiffPlex.Wpf.Controls;assembly=DiffPlex.Wpf"
```

## Side-by-side diff

Control class `SideBySideDiffControl` is used to render side-by-side textual diffs.

```xaml
<diffplex:SideBySideDiffControl x:Name="DiffView" />
```

```csharp
DiffView.SetDiffModel(OldText, NewText);
```

## Inline diff

Control class `SideBySideDiffControl` is used to render inline textual diffs.

```xaml
<diffplex:InlineDiffControl x:Name="DiffView" />
```

```csharp
DiffView.SetDiffModel(OldText, NewText);
```
