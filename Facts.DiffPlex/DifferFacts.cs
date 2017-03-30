using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DiffPlex;
using DiffPlex.Model;
using Xunit;

namespace Facts.DiffPlex
{
    public class DifferFacts
    {
        public class CreateCustomDiffs
        {
            [Fact]
            public void Will_throw_if_oldText_is_null()
            {
                var differ = new TestableDiffer();

                var ex = Record.Exception(() => differ.CreateCustomDiffs(null, "someString", false, null)) as ArgumentNullException;

                Assert.NotNull(ex);
                Assert.Equal("oldText", ex.ParamName);
            }

            [Fact]
            public void Will_throw_if_newText_is_null()
            {
                var differ = new TestableDiffer();

                var ex = Record.Exception(() => differ.CreateCustomDiffs("someString", null, false, null)) as ArgumentNullException;

                Assert.NotNull(ex);
                Assert.Equal("newText", ex.ParamName);
            }

            [Fact]
            public void Will_throw_if_chunker_is_null()
            {
                var differ = new TestableDiffer();

                var ex = Record.Exception(() => differ.CreateCustomDiffs("someString", "otherString", false, null)) as ArgumentNullException;

                Assert.NotNull(ex);
                Assert.Equal("chunker", ex.ParamName);
            }
        }

        public class CreateLineDiffs
        {
            [Fact]
            public void Will_throw_if_oldText_is_null()
            {
                var differ = new TestableDiffer();

                var ex = Record.Exception(() => differ.CreateLineDiffs(null, "someString", false)) as ArgumentNullException;

                Assert.NotNull(ex);
                Assert.Equal("oldText", ex.ParamName);
            }

            [Fact]
            public void Will_throw_if_newText_is_null()
            {
                var differ = new TestableDiffer();

                var ex = Record.Exception(() => differ.CreateLineDiffs("someString", null, false)) as ArgumentNullException;

                Assert.NotNull(ex);
                Assert.Equal("newText", ex.ParamName);
            }

            [Fact]
            public void Will_return_list_of_length_zero_if_there_are_no_differences()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("matt\ncatt\nhat\n", "matt\ncatt\nhat\n", false);

                Assert.NotNull(res);
                Assert.Equal(0, res.DiffBlocks.Count);
            }

