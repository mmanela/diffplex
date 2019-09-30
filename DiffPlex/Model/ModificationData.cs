namespace DiffPlex.Model
{
    public class ModificationData
    {
        private static readonly int[] EmptyHashedPieces = { };
        private static readonly bool[] EmptyModifications = { };
        private static readonly string[] EmptyPieces = { };

        public int[] HashedPieces { get; set; }

        public string RawData { get; }

        public bool[] Modifications { get; set; }

        public string[] Pieces { get; set; }

        public ModificationData(string str)
        {
            RawData = str;

            HashedPieces = EmptyHashedPieces;
            Modifications = EmptyModifications;
            Pieces = EmptyPieces;
        }
    }
}