using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ExportSchedule
{
	public class TeamsExcelCreator
	{
		private ICellStyle _cellStyleDate;
		private ICellStyle _cellStyleLongDate;
		private ICellStyle _cellStyleGeneral;
		private ICellStyle _cellStyleDateBold;
		private ICellStyle _cellStyleGeneralBold;
		private ICellStyle _cellStyelGeneralSmall;
		private ICellStyle _cellStyleGeneralBoldWrapped;
		public byte[] CreateExcelFile(ScheduleExcelExportData input)
		{
			var workbook = new XSSFWorkbook();
			initializeCellStyles(workbook);

			var sheet = workbook.CreateSheet("schedule table");

			createHeaderInfo(sheet, input);
			createHeaderRowsColumns(sheet, input);
			createDataRows(sheet, input);
			setColumnWidth(sheet);
			sheet.CreateFreezePane(0, 9);
			using (var stream = new MemoryStream())
			{
				workbook.Write(stream);
				return stream.ToArray();
			}
		}

		private void setColumnWidth(ISheet sheet)
		{
			sheet.GetRow(7).Cells.ForEach(x => sheet.SetColumnWidth(x.ColumnIndex, 20 * 256));
		}

		private void createDataRows(ISheet sheet, ScheduleExcelExportData input)
		{
			foreach (var rowData in input.PersonRows)
			{
				var rows = createDataRow(rowData);
				addRow(sheet , rows);
			}
		}

		private CellData[] createDataRow(PersonRow data)
		{
			var rowCells = new List<CellData>
			{
				new CellData(data.Name, _cellStyleGeneral),
				new CellData(data.EmploymentNumber, _cellStyleGeneral),
				new CellData(data.SiteNTeam, _cellStyleGeneral)
			};
			foreach (var optionalColumn in data.OptionalColumns)
			{
				rowCells.Add(new CellData(optionalColumn, _cellStyleGeneral));
			}
			foreach (var day in data.ScheduleDaySummarys)
			{
				rowCells.Add(new CellData(day.Summary, _cellStyleGeneral));
			}

			return rowCells.ToArray();
		}

		private void createHeaderInfo(ISheet sheet, ScheduleExcelExportData data)
		{
			addRow(sheet, new[]
			{
				new CellData(Resources.SelectedGroups, _cellStyleGeneralBold),
				new CellData(data.SelectedGroups),
				new CellData(""),
				new CellData(""),
				new CellData(""),
				new CellData(""),
				new CellData(""),
				new CellData(""),
				new CellData(""),
				new CellData(""),
				new CellData(Resources.ReportGeneratedTime, _cellStyelGeneralSmall),
				new CellData(""),
				new CellData(DateTime.Now, _cellStyleLongDate),
			});

			var mergedRegion = new NPOI.SS.Util.CellRangeAddress(0, 0, 10, 11);
			sheet.AddMergedRegion(mergedRegion);

			addRow(sheet, new[]
			{
				new CellData(Resources.From, _cellStyleGeneralBold),
				new CellData(data.DateFrom, _cellStyleDate)
			});
			addRow(sheet, new[]
			{
				new CellData(Resources.To, _cellStyleGeneralBold),
				new CellData(data.DateTo, _cellStyleDate)
			});
			addRow(sheet, new[]
			{
				new CellData(Resources.Scenario, _cellStyleGeneralBold),
				new CellData(data.Scenario)
			});
			addRow(sheet, new[]
			{
				
				new CellData(Resources.OptionalColumn, _cellStyleGeneralBold),
				new CellData(string.Join(",", data.OptionalColumns))
			});
			addRow(sheet, new[]
			{
				new CellData(Resources.TimeZone, _cellStyleGeneralBold),
				new CellData(data.Timezone)
			});
			addRow(sheet, new[]
			{
				new CellData("")
			});

		}

		private void createHeaderRowsColumns(ISheet sheet, ScheduleExcelExportData data)
		{
			var dayHeaderCells = new List<CellData>
			{
				new CellData(""),
				new CellData(""),
				new CellData("")
			};
			
			var columnHeaderCells = new List<CellData>
			{
				new CellData(Resources.Name, _cellStyleGeneralBoldWrapped),
				new CellData(Resources.EmploymentNumber, _cellStyleGeneralBoldWrapped),
				new CellData(Resources.SiteTeam, _cellStyleGeneralBoldWrapped)
			};
			foreach (var column in data.OptionalColumns)
			{
				dayHeaderCells.Add(new CellData(Resources.OptionalColumn, _cellStyleGeneralBoldWrapped));
				columnHeaderCells.Add(new CellData(column, _cellStyleGeneralBoldWrapped));
			}
			var period = new DateOnlyPeriod(new DateOnly(data.DateFrom), new DateOnly(data.DateTo));
			foreach (var day in period.DayCollection())
			{
				dayHeaderCells.Add(new CellData(CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(
					day.DayOfWeek), _cellStyleGeneralBoldWrapped));
				columnHeaderCells.Add(new CellData(day.Date, _cellStyleDateBold));
			}
			addRow(sheet, dayHeaderCells);
			addRow(sheet, columnHeaderCells);
		}

		private void initializeCellStyles(XSSFWorkbook workbook)
		{
			_cellStyleLongDate = workbook.CreateCellStyle();
			_cellStyleLongDate.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy h:mm");
			var smallfont = workbook.CreateFont();
			smallfont.FontName = "Segoe UI";
			smallfont.FontHeightInPoints = 8;
			smallfont.Color = HSSFColor.Grey40Percent.Index;
			_cellStyleLongDate.SetFont(smallfont);

			_cellStyleDate = workbook.CreateCellStyle();
			_cellStyleDate.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy");

			_cellStyleDateBold = workbook.CreateCellStyle();
			_cellStyleDateBold.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy");
			var bold = workbook.CreateFont();
			bold.Boldweight = (short)FontBoldWeight.Bold;
			_cellStyleDateBold.SetFont(bold);
			_cellStyleDateBold.Alignment = HorizontalAlignment.Left;

			_cellStyleGeneralBold = workbook.CreateCellStyle();
			_cellStyleGeneralBold.DataFormat = HSSFDataFormat.GetBuiltinFormat("General");
			_cellStyleGeneralBold.SetFont(bold);

			_cellStyelGeneralSmall = workbook.CreateCellStyle();
			_cellStyelGeneralSmall.SetFont(smallfont);
			_cellStyelGeneralSmall.Alignment = HorizontalAlignment.Right;

			_cellStyleGeneralBoldWrapped = workbook.CreateCellStyle();
			_cellStyleGeneralBoldWrapped.DataFormat = HSSFDataFormat.GetBuiltinFormat("General");
			_cellStyleGeneralBoldWrapped.SetFont(bold);
			_cellStyleGeneralBoldWrapped.WrapText = true;

			_cellStyleGeneral = workbook.CreateCellStyle();
			_cellStyleGeneral.DataFormat = HSSFDataFormat.GetBuiltinFormat("General");

		}

		private void addRow(ISheet sheet, IEnumerable<CellData> cellDataList)
		{
			var row = sheet.CreateRow(sheet.PhysicalNumberOfRows);

			foreach (var cellData in cellDataList)
			{
				var newCell = row.CreateCell(row.PhysicalNumberOfCells);

				if (cellData.CellValue is string)
				{
					newCell.SetCellValue(cellData.CellValue.ToString());
				}
				else if (cellData.CellValue is double d)
				{
					newCell.SetCellValue(d);
				}
				else if (cellData.CellValue is DateTime time)
				{
					newCell.SetCellValue(time);
				}

				if (cellData.SetCellStyle)
				{
					newCell.CellStyle = cellData.CellStyle;
				}
			}
		}

		private class CellData
		{
			public object CellValue { get; }
			public ICellStyle CellStyle { get; }
			public bool SetCellStyle { get; private set; }

			public CellData(object cellValue)
			{
				CellValue = cellValue;
				SetCellStyle = false;
			}

			public CellData(object cellValue, ICellStyle cellStyle)
			{
				CellValue = cellValue;
				SetCellStyle = true;
				CellStyle = cellStyle;
			}
		}
	}
}