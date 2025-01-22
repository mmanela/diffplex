using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DiffPlex.Wpf.Controls
{
    public interface IDiffViewer
    {
        bool IsTextWrapEnabled { get; set; }
    }

}
