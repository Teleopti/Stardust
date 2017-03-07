using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Web.Areas.People.Core.Models;
using Teleopti.Ccc.Web.Areas.People.Core.Persisters;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class FileProcessor :IFileProcessor
	{
		private readonly IImportAgentFileValidator _fileValidator;
		private readonly IAgentPersister _agentPersister;

		public FileProcessor(IImportAgentFileValidator fileValidator, IAgentPersister agentPersister)
		{
			_fileValidator = fileValidator;
			_agentPersister = agentPersister;
		}

		public IList<AgentExtractionResult> ProcessWorkbook(IWorkbook workbook)
		{
			var extractedResult = _fileValidator.ExtractAgentInfoValues(workbook);
			_agentPersister.Persist(extractedResult);
			return extractedResult.Where(r => r.ErrorMessages.Any()).ToList();
		}

		public IList<string> ValidateWorkbook(IWorkbook workbook)
		{
			var columnHearders = _fileValidator.ExtractColumnNames(workbook);
			return _fileValidator.ValidateColumnNames(columnHearders);
		}

		public IWorkbook ParseFiles(HttpContent content)
		{
			var fileName = content.Headers.ContentDisposition.FileName.Trim('\"');
			if (!fileName.ToLower().EndsWith("xls") && !fileName.ToLower().EndsWith("xlsx"))
				return null;
			var dataStream = content.ReadAsStreamAsync().Result;
			return fileName.ToLower().EndsWith("xlsx")
				? (IWorkbook)new XSSFWorkbook(dataStream)
				: new HSSFWorkbook(dataStream);
		}

		public MemoryStream CreateFileForInvalidAgents( IList<AgentExtractionResult> agents, bool isXlsx)
		{
			const string invalidUserSheetName = "Agents";
			var ms = new MemoryStream();
			var agentTemplate = new AgentFileTemplate();
			var returnedFile = agentTemplate.GetTemplateWorkbook(invalidUserSheetName, isXlsx);
			var newSheet = returnedFile.GetSheet(invalidUserSheetName);
			newSheet.GetRow(0).CreateCell(agentTemplate.ColumnHeaderNames.Length).SetCellValue("ErrorMessage");

			for (var i = 0; i < agents.Count; i++)
			{
				var rawAgent = agents[i].Raw;
				var row = newSheet.CreateRow(i + 1);

				row.CreateCell(agentTemplate.ColumnHeaderMap["Firstname"]).SetCellValue(rawAgent.Firstname);
				row.CreateCell(agentTemplate.ColumnHeaderMap["Lastname"]).SetCellValue(rawAgent.Lastname);
				row.CreateCell(agentTemplate.ColumnHeaderMap["WindowsUser"]).SetCellValue(rawAgent.WindowsUser);
				row.CreateCell(agentTemplate.ColumnHeaderMap["ApplicationUserId"]).SetCellValue(rawAgent.ApplicationUserId);
				row.CreateCell(agentTemplate.ColumnHeaderMap["Password"]).SetCellValue(rawAgent.Password);
				row.CreateCell(agentTemplate.ColumnHeaderMap["Role"]).SetCellValue(rawAgent.Role);
				row.CreateCell(agentTemplate.ColumnHeaderMap["StartDate"]).SetCellValue(rawAgent.StartDate.Value);
				row.CreateCell(agentTemplate.ColumnHeaderMap["Organization"]).SetCellValue(rawAgent.Organization);
				row.CreateCell(agentTemplate.ColumnHeaderMap["Skill"]).SetCellValue(rawAgent.Skill);
				row.CreateCell(agentTemplate.ColumnHeaderMap["ExternalLogon"]).SetCellValue(rawAgent.ExternalLogon);
				row.CreateCell(agentTemplate.ColumnHeaderMap["Contract"]).SetCellValue(rawAgent.Contract);
				row.CreateCell(agentTemplate.ColumnHeaderMap["ContractSchedule"]).SetCellValue(rawAgent.ContractSchedule);
				row.CreateCell(agentTemplate.ColumnHeaderMap["PartTimePercentage"]).SetCellValue(rawAgent.PartTimePercentage);
				row.CreateCell(agentTemplate.ColumnHeaderMap["ShiftBag"]).SetCellValue(rawAgent.ShiftBag);
				row.CreateCell(agentTemplate.ColumnHeaderMap["SchedulePeriodType"]).SetCellValue(rawAgent.SchedulePeriodType);
				row.CreateCell(agentTemplate.ColumnHeaderMap["SchedulePeriodLength"]).SetCellValue(rawAgent.SchedulePeriodLength.Value);
				row.CreateCell(agentTemplate.ColumnHeaderMap["ErrorMessage"]).SetCellValue(string.Join(";", rawAgent.ErrorMessage));
			}
			returnedFile.Write(ms);
			return ms;
		}
	}

	public interface IFileProcessor
	{
		IList<AgentExtractionResult> ProcessWorkbook(IWorkbook workbook);
		IList<string> ValidateWorkbook(IWorkbook workbook);
		IWorkbook ParseFiles(HttpContent content);
		MemoryStream CreateFileForInvalidAgents(IList<AgentExtractionResult> agents, bool isXlsx);
	}
}