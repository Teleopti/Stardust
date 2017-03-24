using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class FileProcessor : IFileProcessor
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

		public int GetNumberOfRecordsInSheet(ISheet sheet)
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

		public IList<AgentExtractionResult> ProcessSheet(ISheet sheet, ImportAgentDefaults defaultValues = null)
		{

			_fileValidator.SetDefaultValues(defaultValues);

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


		public MemoryStream CreateFileForInvalidAgents(IList<AgentExtractionResult> agents, bool isXlsx)
		{
			const string invalidUserSheetName = "Agents";
			var ms = new MemoryStream();
			var agentTemplate = new AgentFileTemplate();
			var returnedFile = agentTemplate.GetTemplateWorkbook(invalidUserSheetName, isXlsx);
			var newSheet = returnedFile.GetSheet(invalidUserSheetName);
			var columnsCount = agentTemplate.ColumnHeaderNames.Length;
			var errorMessageColumnIndex = columnsCount;
			var warningMessageColumnIndex = columnsCount + 1;

			newSheet.GetRow(0).CreateCell(errorMessageColumnIndex).SetCellValue("ErrorMessage");
			newSheet.GetRow(0).CreateCell(warningMessageColumnIndex).SetCellValue("WarningMessage");

			for (var i = 0; i < agents.Count; i++)
			{
				var sourceRow = agents[i].Row;
				var newRow = newSheet.CreateRow(i + 1);
				sourceRow.CopyTo(newRow, 0, columnsCount - 1);
				newRow.CreateCell(errorMessageColumnIndex).SetCellValue(string.Join(";", agents[i].Feedback.ErrorMessages));
				newRow.CreateCell(warningMessageColumnIndex).SetCellValue(string.Join(";", agents[i].Feedback.WarningMessages));
			}
			returnedFile.Write(ms);
			return ms;
		}


	}

	public interface IFileProcessor
	{
		IList<AgentExtractionResult> ProcessSheet(ISheet sheet, ImportAgentDefaults defaultValues = null);
		int GetNumberOfRecordsInSheet(ISheet sheet);
		IList<string> ValidateWorkbook(IWorkbook workbook);
		IWorkbook ParseFile(FileData fileData);
		MemoryStream CreateFileForInvalidAgents(IList<AgentExtractionResult> agents, bool isXlsx);
	}
}