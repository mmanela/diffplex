﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DiffPlex.DiffBuilder.Model
{
    public class DiffPaneModel
    {
        public List<DiffPiece> Chunks => Lines;

        public List<DiffPiece> Lines { get; }

        public bool HasDifferences
        {
            get { return Lines.Any(x => x.Type != ChangeType.Unchanged); }
        }

        public DiffPaneModel()
        {
            Lines = new List<DiffPiece>();
        }
    }
}