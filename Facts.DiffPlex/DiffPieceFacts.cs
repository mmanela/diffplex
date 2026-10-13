using System.Collections.Generic;
using DiffPlex.DiffBuilder.Model;
using Xunit;

namespace Facts.DiffPlex
{
    public class DiffPieceFacts
    {
        public class ToStringMethod
        {
            [Fact]
            public void Will_include_position_sub_piece_count_type_and_text()
            {
                var piece = new DiffPiece("hello", ChangeType.Modified, 3);
                piece.SubPieces.Add(new DiffPiece("h", ChangeType.Unchanged, 1));
                piece.SubPieces.Add(new DiffPiece("ello", ChangeType.Inserted, 2));

                Assert.Equal("Pos:3      Subs:2      Modified         hello", piece.ToString());
            }

            [Fact]
            public void Will_show_dash_when_position_is_null()
            {
                var piece = new DiffPiece("text", ChangeType.Inserted);

                Assert.Equal("Pos:-      Subs:0      Inserted         text", piece.ToString());
            }

            [Fact]
            public void Will_not_throw_when_sub_pieces_and_text_are_null()
            {
                var piece = new DiffPiece { SubPieces = null };

                Assert.Equal("Pos:-      Subs:0      Imaginary        ", piece.ToString());
            }
        }
    }
}
