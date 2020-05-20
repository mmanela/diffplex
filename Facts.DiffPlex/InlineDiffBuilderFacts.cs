using System;
using System.Collections.Generic;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;
using Moq;
using Xunit;

namespace Facts.DiffPlex
{
    public class InlineDiffBuilderFacts
    {
        public class Constructor
        {
            [Fact]
            public void Will_not_throw_if_IDiffer_is_null()
            {
                var builder = new InlineDiffBuilder(null);
                Assert.NotNull(builder);
            }
        }

        public class BuildDiffModel
        {
            [Fact]
            public void Will_throw_is_OldText_is_null()
            {
                var differ = new Mock<IDiffer>();
                var builder = new InlineDiffBuilder(differ.Object);

                var ex = Record.Exception(() => builder.BuildDiffModel(null, "asd"));

                Assert.IsType<ArgumentNullException>(ex);
                var an = (ArgumentNullException)ex;
                Assert.Equal("oldText", an.ParamName);
            }

            [Fact]
            public void Will_throw_is_NewText_is_null()
            {
                var differ = new Mock<IDiffer>();
                var builder = new InlineDiffBuilder(differ.Object);

                var ex = Record.Exception(() => builder.BuildDiffModel("asa", null));

                Assert.IsType<ArgumentNullException>(ex);
                var an = (ArgumentNullException)ex;
                Assert.Equal("newText", an.ParamName);
            }

            [Fact]
            public void Will_build_diffModel_for_duplicate_strings()
            {
                string text = "a\nb\nc\nd\n\n";
                string[] textLines = { "a", "b", "c", "d", "" };
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(text, text, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLines, textLines, new List<DiffBlock>() { new DiffBlock(0, 0, 5, 0) }));
                var builder = new InlineDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(text, text);

                Assert.NotNull(bidiff);
                Assert.Equal(textLines.Length, bidiff.Lines.Count);
                for (int i = 0; i < textLines.Length; i++)
                {
                    Assert.Equal(textLines[i], bidiff.Lines[i].Text);
                    Assert.Equal(ChangeType.Unchanged, bidiff.Lines[i].Type);
                    Assert.Equal(i + 1, bidiff.Lines[i].Position);
                }

                Assert.False(bidiff.HasDifferences);
            }

            [Fact]
            public void Will_build_diffModel_when_oldText_is_empty()
            {
                string textOld = "";
                string textNew = "z\ny\nx\nw\n";
                string[] textLinesOld = { };
                string[] textLinesNew = { "z", "y" };
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> { new DiffBlock(0, 0, 0, 2) }));
                var builder = new InlineDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Equal(2, bidiff.Lines.Count);

                for (int j = 0; j < textLinesNew.Length; j++)
                {
                    Assert.Equal(textLinesNew[j], bidiff.Lines[j].Text);
                    Assert.Equal(ChangeType.Inserted, bidiff.Lines[j].Type);
                    Assert.Equal(j + 1, bidiff.Lines[j].Position);
                }
                
