using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class AgentFileTemplate : UserFileTemplate
	{
		private readonly string[] _columnHeaders = {
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

		private readonly IDictionary<string, int> _columnHeaderMap;

		public AgentFileTemplate()
		{
			_columnHeaderMap = base.ColumnHeaderMap;
			var offset = base.ColumnHeaderMap.Count;
			for (var i = 0; i < _columnHeaders.Length; i++)
			{
				_columnHeaderMap.Add(_columnHeaders[i], offset + i);
			}
		}

		public override string[] ColumnHeaderNames => base.ColumnHeaderNames.Concat(_columnHeaders).ToArray();

		public override IDictionary<string, int> ColumnHeaderMap => _columnHeaderMap;

		public override MemoryStream GetFileTemplateWithDemoData()
		{
			const string sheetName = "Agents";

			var ms = new MemoryStream();
			var returnedFile = GetTemplateWorkbook(sheetName);
			var newSheet = returnedFile.GetSheet(sheetName);

			var agent = new RawAgent
			{
				Firstname = "John",
				Lastname = "Smith",
				WindowsUser = "john.smith@teleopti.com",
				ApplicationUserId = "john.smith@teleopti.com",
				Password = "password",
				Role = "agent, \"London, Team Leader\"",
				StartDate = "2017-02-28",
				Organization = "Team Preference",
				Skill = "Outbound, Direct sales",
				ExternalLogon = "001001",
				Contract = "Fixed time staff",
				ContractSchedule = "Full-time",
				PartTimePercentage = "80%",
				ShiftBag = "Early Day Shift",
				SchedulePeriodType = "Week",
				SchedulePeriodLength = 4
			};

			var row = newSheet.CreateRow(1);

			row.CreateCell(ColumnHeaderMap["Firstname"]).SetCellValue(agent.Firstname);
			row.CreateCell(ColumnHeaderMap["Lastname"]).SetCellValue(agent.Lastname);
			row.CreateCell(ColumnHeaderMap["WindowsUser"]).SetCellValue(agent.WindowsUser);
			row.CreateCell(ColumnHeaderMap["ApplicationUserId"]).SetCellValue(agent.ApplicationUserId);
			row.CreateCell(ColumnHeaderMap["Password"]).SetCellValue(agent.Password);
			row.CreateCell(ColumnHeaderMap["Role"]).SetCellValue(agent.Role);
			row.CreateCell(ColumnHeaderMap["StartDate"]).SetCellValue(agent.StartDate);
			row.CreateCell(ColumnHeaderMap["Organization"]).SetCellValue(agent.Organization);
			row.CreateCell(ColumnHeaderMap["Skill"]).SetCellValue(agent.Skill);
			row.CreateCell(ColumnHeaderMap["ExternalLogon"]).SetCellValue(agent.ExternalLogon);
			row.CreateCell(ColumnHeaderMap["Contract"]).SetCellValue(agent.Contract);
			row.CreateCell(ColumnHeaderMap["ContractSchedule"]).SetCellValue(agent.ContractSchedule);
			row.CreateCell(ColumnHeaderMap["PartTimePercentage"]).SetCellValue(agent.PartTimePercentage);
			row.CreateCell(ColumnHeaderMap["ShiftBag"]).SetCellValue(agent.ShiftBag);
			row.CreateCell(ColumnHeaderMap["SchedulePeriodType"]).SetCellValue(agent.SchedulePeriodType);
			row.CreateCell(ColumnHeaderMap["SchedulePeriodLength"]).SetCellValue(agent.SchedulePeriodLength);
			returnedFile.Write(ms);

			return ms;
		}
		//private IWorkbook getTemplateWorkbook(string invalidUserSheetName)
		//{
		//	var returnedFile = new HSSFWorkbook();
		//	var newsheet = returnedFile.CreateSheet(invalidUserSheetName);

		//	var row = newsheet.CreateRow(0);
		//	for (var i = 0; i < ColumnHeaderNames.Length; i++)
		//	{
		//		row.CreateCell(i).SetCellValue(ColumnHeaderNames[i]);
		//	}

		//	return returnedFile;
		//}

	}
}