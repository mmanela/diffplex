# DiffPlex.Blazor

A Blazor web application demonstrating how to use the DiffPlex library to render text diffs in a web interface.

## Features

- **Side-by-Side Diff Viewer**: Display differences between two texts in a split-pane view
- **Inline Diff Viewer**: Show differences in a unified view with change indicators
- **Unified Diff Format**: Generate Git-style unified diff output
- **Interactive Examples**: Pre-built examples showcasing different diff scenarios
- **Real-time Updates**: Live diff rendering as you type
- **Customizable Options**: Control whitespace handling and other diff parameters

## Components

### DiffViewer
A side-by-side diff component that shows old and new text in separate panels with highlighted changes.

```razor
<DiffViewer OldText="@oldText" 
           NewText="@newText" 
           OldTextHeader="Original" 
           NewTextHeader="Modified"
           IgnoreWhiteSpace="true" />
```

### InlineDiffViewer
An inline diff component that shows changes in a unified view with change type indicators.

```razor
<InlineDiffViewer OldText="@oldText" 
                 NewText="@newText" 
                 Header="Diff View"
                 IgnoreWhiteSpace="true" />
```

## Pages

1. **Home** (`/`) - Interactive diff viewer with editable text areas
2. **Examples** (`/examples`) - Pre-built examples showing different diff scenarios
3. **Unified Diff** (`/unified`) - Generate and display unified diff format output

## Running the Application

1. Ensure you have .NET 9.0 or later installed
2. Navigate to the `DiffPlex.Blazor` directory
3. Run the application:
   ```
   dotnet run
   ```
4. Open your browser and navigate to the displayed URL (typically `https://localhost:5001`)

## Example Usage

The application includes several built-in examples:

- **Code Refactor**: Shows typical code refactoring with method extraction and variable renaming
- **JSON Config**: Configuration file changes with property additions and modifications
- **Documentation**: Markdown documentation updates showing content restructuring

## Styling

The components use CSS styling to provide GitHub-like diff visualization:
- Green highlighting for added lines
- Red highlighting for deleted lines
- Yellow highlighting for modified lines
- Line numbers and change type indicators
- Monospace font for proper code alignment

## Dependencies

- DiffPlex (core library)
- Bootstrap (for styling)
- Blazor Server/.NET 9.0
