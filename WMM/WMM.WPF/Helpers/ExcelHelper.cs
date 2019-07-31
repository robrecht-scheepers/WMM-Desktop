using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using WMM.Data;
using WMM.WPF.Forecast;
using WMM.WPF.Resources;

namespace WMM.WPF.Helpers
{
    public static class ExcelHelper
    {
        public static void OpenInExcel(IEnumerable<Transaction> transactions)
        {
            // check if Excel is installed
            if(Type.GetTypeFromProgID("Excel.Application") == null)
                throw new Exception(Captions.ExcelNotInstalled);

            var app = new Application();
            var workBook = app.Workbooks.Add();
            Worksheet dataSheet = workBook.Sheets[1];
            dataSheet.Name = "Data";

            // write the data
            var data = new List<object[]> { new object[] {Captions.Date, Captions.Area, Captions.Category, Captions.Amount, Captions.Comment, Captions.Recurring, Captions.CategoryType} };
            foreach (var t in transactions)
            {
                data.Add(new object[]{t.Date, t.Category.Area, t.Category, t.Amount, t.Comments, t.Recurring, t.Category.ForecastType.ToCaption()});
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

        public static void OpenInExcel(IEnumerable<ForecastLineGroup> monthForecasts, IEnumerable<ForecastLineGroup> generalForecasts)
        {
            // check if Excel is installed
            if (Type.GetTypeFromProgID("Excel.Application") == null)
                throw new Exception(Captions.ExcelNotInstalled);

            var app = new Application();
            var workBook = app.Workbooks.Add();

            // first sheet: current month forecast

            Worksheet dataSheetCurrentMonth = workBook.Sheets[1];
            dataSheetCurrentMonth.Name = Captions.CurrentMonth;

            // write the data
            var data = new List<object[]> { new object[] { Captions.Area, Captions.Category, Captions.Current, Captions.Difference, Captions.Forecast } };
            foreach (var areaForecast in monthForecasts.OrderBy(x => x.Name))
            {
                var area = areaForecast.Name;
                foreach (var fl in areaForecast.ForecastLines)
                {
                    data.Add(new object[] { area, fl.Name, fl.CurrentAmount, fl.Difference, fl.ForecastAmount });
                }
            }

            for (var i = 0; i < data.Count; i++)
                for (var j = 0; j < data[0].Length; j++)
                {
                    dataSheetCurrentMonth.Cells[i + 1, j + 1] = data[i][j];
                }

            // create and format an excel table
            var table = dataSheetCurrentMonth.ListObjects.Add();
            table.Range.EntireColumn.AutoFit();
            table.ListColumns[3].Range.NumberFormat = "0.00";
            table.ListColumns[4].Range.NumberFormat = "0.00";
            table.ListColumns[5].Range.NumberFormat = "0.00";
            table.ShowTotals = true;


            // second sheet: general forecast

            Worksheet dataSheetGeneral = workBook.Sheets[2];
            dataSheetGeneral.Name = Captions.General;

            // write the data
            data = new List<object[]> { new object[] { Captions.Area, Captions.Category, Captions.Forecast } };
            foreach (var areaForecast in generalForecasts.OrderBy(x => x.Name))
            {
                var area = areaForecast.Name;
                foreach (var fl in areaForecast.ForecastLines)
                {
                    data.Add(new object[] { area, fl.Name, fl.ForecastAmount });
                }
            }

            for (var i = 0; i < data.Count; i++)
            for (var j = 0; j < data[0].Length; j++)
            {
                dataSheetGeneral.Cells[i + 1, j + 1] = data[i][j];
            }

            // create and format an excel table
            table = dataSheetGeneral.ListObjects.Add();
            table.Range.EntireColumn.AutoFit();
            table.ListColumns[3].Range.NumberFormat = "0.00";
            table.ShowTotals = true;

            app.Visible = true;
        }
    }
}
