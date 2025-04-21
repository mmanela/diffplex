using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.Model;

namespace DiffPlex.Renderer
{
    /// <summary>
    /// Renderer for generating unified diff (unidiff) format output from diff results
    /// </summary>
    public class UnidiffRenderer
    {
        private readonly IDiffer differ;
        private readonly int contextLines;
        
        /// <summary>
        /// Gets the default singleton instance of the unidiff renderer.
        /// </summary>
        public static UnidiffRenderer Instance { get; } = new UnidiffRenderer();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="UnidiffRenderer"/> class.
        /// </summary>
        /// <param name="differ">The differ to use. If null, uses the default Differ.</param>
        /// <param name="contextLines">Number of unchanged context lines to include around changes.</param>
        public UnidiffRenderer(IDiffer differ = null, int contextLines = 3)
        {
            this.differ = differ ?? Differ.Instance;
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
            
            var diffResult = differ.CreateDiffs(oldText, newText, ignoreWhitespace, ignoreCase, new LineChunker());
            return Generate(diffResult, oldFileName, newFileName);
        }
        
        /// <summary>
        /// Generates a unified diff format output from a diff result.
        /// </summary>
        /// <param name="diffResult">The diff result to render.</param>
        /// <param name="oldFileName">The old file name to show in the headers.</param>
        /// <param name="newFileName">The new file name to show in the headers.</param>
        /// <returns>A string containing the unified diff output.</returns>
        public string Generate(DiffResult diffResult, string oldFileName = "a", string newFileName = "b")
        {
            if (diffResult == null) throw new ArgumentNullException(nameof(diffResult));
            if (oldFileName == null) throw new ArgumentNullException(nameof(oldFileName));
            if (newFileName == null) throw new ArgumentNullException(nameof(newFileName));
            
            if (diffResult.DiffBlocks.Count == 0)
            {
                return string.Empty;
            }
            
            var sb = new StringBuilder();
            
            // Generate the unified diff header
            sb.AppendLine($"--- {oldFileName}");
            sb.AppendLine($"+++ {newFileName}");
            
            // Group changes into hunks with context
            var hunks = CreateHunks(diffResult);
            
            foreach (var hunk in hunks)
            {
                // Calculate line numbers for the hunk header
                int oldStart = hunk.OldStartLine;
                int oldCount = hunk.OldLength;
                int newStart = hunk.NewStartLine;
                int newCount = hunk.NewLength;
                
                // Generate the hunk header
                sb.AppendLine($"@@ -{oldStart},{oldCount} +{newStart},{newCount} @@");
                
                // Generate the hunk content
                foreach (var line in hunk.Lines)
                {
                    switch (line.Type)
                    {
                        case LineType.Unchanged:
                            sb.AppendLine($" {line.Text}");
                            break;
                        case LineType.Deleted:
                            sb.AppendLine($"-{line.Text}");
                            break;
                        case LineType.Inserted:
                            sb.AppendLine($"+{line.Text}");
                            break;
                    }
                }
            }
            
            return sb.ToString();
        }
        
