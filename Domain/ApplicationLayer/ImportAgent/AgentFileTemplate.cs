using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class AgentFileTemplate
	{


		private const string SHEETNAME = "Agents";
		internal readonly PropertyInfo[] ColumnHeaders =
			typeof(RawAgent).GetProperties().OrderBy(p => p.GetCustomAttribute<OrderAttribute>().Order).ToArray();

		public string[] ColumnHeaderNames => ColumnHeaders
			.Select(header => GetColumnDisplayName(header.Name))
			.ToArray();

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
			var ms = new MemoryStream();
			var returnedFile = GetWorkbook(agents);
			returnedFile.Write(ms);
			return ms;
		}

		public IWorkbook GetDefaultFileTemplate()
		{
			return GetWorkbook(GetDefaultAgent());
		}

		private IWorkbook GetWorkbook(params RawAgent[] agents)
		{
			var returnedFile = GetTemplateWorkbook(SHEETNAME);
			var newSheet = returnedFile.GetSheet(SHEETNAME);

			for (var i = 0; i < agents.Length; i++)
			{
				var agent = agents[i];
				var row = newSheet.CreateRow(i + 1);
				for (int j = 0; j < ColumnHeaders.Length; j++)
				{
					var value = ColumnHeaders[j].GetValue(agent);
					var cell = row.CreateCell(j);
					if (value != null)
					{
						DateTime dateValue;
						if (DateTime.TryParse(value.ToString(), out dateValue))
						{
							cell.SetCellValue(((DateTime?)value).Value);
							continue;
						}
						Double numberValue;
						if (Double.TryParse(value.ToString(), out numberValue))
						{
							cell.SetCellValue(numberValue);
							continue;
						}
						cell.SetCellValue(value.ToString());
					}

				}
			}
			return returnedFile;
		}

		public IWorkbook GetTemplateWorkbook(string sheetName, bool isXlsx = false)
		{
			var returnedFile = isXlsx ? (IWorkbook)new XSSFWorkbook() : new HSSFWorkbook();
			var newsheet = returnedFile.CreateSheet(sheetName);

			var fmt = returnedFile.CreateDataFormat();
			var textStyle = returnedFile.CreateCellStyle();
			textStyle.DataFormat = fmt.GetFormat("@");
			var numberStyle = returnedFile.CreateCellStyle();
			numberStyle.DataFormat = fmt.GetFormat("0");
			var dateStyle = returnedFile.CreateCellStyle();
			dateStyle.DataFormat = fmt.GetFormat(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);

			for (var i = 0; i < ColumnHeaders.Length; i++)
			{
				var column = ColumnHeaders[i];

				if (column.PropertyType == typeof(DateTime?))
				{
					newsheet.SetDefaultColumnStyle(i, dateStyle);
				}
				else if (column.PropertyType == typeof(double?))
				{
					newsheet.SetDefaultColumnStyle(i, numberStyle);
				}
				else
				{
					newsheet.SetDefaultColumnStyle(i, textStyle);
				}
			}

			var row = newsheet.CreateRow(0);
			for (var i = 0; i < ColumnHeaders.Length; i++)
			{
				row.CreateCell(i).SetCellValue(GetColumnDisplayName(ColumnHeaders[i].Name));
			}

			return returnedFile;
		}

		public string GetColumnDisplayName(string name)
		{
			var column = ColumnHeaders.FirstOrDefault(c => c.Name == name);
			if (column != null)
				return column.GetCustomAttribute<DescriptionAttribute>()?.Description ?? column.Name;
			return string.Empty;
		}
	}
}