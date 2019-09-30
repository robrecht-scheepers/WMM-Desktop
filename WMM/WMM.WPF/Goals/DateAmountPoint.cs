using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WMM.WPF.Goals
{
    public class DateAmountPoint
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }

        public DateAmountPoint(DateTime date, double amount)
        {
            Date = date;
            Amount = amount;
        }
    }
}
