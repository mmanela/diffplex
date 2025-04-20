using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace DiffPlex.Renderer
{
    /// <summary>
    /// Renderer for generating unified diff (unidiff) format output from inline diff results
    /// </summary>
    public class UnidiffRenderer
    {
        private readonly IInlineDiffBuilder diffBuilder;
        private readonly int contextLines;
        
        /// <summary>
        /// Gets the default singleton instance of the unidiff renderer.
        /// </summary>
        public static UnidiffRenderer Instance { get; } = new UnidiffRenderer();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UnidiffRenderer"/> class.
        /// </summary>
        /// <param name="diffBuilder">The diff builder to use. If null, uses the default InlineDiffBuilder.</param>
        /// <param name="contextLines">Number of unchanged context lines to include around changes.</param>
        public UnidiffRenderer(IInlineDiffBuilder diffBuilder = null, int contextLines = 3)
        {
            this.diffBuilder = diffBuilder ?? InlineDiffBuilder.Instance;
            this.contextLines = contextLines;
        }
        
        /// <summary>
        /// Generates a unified diff format output from two texts.
        /// </summary>
        /// <param name="oldText">The old text to diff.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="oldFileName">The old file name to show in the headers.</param>
        /// <param name="newFileName">The new file name to show in the headers.</param>
        /// <param name="ignoreWhitespace">Whether to ignore whitespace differences.</param>
        /// <param name="ignoreCase">Whether to ignore case differences.</param>
        /// <returns>A string containing the unified diff output.</returns>
        public string Generate(string oldText, string newText, string oldFileName = "a", string newFileName = "b", bool ignoreWhitespace = true, bool ignoreCase = false)
        {
            if (oldText == null) throw new ArgumentNullException(nameof(oldText));
            if (newText == null) throw new ArgumentNullException(nameof(newText));
            if (oldFileName == null) throw new ArgumentNullException(nameof(oldFileName));
            if (newFileName == null) throw new ArgumentNullException(nameof(newFileName));
            
            var model = diffBuilder.BuildDiffModel(oldText, newText, ignoreWhitespace, ignoreCase, new DiffPlex.Chunkers.LineChunker());
            return Generate(model, oldFileName, newFileName);
        }
        
        /// <summary>
        /// Generates a unified diff format output from a diff model.
        /// </summary>
        /// <param name="model">The diff model to render.</param>
        /// <param name="oldFileName">The old file name to show in the headers.</param>
        /// <param name="newFileName">The new file name to show in the headers.</param>
        /// <returns>A string containing the unified diff output.</returns>
        public string Generate(DiffPaneModel model, string oldFileName = "a", string newFileName = "b")
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (oldFileName == null) throw new ArgumentNullException(nameof(oldFileName));
            if (newFileName == null) throw new ArgumentNullException(nameof(newFileName));
            
            if (!model.HasDifferences)
            {
                return string.Empty;
            }
            
            // Normalize the model by filtering out trailing empty lines if present
            var lines = model.Lines;
            var lastChangedIndex = lines.FindLastIndex(line => line.Type != ChangeType.Unchanged);
            if (lastChangedIndex >= 0 && lastChangedIndex < lines.Count - 1)
            {
                // Check if all remaining lines are empty
                bool allEmpty = true;
                for (int i = lastChangedIndex + 1; i < lines.Count; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i].Text))
                    {
                        allEmpty = false;
                        break;
                    }
                }
                
                if (allEmpty)
                {
                    lines = lines.Take(lastChangedIndex + 1).ToList();
                }
            }
            
            var sb = new StringBuilder();
            
            // Generate the unified diff header
            sb.AppendLine($"--- {oldFileName}");
            sb.AppendLine($"+++ {newFileName}");
            
            // Group changes into hunks with context
            var hunks = CreateHunks(lines);
            
            foreach (var hunk in hunks)
            {
                // Calculate line numbers for the hunk header
                int oldStart = GetOldLineNumber(hunk, out int oldCount);
                int newStart = GetNewLineNumber(hunk, out int newCount);
                
                // Generate the hunk header
                sb.AppendLine($"@@ -{oldStart},{oldCount} +{newStart},{newCount} @@");
                
                // Generate the hunk content
                foreach (var line in hunk)
                {
                    switch (line.Type)
                    {
                        case ChangeType.Unchanged:
                            sb.AppendLine($" {line.Text}");
                            break;
                        case ChangeType.Deleted:
                            sb.AppendLine($"-{line.Text}");
                            break;
                        case ChangeType.Inserted:
                            sb.AppendLine($"+{line.Text}");
                            break;
                    }
                }
            }
            
            return sb.ToString();
        }
        
        private IList<List<DiffPiece>> CreateHunks(List<DiffPiece> lines)
        {
            var hunks = new List<List<DiffPiece>>();
            if (lines.Count == 0) return hunks;
            
            var currentHunk = new List<DiffPiece>();
            int lastChanged = -1;
            
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                bool isChanged = line.Type != ChangeType.Unchanged;
                
                if (isChanged)
                {
                    // If we're starting a new hunk after only unchanged lines
                    if (currentHunk.Count == 0 || (lastChanged == -1 && i > 0))
                    {
                        // Add context lines before the change
                        int contextStart = Math.Max(0, i - contextLines);
                        for (int j = contextStart; j < i; j++)
                        {
                            currentHunk.Add(lines[j]);
                        }
                    }
                    
                    currentHunk.Add(line);
                    lastChanged = i;
                }
                else if (lastChanged != -1) // It's an unchanged line after a change
                {
                    currentHunk.Add(line);
                    
                    // If we've collected enough context lines after a change
                    if (i - lastChanged >= contextLines)
                    {
                        hunks.Add(currentHunk);
                        currentHunk = new List<DiffPiece>();
                        lastChanged = -1;
                    }
                }
            }
            
            // Add the last hunk if it's not empty
            if (currentHunk.Count > 0)
            {
                hunks.Add(currentHunk);
            }
            
            return hunks;
        }
        
        private int GetOldLineNumber(List<DiffPiece> hunk, out int count)
        {
            var oldLines = hunk.Where(l => l.Type == ChangeType.Unchanged || l.Type == ChangeType.Deleted).ToList();
            count = oldLines.Count;
            
            if (oldLines.Count == 0)
            {
                // Special case: if there are no old lines, we'll use a zero-based offset
                return 0;
            }
            
            // Find the first line with a position
            var firstLineWithPosition = hunk.FirstOrDefault(l => l.Position.HasValue);
            
            if (firstLineWithPosition != null)
            {
                // For the old file, we need to calculate based on position of unchanged lines
                // or estimate based on the position of inserted lines
                
                // Find the first unchanged line in the hunk with a position
                var firstUnchanged = hunk.FirstOrDefault(l => l.Type == ChangeType.Unchanged && l.Position.HasValue);
                
                if (firstUnchanged != null)
                {
                    // Count deleted lines before this unchanged line
                    int deletedBefore = 0;
                    int hunkIndex = hunk.IndexOf(firstUnchanged);
                    
                    for (int i = 0; i < hunkIndex; i++)
                    {
                        if (hunk[i].Type == ChangeType.Deleted)
                            deletedBefore++;
                    }
                    
                    return Math.Max(1, firstUnchanged.Position.Value - (hunkIndex - deletedBefore));
                }
                else
                {
                    // If there are no unchanged lines but only inserted lines with positions,
                    // we need to estimate the old line number based on the inserted position
                    return Math.Max(1, firstLineWithPosition.Position.Value);
                }
            }
            
            return 1; // Default to 1 if we can't determine the starting line
        }
        
        private int GetNewLineNumber(List<DiffPiece> hunk, out int count)
        {
            var newLines = hunk.Where(l => l.Type == ChangeType.Unchanged || l.Type == ChangeType.Inserted).ToList();
            count = newLines.Count;
            
            if (newLines.Count == 0)
            {
                // Special case: if there are no new lines, we'll use a zero-based offset
                return 0;
            }
            
            // Get the first line with a position (unchanged or inserted line)
            var firstLine = hunk.FirstOrDefault(l => l.Position.HasValue);
            if (firstLine != null)
            {
                // For inserted/unchanged lines, the position directly represents the new file's line number
                // We need to adjust based on how many lines with positions come before this in the hunk
                int positionedLinesBefore = 0;
                int hunkIndex = hunk.IndexOf(firstLine);
                
                for (int i = 0; i < hunkIndex; i++)
                {
                    if (hunk[i].Type == ChangeType.Inserted || hunk[i].Type == ChangeType.Unchanged)
                        positionedLinesBefore++;
                }
                
                return Math.Max(1, firstLine.Position.Value - positionedLinesBefore);
            }
            
            return 1; // Default to 1 if we can't determine the starting line
        }
        
        /// <summary>
        /// Generate a unified diff format output directly from two texts.
        /// </summary>
        /// <param name="oldText">The old text to diff.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="oldFileName">The old file name to show in the headers.</param>
        /// <param name="newFileName">The new file name to show in the headers.</param>
        /// <param name="ignoreWhitespace">Whether to ignore whitespace differences.</param>
        /// <param name="ignoreCase">Whether to ignore case differences.</param>
        /// <param name="contextLines">Number of unchanged context lines to include around changes.</param>
        /// <returns>A string containing the unified diff output.</returns>
        public static string GenerateUnidiff(
            string oldText, 
            string newText, 
            string oldFileName = "a", 
            string newFileName = "b", 
            bool ignoreWhitespace = true, 
            bool ignoreCase = false, 
            int contextLines = 3)
        {
            var renderer = new UnidiffRenderer(contextLines: contextLines);
            return renderer.Generate(oldText, newText, oldFileName, newFileName, ignoreWhitespace, ignoreCase);
        }
    }
}