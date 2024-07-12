using DiffPlex.Chunkers;
using Xunit;

namespace Facts.DiffPlex.Chunkers
{
    public class LineEndingsPreservingChunkerFacts
    {
        [Fact]
        public void should_split_lines_when_first_line_empty()
        {
            //ARRANGE
            var chunker = new LineEndingsPreservingChunker();
            var sampleText = "\r\nFirst\r\nSecond\r\nLast\r\n";

            //ACT
            var chunks = chunker.Chunk(sampleText);

            //ASSERT
            Assert.Equal(4, chunks.Count);
            Assert.Equal("\r\n", chunks[0]);
            Assert.Equal("First\r\n", chunks[1]);
            Assert.Equal("Second\r\n", chunks[2]);
            Assert.Equal("Last\r\n", chunks[3]);
        }

        [Fact]
        public void should_split_lines_when_last_does_not_end_with_lineending()
        {
            //ARRANGE
            var chunker = new LineEndingsPreservingChunker();
            var sampleText = "First\r\nSecond\r\nLast";

            //ACT
            var chunks = chunker.Chunk(sampleText);

            //ASSERT
            Assert.Equal(3, chunks.Count);
            Assert.Equal("First\r\n", chunks[0]);
            Assert.Equal("Second\r\n", chunks[1]);
            Assert.Equal("Last", chunks[2]);
        }

        [Fact]
        public void should_split_when_all_lines_are_empty()
        {
            //ARRANGE
            var chunker = new LineEndingsPreservingChunker();
            var sampleText = "\r\n\r\n\r\n";

            //ACT
            var chunks = chunker.Chunk(sampleText);

            //ASSERT
            Assert.Equal(3, chunks.Count);
            Assert.Equal("\r\n", chunks[0]);
            Assert.Equal("\r\n", chunks[1]);
            Assert.Equal("\r\n", chunks[2]);
        }

        [Fact]
        public void should_split_when_different_line_ending()
        {
            //ARRANGE
            var chunker = new LineEndingsPreservingChunker();
            var sampleText = "\r\nFirst\nSecond\rLast";

            //ACT
            var chunks = chunker.Chunk(sampleText);

            //ASSERT
            Assert.Equal(4, chunks.Count);
            Assert.Equal("\r\n", chunks[0]);
            Assert.Equal("First\n", chunks[1]);
            Assert.Equal("Second\r", chunks[2]);
            Assert.Equal("Last", chunks[3]);
        }
    }
}
