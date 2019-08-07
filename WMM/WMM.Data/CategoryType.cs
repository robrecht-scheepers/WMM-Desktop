using System;
using System.Collections.Generic;
using System.Text;

namespace WMM.Data
{
    public enum CategoryType
    {
        Exception = 0,  // exceptional expenses, not forcastable
        Monthly = 1,    // to be forcasted by a monthly mean value
        Daily = 2,      // to be forcasted by extrapolating a daily mean value 
        Recurring = 3   // forecast only based on recurring templates
    }
}
