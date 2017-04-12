using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class AgentFileTemplate
	{
		private readonly string[] _columnHeaders = {
			"Firstname",
			"Lastname",
			"WindowsUser",
			"ApplicationUserId",
			"Password",
			"Role",
			"StartDate",
			"Site/Team",
			"Skill",
			"ExternalLogon",
			"Contract",
			"ContractSchedule",
			"PartTimePercentage",
			"ShiftBag",
			"SchedulePeriodType",
			"SchedulePeriodLength"
		};



		private readonly IDictionary<string, int> _columnHeaderMap = new Dictionary<string, int>();

		public AgentFileTemplate()
		{
			for (var i = 0; i < _columnHeaders.Length; i++)
			{
				_columnHeaderMap.Add(_columnHeaders[i], i);
			}
		}

		public string[] ColumnHeaderNames => _columnHeaders;

		public IDictionary<string, int> ColumnHeaderMap => _columnHeaderMap;

		public RawAgent GetDefaultAgent()
		{
			return new RawAgent
			{
				Firstname = "John",
				Lastname = "Smith",
				WindowsUser = "john.smith@teleopti.com",
				ApplicationUserId = "john.smith@teleopti.com",
				Password = "password",
				Role = "Agent, \"London, site admin\"",
				StartDate = new DateTime(2017, 3, 1),
				Organization = "London/Team Preferences",
				Skill = "Direct Sales, Channel Sales",
				ExternalLogon = "",
				Contract = "BTS",
				ContractSchedule = "BTS",
				PartTimePercentage = "75%",
				ShiftBag = "London Full Time",
				SchedulePeriodType = "Week",
				SchedulePeriodLength = 4
			};
		}

		public MemoryStream GetFileTemplate(params RawAgent[] agents)
		{
			const string sheetName = "Agents";

			var ms = new MemoryStream();
			var returnedFile = GetTemplateWorkbook(sheetName);
			var newSheet = returnedFile.GetSheet(sheetName);

			for (var i = 0; i < agents.Length; i++)
			{
				var agent = agents[i];
				var row = newSheet.CreateRow(i + 1);

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
				row.CreateCell(ColumnHeaderMap["Site/Team"]).SetCellValue(agent.Organization);
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
			}
			returnedFile.Write(ms);

			return ms;
		}

		public IWorkbook GetTemplateWorkbook(string invalidUserSheetName, bool isXlsx = false)
		{
			var returnedFile = isXlsx ? (IWorkbook)new XSSFWorkbook() : new HSSFWorkbook();
			var newsheet = returnedFile.CreateSheet(invalidUserSheetName);

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
					newsheet.SetDefaultColumnStyle(i, dateStyle);
				}
				else if (ColumnHeaderMap["SchedulePeriodLength"] == i)
				{
					newsheet.SetDefaultColumnStyle(i, numberStyle);
				}
				else
				{
					newsheet.SetDefaultColumnStyle(i, textStyle);
				}
			}

			var row = newsheet.CreateRow(0);
			for (var i = 0; i < ColumnHeaderNames.Length; i++)
			{
				row.CreateCell(i).SetCellValue(ColumnHeaderNames[i]);
			}

			return returnedFile;
		}

	}
}