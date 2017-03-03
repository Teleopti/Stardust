using System.Collections.Generic;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Teleopti.Ccc.Web.Areas.People.Core.Models
{
	public class UserFileTemplate
	{
		public virtual string[] ColumnHeaderNames { get; } = {
			"Firstname",
			"Lastname",
			"WindowsUser",
			"ApplicationUserId",
			"Password",
			"Role"
		};

		public virtual IDictionary<string, int> ColumnHeaderMap { get; } = new Dictionary<string, int>(){
			{"Firstname", 0},
			{"Lastname", 1},
			{"WindowsUser", 2},
			{"ApplicationUserId", 3},
			{"Password", 4},
			{"Role", 5}
		};

		public virtual MemoryStream GetFileTemplateWithDemoData()
		{
			const string sheetName = "Users";
			var ms = new MemoryStream();
			var returnedFile = GetTemplateWorkbook(sheetName);
			var newSheet = returnedFile.GetSheet(sheetName);

			var demoUser = new RawUser
			{
				Firstname = "John",
				Lastname = "Smith",
				WindowsUser = "john.smith@teleopti.com",
				ApplicationUserId = "john.smith@teleopti.com",
				Password = "password",
				Role = "agent, \"London, Team Leader\""
			};

			var row = newSheet.CreateRow(1);

			row.CreateCell(ColumnHeaderMap["Firstname"]).SetCellValue(demoUser.Firstname);
			row.CreateCell(ColumnHeaderMap["Lastname"]).SetCellValue(demoUser.Lastname);
			row.CreateCell(ColumnHeaderMap["WindowsUser"]).SetCellValue(demoUser.WindowsUser);
			row.CreateCell(ColumnHeaderMap["ApplicationUserId"]).SetCellValue(demoUser.ApplicationUserId);
			row.CreateCell(ColumnHeaderMap["Password"]).SetCellValue(demoUser.Password);
			row.CreateCell(ColumnHeaderMap["Role"]).SetCellValue(demoUser.Role);
			returnedFile.Write(ms);
			return ms;
		}

		public IWorkbook GetTemplateWorkbook(string invalidUserSheetName, bool isXlsx=false)
		{
			var returnedFile = isXlsx ? (IWorkbook)new XSSFWorkbook() : new HSSFWorkbook();
			var newsheet = returnedFile.CreateSheet(invalidUserSheetName);

			var row = newsheet.CreateRow(0);
			for (var i = 0; i < ColumnHeaderNames.Length; i++)
			{
				row.CreateCell(i).SetCellValue(ColumnHeaderNames[i]);
			}

			return returnedFile;
		}
	}
}