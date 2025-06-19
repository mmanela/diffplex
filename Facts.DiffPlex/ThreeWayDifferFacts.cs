using System;
using System.Linq;
using DiffPlex;
using DiffPlex.Chunkers;
using DiffPlex.Model;
using Xunit;

namespace Facts.DiffPlex
{
    public class ThreeWayDifferFacts
    {
        private readonly ThreeWayDiffer _differ = new ThreeWayDiffer();

        public class CreateDiffs : ThreeWayDifferFacts
        {
            [Fact]
            public void Will_throw_if_base_parameter_is_null()
            {
                Assert.Throws<ArgumentNullException>(() => 
                    _differ.CreateDiffs(null, "yours", "theirs", false, false, new LineChunker()));
            }

            [Fact]
            public void Will_throw_if_old_parameter_is_null()
            {
            Assert.Throws<ArgumentNullException>(() => 
            _differ.CreateDiffs("base", null, "new", false, false, new LineChunker()));
            }

            [Fact]
            public void Will_throw_if_new_parameter_is_null()
            {
            Assert.Throws<ArgumentNullException>(() => 
            _differ.CreateDiffs("base", "old", null, false, false, new LineChunker()));
            }

            [Fact]
            public void Will_throw_if_chunker_parameter_is_null()
            {
                Assert.Throws<ArgumentNullException>(() => 
                    _differ.CreateDiffs("base", "yours", "theirs", false, false, null));
            }

            [Fact]
            public void Can_diff_identical_texts()
            {
                const string text = "line1\nline2\nline3";
                
                var result = _differ.CreateDiffs(text, text, text, false, false, new LineChunker());
                
                Assert.Single(result.DiffBlocks);
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[0].ChangeType);
                Assert.Equal(3, result.DiffBlocks[0].BaseCount);
            }

            [Fact]  
            public void Can_detect_yours_only_change()
            {
                const string baseText = "line1\nline2\nline3";
                const string yoursText = "line1\nmodified line2\nline3";
                const string theirsText = "line1\nline2\nline3";
                
                var result = _differ.CreateDiffs(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.Equal(3, result.DiffBlocks.Count);
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[0].ChangeType); // line1
                Assert.Equal(ThreeWayChangeType.OldOnly, result.DiffBlocks[1].ChangeType); // modified line2
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[2].ChangeType); // line3
            }

