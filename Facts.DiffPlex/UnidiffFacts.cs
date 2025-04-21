using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Renderer;
using Xunit;

namespace Facts.DiffPlex
{
    public class UnidiffFacts
    {
        // Helper method to normalize line endings for comparisons
        private string NormalizeLineEndings(string text)
        {
            return text?.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd();
        }

        [Fact]
        public void Generate_WithSimpleAddition_ReturnsCorrectUnidiff()
        {
            // Arrange
            string oldText = @"line 1
line 2
line 3";
            string newText = @"line 1
line 2
line 3
line 4";
            
            // Act
            string result = UnidiffRenderer.GenerateUnidiff(oldText, newText, "old.txt", "new.txt");
            
            // Assert
            string expected = @"--- old.txt
+++ new.txt
@@ -1,3 +1,4 @@
 line 1
 line 2
 line 3
+line 4";
            
            Assert.Equal(NormalizeLineEndings(expected), NormalizeLineEndings(result));
        }
        
        [Fact]
        public void Generate_WithSimpleDeletion_ReturnsCorrectUnidiff()
        {
            // Arrange
            string oldText = @"line 1
line 2
line 3
line 4";
            string newText = @"line 1
line 3
line 4";
            
            // Act
            string result = UnidiffRenderer.GenerateUnidiff(oldText, newText, "old.txt", "new.txt");
            
            // Assert
            string expected = @"--- old.txt
+++ new.txt
@@ -1,4 +1,3 @@
 line 1
-line 2
 line 3
 line 4";
            
            Assert.Equal(NormalizeLineEndings(expected), NormalizeLineEndings(result));
        }
        
        [Fact]
        public void Generate_WithMultipleChanges_ReturnsCorrectUnidiff()
        {
            // Arrange
            string oldText = @"header line 1
header line 2
content line 1
content line 2
content line 3
footer line 1
footer line 2";
            string newText = @"header line 1
header line 2 modified
content line 1
new content line
content line 3
footer line 1
additional footer
footer line 2";
            
            // Act
            string result = UnidiffRenderer.GenerateUnidiff(oldText, newText, "old.txt", "new.txt");
            
            // Assert
            string expected = @"--- old.txt
+++ new.txt
@@ -1,7 +1,8 @@
 header line 1
-header line 2
+header line 2 modified
 content line 1
-content line 2
+new content line
 content line 3
 footer line 1
+additional footer
 footer line 2";
            
            Assert.Equal(NormalizeLineEndings(expected), NormalizeLineEndings(result));
        }
        
        [Fact]
        public void Generate_WithSeparatedChanges_CreatesSeparateHunks()
        {
            // Arrange
            string oldText = @"header line 1
header line 2
header line 3
header line 4
header line 5
content line 1
content line 2
content line 3
content line 4
content line 5
footer line 1
footer line 2
footer line 3
footer line 4
footer line 5";
            string newText = @"header line 1
modified header
header line 3
header line 4
header line 5
content line 1
content line 2
content line 3
content line 4
content line 5
footer line 1
footer line 2
modified footer
footer line 4
footer line 5";
            
            // Act
            string result = UnidiffRenderer.GenerateUnidiff(oldText, newText, "old.txt", "new.txt");
            
            // Assert
            string expected = @"--- old.txt
+++ new.txt
@@ -1,5 +1,5 @@
 header line 1
-header line 2
+modified header
 header line 3
 header line 4
 header line 5
@@ -10,6 +10,6 @@
 content line 5
 footer line 1
 footer line 2
-footer line 3
+modified footer
 footer line 4
 footer line 5";
            
            Assert.Equal(NormalizeLineEndings(expected), NormalizeLineEndings(result));
        }
        
        [Fact]
        public void Generate_WithNoChanges_ReturnsEmptyString()
        {
            // Arrange
            string text = @"line 1
line 2
line 3";
            
            // Act
            string result = UnidiffRenderer.GenerateUnidiff(text, text, "old.txt", "new.txt");
            
            // Assert
            Assert.Equal(string.Empty, result);
        }
    }
}