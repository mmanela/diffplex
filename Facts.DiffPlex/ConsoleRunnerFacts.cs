using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Facts.DiffPlex
{
    public class ConsoleRunnerFacts
    {
        private readonly string _consoleRunnerPath;

        public ConsoleRunnerFacts()
        {
            _consoleRunnerPath = FindConsoleRunnerPath();
        }

        private static string FindConsoleRunnerPath()
        {
            // Find the console runner executable
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Try multiple possible paths for the console runner
            var targetFrameworks = new[] { "net6.0", "net7.0", "net8.0", "net9.0" };
            var buildConfigs = new[] { "Debug", "Release" };
            var relativePaths = new[]
            {
                Path.Combine("..", "..", "..", "..", "DiffPlex.ConsoleRunner", "bin"),
                Path.Combine("..", "..", "DiffPlex.ConsoleRunner", "bin"),
                Path.Combine("..", "DiffPlex.ConsoleRunner"),
                ""
            };

            var possiblePaths = new List<string>();
            
            // Generate all combinations of paths, configs, and frameworks
            foreach (var relativePath in relativePaths)
            {
                foreach (var config in buildConfigs)
                {
                    foreach (var framework in targetFrameworks)
                    {
                        var path = Path.Combine(baseDir, relativePath, config, framework, "DiffPlex.ConsoleRunner.dll");
                        possiblePaths.Add(path);
                    }
                }
                
                // Also try without config/framework subfolders (CI scenarios)
                var simplePath = Path.Combine(baseDir, relativePath, "DiffPlex.ConsoleRunner.dll");
                possiblePaths.Add(simplePath);
            }

            var foundPath = possiblePaths.FirstOrDefault(File.Exists);
            
            if (foundPath == null)
            {
                var searchedPaths = string.Join("\n", possiblePaths.Select((path, i) => $"  {i + 1}. {path}"));
                throw new InvalidOperationException($"Could not find DiffPlex.ConsoleRunner.dll in any of the expected locations.\nBase directory: {baseDir}\nSearched paths:\n{searchedPaths}");
            }

            return foundPath;
        }

        [Fact]
        public async Task Will_show_usage_when_no_arguments_provided()
        {
            var result = await RunConsoleRunner();
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("DiffPlex.ConsoleRunner", result.Output);
            Assert.Contains("Usage:", result.Output);
        }

        [Fact]
        public async Task Will_show_usage_when_insufficient_arguments_provided()
        {
            var result = await RunConsoleRunner("file", "onefile.txt");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("Usage:", result.Output);
        }

        [Fact]
        public async Task Will_handle_unknown_command()
        {
            var result = await RunConsoleRunner("unknown", "arg1", "arg2");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("Unknown command: unknown", result.Output);
        }

        [Fact]
        public async Task Will_compare_two_text_strings()
        {
            var result = await RunConsoleRunner("text", "line1\\nline2", "line1\\nline3");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("@@ -1,2 +1,2 @@", result.Output);
            Assert.Contains("-line2", result.Output);
            Assert.Contains("+line3", result.Output);
        }

        [Fact]
        public async Task Will_perform_3way_text_diff()
        {
            var result = await RunConsoleRunner("3way-text", "base\\ncommon", "base\\nold", "base\\nnew");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Three-Way Diff", result.Output);
            Assert.Contains("Base: 2 lines", result.Output);
            Assert.Contains("Old:  2 lines", result.Output);
            Assert.Contains("New:  2 lines", result.Output);
        }

        [Fact]
        public async Task Will_perform_3way_text_diff_with_ignore_case()
        {
            var result = await RunConsoleRunner("3way-text", "-i", "BASE\\ncommon", "base\\nold", "BASE\\nnew");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Three-Way Diff", result.Output);
        }

        [Fact]
        public async Task Will_perform_3way_text_diff_with_ignore_whitespace()
        {
            var result = await RunConsoleRunner("3way-text", "-w", "base \\ncommon", "base\\nold", "base\\nnew");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Three-Way Diff", result.Output);
        }

        [Fact]
        public async Task Will_perform_merge_text_without_conflicts()
        {
            var result = await RunConsoleRunner("merge-text", "base\\ncommon", "base\\nold", "new\\ncommon");
            
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("new", result.Output);
            Assert.Contains("old", result.Output);
        }

        [Fact]
        public async Task Will_perform_merge_text_with_conflicts()
        {
            var result = await RunConsoleRunner("merge-text", "base\\ncommon", "old\\ncommon", "new\\ncommon");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("<<<<<<< old", result.Output);
            Assert.Contains("=======", result.Output);
            Assert.Contains(">>>>>>> new", result.Output);
            Assert.Contains("conflicts", result.Output);
        }

        [Fact]
        public async Task Will_perform_merge_text_with_ignore_case()
        {
            var result = await RunConsoleRunner("merge-text", "-i", "BASE\\ncommon", "base\\nold", "BASE\\nnew");
            
            // This should still be a conflict because the content is different (old vs new)
            Assert.Equal(1, result.ExitCode);
        }

        [Fact]
        public async Task Will_perform_merge_text_with_ignore_whitespace()
        {
            var result = await RunConsoleRunner("merge-text", "-w", "base \\ncommon", "base\\nold", "base\\nnew");
            
            Assert.Equal(0, result.ExitCode);
        }

        [Fact]
        public async Task Will_handle_file_not_found_for_3way_file()
        {
            var result = await RunConsoleRunner("3way-file", "nonexistent1.txt", "nonexistent2.txt", "nonexistent3.txt");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("not found", result.Output);
        }

        [Fact]
        public async Task Will_handle_file_not_found_for_merge_file()
        {
            var result = await RunConsoleRunner("merge-file", "nonexistent1.txt", "nonexistent2.txt", "nonexistent3.txt");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("not found", result.Output);
        }

        [Fact]
        public async Task Will_handle_insufficient_arguments_for_3way_commands()
        {
            var result = await RunConsoleRunner("3way-text", "base", "old");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("requires base, old, and new", result.Output);
        }

        [Fact]
        public async Task Will_handle_insufficient_arguments_for_merge_commands()
        {
            var result = await RunConsoleRunner("merge-text", "base", "old");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("requires base, old, and new", result.Output);
        }

        [Fact]
        public async Task Will_handle_unknown_option()
        {
            var result = await RunConsoleRunner("text", "-x", "old", "new");
            
            Assert.Equal(1, result.ExitCode);
            Assert.Contains("Unknown option: -x", result.Output);
        }

        [Fact]
        public async Task Will_work_with_file_commands()
        {
            // Create temporary files
            var tempDir = Path.GetTempPath();
            var oldFile = Path.Combine(tempDir, "old_test.txt");
            var newFile = Path.Combine(tempDir, "new_test.txt");

            try
            {
                await File.WriteAllTextAsync(oldFile, "line1\nline2\n");
                await File.WriteAllTextAsync(newFile, "line1\nline3\n");

                var result = await RunConsoleRunner("file", oldFile, newFile);
                
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
        public async Task Will_work_with_3way_file_commands()
        {
            // Create temporary files
            var tempDir = Path.GetTempPath();
            var baseFile = Path.Combine(tempDir, "base_test.txt");
            var oldFile = Path.Combine(tempDir, "old_test.txt");
            var newFile = Path.Combine(tempDir, "new_test.txt");

            try
            {
                await File.WriteAllTextAsync(baseFile, "common\nbase\n");
                await File.WriteAllTextAsync(oldFile, "common\nold\n");
                await File.WriteAllTextAsync(newFile, "common\nnew\n");

                var result = await RunConsoleRunner("3way-file", baseFile, oldFile, newFile);
                
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
        public async Task Will_work_with_merge_file_commands()
        {
            // Create temporary files
            var tempDir = Path.GetTempPath();
            var baseFile = Path.Combine(tempDir, "base_merge_test.txt");
            var oldFile = Path.Combine(tempDir, "old_merge_test.txt");
            var newFile = Path.Combine(tempDir, "new_merge_test.txt");

            try
            {
                await File.WriteAllTextAsync(baseFile, "common\nbase\n");
                await File.WriteAllTextAsync(oldFile, "common\nold\n");
                await File.WriteAllTextAsync(newFile, "updated\nbase\n");

                var result = await RunConsoleRunner("merge-file", baseFile, oldFile, newFile);
                
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

        private async Task<ProcessResult> RunConsoleRunner(params string[] args)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{_consoleRunnerPath}\" {string.Join(" ", args)}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
            process.ErrorDataReceived += (sender, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            var output = outputBuilder.ToString();
            var error = errorBuilder.ToString();

            return new ProcessResult
            {
                ExitCode = process.ExitCode,
                Output = string.IsNullOrEmpty(error) ? output : output + error
            };
        }

        private class ProcessResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; } = string.Empty;
        }
    }
}