            [Fact]
            public void Can_detect_theirs_only_change()
            {
                const string baseText = "line1\nline2\nline3";
                const string yoursText = "line1\nline2\nline3";
                const string theirsText = "line1\nmodified line2\nline3";
                
                var result = _differ.CreateDiffs(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.Equal(3, result.DiffBlocks.Count);
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[0].ChangeType); // line1
                Assert.Equal(ThreeWayChangeType.NewOnly, result.DiffBlocks[1].ChangeType); // modified line2
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[2].ChangeType); // line3
            }

            [Fact]
            public void Can_detect_both_same_change()
            {
                const string baseText = "line1\nline2\nline3";
                const string yoursText = "line1\nmodified line2\nline3";
                const string theirsText = "line1\nmodified line2\nline3";
                
                var result = _differ.CreateDiffs(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.Equal(3, result.DiffBlocks.Count);
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[0].ChangeType); // line1
                Assert.Equal(ThreeWayChangeType.BothSame, result.DiffBlocks[1].ChangeType); // modified line2
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[2].ChangeType); // line3
            }

            [Fact]
            public void Can_detect_conflict()
            {
                const string baseText = "line1\nline2\nline3";
                const string yoursText = "line1\nyours modified line2\nline3";
                const string theirsText = "line1\ntheirs modified line2\nline3";
                
                var result = _differ.CreateDiffs(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.Equal(3, result.DiffBlocks.Count);
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[0].ChangeType); // line1
                Assert.Equal(ThreeWayChangeType.Conflict, result.DiffBlocks[1].ChangeType); // conflicting changes
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[2].ChangeType); // line3
            }

            [Fact]
            public void Can_handle_additions_and_deletions()
            {
                const string baseText = "line1\nline2\nline3";
                const string yoursText = "line1\nline2\naddition\nline3";
                const string theirsText = "line1\nline3"; // deleted line2
                
                var result = _differ.CreateDiffs(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.Equal(4, result.DiffBlocks.Count);
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[0].ChangeType); // line1
                Assert.Equal(ThreeWayChangeType.NewOnly, result.DiffBlocks[1].ChangeType); // line2 deleted in theirs
                Assert.Equal(ThreeWayChangeType.OldOnly, result.DiffBlocks[2].ChangeType); // addition in yours
                Assert.Equal(ThreeWayChangeType.Unchanged, result.DiffBlocks[3].ChangeType); // line3
            }
        }

        public class CreateMerge : ThreeWayDifferFacts
        {
            [Fact]
            public void Can_merge_identical_texts()
            {
                const string text = "line1\nline2\nline3";
                
                var result = _differ.CreateMerge(text, text, text, false, false, new LineChunker());
                
                Assert.True(result.IsSuccessful);
                Assert.Empty(result.ConflictBlocks);
                Assert.Equal(3, result.MergedPieces.Count);
                Assert.Equal("line1", result.MergedPieces[0]);
                Assert.Equal("line2", result.MergedPieces[1]);
                Assert.Equal("line3", result.MergedPieces[2]);
            }

            [Fact]
            public void Can_merge_yours_only_change()
            {
                const string baseText = "line1\nline2\nline3";
                const string yoursText = "line1\nmodified line2\nline3";
                const string theirsText = "line1\nline2\nline3";
                
                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.True(result.IsSuccessful);
                Assert.Empty(result.ConflictBlocks);
                Assert.Equal(3, result.MergedPieces.Count);
                Assert.Equal("line1", result.MergedPieces[0]);
                Assert.Equal("modified line2", result.MergedPieces[1]);
                Assert.Equal("line3", result.MergedPieces[2]);
            }

            [Fact]
            public void Can_merge_theirs_only_change()
            {
                const string baseText = "line1\nline2\nline3";
                const string yoursText = "line1\nline2\nline3";
                const string theirsText = "line1\nmodified line2\nline3";
                
                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.True(result.IsSuccessful);
                Assert.Empty(result.ConflictBlocks);
                Assert.Equal(3, result.MergedPieces.Count);
                Assert.Equal("line1", result.MergedPieces[0]);
                Assert.Equal("modified line2", result.MergedPieces[1]);
                Assert.Equal("line3", result.MergedPieces[2]);
            }

            [Fact]
            public void Can_merge_both_same_change()
            {
                const string baseText = "line1\nline2\nline3";
                const string yoursText = "line1\nmodified line2\nline3";
                const string theirsText = "line1\nmodified line2\nline3";
                
                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.True(result.IsSuccessful);
                Assert.Empty(result.ConflictBlocks);
                Assert.Equal(3, result.MergedPieces.Count);
                Assert.Equal("line1", result.MergedPieces[0]);
                Assert.Equal("modified line2", result.MergedPieces[1]);
                Assert.Equal("line3", result.MergedPieces[2]);
            }

            [Fact]
            public void Can_handle_conflicts_with_markers()
            {
                const string baseText = "line1\nline2\nline3";
                const string yoursText = "line1\nyours modified line2\nline3";
                const string theirsText = "line1\ntheirs modified line2\nline3";
                
                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.False(result.IsSuccessful);
                Assert.Single(result.ConflictBlocks);
                
                var conflict = result.ConflictBlocks[0];
                Assert.Equal("line2", conflict.BasePieces.Single());
                Assert.Equal("yours modified line2", conflict.OldPieces.Single());
                Assert.Equal("theirs modified line2", conflict.NewPieces.Single());
                
                // Check conflict markers in merged result
                var mergedText = string.Join("\n", result.MergedPieces);
                Assert.Contains("<<<<<<< old", mergedText);
                Assert.Contains("||||||| base", mergedText);
                Assert.Contains("=======", mergedText);
                Assert.Contains(">>>>>>> new", mergedText);
            }

            [Fact]
            public void Can_merge_complex_scenario()
            {
                const string baseText = "header\nshared line\nmodify me\ndelete me\nkeep me\nfooter";
                const string yoursText = "header\nshared line\nyour modification\nkeep me\nyour addition\nfooter";
                const string theirsText = "header\nshared line\ntheir modification\nkeep me\nfooter";
                
                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.False(result.IsSuccessful); // Due to conflict in modification
                Assert.Single(result.ConflictBlocks);
                
                // Verify the conflict is about the "modify me" line
                var conflict = result.ConflictBlocks[0];
                Assert.Contains("modify me", conflict.BasePieces);
                Assert.Contains("your modification", conflict.OldPieces);  
                Assert.Contains("their modification", conflict.NewPieces);
            }
        }

        public class RealWorldScenarios : ThreeWayDifferFacts
        {
            [Fact]
            public void Can_merge_git_like_scenario()
            {
                const string baseText = @"class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}";

                const string yoursText = @"class Calculator
{
    public int Add(int a, int b)
    {
        // Added validation
        if (a < 0 || b < 0) throw new ArgumentException();
        return a + b;
    }
    
    public int Multiply(int a, int b)
    {
        return a * b;
    }
}";

                const string theirsText = @"class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
    
    public int Subtract(int a, int b)
    {
        return a - b;
    }
}";

                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());
                
                Assert.True(result.IsSuccessful);
                Assert.Empty(result.ConflictBlocks);
                
                var mergedText = string.Join("\n", result.MergedPieces);
                Assert.Contains("// Added validation", mergedText);
                Assert.Contains("public int Multiply", mergedText);
                Assert.Contains("public int Subtract", mergedText);
            }

            [Fact]
            public void Can_handle_whitespace_conflicts()
            {
                const string baseText = "line1\n    line2\nline3";
                const string yoursText = "line1\n\tline2\nline3";  // Tab instead of spaces
                const string theirsText = "line1\n        line2\nline3";  // Different number of spaces
                
                var result = _differ.CreateMerge(baseText, yoursText, theirsText, true, false, new LineChunker());
                
                // With ignoreWhiteSpace=true, this should be treated as BothSame
                Assert.True(result.IsSuccessful);
                Assert.Empty(result.ConflictBlocks);
            }
        }
    }
}
