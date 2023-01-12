using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Office.Interop.Excel;
using Application = Microsoft.Office.Interop.Excel.Application;
using DataTable = System.Data.DataTable;
using Sheets = DocumentFormat.OpenXml.Spreadsheet.Sheets;
using Workbook = DocumentFormat.OpenXml.Spreadsheet.Workbook;
using Worksheet = Microsoft.Office.Interop.Excel.Worksheet;

namespace AirlinePlanChanges_MailCenter.Extensions
{
    public static class EnumerableExtensions
    {
        /*
        public static DataTable AsDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
        */
        public static void ExportToExcel(this DataTable tbl, string excelFilePath)
        {
            var xlApplication = new Application { Visible = false };
            object misValue = Missing.Value;
            var xlWorkbooks = xlApplication.Workbooks;
            var xlWorkbook = xlWorkbooks.Add(misValue);
            var xlWorksheet = (Worksheet)xlWorkbook.Worksheets.Item[1];

            xlWorksheet.Cells[1, 1] = "ArtickelNr";
            xlWorksheet.Cells[1, 2] = "Name";
            xlWorksheet.Cells[1, 3] = "Preis";
            xlWorksheet.Cells[1, 4] = "Netto Preis";
            xlWorksheet.Cells[1, 5] = "Category";
            xlWorksheet.Cells[1, 6] = "Width";
            xlWorksheet.Cells[1, 7] = "Height";

            var xlCellRange =
                xlWorksheet.Range[xlWorksheet.Cells[1, 1], xlWorksheet.Cells[1 + tbl.Rows.Count, 7]];
            var values = new object[tbl.Rows.Count + 1, 7];
            var rows = 0;
            values[rows, 0] = "ArtickelNr";
            values[rows, 1] = "Name";
            values[rows, 2] = "Preis";
            values[rows, 3] = "Netto Preis";
            values[rows, 4] = "Category";
            values[rows, 5] = "Width";
            values[rows, 6] = "Height";
            foreach (DataRow dataRow in tbl.Rows)
            {
                rows++;
                values[rows, 0] = dataRow["ArtickelNumber"].ToString();
                values[rows, 1] = dataRow["Name"].ToString();
                values[rows, 2] = (decimal)dataRow["BruttoPrice"];
                values[rows, 3] = (decimal)dataRow["NettoPrice"];
                values[rows, 4] = dataRow["Category"].ToString();
                values[rows, 5] = (int)dataRow["Width"];
                values[rows, 6] = (int)dataRow["Height"];
            }

            xlCellRange.Value2 = values;
            xlWorkbook.SaveAs(excelFilePath, XlFileFormat.xlWorkbookDefault, misValue, misValue, misValue, misValue, XlSaveAsAccessMode.xlExclusive, misValue, misValue, misValue, misValue, misValue);
            xlWorkbook.Close(true, misValue, misValue);
            xlApplication.Quit();
            Marshal.ReleaseComObject(xlWorksheet);
            Marshal.ReleaseComObject(xlWorkbook);
            Marshal.ReleaseComObject(xlApplication);
            MessageBox.Show(@"Export completed", @"Export report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ExportToOxml(this DataTable tbl, string excelFilePath, bool newFile)
        {
            //Delete the file if it exists. 
            if (newFile && File.Exists(excelFilePath))
            {
                File.Delete(excelFilePath);
            }

            uint sheetId = 1; //Start at the first sheet in the Excel workbook.

            if (newFile)
            {
                //This is the first time of creating the excel file and the first sheet.
                // Create a spreadsheet document by supplying the filepath.
                // By default, AutoSave = true, Editable = true, and Type = xlsx.
                var spreadsheetDocument = SpreadsheetDocument.Create(excelFilePath, SpreadsheetDocumentType.Workbook);

                // Add a WorkbookPart to the document.
                var workbookpart = spreadsheetDocument.AddWorkbookPart();
                workbookpart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);
                // Add Sheets to the Workbook.
                var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
                // Append a new worksheet and associate it with the workbook.
                var sheet = new Sheet
                            {
                    Id = spreadsheetDocument.WorkbookPart.
                        GetIdOfPart(worksheetPart),
                    SheetId = sheetId,
                    Name = "Sheet" + sheetId
                };
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                sheets.Append(sheet);
                //Add Header Row.
                var headerRow = new Row();
                foreach (DataColumn column in tbl.Columns)
                {
                    var cell = new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(column.ColumnName)
                    };
                    headerRow.AppendChild(cell);
                }
                sheetData.AppendChild(headerRow);
                foreach (DataRow row in tbl.Rows)
                {
                    var newRow = new Row();
                    foreach (DataColumn col in tbl.Columns)
                    {
                        var cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(row[col].ToString())
                        };
                        newRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(newRow);
                }
                workbookpart.Workbook.Save();

                spreadsheetDocument.Close();
            }
            else
            {
                // Open the Excel file that we created before, and start to add sheets to it.
                var spreadsheetDocument = SpreadsheetDocument.Open(excelFilePath, true);

                var workbookpart = spreadsheetDocument.WorkbookPart;
                if (workbookpart.Workbook == null)
                    workbookpart.Workbook = new Workbook();

                var worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                worksheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);
                var sheets = spreadsheetDocument.WorkbookPart.Workbook.Sheets;

                if (sheets.Elements<Sheet>().Any())
                {
                    //Set the new sheet id
                    sheetId = sheets.Elements<Sheet>().Max(s => s.SheetId.Value) + 1;
                }
                else
                {
                    sheetId = 1;
                }

                // Append a new worksheet and associate it with the workbook.
                var sheet = new Sheet
                            {
                    Id = spreadsheetDocument.WorkbookPart.
                        GetIdOfPart(worksheetPart),
                    SheetId = sheetId,
                    Name = "Sheet" + sheetId
                };
                // ReSharper disable once PossiblyMistakenUseOfParamsMethod
                sheets.Append(sheet);

                //Add the header row here.
                var headerRow = new Row();

                foreach (DataColumn column in tbl.Columns)
                {
                    var cell = new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(column.ColumnName)
                    };
                    headerRow.AppendChild(cell);
                }
                sheetData.AppendChild(headerRow);

                foreach (DataRow row in tbl.Rows)
                {
                    var newRow = new Row();

                    foreach (DataColumn col in tbl.Columns)
                    {
                        var cell = new Cell
                        {
                            DataType = CellValues.String,
                            CellValue = new CellValue(row[col].ToString())
                        };
                        newRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(newRow);
                }
                workbookpart.Workbook.Save();

                // Close the document.
                spreadsheetDocument.Close();
            }
            MessageBox.Show(@"Export completed", @"Export report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ExportToCsv(this DataTable tbl, string csvFilePath = null)
        {
            var lines = new List<string>();

            /*var columnNames = tbl.Columns.Cast<DataColumn>().
                Select(column => column.ColumnName).
                ToArray();
            */

            string[] columnNames = { "ArtickelNumber", "Name", "BruttoPrice" };
            var header = string.Join(";", columnNames);
            lines.Add(header);

            var tblCopy = tbl.Copy();
            tblCopy.Columns.Remove("NettoPrice");
            tblCopy.Columns.Remove("Category");
            tblCopy.Columns.Remove("Width");
            tblCopy.Columns.Remove("Height");

            var valueLines = tblCopy.AsEnumerable()
                                    .Select(row => string.Join(";", row.ItemArray));
            lines.AddRange(valueLines);

            File.WriteAllLines(csvFilePath ?? throw new ArgumentNullException(nameof(csvFilePath)), lines);
            MessageBox.Show(@"Export completed", @"Export report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