            [Fact]
            public void Will_return_correct_diff_block_when_oldText_is_empty_and_newText_is_non_empty()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("", "matt\npat\nhat\n", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(0, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_diff_block_when_newText_is_empty_and_oldText_is_non_empty()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("matt\npat\nhat\n", "", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(4, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(0, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_one_item_list_for_strings_with_one_difference()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("matt\ncat\nhat\n", "matt\npat\nhat\n", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(1, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(1, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(1, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_one_item_list_when_no_deletions()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("matt\ncat\nhat\n", "matt\ncat\ntat\nhat\n", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(0, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(2, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(1, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_one_item_list_when_no_insertions()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("matt\r\ncat\ntat\r\nhat\n", "matt\ncat\nhat\n", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(2, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_one_item_list_for_strings_with_multiple_difference()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("a\nb\nc\nd", "a\nb\ne\nf\ng\nh", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(2, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(2, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(2, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_two_item_list_for_strings_with_multiple_difference_non_consecutively()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("z\ra\nb\rc\nd", "x\nv\na\rb\ne\nf\ng\nh", false);

                Assert.NotNull(res);
                Assert.Equal(2, res.DiffBlocks.Count);

                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(2, res.DiffBlocks[0].InsertCountB);

                Assert.Equal(3, res.DiffBlocks[1].DeleteStartA);
                Assert.Equal(2, res.DiffBlocks[1].DeleteCountA);
                Assert.Equal(4, res.DiffBlocks[1].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[1].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_two_item_list_for_strings_with_multiple_difference_non_consecutively_and_ignoring_whitespace()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("z\t\na  \n b\nc\n\t\td", "x\nv\n a\nb\n e\nf\t\ng\nh\t\t", true);

                Assert.NotNull(res);
                Assert.Equal(2, res.DiffBlocks.Count);

                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(2, res.DiffBlocks[0].InsertCountB);

                Assert.Equal(3, res.DiffBlocks[1].DeleteStartA);
                Assert.Equal(2, res.DiffBlocks[1].DeleteCountA);
                Assert.Equal(4, res.DiffBlocks[1].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[1].InsertCountB);
            }

            [Fact]
            public void Will_ignore_case_when_ignore_case_is_true()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateLineDiffs("z\t\na  \n b\nc\n\t\td", "X\nV\n A\nB\n E\nF\t\nG\nH\t\t", true, true);

                Assert.NotNull(res);
                Assert.Equal(2, res.DiffBlocks.Count);

                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(2, res.DiffBlocks[0].InsertCountB);

                Assert.Equal(3, res.DiffBlocks[1].DeleteStartA);
                Assert.Equal(2, res.DiffBlocks[1].DeleteCountA);
                Assert.Equal(4, res.DiffBlocks[1].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[1].InsertCountB);
            }
        }


        public class CreateCharacterDiffs
        {
            [Fact]
            public void Will_throw_if_oldText_is_null()
            {
                var differ = new TestableDiffer();

                var ex = Record.Exception(() => differ.CreateCharacterDiffs(null, "someString", false)) as ArgumentNullException;

                Assert.NotNull(ex);
                Assert.Equal("oldText", ex.ParamName);
            }

            [Fact]
            public void Will_throw_if_newText_is_null()
            {
                var differ = new TestableDiffer();

                var ex = Record.Exception(() => differ.CreateCharacterDiffs("someString", null, false)) as ArgumentNullException;

                Assert.NotNull(ex);
                Assert.Equal("newText", ex.ParamName);
            }

            [Fact]
            public void Will_return_list_of_length_zero_if_there_are_no_differences()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateCharacterDiffs("abc defg", "abc defg", false);

                Assert.NotNull(res);
                Assert.Equal(0, res.DiffBlocks.Count);
            }

            [Fact]
            public void Will_return_correct_diff_block_when_oldText_is_empty_and_newText_is_non_empty()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateCharacterDiffs("", "ab c", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(0, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_diff_block_when_newText_is_empty_and_oldText_is_non_empty()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateCharacterDiffs("xy w", "", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(4, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(0, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_one_item_list_for_strings_with_one_difference()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateCharacterDiffs("xjzwv", "xyzwv", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(1, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(1, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(1, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_one_item_list_when_no_deletions()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateCharacterDiffs("abce", "ab ce", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(0, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(2, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(1, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_one_item_list_when_no_insertions()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateCharacterDiffs("ab ce", "abce", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(2, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_one_item_list_for_strings_with_multiple_difference()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateCharacterDiffs("abcd", "abefgh", false);

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(2, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(2, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(2, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_two_item_list_for_strings_with_multiple_difference_non_consecutively()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateCharacterDiffs("zabcd", "xvabefgh", false);

                Assert.NotNull(res);
                Assert.Equal(2, res.DiffBlocks.Count);

                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(2, res.DiffBlocks[0].InsertCountB);

                Assert.Equal(3, res.DiffBlocks[1].DeleteStartA);
                Assert.Equal(2, res.DiffBlocks[1].DeleteCountA);
                Assert.Equal(4, res.DiffBlocks[1].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[1].InsertCountB);
            }

            [Fact]
            public void Will_ignore_case_when_ignore_case_is_true()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateCharacterDiffs("zabcd", "XVABEFGH", false, true);

                Assert.NotNull(res);
                Assert.Equal(2, res.DiffBlocks.Count);

                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(2, res.DiffBlocks[0].InsertCountB);

                Assert.Equal(3, res.DiffBlocks[1].DeleteStartA);
                Assert.Equal(2, res.DiffBlocks[1].DeleteCountA);
                Assert.Equal(4, res.DiffBlocks[1].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[1].InsertCountB);
            }
        }

        public class CreateWordDiffs
        {
            [Fact]
            public void Will_throw_if_oldText_is_null()
            {
                var differ = new TestableDiffer();

                var ex = Record.Exception(() => differ.CreateWordDiffs(null, "someString", false, new[] { ' ' })) as ArgumentNullException;

                Assert.NotNull(ex);
                Assert.Equal("oldText", ex.ParamName);
            }

            [Fact]
            public void Will_throw_if_newText_is_null()
            {
                var differ = new TestableDiffer();

                var ex = Record.Exception(() => differ.CreateWordDiffs("someString", null, false, new[] { ' ' })) as ArgumentNullException;

                Assert.NotNull(ex);
                Assert.Equal("newText", ex.ParamName);
            }

            [Fact]
            public void Will_return_list_of_length_zero_if_there_are_no_differences()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateWordDiffs("abc defg", "abc defg", false, new[] { ' ' });

                Assert.NotNull(res);
                Assert.Equal(0, res.DiffBlocks.Count);
            }

            [Fact]
            public void Will_return_correct_diff_block_when_oldText_is_empty_and_newText_is_non_empty()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateWordDiffs("", "ab c", false, new[] { ' ' });

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(0, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(3, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_diff_block_when_newText_is_empty_and_oldText_is_non_empty()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateWordDiffs("xy w", "", false, new[] { ' ' });

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(3, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(0, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_one_item_list_for_strings_with_one_difference()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateWordDiffs("x j zwv", "x y zwv", false, new[] { ' ' });

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);
                Assert.Equal(2, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(2, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(1, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_two_item_list_for_strings_with_multiple_difference_non_consecutively()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateWordDiffs("z a b c d  ", "x v a b e f g h  ", false, new[] { ' ' });

                Assert.NotNull(res);
                Assert.Equal(4, res.DiffBlocks.Count);

                Assert.Equal(0, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(0, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(3, res.DiffBlocks[0].InsertCountB);

                Assert.Equal(5, res.DiffBlocks[1].DeleteStartA);
                Assert.Equal(0, res.DiffBlocks[1].DeleteCountA);
                Assert.Equal(7, res.DiffBlocks[1].InsertStartB);
                Assert.Equal(4, res.DiffBlocks[1].InsertCountB);

                Assert.Equal(6, res.DiffBlocks[2].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[2].DeleteCountA);
                Assert.Equal(12, res.DiffBlocks[2].InsertStartB);
                Assert.Equal(1, res.DiffBlocks[2].InsertCountB);

                Assert.Equal(8, res.DiffBlocks[3].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[3].DeleteCountA);
                Assert.Equal(14, res.DiffBlocks[3].InsertStartB);
                Assert.Equal(1, res.DiffBlocks[3].InsertCountB);
            }


            [Fact]
            public void Will_ignore_case_when_ignore_case_is_true()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateWordDiffs("z a", "Z A", false, true, new[] { ' ' });

                Assert.NotNull(res);
                Assert.Equal(0, res.DiffBlocks.Count);
            }


            [Theory]
            [InlineData(' ')]
            [InlineData(';')]
            [InlineData(',')]
            [InlineData('-')]
            [InlineData('(')]
            public void Will_return_correct_diff_for_arbitrary_separators(char separator)
            {
                var differ = new TestableDiffer();

                var res = differ.CreateWordDiffs(string.Format("z{0}a{0}{0}", separator),
                                                 string.Format("z{0}v{0}{0}", separator),
                                                 false,
                                                 new[] { separator });

                Assert.NotNull(res);
                Assert.Equal(1, res.DiffBlocks.Count);

                Assert.Equal(2, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(2, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(1, res.DiffBlocks[0].InsertCountB);
            }

            [Fact]
            public void Will_return_correct_diff_for_different_separators()
            {
                var differ = new TestableDiffer();

                var res = differ.CreateWordDiffs(string.Format("z{0}a{0}{0}", ' '), string.Format("z{0}v{0}{0}", ';'), false, new[] { ' ', ';' });

                Assert.NotNull(res);
                Assert.Equal(2, res.DiffBlocks.Count);

                Assert.Equal(1, res.DiffBlocks[0].DeleteStartA);
                Assert.Equal(3, res.DiffBlocks[0].DeleteCountA);
                Assert.Equal(1, res.DiffBlocks[0].InsertStartB);
                Assert.Equal(3, res.DiffBlocks[0].InsertCountB);


                Assert.Equal(5, res.DiffBlocks[1].DeleteStartA);
                Assert.Equal(1, res.DiffBlocks[1].DeleteCountA);
                Assert.Equal(5, res.DiffBlocks[1].InsertStartB);
                Assert.Equal(1, res.DiffBlocks[1].InsertCountB);
            }
        }

        public class BuildModificationData
        {
            [Fact]
            public void Will_return_empty_modifications_for_empty_strings()
            {
                var differ = new TestableDiffer();
                var a = new ModificationData("");
                var b = new ModificationData("");
                a.HashedPieces = new int[0];
                b.HashedPieces = new int[0];
                a.Modifications = new bool[a.HashedPieces.Length];
                b.Modifications = new bool[b.HashedPieces.Length];

                differ.TestBuildModificationData(a, b);

                Assert.Equal(0, a.Modifications.Length);
                Assert.Equal(0, b.Modifications.Length);
            }

            [Fact]
            public void Will_return_all_modifications_for_empty_vs_non_empty_string()
            {
                var differ = new TestableDiffer();
                var a = new ModificationData("");
                var b = new ModificationData("cat\nhat\npat\nmatt");
                a.HashedPieces = new int[] { };
                b.HashedPieces = new[] { 1, 2, 3, 4 };
                a.Modifications = new bool[a.HashedPieces.Length];
                b.Modifications = new bool[b.HashedPieces.Length];

                differ.TestBuildModificationData(a, b);

                foreach (var mod in b.Modifications)
                {
                    Assert.True(mod);
                }
            }

            [Fact]
            public void Will_return_all_modifications_for_non_empty_vs_empty_string()
            {
                var differ = new TestableDiffer();
                var a = new ModificationData("cat\nhat\npat\nmatt");
                var b = new ModificationData("");
                a.HashedPieces = new[] { 1, 2, 3, 4 };
                b.HashedPieces = new int[] { };
                a.Modifications = new bool[a.HashedPieces.Length];
                b.Modifications = new bool[b.HashedPieces.Length];

                differ.TestBuildModificationData(a, b);

                foreach (var mod in a.Modifications)
                {
                    Assert.True(mod);
                }
            }

            [Fact]
            public void Will_return_no_modifications_for_same_strings()
            {
                var differ = new TestableDiffer();
                var a = new ModificationData("cat\nhat\npat\nmatt");
                var b = new ModificationData("cat\nhat\npat\nmatt");
                a.HashedPieces = new[] { 1, 2, 3, 4 };
                b.HashedPieces = new[] { 1, 2, 3, 4 };
                a.Modifications = new bool[a.HashedPieces.Length];
                b.Modifications = new bool[b.HashedPieces.Length];

                differ.TestBuildModificationData(a, b);

                foreach (var mod in a.Modifications)
                {
                    Assert.False(mod);
                }
                foreach (var mod in b.Modifications)
                {
                    Assert.False(mod);
                }
            }

            [Fact]
            public void Will_return_all_modifications_for_unique_strings()
            {
                var differ = new TestableDiffer();
                var a = new ModificationData("cat\nhat\npat\nmatt");
                var b = new ModificationData("door\nfloor\nbore\nmore");
                a.HashedPieces = new[] { 1, 2, 3, 4 };
                b.HashedPieces = new[] { 5, 6, 7, 8 };
                a.Modifications = new bool[a.HashedPieces.Length];
                b.Modifications = new bool[b.HashedPieces.Length];

                differ.TestBuildModificationData(a, b);

                foreach (var mod in a.Modifications)
                {
                    Assert.True(mod);
                }
                foreach (var mod in b.Modifications)
                {
                    Assert.True(mod);
                }
            }

            [Fact]
            public void Will_return_correct_modifications_two_partially_similar_strings()
            {
                var differ = new TestableDiffer();
                var a = new ModificationData("cat\nhat\npat\nmatt");
                var b = new ModificationData("cat\nmatt\ntac");
                a.HashedPieces = new[] { 1, 2, 3, 4 };
                b.HashedPieces = new[] { 1, 4, 5 };
                a.Modifications = new bool[a.HashedPieces.Length];
                b.Modifications = new bool[b.HashedPieces.Length];

                differ.TestBuildModificationData(a, b);

                Assert.False(a.Modifications[0]);
                Assert.True(a.Modifications[1]);
                Assert.True(a.Modifications[2]);
                Assert.False(a.Modifications[3]);

                Assert.False(b.Modifications[0]);
                Assert.False(b.Modifications[1]);
                Assert.True(b.Modifications[2]);
            }

            [Fact]
            public void Will_return_correct_modifications_for_strings_with_two_differences()
            {
                var differ = new TestableDiffer();
                var a = new ModificationData("cat\nfat\ntac");
                var b = new ModificationData("cat\nmatt\ntac");
                a.HashedPieces = new[] { 1, 2, 3 };
                b.HashedPieces = new[] { 1, 4, 3 };
                a.Modifications = new bool[a.HashedPieces.Length];
                b.Modifications = new bool[b.HashedPieces.Length];

                differ.TestBuildModificationData(a, b);

                Assert.False(a.Modifications[0]);
                Assert.True(a.Modifications[1]);
                Assert.False(a.Modifications[2]);

                Assert.False(b.Modifications[0]);
                Assert.True(b.Modifications[1]);
                Assert.False(b.Modifications[2]);
            }

            [Fact]
            public void Will_return_correct_modifications_for_strings_with_one_difference()
            {
                var differ = new TestableDiffer();
                var a = new ModificationData("matt\ncat\nhat\n");
                var b = new ModificationData("matt\ncat\ntat\nhat\n");
                a.HashedPieces = new[] { 1, 2, 3 };
                b.HashedPieces = new[] { 1, 2, 4, 3 };
                a.Modifications = new bool[a.HashedPieces.Length];
                b.Modifications = new bool[b.HashedPieces.Length];

                differ.TestBuildModificationData(a, b);

                Assert.False(a.Modifications[0]);
                Assert.False(a.Modifications[1]);
                Assert.False(a.Modifications[2]);

                Assert.False(b.Modifications[0]);
                Assert.False(b.Modifications[1]);
                Assert.True(b.Modifications[2]);
                Assert.False(b.Modifications[3]);
            }

            [Theory]
            [ClassData(typeof(TestingEditLengthGenerator))]
            public void Will_return_correct_modifications_count_for_random_data(int[] aLines, int[] bLines, int editLength)
            {
                var differ = new TestableDiffer();
                var a = new ModificationData("");
                var b = new ModificationData("");
                a.HashedPieces = aLines;
                b.HashedPieces = bLines;
                a.Modifications = new bool[aLines.Length];
                b.Modifications = new bool[bLines.Length];

                differ.TestBuildModificationData(a, b);

                int modCount = a.Modifications.Count(x => x == true) + b.Modifications.Count(x => x == true);

                Assert.Equal(editLength, modCount);
            }
        }

        public class CalculateEditLength
        {
            [Fact]
            public void Will_throw_if_arrays_are_null()
            {
                var differ = new TestableDiffer();
                int[] a = null;
                int[] b = null;

                var ex = Record.Exception(() => differ.TestCalculateEditLength(a, 0, 0, b, 0, 0)) as ArgumentNullException;

                Assert.NotNull(ex);
            }

            [Fact]
            public void Will_return_length_0_for_arrays()
            {
                var differ = new TestableDiffer();

                var res = differ.TestCalculateEditLength(new int[0], 0, 0, new int[0], 0, 0);

                Assert.Equal(0, res.EditLength);
            }

            [Fact]
            public void Will_return_length_of_a_if_b_is_empty()
            {
                var differ = new TestableDiffer();
                int[] a = { 1, 2, 3, 4, 5, 6, 7, 8 };
                int[] b = { };

                var res = differ.TestCalculateEditLength(a, 0, a.Length, b, 0, b.Length);

                Assert.Equal(a.Length, res.EditLength);
            }

            [Fact]
            public void Will_return_length_of_b_if_a_is_empty()
            {
                var differ = new TestableDiffer();
                int[] b = { 1, 2, 3, 4, 5, 6, 7, 8 };
                int[] a = { };

                var res = differ.TestCalculateEditLength(a, 0, a.Length, b, 0, b.Length);

                Assert.Equal(b.Length, res.EditLength);
            }

            [Fact]
            public void Will_return_correct_length_when_start_and_ends_are_changed()
            {
                var differ = new TestableDiffer();
                int[] b = { 1, 2, 3, 0, 5, 6, 7, 8 };
                int[] a = { 4, 2, 3, 4, 5, 6, 7, 9 };

                var res = differ.TestCalculateEditLength(a, 1, a.Length - 1, b, 1, b.Length - 1);

                Assert.Equal(2, res.EditLength);
            }

            [Fact]
            public void Will_return_snake_of_zero_length_for_unique_arrays()
            {
                var differ = new TestableDiffer();
                int[] a = { 1, 2, 3, 4, 5, 6, 7, 8 };
                int[] b = { 11, 12, 23, 54, 56 };

                var res = differ.TestCalculateEditLength(a, 0, a.Length, b, 0, b.Length);

                Assert.Equal(res.StartX, res.EndX);
                Assert.Equal(res.StartY, res.EndY);
                Assert.Equal(a.Length + b.Length, res.EditLength);
            }

            [Theory]
            [ClassData(typeof(TestingEditLengthGenerator))]
            public void Will_return_correct_edit_length_random_strings(int[] a, int[] b, int actualEditLength)
            {
                var differ = new TestableDiffer();

                var res = differ.TestCalculateEditLength(a, 0, a.Length, b, 0, b.Length);

                Assert.Equal(actualEditLength, res.EditLength);
            }
        }

        private class TestingEditLengthGenerator : IEnumerable<object[]>
        {
            private readonly int count;
            private readonly ArrayGenerator generator = new ArrayGenerator();

            public TestingEditLengthGenerator()
                : this(50)
            {
            }

            private TestingEditLengthGenerator(int count)
            {
                if (count < 0) throw new ArgumentNullException(nameof(count));

                this.count = count;
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                for (int i = 0; i < count; i++)
                {
                    int[] a, b;
                    int editLength;
                    generator.Generate(out a, out b, out editLength);

                    yield return new object[] { a, b, editLength };
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        private class ArrayGenerator
        {
            private readonly int minLength;
            private readonly int maxLength;
            private readonly Random random = new Random();

            public ArrayGenerator()
                : this(0, 1005)
            {
            }

            private ArrayGenerator(int minLength, int maxLength)
            {
                if (minLength < 0) throw new ArgumentNullException(nameof(minLength));
                if (maxLength < 0) throw new ArgumentNullException(nameof(maxLength));

                this.maxLength = maxLength;
                this.minLength = minLength;
            }

            public void Generate(out int[] aArr, out int[] bArr, out int editLength)
            {
                var a = new List<int>();
                var b = new List<int>();

                int aLength = random.Next(minLength, maxLength);
                int bLength = random.Next(minLength, maxLength);

                int commonLength = random.Next(0, Math.Min(aLength, bLength));
                editLength = (aLength + bLength) - 2 * commonLength;

                const int someInt = 1;
                for (int j = 0; j < commonLength; j++)
                {
                    a.Add(someInt);
                    b.Add(someInt);
                }

                while (a.Count < aLength)
                    a.Add(2);
                while (b.Count < bLength)
                    b.Add(3);
                Shuffle(a);
                Shuffle(b);

                aArr = a.ToArray();
                bArr = b.ToArray();
            }

            private void Shuffle(IList<int> arr)
            {
                for (int i = 0; i < arr.Count; i++)
                {
                    int temp = arr[i];
                    int rand = random.Next(0, arr.Count - 1);
                    arr[i] = arr[rand];
                    arr[rand] = temp;
                }
            }
        }

        private class TestableDiffer : Differ
        {
            public void TestBuildModificationData(ModificationData A, ModificationData B)
            {
                BuildModificationData(A, B);
            }

            public EditLengthResult TestCalculateEditLength(int[] A, int startA, int endA, int[] B, int startB, int endB)
            {
                return CalculateEditLength(A, startA, endA, B, startB, endB);
            }
        }
    }
}