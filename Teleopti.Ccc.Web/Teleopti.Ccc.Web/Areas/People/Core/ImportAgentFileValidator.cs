using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NPOI.SS.UserModel;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public interface IImportAgentFileValidator
	{
		string[] ExtractColumnNames(IWorkbook workbook);
		List<string> ValidateColumnNames(string[] columnNames);
	}

	public class ImportAgentFileValidator : IImportAgentFileValidator
	{
		private readonly string[] expectedColumnNames = new []
		{
			"Firstname",
			"Lastname",
			"WindowsUser",
			"ApplicationUserId",
			"Password",
			"Role",
			"StartDate",
			"Organization",
			"Skill",
			"ExternalLogon",
			"Contract",
			"ContractSchedule",
			"PartTimePercentage",
			"ShiftBag",
			"SchedulePeriodType",
			"SchedulePeriodLength"
		};

		public string[] ExtractColumnNames(IWorkbook workbook)
		{
			var sheet = workbook.GetSheetAt(0);
			var headerRow = sheet.GetRow(0);
			return headerRow.Cells.Select(x => x.StringCellValue).ToArray();
		}

		public List<string> ValidateColumnNames(string[] columnNames)
		{
			var errorMessages = new List<string>();
			for (var i = 0; i < expectedColumnNames.Length; i++)
			{
				var expectedColumnName = expectedColumnNames[i];
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
	}
}