        private List<DiffHunk> CreateHunks(DiffResult diffResult)
        {
            var hunks = new List<DiffHunk>();
            
            if (diffResult.DiffBlocks.Count == 0) return hunks;
            
            var oldPieces = diffResult.PiecesOld;
            var newPieces = diffResult.PiecesNew;
            
            // First, organize the diff blocks into potential hunks separated by contextLines boundary
            List<List<DiffBlock>> hunkGroups = new List<List<DiffBlock>>();
            List<DiffBlock> currentGroup = new List<DiffBlock>();
            hunkGroups.Add(currentGroup);
            
            DiffBlock previousBlock = null;
            foreach (var block in diffResult.DiffBlocks)
            {
                if (previousBlock != null)
                {
                    // If the blocks are too far apart, start a new group
                    // We want to create a new hunk if the distance between blocks is more than 2*contextLines
                    if (block.DeleteStartA > (previousBlock.DeleteStartA + previousBlock.DeleteCountA + 2 * contextLines))
                    {
                        currentGroup = new List<DiffBlock>();
                        hunkGroups.Add(currentGroup);
                    }
                }
                
                currentGroup.Add(block);
                previousBlock = block;
            }
            
            // Now convert each group to a hunk
            foreach (var group in hunkGroups)
            {
                if (group.Count == 0) continue;
                
                // Find the range of the entire group with context
                int firstBlockStartA = group[0].DeleteStartA;
                int lastBlockEndA = group[group.Count - 1].DeleteStartA + group[group.Count - 1].DeleteCountA;
                
                int contextStartA = Math.Max(0, firstBlockStartA - contextLines);
                int contextEndA = Math.Min(oldPieces.Count, lastBlockEndA + contextLines);
                
                // Calculate B file positions
                int firstBlockStartB = group[0].InsertStartB;
                int contextStartB = Math.Max(0, firstBlockStartB - contextLines);
                
                // Create a new hunk
                DiffHunk hunk = new DiffHunk
                {
                    OldStartLine = contextStartA + 1, // 1-based indexing
                    NewStartLine = contextStartB + 1  // 1-based indexing
                };
                
                // Add context lines before first change
                for (int i = contextStartA; i < firstBlockStartA; i++)
                {
                    hunk.Lines.Add(new DiffLine
                    {
                        Type = LineType.Unchanged,
                        Text = oldPieces[i],
                        OldIndex = i,
                        NewIndex = contextStartB + (i - contextStartA)
                    });
                }
                
                // Add all blocks and intermediate context
                int currentPosA = firstBlockStartA;
                int currentPosB = firstBlockStartB;
                
                for (int blockIndex = 0; blockIndex < group.Count; blockIndex++)
                {
                    var block = group[blockIndex];
                    
                    // Add context between blocks if needed
                    for (int i = currentPosA; i < block.DeleteStartA; i++)
                    {
                        int newIndex = currentPosB + (i - currentPosA);
                        hunk.Lines.Add(new DiffLine
                        {
                            Type = LineType.Unchanged,
                            Text = oldPieces[i],
                            OldIndex = i,
                            NewIndex = newIndex
                        });
                    }
                    
                    // Update the current position in B
                    if (currentPosA < block.DeleteStartA)
                    {
                        currentPosB += (block.DeleteStartA - currentPosA);
                    }
                    
                    // Add deleted lines
                    for (int i = 0; i < block.DeleteCountA; i++)
                    {
                        hunk.Lines.Add(new DiffLine
                        {
                            Type = LineType.Deleted,
                            Text = oldPieces[block.DeleteStartA + i],
                            OldIndex = block.DeleteStartA + i,
                            NewIndex = -1
                        });
                    }
                    
                    // Add inserted lines
                    for (int i = 0; i < block.InsertCountB; i++)
                    {
                        hunk.Lines.Add(new DiffLine
                        {
                            Type = LineType.Inserted,
                            Text = newPieces[block.InsertStartB + i],
                            OldIndex = -1,
                            NewIndex = block.InsertStartB + i
                        });
                    }
                    
                    currentPosA = block.DeleteStartA + block.DeleteCountA;
                    currentPosB = block.InsertStartB + block.InsertCountB;
                }
                
                // Add context after last block
                for (int i = currentPosA; i < contextEndA; i++)
                {
                    int newIndex = currentPosB + (i - currentPosA);
                    if (newIndex < newPieces.Count) // Ensure we don't go out of bounds
                    {
                        hunk.Lines.Add(new DiffLine
                        {
                            Type = LineType.Unchanged,
                            Text = oldPieces[i],
                            OldIndex = i,
                            NewIndex = newIndex
                        });
                    }
                }
                
                // Calculate final hunk lengths
                hunk.OldLength = hunk.Lines.Count(l => l.Type != LineType.Inserted);
                hunk.NewLength = hunk.Lines.Count(l => l.Type != LineType.Deleted);
                
                hunks.Add(hunk);
            }
            
            return hunks;
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
        
        #region Helper Classes
        private enum LineType
        {
            Unchanged,
            Deleted,
            Inserted
        }
        
        private class DiffLine
        {
            public LineType Type { get; set; }
            public string Text { get; set; }
            public int OldIndex { get; set; }
            public int NewIndex { get; set; }
        }
        
        private class DiffHunk
        {
            public int OldStartLine { get; set; }
            public int OldLength { get; set; }
            public int NewStartLine { get; set; }
            public int NewLength { get; set; }
            public List<DiffLine> Lines { get; } = new List<DiffLine>();
        }
        #endregion
    }
}