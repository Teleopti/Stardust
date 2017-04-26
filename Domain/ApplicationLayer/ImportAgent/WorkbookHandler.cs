using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NPOI.SS.UserModel;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class WorkbookHandler : IWorkbookHandler
	{
		private readonly IImportAgentFileValidator _rawAgentMapper;
		private readonly AgentFileTemplate _agentFileTemplate;
		private const int MAXIMUM_NUMBER_OF_ROWS = 5000;
		public WorkbookHandler(IImportAgentFileValidator rawAgentMapper)
		{
			_rawAgentMapper = rawAgentMapper;
			_agentFileTemplate = new AgentFileTemplate();
		}

		public AgentFileProcessResult Process(IWorkbook workbook, ImportAgentDefaults defaultValues = null)
		{
			var result = new AgentFileProcessResult();
			var validateMessage = validateWorkbook(workbook);
			if (!validateMessage.IsNullOrEmpty())
			{
				result.Feedback.ErrorMessages.Add(validateMessage);
				return result;
			}
			var sheet = workbook.GetSheetAt(0);
			_rawAgentMapper.SetDefaultValues(defaultValues);
			for (var i = 1; i <= sheet.LastRowNum; i++)
			{
				var row = sheet.GetRow(i);
				if (row.IsBlank())
				{
					continue;
				}
				var extractedRow = new AgentExtractionResult { Row = row };
				IList<string> rowErrors;
				var raw = parseRow(row, out rowErrors);
				extractedRow.Raw = raw;
				if (rowErrors.Any())
				{
					extractedRow.Feedback.ErrorMessages.AddRange(rowErrors);
				}
				else
				{
					Feedback feedback;
					var agentInfo = _rawAgentMapper.MapRawData(raw, out feedback);
					extractedRow.Feedback.Merge(feedback);
					if (!extractedRow.Feedback.ErrorMessages.Any())
					{
						extractedRow.Agent = agentInfo;
					}
				}
				result.RawResults.Add(extractedRow);
			}

			return result;
		}


		private RawAgent parseRow(IRow row, out IList<string> errors)
		{
			var raw = new RawAgent();
			errors = new List<string>();
			for (var i = 0; i < _agentFileTemplate.ColumnHeaders.Length; i++)
			{
				var pro = _agentFileTemplate.ColumnHeaders[i];
				var cell = row.GetCell(i);
				try
				{
					var proValue = getValue(cell, pro);
					pro.SetValue(raw, proValue);

				}
				catch (Exception)
				{

					var expectedFormat = "text";
					switch (pro.Name)
					{
						case nameof(RawAgent.StartDate):
							expectedFormat = "date";
							break;

						case nameof(RawAgent.SchedulePeriodLength):
							expectedFormat = "number";
							break;
					}

					errors.Add(string.Format(Resources.InvalidColumn, pro.Name,
						string.Format(Resources.RequireXCellFormat, expectedFormat)));
				}
			}

			return raw;
		}

		private List<string> extractSheetColumnNames(IWorkbook workbook)
		{
			if (workbook.NumberOfSheets == 0)
				return new List<string>();
			var sheet = workbook.GetSheetAt(0);
			var headerRow = sheet.GetRow(0);
			return headerRow.GetCellsIncludeBlankOrNull().Select(x =>
			{
				if (x == null || x.CellType == CellType.Blank)
				{
					return string.Empty;
				}
				try
				{
					return x.StringCellValue;
				}
				catch (Exception)
				{

					return string.Empty;
				}

			}).ToList();
		}

		private object getValue(ICell cell, PropertyInfo pro)
		{
			if (cell.IsBlank())
			{
				return null;
			}

			switch (pro.Name)
			{
				case nameof(RawAgent.StartDate):
					if (cell.DateCellValue != DateTime.MinValue && cell.DateCellValue != DateTime.MaxValue)
						return cell.DateCellValue;
					break;
				case nameof(RawAgent.SchedulePeriodLength):
					if (cell.CellType == CellType.Numeric)
						return cell.NumericCellValue;
					break;

				default:
					if (cell.CellType == CellType.String)
						return cell.StringCellValue;
					break;
			}
			throw new Exception();
		}

		private string validateWorkbook(IWorkbook workbook)
		{
			if (workbook == null || workbook.NumberOfSheets == 0)
			{
				return Resources.InvalidInput;
			}
			var sheet = workbook.GetSheetAt(0);

			if (sheet.LastRowNum == 0 && sheet.GetRow(0) == null)
			{
				return Resources.EmptyFile;
			}

			var missHeaderErrorMsg = validateSheetColumnHeader(workbook);
			if (!missHeaderErrorMsg.IsNullOrEmpty())
			{
				return missHeaderErrorMsg;
			}

			var errorMsg = "";
			if (!(errorMsg = validateSheetColumnHeader(workbook)).IsNullOrEmpty())
			{
				return errorMsg;
			}
			var numberOfRecords = getNumberOfRecordsInSheet(sheet);
			if (numberOfRecords == 0)
			{
				return Resources.NoDataAvailable;
			}

			if (numberOfRecords > MAXIMUM_NUMBER_OF_ROWS)
			{
				return string.Format(Resources.NumberOfRowsExceedsTheMaximum, MAXIMUM_NUMBER_OF_ROWS);
			}
			return string.Empty;
		}
		private string validateSheetColumnHeader(IWorkbook workbook)
		{
			var columnNames = extractSheetColumnNames(workbook).ToArray();
			var missHeaders = new List<string>();
			for (var i = 0; i < _agentFileTemplate.ColumnHeaders.Length; i++)
			{
				var pro = _agentFileTemplate.ColumnHeaders[i];
				var dispalyColumnName = _agentFileTemplate.GetColumnDisplayName(pro.Name);
				if (i >= columnNames.Length)
				{
					missHeaders.Add(dispalyColumnName);
					continue;
				}
				var columnName = columnNames[i];
				if (!(string.Compare(columnName, dispalyColumnName, true, CultureInfo.CurrentCulture) == 0
					|| string.Compare(columnName, pro.Name, true, CultureInfo.CurrentCulture) == 0)
				)
				{
					missHeaders.Add(dispalyColumnName);
				}
			}

			return missHeaders.Any() ? string.Format(Resources.MissingColumnX, string.Join(", ", missHeaders)) : string.Empty;
		}

		private int getNumberOfRecordsInSheet(ISheet sheet)
		{
			var count = 0;

			for (var i = 1; i <= sheet.LastRowNum; i++)
			{
				var row = sheet.GetRow(i);
				if (!row.IsBlank())
				{
					count++;
				}
			}
			return count;
		}

	}

	public interface IWorkbookHandler
	{
		AgentFileProcessResult Process(IWorkbook workbook, ImportAgentDefaults defaultValues = null);
	}
}