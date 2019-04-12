using System;
using System.Collections.Generic;
using System.Text;

namespace WMM.Data
{
    public class SearchConfiguration
    {
        public SearchConfiguration()
        {
            Parameters = SearchParameter.None;
        }

        public SearchParameter Parameters { get; set; }

        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Area { get; set; }
        public string CategoryName { get; set; }
        public string Comments { get; set; }
        public double Amount { get; set; }
        public bool TransactionDirectionPositive { get; set; }
    }
}
