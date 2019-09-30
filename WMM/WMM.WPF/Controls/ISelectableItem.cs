using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMM.WPF.Controls
{
    public interface ISelectableItem
    {
        bool IsSelected { get; set; }
        bool IsSelectable { get; }
        string Caption { get; }
    }
}
