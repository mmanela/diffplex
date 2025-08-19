using System;
using System.IO;
using System.Text;
using Xunit;

namespace Facts.DiffPlex
{
    public class ConsoleRunnerFacts
    {

        [Fact]
        public void Will_show_usage_when_no_arguments_provided()
        {
            var result = RunConsoleRunner();
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("DiffPlex.ConsoleRunner", result.Output);
            Assert.Contains("Usage:", result.Output);
        }

        [Fact]
        public void Will_show_usage_when_insufficient_arguments_provided()
        {
            var result = RunConsoleRunner("file", "onefile.txt");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("Usage:", result.Output);
        }

        [Fact]
        public void Will_handle_unknown_command()
        {
            var result = RunConsoleRunner("unknown", "arg1", "arg2");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("Unknown command: unknown", result.Output);
        }

        [Fact]
        public void Will_compare_two_text_strings()
        {
            var result = RunConsoleRunner("text", "line1\\nline2", "line1\\nline3");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("@@ -1,2 +1,2 @@", result.Output);
            Assert.Contains("-line2", result.Output);
            Assert.Contains("+line3", result.Output);
        }

        [Fact]
        public void Will_perform_3way_text_diff()
        {
            var result = RunConsoleRunner("3way-text", "base\\ncommon", "base\\nold", "base\\nnew");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Three-Way Diff", result.Output);
            Assert.Contains("Base: 2 lines", result.Output);
            Assert.Contains("Old:  2 lines", result.Output);
            Assert.Contains("New:  2 lines", result.Output);
        }

        [Fact]
        public void Will_perform_3way_text_diff_with_ignore_case()
        {
            var result = RunConsoleRunner("3way-text", "-i", "BASE\\ncommon", "base\\nold", "BASE\\nnew");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Three-Way Diff", result.Output);
        }

        [Fact]
        public void Will_perform_3way_text_diff_with_ignore_whitespace()
        {
            var result = RunConsoleRunner("3way-text", "-w", "base \\ncommon", "base\\nold", "base\\nnew");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Three-Way Diff", result.Output);
        }

        [Fact]
        public void Will_perform_merge_text_without_conflicts()
        {
            var result = RunConsoleRunner("merge-text", "base\\ncommon", "base\\nold", "new\\ncommon");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("new", result.Output);
            Assert.Contains("old", result.Output);
        }

        [Fact]
        public void Will_perform_merge_text_with_conflicts()
        {
            var result = RunConsoleRunner("merge-text", "base\\ncommon", "old\\ncommon", "new\\ncommon");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("<<<<<<< old", result.Output);
            Assert.Contains("=======", result.Output);
            Assert.Contains(">>>>>>> new", result.Output);
            Assert.Contains("conflicts", result.Output);
        }

        [Fact]
        public void Will_perform_merge_text_with_ignore_case()
        {
            var result = RunConsoleRunner("merge-text", "-i", "BASE\\ncommon", "base\\nold", "BASE\\nnew");
            
            // This should still be a conflict because the content is different (old vs new)
            Assert.Equal(1, result.ExitCode);
        }

        [Fact]
        public void Will_perform_merge_text_with_ignore_whitespace()
        {
            var result = RunConsoleRunner("merge-text", "-w", "base \\ncommon", "base\\nold", "new\\ncommon");
            
            Assert.Equal(0, result.ExitCode);
        }

        [Fact]
        public void Will_handle_file_not_found_for_3way_file()
        {
            var result = RunConsoleRunner("3way-file", "nonexistent1.txt", "nonexistent2.txt", "nonexistent3.txt");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("not found", result.Output);
        }

        [Fact]
        public void Will_handle_file_not_found_for_merge_file()
        {
            var result = RunConsoleRunner("merge-file", "nonexistent1.txt", "nonexistent2.txt", "nonexistent3.txt");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("not found", result.Output);
        }

        [Fact]
        public void Will_handle_insufficient_arguments_for_3way_commands()
        {
            var result = RunConsoleRunner("3way-text", "base", "old");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("requires base, old, and new", result.Output);
        }

