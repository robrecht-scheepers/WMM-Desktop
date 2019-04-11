using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static void OpenInExcel(IEnumerable<Transaction> transactions, IRepository repository)
        {
            // check if Excel is installed
            if(Type.GetTypeFromProgID("Excel.Application") == null)
                throw new Exception("Excel ist nicht installiert");

            var app = new Application();
            var workBook = app.Workbooks.Add();
            Worksheet dataSheet = workBook.Sheets[1];
            dataSheet.Name = "Data";

            // write the data
            var categories = repository.GetCategories();
            var data = new List<object[]>() { new object[] {"Datum", "Bereich", "Kategorie", "Betrag", "Kommentar", "Fix"} };
            foreach (var t in transactions)
            {
                var area = categories.First(x => x.Name == t.Category).Area;
                data.Add(new object[]{t.Date, area, t.Category, t.Amount, t.Comments, t.Recurring});
            }

            for(var i = 0; i < data.Count; i++)
            for(var j = 0; j < data[0].Length; j++)
            {
                dataSheet.Cells[i + 1, j + 1] = data[i][j];
            }

            // create and format an excel table
            var table = dataSheet.ListObjects.Add();
            table.Range.EntireColumn.AutoFit();
            table.ListColumns[4].Range.NumberFormat = "0.00";

            //// create a pivot table
            //Worksheet pivotSheet = workBook.Sheets[2];
            //pivotSheet.Name = "Pivot";
            //var cache = workBook.PivotCaches().Add(XlPivotTableSourceType.xlDatabase, table.Range);
            //var pivotTable = cache.CreatePivotTable(pivotSheet.Range["A1"], "Pivot name");

            //pivotTable.SmallGrid = false;
            //pivotTable.ShowTableStyleRowStripes = true;
            //pivotTable.TableStyle2 = "PivotStyleLight1";

            //// configure pivotTable
            //var datum = pivotTable.PivotFields("Datum");
            //datum.Orientation = XlPivotFieldOrientation.xlColumnField;
            ////datum.DataRange.Group(Missing.Value, Missing.Value, Missing.Value,
            ////    new[] {false, false, false, false, true, false, true});

            app.Visible = true;

        }
    }
}
