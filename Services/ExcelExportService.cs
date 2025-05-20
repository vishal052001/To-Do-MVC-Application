using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using ToDoListAppMVC.Models;
using ToDoListAppMVC.Utilities;

namespace ToDoListAppMVC.Services
{
    public class ExcelExportService
    {
        public byte[] ExportToExcel(List<ToDoItem> items)
        {
            try
            {
                Logger.LogInfo("Starting the Excel export process", "ExcelExportService");

                // Ensure EPPlus is licensed
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("ToDo List");

                    // Add a Title Row
                    worksheet.Cells[1, 1].Value = "To-Do List Report";
                    worksheet.Cells[1, 1, 1, 3].Merge = true;
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[1, 1].Style.Font.Size = 16;
                    worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[1, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                    worksheet.Cells[1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Black);
                    worksheet.Cells[1, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    // Add headers
                    worksheet.Cells[2, 1].Value = "ID";
                    worksheet.Cells[2, 2].Value = "Title";
                    worksheet.Cells[2, 3].Value = "Is Completed";

                    // Apply header styles with background color
                    using (var headerRange = worksheet.Cells[2, 1, 2, 3])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Font.Size = 12;
                        headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        headerRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGray);
                        headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White); // White text
                        headerRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Add data with alternating row colors
                    for (int i = 0; i < items.Count; i++)
                    {
                        var rowIndex = i + 3;
                        worksheet.Cells[rowIndex, 1].Value = items[i].Id;
                        worksheet.Cells[rowIndex, 2].Value = items[i].Title;
                        worksheet.Cells[rowIndex, 3].Value = items[i].IsCompleted ? "Yes" : "No";

                        // Apply alternating row colors
                        var rowRange = worksheet.Cells[rowIndex, 1, rowIndex, 3];
                        rowRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        rowRange.Style.Fill.BackgroundColor.SetColor(i % 2 == 0
                            ? System.Drawing.Color.LightGray  // Even rows
                            : System.Drawing.Color.White);    // Odd rows
                        rowRange.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Auto-fit columns
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Set column width for better readability
                    worksheet.Column(1).Width = 10; // Adjust the width of the ID column
                    worksheet.Column(2).Width = 40; // Adjust the width of the Title column
                    worksheet.Column(3).Width = 15; // Adjust the width of the Is Completed column

                    Logger.LogInfo("Excel export process completed successfully", "ExcelExportService");
                    return package.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error occurred during the Excel export process", ex, "ExcelExportService");
                throw;
            }
        }
    }
}
