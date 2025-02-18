using System;
using System.Collections.Generic;

namespace DiffPlex.Model;

public class ModificationData : ModificationDataInfo
{
    public ModificationData(string str)
    {
        RawData = str;
    }

    public string RawData { get; }
}

public class ModificationDataInfo
{
    public int[] HashedPieces { get; set; }

    public bool[] Modifications { get; set; }

    public IReadOnlyList<string> Pieces { get; set; }
}