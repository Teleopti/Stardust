using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		private readonly IWorkbookHandler _workbookHandler;

		public FileProcessor(IImportAgentFileValidator fileValidator, IAgentPersister agentPersister, IWorkbookHandler workbookHandler)
		{
			_fileValidator = fileValidator;
			_agentPersister = agentPersister;
			_workbookHandler = workbookHandler;
		}

		public IList<AgentExtractionResult> ProcessSheet(ISheet sheet, ImportAgentFormData defaultValues = null)
		{
			if (defaultValues != null)
			{
				_fileValidator.SetDefaultValues(defaultValues);
			}
			var extractedResult = _workbookHandler.ProcessSheet(sheet);
			_agentPersister.Persist(extractedResult);
			return extractedResult.Where(r => r.Feedback.ErrorMessages.Any() || r.Feedback.WarningMessages.Any()).ToList();
		}
		
		public IList<string> ValidateWorkbook(IWorkbook workbook)
		{
			return _workbookHandler.ValidateSheetColumnHeader(workbook);
		}

		public IWorkbook ParseFile(FileData fileData)
		{
			if (fileData == null) return null;
			var fileName = fileData.FileName;
			if (!fileName.ToLower().EndsWith("xls") && !fileName.ToLower().EndsWith("xlsx"))
				return null;
			var dataStream = new MemoryStream(fileData.Data); 
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

			var errorMessageColumnIndex = agentTemplate.ColumnHeaderNames.Length;
			var warningMessageColumnIndex = agentTemplate.ColumnHeaderNames.Length + 1;

			newSheet.GetRow(0).CreateCell(errorMessageColumnIndex).SetCellValue("ErrorMessage");
			newSheet.GetRow(0).CreateCell(warningMessageColumnIndex).SetCellValue("WarningMessage");

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
				row.CreateCell(agentTemplate.ColumnHeaderMap["StartDate"]).SetCellValue(rawAgent.StartDate);
				row.CreateCell(agentTemplate.ColumnHeaderMap["Organization"]).SetCellValue(rawAgent.Organization);
				row.CreateCell(agentTemplate.ColumnHeaderMap["Skill"]).SetCellValue(rawAgent.Skill);
				row.CreateCell(agentTemplate.ColumnHeaderMap["ExternalLogon"]).SetCellValue(rawAgent.ExternalLogon);
				row.CreateCell(agentTemplate.ColumnHeaderMap["Contract"]).SetCellValue(rawAgent.Contract);
				row.CreateCell(agentTemplate.ColumnHeaderMap["ContractSchedule"]).SetCellValue(rawAgent.ContractSchedule);
				row.CreateCell(agentTemplate.ColumnHeaderMap["PartTimePercentage"]).SetCellValue(rawAgent.PartTimePercentage);
				row.CreateCell(agentTemplate.ColumnHeaderMap["ShiftBag"]).SetCellValue(rawAgent.ShiftBag);
				row.CreateCell(agentTemplate.ColumnHeaderMap["SchedulePeriodType"]).SetCellValue(rawAgent.SchedulePeriodType);
				row.CreateCell(agentTemplate.ColumnHeaderMap["SchedulePeriodLength"]).SetCellValue(rawAgent.SchedulePeriodLength);
				row.CreateCell(errorMessageColumnIndex).SetCellValue(string.Join(";", agents[i].Feedback.ErrorMessages));
				row.CreateCell(warningMessageColumnIndex).SetCellValue(string.Join(";",agents[i].Feedback.WarningMessages));
			}
			returnedFile.Write(ms);
			return ms;
		}
	}

	public interface IFileProcessor
	{
		IList<AgentExtractionResult> ProcessSheet(ISheet sheet, ImportAgentFormData defaultValues = null);
		IList<string> ValidateWorkbook(IWorkbook workbook);
		IWorkbook ParseFile(FileData fileData);
		MemoryStream CreateFileForInvalidAgents(IList<AgentExtractionResult> agents, bool isXlsx);
	}
}