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

            [Fact]
            public void Can_merge_config_file_changes()
            {
                const string baseText = @"{
  ""database"": {
    ""host"": ""localhost"",
    ""port"": 5432
  },
  ""logging"": {
    ""level"": ""info""    
  }
}";

                const string yoursText = @"{
  ""database"": {
    ""host"": ""localhost"",
    ""port"": 5432,
    ""timeout"": 30
  },
  ""logging"": {
    ""level"": ""info""    
  }
}";

                const string theirsText = @"{
  ""database"": {
    ""host"": ""localhost"",
    ""port"": 5432
  },
  ""logging"": {
    ""level"": ""debug"",
    ""file"": ""app.log""
  }
}";

                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());

                Assert.True(result.IsSuccessful);
                var mergedText = string.Join("\n", result.MergedPieces);
                Assert.Contains("\"timeout\": 30", mergedText);
                Assert.Contains("\"level\": \"debug\"", mergedText);
                Assert.Contains("\"file\": \"app.log\"", mergedText);
            }

            [Fact]
            public void Can_handle_import_statement_conflicts()
            {
                const string baseText = @"using System;
using System.Collections.Generic;

namespace MyApp
{
    public class Service
    {
        public void DoWork() { }
    }
}";

                const string yoursText = @"using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyApp
{
    public class Service
    {
        public async Task DoWorkAsync() { }
    }
}";

                const string theirsText = @"using System;
using System.Collections.Generic;
using System.Linq;

namespace MyApp
{
    public class Service
    {
        public void DoWork() 
        { 
            var items = new List<int>().Where(x => x > 0);
        }
    }
}";

                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());

                Assert.False(result.IsSuccessful); // Method signature conflict
                Assert.True(result.ConflictBlocks.Count > 0);

                var mergedText = string.Join("\n", result.MergedPieces);
                Assert.Contains("using System.Threading.Tasks", mergedText);
                Assert.Contains("using System.Linq", mergedText);
            }

            [Fact]
            public void Can_merge_documentation_changes()
            {
                const string baseText = @"# Project Documentation

## Overview
This is a sample project.

## Installation
Run npm install.

## Usage
Basic usage instructions.";

                const string yoursText = @"# Project Documentation

## Overview
This is a sample project for demonstrating features.

## Installation
Run npm install.

## Usage
Basic usage instructions.

## Examples
Here are some examples of usage.";

                const string theirsText = @"# Project Documentation

## Overview
This is a sample project.

## Requirements
- Node.js 16+
- npm 8+

## Installation
Run npm install.

## Usage
Basic usage instructions.";

                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());

                Assert.True(result.IsSuccessful);
                var mergedText = string.Join("\n", result.MergedPieces);
                Assert.Contains("for demonstrating features", mergedText);
                Assert.Contains("## Requirements", mergedText);
                Assert.Contains("## Examples", mergedText);
            }

            [Fact]
            public void Can_handle_complex_refactoring_conflict()
            {
                const string baseText = @"public class UserService
{
    private readonly IRepository repository;
    
    public User GetUser(int id)
    {
        return repository.GetById(id);
    }
    
    public void UpdateUser(User user)
    {
        repository.Update(user);
    }
}";

                const string yoursText = @"public class UserService
{
    private readonly IUserRepository userRepository;
    
    public User GetUser(int id)
    {
        var user = userRepository.GetById(id);
        if (user == null) throw new UserNotFoundException();
        return user;
    }
    
    public void UpdateUser(User user)
    {
        userRepository.Update(user);
    }
}";

                const string theirsText = @"public class UserService
{
    private readonly IRepository repository;
    
    public async Task<User> GetUserAsync(int id)
    {
        return await repository.GetByIdAsync(id);
    }
    
    public async Task UpdateUserAsync(User user)
    {
        await repository.UpdateAsync(user);
    }
}";

                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());

                Assert.False(result.IsSuccessful);
                Assert.True(result.ConflictBlocks.Count > 0);

                // Verify conflicts contain both refactoring attempts
                var conflictText = string.Join(" ", result.ConflictBlocks.SelectMany(c => c.OldPieces.Concat(c.NewPieces)));
                Assert.Contains("userRepository", conflictText);
                Assert.Contains("await", conflictText);
            }

            [Fact]
            public void Can_merge_html_template_changes()
            {
                const string baseText = @"<div class=""container"">
    <h1>Welcome</h1>
    <p>Basic content</p>
</div>";

                const string yoursText = @"<div class=""container"">
    <h1>Welcome to Our Site</h1>
    <p>Basic content</p>
    <button onclick=""showMore()"">Show More</button>
</div>";

                const string theirsText = @"<div class=""container responsive"">
    <h1>Welcome</h1>
    <p>Enhanced content with more details</p>
</div>";

                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());

                Assert.True(result.IsSuccessful);
                var mergedText = string.Join("\n", result.MergedPieces);
                Assert.Contains("responsive", mergedText);
                Assert.Contains("Welcome to Our Site", mergedText);
                Assert.Contains("Enhanced content", mergedText);
                Assert.Contains("showMore()", mergedText);
            }

            [Fact]
            public void Can_handle_sql_schema_migration_conflict()
            {
                const string baseText = @"CREATE TABLE users (
    id INTEGER PRIMARY KEY,
    name VARCHAR(100),
    email VARCHAR(255)
);";

                const string yoursText = @"CREATE TABLE users (
    id INTEGER PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(255) UNIQUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);";

                const string theirsText = @"CREATE TABLE users (
    id INTEGER PRIMARY KEY,
    first_name VARCHAR(50),
    last_name VARCHAR(50), 
    email VARCHAR(255)
);";

                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());

                Assert.False(result.IsSuccessful);
                Assert.Single(result.ConflictBlocks);

                // Verify conflict contains both approaches to name field
                var conflictText = string.Join(" ", result.ConflictBlocks[0].OldPieces.Concat(result.ConflictBlocks[0].NewPieces));
                Assert.Contains("name VARCHAR(100) NOT NULL", conflictText);
                Assert.Contains("first_name", conflictText);
                Assert.Contains("last_name", conflictText);
            }

            [Fact]
            public void Can_merge_different_feature_additions()
            {
                const string baseText = @"public class Calculator
{
    public double Add(double a, double b) => a + b;
    public double Subtract(double a, double b) => a - b;
}";

                const string yoursText = @"public class Calculator
{
    public double Add(double a, double b) => a + b;
    public double Subtract(double a, double b) => a - b;
    public double Multiply(double a, double b) => a * b;
    public double Power(double baseNum, double exponent) => Math.Pow(baseNum, exponent);
}";

                const string theirsText = @"public class Calculator
{
    public double Add(double a, double b) => a + b;
    public double Subtract(double a, double b) => a - b;
    public double Divide(double a, double b) => b != 0 ? a / b : throw new DivideByZeroException();
    public double Modulo(double a, double b) => a % b;
}";

                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());

                // This might conflict if additions are on same lines
                var mergedText = string.Join("\n", result.MergedPieces);
                Assert.Contains("Multiply", mergedText);
                Assert.Contains("Power", mergedText);
                Assert.Contains("Divide", mergedText);
                Assert.Contains("Modulo", mergedText);
            }

            [Fact]
            public void Can_handle_comment_and_code_changes()
            {
                const string baseText = @"// Basic validation
public bool IsValid(string input)
{
    return !string.IsNullOrEmpty(input);
}";

                const string yoursText = @"/// <summary>
/// Validates input string for basic requirements
/// </summary>
/// <param name=""input"">The input string to validate</param>
/// <returns>True if valid, false otherwise</returns>
public bool IsValid(string input)
{
    return !string.IsNullOrEmpty(input) && input.Length > 2;
}";

                const string theirsText = @"// Enhanced validation with trim
public bool IsValid(string input)
{
    return !string.IsNullOrWhiteSpace(input?.Trim());
}";

                var result = _differ.CreateMerge(baseText, yoursText, theirsText, false, false, new LineChunker());

                Assert.False(result.IsSuccessful);
                Assert.True(result.ConflictBlocks.Count > 0);

                // Both changed the implementation differently  
                var mergedText = string.Join("\n", result.MergedPieces);
                Assert.Contains("summary", mergedText);  // XML doc was added by yours
            }

            [Fact]
            public void Will_handle_consecutive_pure_insertions_at_same_position()
            {
                // This tests for the potential infinite loop mentioned by the Oracle
                // where two consecutive pure insertion blocks at the same BaseStart could cause issues

                var baseText = "line1\nline3\n";
                var oldText = "line1\ninserted_old1\ninserted_old2\nline3\n";
                var newText = "line1\ninserted_new1\ninserted_new2\nline3\n";

                var differ = ThreeWayDiffer.Instance;

                // This should not hang or throw an exception
                var result = differ.CreateDiffs(baseText, oldText, newText, false, false, LineChunker.Instance);

                Assert.NotNull(result);
                Assert.True(result.DiffBlocks.Count > 0);
            }

            [Fact]
            public void Will_handle_empty_base_with_insertions()
            {
                var baseText = "";
                var oldText = "old_line1\nold_line2\n";
                var newText = "new_line1\nnew_line2\n";

                var differ = ThreeWayDiffer.Instance;

                var result = differ.CreateDiffs(baseText, oldText, newText, false, false, LineChunker.Instance);

                Assert.NotNull(result);
                Assert.True(result.DiffBlocks.Count >= 1);
                Assert.Contains(result.DiffBlocks, block => block.ChangeType == ThreeWayChangeType.Conflict);
            }

            [Fact]
            public void Will_handle_pure_deletions()
            {
                var baseText = "line1\nline2\nline3\n";
                var oldText = "line1\nline3\n";
                var newText = "line1\nline3\n";

                var differ = ThreeWayDiffer.Instance;

                var result = differ.CreateDiffs(baseText, oldText, newText, false, false, LineChunker.Instance);

                Assert.NotNull(result);
            }

            [Fact]
            public void Will_create_merge_with_consecutive_insertions()
            {
                var baseText = "line1\nline3\n";
                var oldText = "line1\ninserted_old\nline3\n";
                var newText = "line1\ninserted_new\nline3\n";

                var differ = ThreeWayDiffer.Instance;

                var result = differ.CreateMerge(baseText, oldText, newText, false, false, LineChunker.Instance);

                Assert.NotNull(result);
                Assert.False(result.IsSuccessful); // Should be a conflict
                Assert.True(result.ConflictBlocks.Count > 0);
            }

            [Fact]
            public void Will_handle_zero_length_deletions_properly()
            {
                // Test case where DeleteCountA is 0 (pure insertion)
                var baseText = "line1\nline2\n";
                var oldText = "line1\ninserted\nline2\n";  // Pure insertion at position 1
                var newText = "line1\nline2\n";  // No change

                var differ = ThreeWayDiffer.Instance;

                var result = differ.CreateDiffs(baseText, oldText, newText, false, false, LineChunker.Instance);

                Assert.NotNull(result);
                Assert.True(result.DiffBlocks.Count > 0);

                // Should have OldOnly change type for the insertion
                Assert.Contains(result.DiffBlocks, block => block.ChangeType == ThreeWayChangeType.OldOnly);
            }

            [Fact]
            public void Will_handle_complex_three_way_scenario()
            {
                // A more complex scenario that could stress test the loop logic
                var baseText = "A\nB\nC\nD\nE\n";
                var oldText = "A\nB_OLD\nNEW_OLD\nC\nD\nE_OLD\n";
                var newText = "A_NEW\nB\nNEW_NEW\nC\nD_NEW\nE\n";

                var differ = ThreeWayDiffer.Instance;

                var result = differ.CreateDiffs(baseText, oldText, newText, false, false, LineChunker.Instance);

                Assert.NotNull(result);
                Assert.True(result.DiffBlocks.Count > 0);

                // Test that merge doesn't hang
                var mergeResult = differ.CreateMerge(baseText, oldText, newText, false, false, LineChunker.Instance);
                Assert.NotNull(mergeResult);
            }
        }
    }
}
