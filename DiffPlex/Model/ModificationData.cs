using System.Collections.Generic;

namespace DiffPlex.Model;

public class ModificationData(string str)
{
    public int[] HashedPieces { get; set; }

    public string RawData { get; } = str;

    public bool[] Modifications { get; set; }

    public IReadOnlyList<string> Pieces { get; set; }
}