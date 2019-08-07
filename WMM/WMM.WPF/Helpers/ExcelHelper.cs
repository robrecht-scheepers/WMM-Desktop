using Microsoft.Office.Interop.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WMM.Data;
using WMM.WPF.Forecast;
using WMM.WPF.Resources;
using ListObject = Microsoft.Office.Interop.Excel.ListObject;

namespace WMM.WPF.Helpers
{
    public static class ExcelHelper
    {
        public static void OpenInExcel(IEnumerable<Transaction> transactions)
        {
            var dateCaption = Captions.Date;
            var areaCaption = Captions.Area;
            var categoryCaption = Captions.Category;
            var amountCaption = Captions.Amount;

            // check if Excel is installed
            if (Type.GetTypeFromProgID("Excel.Application") == null)
                throw new Exception(Captions.ExcelNotInstalled);

            var app = new Application();
            var workBook = app.Workbooks.Add(@"c:\TEMP\WMM\Excel\WMM_file.xlsx");
            var dataSheet = workBook.Worksheets["Data"];
            ListObject dataTable = null;
            Range tableRange = null;
            foreach (ListObject table in dataSheet.ListObjects)
            {
                dataTable = table;
                tableRange = table.Range;
                break;
            }
            if(tableRange == null)
                return; // TODO: better handling?

            // write the data
            var data = new List<object[]>();// { new object[] { dateCaption, areaCaption, categoryCaption, amountCaption, Captions.Comment, Captions.Recurring, Captions.CategoryType } };
            foreach (var t in transactions)
            {
                data.Add(new object[] { t.Date, t.Category.Area, t.Category.Area, t.Category, t.Amount, t.Comments, t.Recurring, t.Category.CategoryType.ToCaption() });
            }
            for (var i = 0; i < data.Count; i++)
                for (var j = 0; j < data[0].Length; j++)
                {
                    dataSheet.Cells[i + 2, j + 1] = data[i][j];
                }


            tableRange.EntireColumn.AutoFit();
            dataTable.ListColumns[4].Range.Style = "Currency";

            //// create a pivot table
            //Worksheet pivotSheet = workBook.Sheets[2];
            //pivotSheet.Name = "Pivot";
            //var cache = workBook.PivotCaches().Add(XlPivotTableSourceType.xlDatabase, table.Range);
            //var pivotTable = cache.CreatePivotTable(pivotSheet.Range["A1"], "Pivot name");

            ////pivotTable.SmallGrid = false;
            ////pivotTable.ShowTableStyleRowStripes = true;
            ////pivotTable.TableStyle2 = "PivotStyleLight1";

            //// configure pivotTable
            //var date = pivotTable.PivotFields(dateCaption);
            //date.Orientation = XlPivotFieldOrientation.xlColumnField;
            //var dateRange = (Range)date.DataRange;
            //dateRange.Name = "DateRange";
            //dateRange.Group(Missing.Value, Missing.Value, Missing.Value,
            //    new[] {false, false, false, false, true, false, true});

            //var area = pivotTable.PivotFields(areaCaption);
            //area.Orientation = XlPivotFieldOrientation.xlRowField;

            //var amount = pivotTable.PivotFields(amountCaption);
            //amount.Orientation = XlPivotFieldOrientation.xlDataField;

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
