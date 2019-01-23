using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using Microsoft.Office.Interop;
using Microsoft.Office.Interop.Excel;
using WMM.Data;

namespace WMM.WPF.Helpers
{
    public static class ExcelHelper
    {
        public static void OpenInExcel(IEnumerable<Transaction> transactions)
        {
            // check if Excel is installed
            if(Type.GetTypeFromProgID("Excel.Application") == null)
                throw new Exception("Excel ist nicht installiert");

            var app = new Application();
            var workBook = app.Workbooks.Add();
            Worksheet sheet = workBook.Sheets[1];

            // create header
            var data = new List<object[]>() { new object[] {"Datum", "Bereich", "Kategorie", "Betrag", "Kommentar", "Fix"} };
            foreach (var t in transactions)
            {
                data.Add(new object[]{t.Date, "", t.Category, t.Amount, t.Comments, t.Recurring});
            }

            for(var i = 0; i < data.Count; i++)
            for(var j = 0; j < data[0].Length; j++)
            {
                sheet.Cells[i + 1, j + 1] = data[i][j];
            }

            var table = sheet.ListObjects.Add();
            table.Range.EntireColumn.AutoFit();
            table.ListColumns[4].Range.NumberFormat = "0.00";

            app.Visible = true;

        }
    }
}