                Assert.True(bidiff.HasDifferences);
            }

            [Fact]
            public void Will_build_diffModel_when_newText_is_empty()
            {
                string textNew = "";
                string textOld = "z\ny\nx\nw\n";
                string[] textLinesNew = { };
                string[] textLinesOld = { "z", "y" };
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true,false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> { new DiffBlock(0, 2, 0, 0) }));
                var builder = new InlineDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Equal(2, bidiff.Lines.Count);

                for (int j = 0; j < textLinesOld.Length; j++)
                {
                    Assert.Equal(textLinesOld[j], bidiff.Lines[j].Text);
                    Assert.Equal(ChangeType.Deleted, bidiff.Lines[j].Type);
                    Assert.Null(bidiff.Lines[j].Position);
                }
                
                Assert.True(bidiff.HasDifferences);
            }

            [Fact]
            public void Will_build_diffModel_for_unique_strings()
            {
                string textOld = "a\nb\nc\nd\n\n";
                string textNew = "z\ny\nx\nw\n";
                string[] textLinesOld = { "a", "b", "c", "d", "" };
                string[] textLinesNew = { "z", "y", "x", "w" };
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> { new DiffBlock(0, 5, 0, 4) }));
                var builder = new InlineDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Equal(9, bidiff.Lines.Count);
                int i = 0;

                for (; i < textLinesOld.Length - 1; i++)
                {
                    Assert.Equal(textLinesOld[i], bidiff.Lines[i].Text);
                    Assert.Equal(ChangeType.Deleted, bidiff.Lines[i].Type);
                    Assert.Null(bidiff.Lines[i].Position);
                }

                for (var j = 0; j < textLinesNew.Length; j++)
                {
                    Assert.Equal(textLinesNew[j], bidiff.Lines[i].Text);
                    Assert.Equal(ChangeType.Inserted, bidiff.Lines[i].Type);
                    Assert.Equal(j + 1, bidiff.Lines[i].Position);
                    i++;
                }

                Assert.Equal("", bidiff.Lines[8].Text);
                Assert.Equal(ChangeType.Deleted, bidiff.Lines[8].Type);
                Assert.Null(bidiff.Lines[8].Position);
                Assert.True(bidiff.HasDifferences);
            }

            [Fact]
            public void Will_build_diffModel_for_middle_is_different_documents()
            {
                string textOld = "1\n2\na\nb\nc\nd\ne\f\n";
                string textNew = "1\n2\nz\ny\nx\nw\ne\f\n";
                string[] textLinesOld = { "1", "2", "a", "b", "c", "d", "e", "f" };
                string[] textLinesNew = { "1", "2", "z", "y", "x", "w", "e", "f" };
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> { new DiffBlock(2, 4, 2, 4) }));
                var builder = new InlineDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Equal(12, bidiff.Lines.Count);

                Assert.Equal("1", bidiff.Lines[0].Text);
                Assert.Equal(ChangeType.Unchanged, bidiff.Lines[0].Type);
                Assert.Equal(1, bidiff.Lines[0].Position);
                Assert.Equal("2", bidiff.Lines[1].Text);
                Assert.Equal(ChangeType.Unchanged, bidiff.Lines[1].Type);
                Assert.Equal(2, bidiff.Lines[1].Position);

                for (int i = 2; i <= 5; i++)
                {
                    Assert.Equal(textLinesOld[i], bidiff.Lines[i].Text);
                    Assert.Equal(ChangeType.Deleted, bidiff.Lines[i].Type);
                    Assert.Null(bidiff.Lines[i].Position);
                }

                for (int i = 6; i <= 9; i++)
                {
                    Assert.Equal(textLinesNew[i - 4], bidiff.Lines[i].Text);
                    Assert.Equal(ChangeType.Inserted, bidiff.Lines[i].Type);
                    Assert.Equal(i - 3, bidiff.Lines[i].Position);
                }

                Assert.Equal("e", bidiff.Lines[10].Text);
                Assert.Equal(ChangeType.Unchanged, bidiff.Lines[10].Type);
                Assert.Equal(7, bidiff.Lines[10].Position);
                Assert.Equal("f", bidiff.Lines[11].Text);
                Assert.Equal(ChangeType.Unchanged, bidiff.Lines[11].Type);
                Assert.Equal(8, bidiff.Lines[11].Position);
                Assert.True(bidiff.HasDifferences);
            }

            [Fact]
            public void Will_build_diffModel_for_multiple_diff_blocks()
            {
                string textOld = "1\n2\na\nb\nc\nd\ne\f\n";
                string textNew = "1\n2\nz\ny\nc\nw\ne\f\n";
                string[] textLinesOld = { "1", "2", "a", "b", "c", "d", "e", "f" };
                string[] textLinesNew = { "1", "2", "z", "y", "c", "w", "e", "f" };
                var differ = new Mock<IDiffer>();
                differ.Setup(x => x.CreateDiffs(textOld, textNew, true, false, It.IsNotNull<IChunker>()))
                    .Returns(new DiffResult(textLinesOld, textLinesNew, new List<DiffBlock> { new DiffBlock(2, 2, 2, 2), new DiffBlock(5, 1, 5, 1) }));
                var builder = new InlineDiffBuilder(differ.Object);

                var bidiff = builder.BuildDiffModel(textOld, textNew);

                Assert.NotNull(bidiff);
                Assert.Equal(11, bidiff.Lines.Count);

                Assert.Equal("1", bidiff.Lines[0].Text);
                Assert.Equal(ChangeType.Unchanged, bidiff.Lines[0].Type);
                Assert.Equal(1, bidiff.Lines[0].Position);
                Assert.Equal("2", bidiff.Lines[1].Text);
                Assert.Equal(ChangeType.Unchanged, bidiff.Lines[1].Type);
                Assert.Equal(2, bidiff.Lines[1].Position);
                Assert.Equal("a", bidiff.Lines[2].Text);
                Assert.Equal(ChangeType.Deleted, bidiff.Lines[2].Type);
                Assert.Null(bidiff.Lines[2].Position);
                Assert.Equal("b", bidiff.Lines[3].Text);
                Assert.Equal(ChangeType.Deleted, bidiff.Lines[3].Type);
                Assert.Null(bidiff.Lines[3].Position);
                Assert.Equal("z", bidiff.Lines[4].Text);
                Assert.Equal(ChangeType.Inserted, bidiff.Lines[4].Type);
                Assert.Equal(3, bidiff.Lines[4].Position);
                Assert.Equal("y", bidiff.Lines[5].Text);
                Assert.Equal(ChangeType.Inserted, bidiff.Lines[5].Type);
                Assert.Equal(4, bidiff.Lines[5].Position);
                Assert.Equal("c", bidiff.Lines[6].Text);
                Assert.Equal(ChangeType.Unchanged, bidiff.Lines[6].Type);
                Assert.Equal(5, bidiff.Lines[6].Position);
                Assert.Equal("d", bidiff.Lines[7].Text);
                Assert.Equal(ChangeType.Deleted, bidiff.Lines[7].Type);
                Assert.Null(bidiff.Lines[7].Position);
                Assert.Equal("w", bidiff.Lines[8].Text);
                Assert.Equal(ChangeType.Inserted, bidiff.Lines[8].Type);
                Assert.Equal(6, bidiff.Lines[8].Position);
                Assert.Equal("e", bidiff.Lines[9].Text);
                Assert.Equal(ChangeType.Unchanged, bidiff.Lines[9].Type);
                Assert.Equal(7, bidiff.Lines[9].Position);
                Assert.Equal("f", bidiff.Lines[10].Text);
                Assert.Equal(ChangeType.Unchanged, bidiff.Lines[10].Type);
                Assert.Equal(8, bidiff.Lines[10].Position);
                Assert.True(bidiff.HasDifferences);
            }

            [Fact]
            public void Will_ignore_whitespace_by_default_1()
            {
                string textOld = "1\n 2\n3 \n 4 \n5";
                string textNew = "1\n2\n3\n4\n5";
                var builder = new InlineDiffBuilder(new Differ());
                var model = builder.BuildDiffModel(textOld, textNew);
                Assert.Equal(
                    model.Lines,
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
                var builder = new InlineDiffBuilder(new Differ());
                var model = builder.BuildDiffModel(textOld, textNew);
                Assert.Equal(
                    model.Lines,
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
                var builder = new InlineDiffBuilder(new Differ());
                var model = builder.BuildDiffModel(textOld, textNew, ignoreWhitespace: true);
                Assert.Equal(
                    model.Lines,
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
                var builder = new InlineDiffBuilder(new Differ());
                var model = builder.BuildDiffModel(textOld, textNew, ignoreWhitespace: false);
                Assert.Equal(
                    model.Lines,
                    new DiffPiece[]
                    {
                        new DiffPiece("1", ChangeType.Unchanged, 1),
                        new DiffPiece(" 2", ChangeType.Deleted),
                        new DiffPiece("3 ", ChangeType.Deleted),
                        new DiffPiece(" 4 ", ChangeType.Deleted),
                        new DiffPiece("2", ChangeType.Inserted, 2),
                        new DiffPiece("3", ChangeType.Inserted, 3),
                        new DiffPiece("4", ChangeType.Inserted, 4),
                        new DiffPiece("5", ChangeType.Unchanged, 5),
                    });
            }
        }
    }
}
