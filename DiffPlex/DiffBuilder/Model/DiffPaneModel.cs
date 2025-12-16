using System.Collections.Generic;
using System.Linq;

namespace DiffPlex.DiffBuilder.Model;

public class DiffPaneModel
{
    public List<DiffPiece> Lines { get; } = [];

    public bool HasDifferences => Lines.Any(x => x.Type != ChangeType.Unchanged);
}