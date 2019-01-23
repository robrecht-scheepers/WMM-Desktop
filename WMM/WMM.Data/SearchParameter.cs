using System;
using System.Collections.Generic;
using System.Text;

namespace WMM.Data
{
    [Flags]
    public enum SearchParameter
    {
        Date = 1,
        Area = 2,
        Category = 4,
        Comments = 8,
        Amount = 16,
        Direction = 32
    }
}
