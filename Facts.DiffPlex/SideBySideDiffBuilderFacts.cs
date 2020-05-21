using System;
using System.Collections.Generic;
using DiffPlex;
using DiffPlex.Model;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Moq;
using Xunit;

namespace Facts.DiffPlex
{
    public class SideBySideDiffBuilderFacts
    {
        public class Constructor
        {
            [Fact]
            public void Will_not_throw_if_IDiffer_is_null()
            {
                var builder = new SideBySideDiffBuilder(null);
                Assert.NotNull(builder);
            }
        }

        public class BuildDiffModel
        {
            [Fact]
            public void Will_throw_is_OldText_is_null()
            {
                var differ = new Mock<IDiffer>();
                var builder = new SideBySideDiffBuilder(differ.Object);

                var ex = Record.Exception(() => builder.BuildDiffModel(null, "asd"));

                Assert.IsType<ArgumentNullException>(ex);
                var an = (ArgumentNullException) ex;
                Assert.Equal("oldText", an.ParamName);
            }

            [Fact]
            public void Will_throw_is_NewText_is_null()
            {
                var differ = new Mock<IDiffer>();
                var builder = new SideBySideDiffBuilder(differ.Object);

                var ex = Record.Exception(() => builder.BuildDiffModel("asa", null));

                Assert.IsType<ArgumentNullException>(ex);
                var an = (ArgumentNullException) ex;
                Assert.Equal("newText", an.ParamName);
            }

            [Fact]
            public void Will_build_diffModel_for_duplicate_strings()
            {
                string text = "a\nb\nc\nd\n\n";
                string[] textLines = {"a", "b", "c", "d", ""};
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(text, text, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLines, textLines, new List<DiffBlock>()));
                differ.Setup(x => x.CreateDiffs(It.IsAny<string>(), It.IsAny<string>(), false, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(new string[0], new string[0], new List<DiffBlock>()));
                var builder = new SideBySideDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(text, text);

                Assert.NotNull(bidiff);
                Assert.Equal(textLines.Length, bidiff.OldText.Lines.Count);
                Assert.Equal(textLines.Length, bidiff.NewText.Lines.Count);
                for (int i = 0; i < textLines.Length; i++)
                {
                    Assert.Equal(textLines[i], bidiff.OldText.Lines[i].Text);
                    Assert.Equal(ChangeType.Unchanged, bidiff.OldText.Lines[i].Type);
                    Assert.Equal(i + 1, bidiff.OldText.Lines[i].Position);
                    Assert.Equal(textLines[i], bidiff.NewText.Lines[i].Text);
                    Assert.Equal(ChangeType.Unchanged, bidiff.NewText.Lines[i].Type);
                    Assert.Equal(i + 1, bidiff.NewText.Lines[i].Position);
                }

                Assert.False(bidiff.OldText.HasDifferences && bidiff.NewText.HasDifferences);
            }

            [Fact]
            public void Will_build_diffModel_when_oldText_is_empty()
            {
                string textOld = "";
                string textNew = "z\ny\nx\nw\n";
                string[] textLinesOld = {};
                string[] textLinesNew = {"z", "y"};
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> {new DiffBlock(0, 0, 0, 2)}));
                differ.Setup(x => x.CreateDiffs(It.IsAny<string>(), It.IsAny<string>(), false, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(new string[0], new string[0], new List<DiffBlock>()));
                var builder = new SideBySideDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Equal(2, bidiff.OldText.Lines.Count);
                Assert.Equal(2, bidiff.NewText.Lines.Count);

                for (int j = 0; j < textLinesNew.Length; j++)
                {
                    Assert.Equal(textLinesNew[j], bidiff.NewText.Lines[j].Text);
                    Assert.Equal(ChangeType.Inserted, bidiff.NewText.Lines[j].Type);
                    Assert.Equal(j + 1, bidiff.NewText.Lines[j].Position);

                    Assert.Null(bidiff.OldText.Lines[j].Text);
                    Assert.Equal(ChangeType.Imaginary, bidiff.OldText.Lines[j].Type);
                    Assert.False(bidiff.OldText.Lines[j].Position.HasValue);
                }
                
                Assert.True(bidiff.OldText.HasDifferences && bidiff.NewText.HasDifferences);
            }

