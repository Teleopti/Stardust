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
		private void createDailySheet(XSSFWorkbook workbook, ForecastExportModel exportModel)
		{
			var dailySheet = workbook.CreateSheet("Daily");
			sheetHeader(dailySheet, exportModel);
			dailyDetailHeader(dailySheet);
			dailyDetails(dailySheet, exportModel.DailyModelForecast);
			autoSizeColumns(dailySheet);
		}

		private void createIntervalSheet(XSSFWorkbook workbook, ForecastExportModel exportModel)
		{
			var intervalSheet = workbook.CreateSheet("Interval");
			sheetHeader(intervalSheet, exportModel);
			intervalDetailHeader(intervalSheet);
			intervalDetails(intervalSheet, exportModel.IntervalModelForecast);
			autoSizeColumns(intervalSheet);
		}

		private void autoSizeColumns(ISheet sheet)
		{
			var lastRowIndex = sheet.PhysicalNumberOfRows - 1;
			var lastRow = sheet.GetRow(lastRowIndex);
			var lastCellIndex = lastRow.PhysicalNumberOfCells - 1;
			var lastColumnIndex = lastRow.GetCell(lastCellIndex).ColumnIndex;

			for (int columnIndex = 0; columnIndex <= lastColumnIndex; columnIndex++)
			{
				sheet.AutoSizeColumn(columnIndex);
			}
		}

		private void initializeCellStyles(XSSFWorkbook workbook)
		{
			_cellStyleDate = workbook.CreateCellStyle();
			_cellStyleDate.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy");

			_cellStylePercentage = workbook.CreateCellStyle();
			_cellStylePercentage.DataFormat = HSSFDataFormat.GetBuiltinFormat("0%");

			_cellStyleTwoDecimals = workbook.CreateCellStyle();
			_cellStyleTwoDecimals.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00");

			_cellStyleTime= workbook.CreateCellStyle();
			_cellStyleTime.DataFormat = HSSFDataFormat.GetBuiltinFormat("h:mm");
		}

		private void sheetHeader(ISheet dailySheet, ForecastExportModel exportModel)
		{
			addRow(dailySheet, new[]
			{
				new CellData("Period Start:"),
				new CellData(exportModel.Header.Period.StartDate.Date, _cellStyleDate)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Period End:"),
				new CellData(exportModel.Header.Period.EndDate.Date, _cellStyleDate)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Skill Name:"),
				new CellData(exportModel.Header.SkillName)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Time Zone:"),
				new CellData(exportModel.Header.SkillTimeZoneName)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Service Level:"),
				exportModel.Header.ServiceLevelPercent.HasValue
					? new CellData(exportModel.Header.ServiceLevelPercent.Value.Value, _cellStylePercentage)
					: new CellData(string.Empty)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Service Level s:"),
				exportModel.Header.ServiceLevelSeconds.HasValue
					? new CellData(exportModel.Header.ServiceLevelSeconds)
					: new CellData(string.Empty)
			});
			addRow(dailySheet, new[]
			{
				new CellData("Shrinkage:"),
				exportModel.Header.ShrinkagePercent.HasValue
					? new CellData(exportModel.Header.ShrinkagePercent.Value.Value, _cellStylePercentage)
					: new CellData(string.Empty)
			});
			addRow(dailySheet, new CellData[0]);
			addRow(dailySheet, new CellData[0]);
		}

		private void dailyDetailHeader(ISheet sheet)
		{
			addRow(sheet, new[]
			{
				new CellData("Date"),
				new CellData("Calls"),
				new CellData("Average Talk time (s)"),
				new CellData("Average ACW (s)"),
				new CellData("Average AHT (s)"),
				new CellData("Hours"),
				new CellData("Hours with shrinkage"),
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
		
		private void intervalDetailHeader(ISheet sheet)
		{
			addRow(sheet, new[]
			{
				new CellData("Date"),
				new CellData("Interval (hh:mm)"),
				new CellData("Calls"),
				new CellData("Average Talk time (s)"),
				new CellData("Average ACW (s)"),
				new CellData("Average AHT (s)"),
				new CellData("Agents"),
				new CellData("Agents with shrinkage"),
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
			var row = sheet.CreateRow(sheet.PhysicalNumberOfRows);

			foreach (var cellData in cellDataList)
			{
				var newCell = row.CreateCell(row.PhysicalNumberOfCells);

				if (cellData.CellValue is string)
				{
					newCell.SetCellValue(cellData.CellValue.ToString());
				}
				else if (cellData.CellValue is double)
				{
					newCell.SetCellValue((double)cellData.CellValue);
				}
				else if (cellData.CellValue is DateTime)
				{
					newCell.SetCellValue((DateTime)cellData.CellValue);
				}

				if (cellData.SetCellStyle)
					newCell.CellStyle = cellData.CellStyle;
			}
		}
	}

	public class CellData
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