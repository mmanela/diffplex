namespace DiffPlex.Model
{
    /// <summary>
    /// A block of consecutive edits from A and/or B
    /// </summary>
    public class DiffBlock
    {
        /// <summary>
        /// Position where deletions in A begin
        /// </summary>
        public int DeleteStartA { get; private set; }

        /// <summary>
        /// The number of deletions in A
        /// </summary>
        public int DeleteCountA { get; private set; }

        /// <summary>
        /// Position where insertion in B begin
        /// </summary>
        public int InsertStartB { get; private set; }

        /// <summary>
        /// The number of insertions in B
        /// </summary>
        public int InsertCountB { get; private set; }


        public DiffBlock(int deleteStartA, int deleteCountA, int insertStartB, int insertCountB)
        {
            DeleteStartA = deleteStartA;
            DeleteCountA = deleteCountA;
            InsertStartB = insertStartB;
            InsertCountB = insertCountB;
        }
    }
}