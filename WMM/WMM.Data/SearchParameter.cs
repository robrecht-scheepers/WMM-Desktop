using System;
using System.Collections.Generic;
using System.Text;

namespace WMM.Data
{
    [Flags]
    public enum SearchParameter
    {
        None = 0,
        Date = 1,
        Area = 2,
        Category = 4,
        Comments = 8,
        Amount = 16,
        Direction = 32,
        Recurring = 64,
        CategoryType = 128
    }
}
