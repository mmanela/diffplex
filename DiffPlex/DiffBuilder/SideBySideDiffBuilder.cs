using System;
using System.Collections.Generic;
using System.Linq;
using DiffPlex.Chunkers;
using DiffPlex.DiffBuilder.Model;
using DiffPlex.Model;

namespace DiffPlex.DiffBuilder
{
    public class SideBySideDiffBuilder : ISideBySideDiffBuilder
    {
        private readonly IDiffer differ;
        private readonly IChunker lineChunker;
        private readonly IChunker wordChunker;

        private delegate ChangeType PieceBuilder(string oldText, string newText, List<DiffPiece> oldPieces, List<DiffPiece> newPieces, bool ignoreWhitespace, bool ignoreCase);

        /// <summary>
        /// Gets the default singleton instance.
        /// </summary>
        public static SideBySideDiffBuilder Instance { get; } = new SideBySideDiffBuilder();

        public SideBySideDiffBuilder(IDiffer differ, IChunker lineChunker, IChunker wordChunker)
        {
            this.differ = differ ?? Differ.Instance;
            this.lineChunker = lineChunker ?? throw new ArgumentNullException(nameof(lineChunker));
            this.wordChunker = wordChunker ?? throw new ArgumentNullException(nameof(wordChunker));
        }

        public SideBySideDiffBuilder(IDiffer differ = null) :
            this(differ, new LineChunker(), new WordChunker())
        {
        }

        public SideBySideDiffBuilder(IDiffer differ, char[] wordSeparators)
            : this(differ, new LineChunker(), new DelimiterChunker(wordSeparators))
        {
        }

        public SideBySideDiffModel BuildDiffModel(string oldText, string newText)
            => BuildDiffModel(oldText, newText, ignoreWhitespace: true);

        public SideBySideDiffModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace) => BuildDiffModel(
                oldText,
                newText,
                ignoreWhitespace,
                false);

        public SideBySideDiffModel BuildDiffModel(string oldText, string newText, bool ignoreWhitespace, bool ignoreCase)
        {
            return BuildLineDiff(
                oldText ?? throw new ArgumentNullException(nameof(oldText)),
                newText ?? throw new ArgumentNullException(nameof(newText)),
                ignoreWhitespace,
                ignoreCase);

            return DiffPaneModelBuilder.BuildDiffModel(this.differ, oldText, newText, ignoreWhitespace, ignoreCase, lineChunker, wordChunker);   
        }

        /// <summary>
        /// Gets the side-by-side textual diffs.
        /// </summary>
        /// <param name="oldText">The old text to diff.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhiteSpace"><see langword="true"/> if ignore the white space; otherwise, <see langword="false"/>.</param>
        /// <param name="ignoreCase"><see langword="true"/> if case-insensitive; otherwise, <see langword="false"/>.</param>
        /// <returns>The diffs result.</returns>
        public static SideBySideDiffModel Diff(string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false)
        { 
            return BuildDiffModel.BuildDiffModel(Differ.Instance, oldText, newText, ignoreWhiteSpace, ignoreCase, LineChunker.Instance, WordChunker.Instance);
        }

        /// <summary>
        /// Gets the side-by-side textual diffs.
        /// </summary>
        /// <param name="differ">The differ instance.</param>
        /// <param name="oldText">The old text to diff.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="ignoreWhiteSpace"><see langword="true"/> if ignore the white space; otherwise, <see langword="false"/>.</param>
        /// <param name="ignoreCase"><see langword="true"/> if case-insensitive; otherwise, <see langword="false"/>.</param>
        /// <param name="lineChunker">The line chunker.</param>
        /// <param name="wordChunker">The word chunker.</param>
        /// <returns>The diffs result.</returns>
        public static SideBySideDiffModel Diff(IDiffer differ, string oldText, string newText, bool ignoreWhiteSpace = true, bool ignoreCase = false, IChunker lineChunker = null, IChunker wordChunker = null)
        {
           return BuildDiffModel.BuildDiffModel(differ, oldText, newText, ignoreWhiteSpace, ignoreCase, lineChunker, wordChunker);
        }
    }
}