        [Fact]
        public void Will_handle_insufficient_arguments_for_merge_commands()
        {
            var result = RunConsoleRunner("merge-text", "base", "old");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("requires base, old, and new", result.Output);
        }

        [Fact]
        public void Will_handle_unknown_option()
        {
            var result = RunConsoleRunner("text", "-x", "old", "new");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("Unknown option: -x", result.Output);
        }

        [Fact]
        public void Will_work_with_file_commands()
        {
            // Create temporary files
            var tempDir = Path.GetTempPath();
            var oldFile = Path.Combine(tempDir, "old_test.txt");
            var newFile = Path.Combine(tempDir, "new_test.txt");

            try
            {
                File.WriteAllText(oldFile, "line1\nline2\n");
                File.WriteAllText(newFile, "line1\nline3\n");

                var result = RunConsoleRunner("file", oldFile, newFile);
                
                Assert.Equal(0, result.ExitCode);
                Assert.Contains("@@ -1,", result.Output);
                Assert.Contains("-line2", result.Output);
                Assert.Contains("+line3", result.Output);
            }
            finally
            {
                // Clean up
                if (File.Exists(oldFile)) File.Delete(oldFile);
                if (File.Exists(newFile)) File.Delete(newFile);
            }
        }

        [Fact]
        public void Will_work_with_3way_file_commands()
        {
            // Create temporary files
            var tempDir = Path.GetTempPath();
            var baseFile = Path.Combine(tempDir, "base_test.txt");
            var oldFile = Path.Combine(tempDir, "old_test.txt");
            var newFile = Path.Combine(tempDir, "new_test.txt");

            try
            {
                File.WriteAllText(baseFile, "common\nbase\n");
                File.WriteAllText(oldFile, "common\nold\n");
                File.WriteAllText(newFile, "common\nnew\n");

                var result = RunConsoleRunner("3way-file", baseFile, oldFile, newFile);
                
                Assert.Equal(0, result.ExitCode);
                Assert.Contains("Three-Way Diff", result.Output);
                Assert.Contains("Base:", result.Output);
            }
            finally
            {
                // Clean up
                if (File.Exists(baseFile)) File.Delete(baseFile);
                if (File.Exists(oldFile)) File.Delete(oldFile);
                if (File.Exists(newFile)) File.Delete(newFile);
            }
        }

        [Fact]
        public void Will_work_with_merge_file_commands()
        {
            // Create temporary files
            var tempDir = Path.GetTempPath();
            var baseFile = Path.Combine(tempDir, "base_merge_test.txt");
            var oldFile = Path.Combine(tempDir, "old_merge_test.txt");
            var newFile = Path.Combine(tempDir, "new_merge_test.txt");

            try
            {
                File.WriteAllText(baseFile, "common\nbase\n");
                File.WriteAllText(oldFile, "common\nold\n");
                File.WriteAllText(newFile, "updated\nbase\n");

                var result = RunConsoleRunner("merge-file", baseFile, oldFile, newFile);
                
                Assert.Equal(0, result.ExitCode);
                Assert.Contains("updated", result.Output);
                Assert.Contains("old", result.Output);
            }
            finally
            {
                // Clean up
                if (File.Exists(baseFile)) File.Delete(baseFile);
                if (File.Exists(oldFile)) File.Delete(oldFile);
                if (File.Exists(newFile)) File.Delete(newFile);
            }
        }

        private static ConsoleResult RunConsoleRunner(params string[] args)
        {
            var output = new StringBuilder();
            var originalOut = Console.Out;
            var originalError = Console.Error;
            
            try
            {
                using var writer = new StringWriter(output);
                Console.SetOut(writer);
                Console.SetError(writer);
                
                var exitCode = global::DiffPlex.ConsoleRunner.Program.Main(args);
                
                return new ConsoleResult
                {
                    ExitCode = exitCode,
                    Output = output.ToString()
                };
            }
            catch (Exception ex)
            {
                return new ConsoleResult
                {
                    ExitCode = 1,
                    Output = $"Error: {ex.Message}"
                };
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }

        private class ConsoleResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; } = string.Empty;
        }
    }
}
