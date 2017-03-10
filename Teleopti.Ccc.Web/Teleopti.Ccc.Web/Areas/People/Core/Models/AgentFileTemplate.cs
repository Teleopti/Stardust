using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

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

		public RawAgent GetDefaultAgent()
		{
			return new RawAgent
			{
				Firstname = "John",
				Lastname = "Smith",
				WindowsUser = "john.smith@teleopti.com",
				ApplicationUserId = "john.smith@teleopti.com",
				Password = "password",
				Role = "agent, \"London, Team Leader\"",
				StartDate = new DateTime(2017,3,1),
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
		}

		public MemoryStream GetFileTemplate(RawAgent agent)
		{
			const string sheetName = "Agents";

			var ms = new MemoryStream();
			var returnedFile = GetTemplateWorkbook(sheetName);
			var newSheet = returnedFile.GetSheet(sheetName);

			var fmt = returnedFile.CreateDataFormat();
			var textStyle = returnedFile.CreateCellStyle();
			textStyle.DataFormat = fmt.GetFormat("@");
			var numberStyle = returnedFile.CreateCellStyle();			
			numberStyle.DataFormat = fmt.GetFormat("0");
			var dateStyle = returnedFile.CreateCellStyle();
			dateStyle.DataFormat = fmt.GetFormat(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);

			for (var i = 0; i < ColumnHeaderMap.Count; i++)
			{
				if (ColumnHeaderMap["StartDate"] == i)
				{
					newSheet.SetDefaultColumnStyle(i, dateStyle);
				} else if (ColumnHeaderMap["SchedulePeriodLength"] == i)
				{
					newSheet.SetDefaultColumnStyle(i, numberStyle);
				}
				else
				{
					newSheet.SetDefaultColumnStyle(i, textStyle);
				}
			}


			var row = newSheet.CreateRow(1);

			row.CreateCell(ColumnHeaderMap["Firstname"]).SetCellValue(agent.Firstname);
			
			row.CreateCell(ColumnHeaderMap["Lastname"]).SetCellValue(agent.Lastname);
			
			row.CreateCell(ColumnHeaderMap["WindowsUser"]).SetCellValue(agent.WindowsUser);
			row.CreateCell(ColumnHeaderMap["ApplicationUserId"]).SetCellValue(agent.ApplicationUserId);
			row.CreateCell(ColumnHeaderMap["Password"]).SetCellValue(agent.Password);
			row.CreateCell(ColumnHeaderMap["Role"]).SetCellValue(agent.Role);

			var startDateCell = row.CreateCell(ColumnHeaderMap["StartDate"]);
			if (agent.StartDate.HasValue)
			{
				startDateCell.SetCellValue(agent.StartDate.Value);
			}
			row.CreateCell(ColumnHeaderMap["Organization"]).SetCellValue(agent.Organization);
			row.CreateCell(ColumnHeaderMap["Skill"]).SetCellValue(agent.Skill);
			row.CreateCell(ColumnHeaderMap["ExternalLogon"]).SetCellValue(agent.ExternalLogon);
			row.CreateCell(ColumnHeaderMap["Contract"]).SetCellValue(agent.Contract);
			row.CreateCell(ColumnHeaderMap["ContractSchedule"]).SetCellValue(agent.ContractSchedule);
			row.CreateCell(ColumnHeaderMap["PartTimePercentage"]).SetCellValue(agent.PartTimePercentage);
			row.CreateCell(ColumnHeaderMap["ShiftBag"]).SetCellValue(agent.ShiftBag);
			row.CreateCell(ColumnHeaderMap["SchedulePeriodType"]).SetCellValue(agent.SchedulePeriodType);

			var schedulePeriodLengthCell = row.CreateCell(ColumnHeaderMap["SchedulePeriodLength"]);
			if (agent.SchedulePeriodLength.HasValue)
			{
				schedulePeriodLengthCell.SetCellValue(agent.SchedulePeriodLength.Value);
			}
			returnedFile.Write(ms);

			return ms;
		}

	}
}