            [Fact]
            public void Will_build_diffModel_when_newText_is_empty()
            {
                string textNew = "";
                string textOld = "z\ny\nx\nw\n";
                string[] textLinesNew = {};
                string[] textLinesOld = {"z", "y"};
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> {new DiffBlock(0, 2, 0, 0)}));
                var builder = new SideBySideDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Equal(2, bidiff.OldText.Lines.Count);
                Assert.Equal(2, bidiff.NewText.Lines.Count);

                for (int j = 0; j < textLinesOld.Length; j++)
                {
                    Assert.Equal(textLinesOld[j], bidiff.OldText.Lines[j].Text);
                    Assert.Equal(ChangeType.Deleted, bidiff.OldText.Lines[j].Type);
                    Assert.Equal(j + 1, bidiff.OldText.Lines[j].Position);

                    Assert.Null(bidiff.NewText.Lines[j].Text);
                    Assert.Equal(ChangeType.Imaginary, bidiff.NewText.Lines[j].Type);
                    Assert.False(bidiff.NewText.Lines[j].Position.HasValue);
                }
                
                Assert.True(bidiff.OldText.HasDifferences && bidiff.NewText.HasDifferences);
            }

            [Fact]
            public void Will_build_diffModel_for_unique_strings()
            {
                string textOld = "a\nb\nc\nd\n\n";
                string textNew = "z\ny\nx\nw\n";
                string[] textLinesOld = {"a", "b", "c", "d", ""};
                string[] textLinesNew = {"z", "y", "x", "w"};
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> {new DiffBlock(0, 5, 0, 4)}));
                differ.Setup(x => x.CreateDiffs(It.IsAny<string>(), It.IsAny<string>(), false, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(new string[0], new string[0], new List<DiffBlock>()));
                var builder = new SideBySideDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Equal(5, bidiff.OldText.Lines.Count);
                Assert.Equal(5, bidiff.NewText.Lines.Count);
                int i = 0;
                for (; i < Math.Min(textLinesOld.Length, textLinesNew.Length); i++)
                {
                    Assert.Equal(textLinesOld[i], bidiff.OldText.Lines[i].Text);
                    Assert.Equal(ChangeType.Modified, bidiff.OldText.Lines[i].Type);
                    Assert.Equal(i + 1, bidiff.OldText.Lines[i].Position);

                    Assert.Equal(textLinesNew[i], bidiff.NewText.Lines[i].Text);
                    Assert.Equal(ChangeType.Modified, bidiff.NewText.Lines[i].Type);
                    Assert.Equal(i + 1, bidiff.NewText.Lines[i].Position);
                }

                if (textLinesOld.Length < textLinesNew.Length)
                {
                    for (int j = i; j < textLinesNew.Length; j++)
                    {
                        Assert.Equal(textLinesNew[j], bidiff.NewText.Lines[j].Text);
                        Assert.Equal(ChangeType.Inserted, bidiff.NewText.Lines[j].Type);
                        Assert.Equal(j + 1, bidiff.NewText.Lines[j].Position);

                        Assert.Null(bidiff.OldText.Lines[j].Text);
                        Assert.Equal(ChangeType.Imaginary, bidiff.OldText.Lines[j].Type);
                        Assert.False(bidiff.OldText.Lines[j].Position.HasValue);
                    }
                }
                else
                {
                    for (int j = i; j < textLinesOld.Length; j++)
                    {
                        Assert.Equal(textLinesOld[j], bidiff.OldText.Lines[j].Text);
                        Assert.Equal(ChangeType.Deleted, bidiff.OldText.Lines[j].Type);
                        Assert.Equal(j + 1, bidiff.OldText.Lines[j].Position);

                        Assert.Null(bidiff.NewText.Lines[j].Text);
                        Assert.Equal(ChangeType.Imaginary, bidiff.NewText.Lines[j].Type);
                        Assert.False(bidiff.NewText.Lines[j].Position.HasValue);
                    }
                }
                
                Assert.True(bidiff.OldText.HasDifferences && bidiff.NewText.HasDifferences);
            }

            [Fact]
            public void Will_build_diffModel_for_partially_different_documents()
            {
                string textOld = "1\n2\na\nb\nc\nd\n\n";
                string textNew = "1\n2\nz\ny\nx\nw\n";
                string[] textLinesOld = {"1", "2", "a", "b", "c", "d", ""};
                string[] textLinesNew = {"1", "2", "z", "y", "x", "w"};
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> {new DiffBlock(2, 5, 2, 4)}));
                differ.Setup(x => x.CreateDiffs(It.IsAny<string>(), It.IsAny<string>(), false, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(new string[0], new string[0], new List<DiffBlock>()));
                var builder = new SideBySideDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Equal(7, bidiff.OldText.Lines.Count);
                Assert.Equal(7, bidiff.NewText.Lines.Count);
                int i = 0;
                for (; i < 2; i++)
                {
                    Assert.Equal(textLinesNew[i], bidiff.NewText.Lines[i].Text);
                    Assert.Equal(ChangeType.Unchanged, bidiff.NewText.Lines[i].Type);
                    Assert.Equal(i + 1, bidiff.NewText.Lines[i].Position);

                    Assert.Equal(textLinesOld[i], bidiff.OldText.Lines[i].Text);
                    Assert.Equal(ChangeType.Unchanged, bidiff.OldText.Lines[i].Type);
                    Assert.Equal(i + 1, bidiff.OldText.Lines[i].Position);
                }

                for (; i < Math.Min(textLinesOld.Length, textLinesNew.Length); i++)
                {
                    Assert.Equal(textLinesOld[i], bidiff.OldText.Lines[i].Text);
                    Assert.Equal(ChangeType.Modified, bidiff.OldText.Lines[i].Type);
                    Assert.Equal(i + 1, bidiff.OldText.Lines[i].Position);

                    Assert.Equal(textLinesNew[i], bidiff.NewText.Lines[i].Text);
                    Assert.Equal(ChangeType.Modified, bidiff.NewText.Lines[i].Type);
                    Assert.Equal(i + 1, bidiff.NewText.Lines[i].Position);
                }

                if (textLinesOld.Length < textLinesNew.Length)
                {
                    for (int j = i; j < textLinesNew.Length; j++)
                    {
                        Assert.Equal(textLinesNew[j], bidiff.NewText.Lines[j].Text);
                        Assert.Equal(ChangeType.Inserted, bidiff.NewText.Lines[j].Type);
                        Assert.Equal(j + 1, bidiff.NewText.Lines[j].Position);

                        Assert.Null(bidiff.OldText.Lines[j].Text);
                        Assert.Equal(ChangeType.Imaginary, bidiff.OldText.Lines[j].Type);
                        Assert.False(bidiff.OldText.Lines[j].Position.HasValue);
                    }
                }
                else
                {
                    for (int j = i; j < textLinesOld.Length; j++)
                    {
                        Assert.Equal(textLinesOld[j], bidiff.OldText.Lines[j].Text);
                        Assert.Equal(ChangeType.Deleted, bidiff.OldText.Lines[j].Type);
                        Assert.Equal(j + 1, bidiff.OldText.Lines[j].Position);

                        Assert.Null(bidiff.NewText.Lines[j].Text);
                        Assert.Equal(ChangeType.Imaginary, bidiff.NewText.Lines[j].Type);
                        Assert.False(bidiff.NewText.Lines[j].Position.HasValue);
                    }
                }
                
                Assert.True(bidiff.OldText.HasDifferences && bidiff.NewText.HasDifferences);
            }


            [Fact]
            public void Will_build_diffModel_for_partially_different_lines()
            {
                string textOld = "m is h";
                string textNew = "m ai is n h";
                string[] textLinesOld = {"m is h"};
                string[] textLinesNew = {"m ai is n h"};
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> {new DiffBlock(0, 1, 0, 1)}));
                differ.Setup(x => x.CreateDiffs(It.IsAny<string>(), It.IsAny<string>(), false, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(
                                 new[] {"m ", "is ", "h"},
                                 new[] {"m ", "ai ", "is ", "n ", "h"},
                                 new List<DiffBlock> {new DiffBlock(1, 0, 1, 1), new DiffBlock(3, 0, 3, 1)}));
                var builder = new SideBySideDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Single(bidiff.OldText.Lines);
                Assert.Single(bidiff.NewText.Lines);
                Assert.Equal(ChangeType.Unchanged, bidiff.NewText.Lines[0].SubPieces[0].Type);
                Assert.Equal(ChangeType.Inserted, bidiff.NewText.Lines[0].SubPieces[1].Type);
                Assert.Equal(ChangeType.Unchanged, bidiff.NewText.Lines[0].SubPieces[2].Type);
                Assert.Equal(ChangeType.Inserted, bidiff.NewText.Lines[0].SubPieces[3].Type);
                Assert.Equal(ChangeType.Unchanged, bidiff.NewText.Lines[0].SubPieces[4].Type);

                Assert.Equal(ChangeType.Unchanged, bidiff.OldText.Lines[0].SubPieces[0].Type);
                Assert.Equal(ChangeType.Imaginary, bidiff.OldText.Lines[0].SubPieces[1].Type);
                Assert.Equal(ChangeType.Unchanged, bidiff.OldText.Lines[0].SubPieces[2].Type);
                Assert.Equal(ChangeType.Imaginary, bidiff.OldText.Lines[0].SubPieces[3].Type);
                Assert.Equal(ChangeType.Unchanged, bidiff.OldText.Lines[0].SubPieces[4].Type);
                
                Assert.True(bidiff.OldText.HasDifferences && bidiff.NewText.HasDifferences);
            }

            [Fact]
            public void Will_ignore_whitespace_by_default_1()
            {
                string textOld = "1\n 2\n3 \n 4 \n5";
                string textNew = "1\n2\n3\n4\n5";
                var builder = new SideBySideDiffBuilder(new Differ());
                var model = builder.BuildDiffModel(textOld, textNew);
                Assert.Equal(
                    model.OldText.Lines,
                    new DiffPiece[]
                    {
                        new DiffPiece("1", ChangeType.Unchanged, 1),
                        new DiffPiece(" 2", ChangeType.Unchanged, 2),
                        new DiffPiece("3 ", ChangeType.Unchanged, 3),
                        new DiffPiece(" 4 ", ChangeType.Unchanged, 4),
                        new DiffPiece("5", ChangeType.Unchanged, 5),
                    });
                Assert.Equal(
                    model.NewText.Lines,
                    new DiffPiece[]
                    {
                        new DiffPiece("1", ChangeType.Unchanged, 1),
                        new DiffPiece("2", ChangeType.Unchanged, 2),
                        new DiffPiece("3", ChangeType.Unchanged, 3),
                        new DiffPiece("4", ChangeType.Unchanged, 4),
                        new DiffPiece("5", ChangeType.Unchanged, 5),
                    });
            }

            [Fact]
            public void Will_ignore_whitespace_by_default_2()
            {
                string textOld = "1\n2\n3\n4\n5";
                string textNew = "1\n 2\n3 \n 4 \n5";
                var builder = new SideBySideDiffBuilder(new Differ());
                var model = builder.BuildDiffModel(textOld, textNew);
                Assert.Equal(
                    model.OldText.Lines,
                    new DiffPiece[]
                    {
                        new DiffPiece("1", ChangeType.Unchanged, 1),
                        new DiffPiece("2", ChangeType.Unchanged, 2),
                        new DiffPiece("3", ChangeType.Unchanged, 3),
                        new DiffPiece("4", ChangeType.Unchanged, 4),
                        new DiffPiece("5", ChangeType.Unchanged, 5),
                    });
                Assert.Equal(
                    model.NewText.Lines,
                    new DiffPiece[]
                    {
                        new DiffPiece("1", ChangeType.Unchanged, 1),
                        new DiffPiece(" 2", ChangeType.Unchanged, 2),
                        new DiffPiece("3 ", ChangeType.Unchanged, 3),
                        new DiffPiece(" 4 ", ChangeType.Unchanged, 4),
                        new DiffPiece("5", ChangeType.Unchanged, 5),
                    });
            }

            [Fact]
            public void Will_ignore_whitespace_by_default_3()
            {
                string textOld = "1\n 2\n3 \n 4 \n5";
                string textNew = "1\n2\n3\n4\n5";
                var builder = new SideBySideDiffBuilder(new Differ());
                var model = builder.BuildDiffModel(textOld, textNew, ignoreWhitespace: true);
                Assert.Equal(
                    model.OldText.Lines,
                    new DiffPiece[]
                    {
                        new DiffPiece("1", ChangeType.Unchanged, 1),
                        new DiffPiece(" 2", ChangeType.Unchanged, 2),
                        new DiffPiece("3 ", ChangeType.Unchanged, 3),
                        new DiffPiece(" 4 ", ChangeType.Unchanged, 4),
                        new DiffPiece("5", ChangeType.Unchanged, 5),
                    });
                Assert.Equal(
                    model.NewText.Lines,
                    new DiffPiece[]
                    {
                        new DiffPiece("1", ChangeType.Unchanged, 1),
                        new DiffPiece("2", ChangeType.Unchanged, 2),
                        new DiffPiece("3", ChangeType.Unchanged, 3),
                        new DiffPiece("4", ChangeType.Unchanged, 4),
                        new DiffPiece("5", ChangeType.Unchanged, 5),
                    });
            }

            [Fact]
            public void Can_compare_whitespace()
            {
                string textOld = "1\n 2\n3 \n 4 \n5";
                string textNew = "1\n2\n3\n4\n5";
                var builder = new SideBySideDiffBuilder(new Differ());
                var model = builder.BuildDiffModel(textOld, textNew, ignoreWhitespace: false);
                Assert.Equal(
                    model.OldText.Lines,
                    new DiffPiece[]
                    {
                        new DiffPiece("1", ChangeType.Unchanged, 1),
                        new DiffPiece(" 2", ChangeType.Modified, 2)
                        {
                            SubPieces =
                            {
                                new DiffPiece("", ChangeType.Deleted, 1),
                                new DiffPiece(" ", ChangeType.Deleted, 2),
                                new DiffPiece("2", ChangeType.Unchanged, 3),
                            },
                        },
                        new DiffPiece("3 ", ChangeType.Modified, 3)
                        {
                            SubPieces =
                            {
                                new DiffPiece("3", ChangeType.Unchanged, 1),
                                new DiffPiece(" ", ChangeType.Deleted, 2),
                            },
                        },
                        new DiffPiece(" 4 ", ChangeType.Modified, 4)
                        {
                            SubPieces =
                            {
                                new DiffPiece("", ChangeType.Deleted, 1),
                                new DiffPiece(" ", ChangeType.Deleted, 2),
                                new DiffPiece("4", ChangeType.Unchanged, 3),
                                new DiffPiece(" ", ChangeType.Deleted, 4),
                            },
                        },
                        new DiffPiece("5", ChangeType.Unchanged, 5),
                    });
                Assert.Equal(
                    model.NewText.Lines,
                    new DiffPiece[]
                    {
                        new DiffPiece("1", ChangeType.Unchanged, 1),
                        new DiffPiece("2", ChangeType.Modified, 2)
                        {
                            SubPieces =
                            {
                                new DiffPiece(null, ChangeType.Imaginary),
                                new DiffPiece(null, ChangeType.Imaginary),
                                new DiffPiece("2", ChangeType.Unchanged, 1),
                            },
                        },
                        new DiffPiece("3", ChangeType.Modified, 3)
                        {
                            SubPieces =
                            {
                                new DiffPiece("3", ChangeType.Unchanged, 1),
                                new DiffPiece(null, ChangeType.Imaginary),
                            },
                        },
                        new DiffPiece("4", ChangeType.Modified, 4)
                        {
                            SubPieces =
                            {
                                new DiffPiece(null, ChangeType.Imaginary),
                                new DiffPiece(null, ChangeType.Imaginary),
                                new DiffPiece("4", ChangeType.Unchanged, 1),
                                new DiffPiece(null, ChangeType.Imaginary),
                            },
                        },
                        new DiffPiece("5", ChangeType.Unchanged, 5),
                    });
            }
        }
    }
}