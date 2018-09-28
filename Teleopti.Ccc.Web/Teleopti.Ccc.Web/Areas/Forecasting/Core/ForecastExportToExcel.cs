using System;
using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Forecasting.Export.Web;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class ForecastExportToExcel
	{
		private ICellStyle _cellStyleTitle;
		private ICellStyle _cellStyleDate;
		private ICellStyle _cellStylePercentage;
		private ICellStyle _cellStyleTwoDecimals;
		private ICellStyle _cellStyleTime;

		public byte[] Export(ForecastExportModel exportModel)
		{
			var workbook = new XSSFWorkbook();
			initializeCellStyles(workbook);
			createDailySheet(workbook, exportModel);
			createIntervalSheet(workbook, exportModel);
			
			using (var exportData = new MemoryStream())
			{
				workbook.Write(exportData);
				return exportData.ToArray();
			}
		}
		private void createDailySheet(IWorkbook workbook, ForecastExportModel exportModel)
		{
			var dailySheet = workbook.CreateSheet("Daily");
			sheetHeader(dailySheet, exportModel);
			dailyDetailHeader(dailySheet, exportModel.Workloads);
			dailyDetails(dailySheet, exportModel.DailyModelForecast);
			autoSizeColumns(dailySheet);
		}

		private void createIntervalSheet(IWorkbook workbook, ForecastExportModel exportModel)
		{
			var intervalSheet = workbook.CreateSheet("Interval");
			sheetHeader(intervalSheet, exportModel);
			intervalDetailHeader(intervalSheet, exportModel.Workloads);
			intervalDetails(intervalSheet, exportModel.IntervalModelForecast);
			autoSizeColumns(intervalSheet);
		}

		private void autoSizeColumns(ISheet sheet)
		{
			var lastRowIndex = sheet.PhysicalNumberOfRows - 1;
			var lastRow = sheet.GetRow(lastRowIndex);
			var lastCellIndex = lastRow.PhysicalNumberOfCells - 1;
			var lastColumnIndex = lastRow.GetCell(lastCellIndex).ColumnIndex;

			for (var columnIndex = 0; columnIndex <= lastColumnIndex; columnIndex++)
			{
				sheet.AutoSizeColumn(columnIndex);
			}
		}

		private void initializeCellStyles(IWorkbook workbook)
		{
			var titleFont = workbook.CreateFont();
			titleFont.Boldweight = (short) FontBoldWeight.Bold;

			_cellStyleTitle = workbook.CreateCellStyle();
			_cellStyleTitle.SetFont(titleFont);

			_cellStyleDate = workbook.CreateCellStyle();
			_cellStyleDate.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy");

			_cellStylePercentage = workbook.CreateCellStyle();
			_cellStylePercentage.DataFormat = HSSFDataFormat.GetBuiltinFormat("0%");

			_cellStyleTwoDecimals = workbook.CreateCellStyle();
			_cellStyleTwoDecimals.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00");

			_cellStyleTime = workbook.CreateCellStyle();
			_cellStyleTime.DataFormat = HSSFDataFormat.GetBuiltinFormat("h:mm");
		}

		private void sheetHeader(ISheet dailySheet, ForecastExportModel exportModel)
		{
			addRow(dailySheet, new[]
			{
				new CellData("Period Start:", _cellStyleTitle),
				new CellData(exportModel.Header.Period.StartDate.Date, _cellStyleDate)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Period End:", _cellStyleTitle),
				new CellData(exportModel.Header.Period.EndDate.Date, _cellStyleDate)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Scenario:", _cellStyleTitle),
				new CellData(exportModel.Header.Scenario)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Skill Name:", _cellStyleTitle),
				new CellData(exportModel.Header.SkillName)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Time Zone:", _cellStyleTitle),
				new CellData(exportModel.Header.SkillTimeZoneName)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Service Level:", _cellStyleTitle),
				exportModel.Header.ServiceLevelPercent.HasValue
					? new CellData(exportModel.Header.ServiceLevelPercent.Value.Value, _cellStylePercentage)
					: new CellData(string.Empty)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Service Level s:", _cellStyleTitle),
				exportModel.Header.ServiceLevelSeconds.HasValue
					? new CellData(exportModel.Header.ServiceLevelSeconds)
					: new CellData(string.Empty)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Shrinkage:", _cellStyleTitle),
				exportModel.Header.ShrinkagePercent.HasValue
					? new CellData(exportModel.Header.ShrinkagePercent.Value.Value, _cellStylePercentage)
					: new CellData(string.Empty)
			});
			addRow(dailySheet, new CellData[0]);
			addRow(dailySheet, new CellData[0]);
		}

		private void dailyDetailHeader(ISheet sheet, IReadOnlyCollection<string> workloads)
		{
			const string splitter = "\r\n  ";

			string comment = null;
			if (workloads.Count > 1)
			{
				comment = "This includes hours from:" + splitter + string.Join(splitter, workloads);
			}

			addRow(sheet, new[]
			{
				new CellData("Date", _cellStyleTitle),
				new CellData("Calls", _cellStyleTitle),
				new CellData("Average Talk time (s)", _cellStyleTitle),
				new CellData("Average ACW (s)", _cellStyleTitle),
				new CellData("AHT (s)", _cellStyleTitle),
				new CellData("Hours", _cellStyleTitle, comment),
				new CellData("Hours with shrinkage", _cellStyleTitle, comment),
			});
		}

		private void dailyDetails(ISheet dailySheet, IList<ForecastExportDailyModel> dailyModels)
		{
			foreach (var dailyModel in dailyModels)
			{
				addRow(dailySheet, new []
				{
					new CellData(dailyModel.ForecastDate, _cellStyleDate),
					new CellData(dailyModel.Calls, _cellStyleTwoDecimals),
					new CellData(dailyModel.AverageTalkTime, _cellStyleTwoDecimals),
					new CellData(dailyModel.AverageAfterCallWork, _cellStyleTwoDecimals),
					new CellData(dailyModel.AverageHandleTime, _cellStyleTwoDecimals),
					new CellData(dailyModel.ForecastedHours, _cellStyleTwoDecimals),
					new CellData(dailyModel.ForecastedHoursShrinkage, _cellStyleTwoDecimals)
				});
			}
		}
		
		private void intervalDetailHeader(ISheet sheet, IReadOnlyCollection<string> workloads)
		{
			const string splitter = "\r\n  ";

			string comment = null;
			if (workloads.Count > 1)
			{
				comment = "This includes agents from:" + splitter + string.Join(splitter, workloads);
			}

			addRow(sheet, new[]
			{
				new CellData("Date", _cellStyleTitle),
				new CellData("Interval (hh:mm)", _cellStyleTitle),
				new CellData("Calls", _cellStyleTitle),
				new CellData("Average Talk time (s)", _cellStyleTitle),
				new CellData("Average ACW (s)", _cellStyleTitle),
				new CellData("AHT (s)", _cellStyleTitle),
				new CellData("Agents", _cellStyleTitle, comment),
				new CellData("Agents with shrinkage", _cellStyleTitle, comment)
			});
		}

		private void intervalDetails(ISheet sheet, IList<ForecastExportIntervalModel> intervalModels)
		{
			foreach (var intervalModel in intervalModels)
			{
				addRow(sheet, new[]
				{
					new CellData(intervalModel.IntervalStart.Date, _cellStyleDate),
					new CellData(intervalModel.IntervalStart, _cellStyleTime),
					new CellData(intervalModel.Calls, _cellStyleTwoDecimals),
					new CellData(intervalModel.AverageTalkTime, _cellStyleTwoDecimals),
					new CellData(intervalModel.AverageAfterCallWork, _cellStyleTwoDecimals),
					new CellData(intervalModel.AverageHandleTime, _cellStyleTwoDecimals),
					new CellData(intervalModel.Agents, _cellStyleTwoDecimals),
					new CellData(intervalModel.AgentsShrinkage, _cellStyleTwoDecimals)
				});
			}
		}

		private void addRow(ISheet sheet, IEnumerable<CellData> cellDataList)
		{
			var patriarch = sheet.CreateDrawingPatriarch();
			var row = sheet.CreateRow(sheet.PhysicalNumberOfRows);

			foreach (var cellData in cellDataList)
			{
				var newCell = row.CreateCell(row.PhysicalNumberOfCells);

				switch (cellData.CellValue)
				{
					case string _:
						newCell.SetCellValue(cellData.CellValue.ToString());
						break;
					case double _:
						newCell.SetCellValue((double)cellData.CellValue);
						break;
					case DateTime _:
						newCell.SetCellValue((DateTime)cellData.CellValue);
						break;
				}

				if (cellData.CellStyle != null)
				{
					newCell.CellStyle =  cellData.CellStyle;
				}

				if (!string.IsNullOrEmpty(cellData.Comment))
				{
					// Client anchor defines size and position of the comment in the worksheet
					var startRowIndex = newCell.RowIndex > 1 ? newCell.RowIndex - 1 : 0;
					var anchor = new XSSFClientAnchor(0, 0, 0, 0, newCell.ColumnIndex + 2, startRowIndex,
						newCell.ColumnIndex + 5, startRowIndex + cellData.Comment.Split('\n').Length + 1);
					var comment = patriarch.CreateCellComment(anchor);
					comment.String = new XSSFRichTextString(cellData.Comment);

					newCell.CellComment = comment;
				}
			}
		}
	}

	public class CellData
	{
		public object CellValue { get; }
		public ICellStyle CellStyle { get; }
		public string Comment { get; }

		public CellData(object cellValue)
		{
			CellValue = cellValue;
		}

		public CellData(object cellValue, ICellStyle cellStyle)
		{
			CellValue = cellValue;
			CellStyle = cellStyle;
		}

		public CellData(object cellValue, ICellStyle cellStyle, string comment)
		{
			CellValue = cellValue;
			Comment = comment;
			CellStyle = cellStyle;
		}
	}
}