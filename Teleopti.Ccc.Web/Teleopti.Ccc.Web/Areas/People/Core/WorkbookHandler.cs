using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using NPOI.SS.UserModel;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.People.Core.Models;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class WorkbookHandler : IWorkbookHandler
	{
		private static readonly PropertyInfo[] propertyInfos =
			typeof(RawAgent).GetProperties().OrderBy(p => p.GetCustomAttribute<OrderAttribute>().Order).ToArray();

		private readonly IImportAgentFileValidator _rawAgentMapper;

		public WorkbookHandler(IImportAgentFileValidator rawAgentMapper)
		{
			_rawAgentMapper = rawAgentMapper;
		}

		public List<string> ValidateSheetColumnHeader(IWorkbook workbook)
		{
			var columnNames = extractSheetColumnNames(workbook).ToArray();
			var errorMessages = new List<string>();
			for (var i = 0; i < propertyInfos.Length - 1; i++)
			{
				var expectedColumnName = propertyInfos[i].Name;
				if (i >= columnNames.Length)
				{
					errorMessages.Add(expectedColumnName);
					continue;
				}
				var columnName = columnNames[i];
				if (string.Compare(columnName, expectedColumnName, true, CultureInfo.CurrentCulture) != 0)
				{
					errorMessages.Add(expectedColumnName);
				}
			}

			return errorMessages;
		}

		public IList<AgentExtractionResult> ProcessSheet(ISheet sheet)
		{
			var results = new List<AgentExtractionResult>();
			for (var i = 1; i <= sheet.LastRowNum; i++)
			{
				var row = sheet.GetRow(i);
				var extractedRow = new AgentExtractionResult();
				IList<string> rowErrors;
				var raw = ParseRow(row, out rowErrors);
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
				results.Add(extractedRow);
			}

			return results;
		}

		protected RawAgent ParseRow(IRow row, out IList<string> errors)
		{
			var raw = new RawAgent();
			errors = new List<string>();
			var cells = row.Cells;
			for (var i = 0; i < propertyInfos.Length - 1; i++)
			{
				var pro = propertyInfos[i];
				try
				{
					pro.SetValue(raw, getVale(cells[i], pro));
				}
				catch (Exception)
				{

					var expectedFormat = "text";
					switch (pro.Name)
					{
						case "StartDate":
							expectedFormat = "date";
							break;

						case "SchedulePeriodLength":
							expectedFormat = "number";
							break;
					}

					errors.Add(string.Format(Resources.InvalidColumn, pro.Name, string.Format(Resources.ExpectedXColumnFormat, expectedFormat)));
				}
			}

			return raw;
		}

		private static List<string> extractSheetColumnNames(IWorkbook workbook)
		{
			if (workbook.NumberOfSheets == 0)
				return new List<string>();
			var sheet = workbook.GetSheetAt(0);
			var headerRow = sheet.GetRow(0);
			return headerRow.Cells.Select(x =>
			{
				try
				{
					return x.StringCellValue;
				}
				catch (Exception)
				{

					return null;
				}

			}).Where(n => n != null).ToList();
		}

		private static object getVale(ICell cell, PropertyInfo pro)
		{
			if (pro.Name == "StartDate")
			{
				return cell.DateCellValue;
			}
			switch (cell.CellType)
			{

				case CellType.Numeric:
					return cell.NumericCellValue;
				case CellType.String:
					return cell.StringCellValue;
				case CellType.Boolean:
					return cell.BooleanCellValue;
			}
			
			return null;
		}
	}

	public interface IWorkbookHandler
	{
		List<string> ValidateSheetColumnHeader(IWorkbook workbook);
		IList<AgentExtractionResult> ProcessSheet(ISheet sheet);
	}
}