using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class IntradayExcelCreator
	{
		private ICellStyle _cellStyleDate;
		private ICellStyle _cellStylePercentage;
		private ICellStyle _cellStyleTwoDecimals;
		private ICellStyle _cellStyleTime;
		private ICellStyle _cellStyleLongDate;
		private ICellStyle _cellStyleGeneral;
	
		public byte[] CreateExcel(IntradayExcelExportData data)
		{
			var workbook = new XSSFWorkbook();
			initializeCellStyles(workbook);

			var sheet = workbook.CreateSheet("Data table");

			createHeaderInfo(sheet, data);
			createHeaderRowsColums(sheet);
			createSummaryRow(sheet, data);
			createDataRows(sheet, data);
			autofitColumns(sheet);
			sheet.CreateFreezePane(0, 6);

			using (var stream = new MemoryStream())
			{
				workbook.Write(stream);
				return stream.ToArray();
			}
		}

		private void autofitColumns(ISheet sheet)
		{
			Enumerable.Range(0, sheet.GetRow(4).Cells.Count).ForEach(sheet.AutoSizeColumn);
		}

		private void initializeCellStyles(XSSFWorkbook workbook)
		{
			_cellStyleLongDate = workbook.CreateCellStyle();
			_cellStyleLongDate.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy h:mm");

			_cellStyleDate = workbook.CreateCellStyle();
			_cellStyleDate.DataFormat = HSSFDataFormat.GetBuiltinFormat("m/d/yy");

			_cellStylePercentage = workbook.CreateCellStyle();
			_cellStylePercentage.DataFormat = HSSFDataFormat.GetBuiltinFormat("0%");

			_cellStyleTwoDecimals = workbook.CreateCellStyle();
			_cellStyleTwoDecimals.DataFormat = HSSFDataFormat.GetBuiltinFormat("0.00");

			_cellStyleTime = workbook.CreateCellStyle();
			_cellStyleTime.DataFormat = HSSFDataFormat.GetBuiltinFormat("h:mm");

			_cellStyleGeneral = workbook.CreateCellStyle();
			_cellStyleGeneral.DataFormat = HSSFDataFormat.GetBuiltinFormat("General");
		}

		private void createHeaderInfo(ISheet sheet, IntradayExcelExportData exportData)
		{
			addRow(sheet, new []
			{
				new CellData(Resources.DateColon),
				new CellData(exportData.Date, _cellStyleDate), 
				new CellData(""), 
				new CellData(""), 
				new CellData(""), 
				new CellData(""), 
				new CellData(""), 
				new CellData(""), 
				new CellData(""), 
				new CellData(""), 
				new CellData(""), 
				new CellData(DateTime.Now, _cellStyleLongDate), 
			});
			addRow(sheet, new[]
			{
				new CellData(Resources.SkillAreaColon),
				new CellData(exportData.SkillAreaName),
			});
			addRow(sheet, new[]
			{
				new CellData(Resources.SkillsParenthesisS),
				new CellData(string.Join("; ", exportData.Skills)),
			});

			addRow(sheet, new[]
			{
				new CellData("")
			});
		}

		private void createHeaderRowsColums(ISheet sheet)
		{
			addRow(sheet, new[]
			{
				new CellData(Resources.Interval),
				new CellData(Resources.ForecastedVolume),
				new CellData(Resources.ActualVolume),
				new CellData(Resources.DifferencePercent),
				new CellData(Resources.ForecastedAverageHandleTime),
				new CellData(Resources.ActualAverageHandlingTime),
				new CellData(Resources.DifferencePercent),
				new CellData(Resources.ServiceLevelParenthesisPercentSign),
				new CellData(Resources.ESLParenthesisPercentSign),
				new CellData(Resources.AbandonedRateParenthesisPercentSign),
				new CellData(Resources.AverageSpeedOfAnswersParenthesisSeconds),
				new CellData(Resources.ForecastedAgents),
				new CellData(Resources.RequiredStaff),
				new CellData(Resources.ScheduledStaff),
				new CellData(Resources.ReforecastedStaff)
			});
		}

		private void createSummaryRow(ISheet sheet, IntradayExcelExportData data)
		{
			var rows = createDataRow(data.DataTotal);
			rows[0] = new CellData(Resources.Total, _cellStyleGeneral);

			addRow(sheet, rows);
		}

		private void createDataRows(ISheet sheet, IntradayExcelExportData data)
		{
			foreach (var rowData in data.RowData)
			{
				var rows = createDataRow(rowData);
				addRow(sheet, rows);
			}
		}

		private CellData[] createDataRow(IntradayExcelExportRowData data)
		{
			return new[]
			{
				new CellData(data.Interval, _cellStyleTime),
				new CellData(data.ForecastedVolume, _cellStyleTwoDecimals),
				new CellData(data.ActualVolume),
				new CellData(data.DifferenceVolume, _cellStylePercentage),
				new CellData(data.ForecastedAht, _cellStyleTwoDecimals),
				new CellData(data.ActualAht, _cellStyleTwoDecimals),
				new CellData(data.DifferenceAht, _cellStylePercentage),
				new CellData(data.ServiceLevel, _cellStylePercentage),
				new CellData(data.Esl, _cellStylePercentage),
				new CellData(data.AbandonedRate, _cellStylePercentage),
				new CellData(data.AverageSpeedOfAnswer, _cellStyleTwoDecimals),
				new CellData(data.ForecastedAgents, _cellStyleTwoDecimals),
				new CellData(data.RequiredAgents, _cellStyleTwoDecimals),
				new CellData(data.ScheduledAgents, _cellStyleTwoDecimals),
				new CellData(data.ReforecastedAgents, _cellStyleTwoDecimals),
			};
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