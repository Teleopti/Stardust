using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class FileProcessor : IFileProcessor
	{
		private readonly IAgentPersister _agentPersister;
		private readonly IWorkbookHandler _workbookHandler;
		private const int MAXIMUM_NUMBER_OF_AGENT_ROWS_REACH = 5000;
		public FileProcessor(IAgentPersister agentPersister, IWorkbookHandler workbookHandler)
		{
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



		public IList<AgentExtractionResult> ProcessSheet(ISheet sheet, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null)
		{
			var extractedResult = _workbookHandler.ProcessSheet(sheet, defaultValues);
			_agentPersister.Persist(extractedResult, timezone);
			return extractedResult.Where(r => r.Feedback.ErrorMessages.Any() || r.Feedback.WarningMessages.Any()).ToList();
		}


		public AgentFileProcessResult Process(FileData fileData, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null)
		{
			var processResult = new AgentFileProcessResult();

			var workbook = this.ParseFile(fileData);
			var sheet = workbook.GetSheetAt(0);

			var error = this.ValidateWorkbook(workbook);
			if (!error.IsNullOrEmpty())
			{
				processResult.DetailLevel = DetailLevel.Error;
				processResult.ErrorMessages.Add(error);
				return processResult;
			}

			var extractedResults = _workbookHandler.ProcessSheet(sheet, defaultValues);
			_agentPersister.Persist(extractedResults, timezone);

			processResult.WarningAgents = extractedResults.Where(a => a.DetailLevel == DetailLevel.Warning).ToList();
			processResult.FailedAgents = extractedResults.Where(a => a.DetailLevel == DetailLevel.Error).ToList();
			processResult.SucceedAgents = extractedResults.Where(a => a.DetailLevel == DetailLevel.Info).ToList();

			processResult.DetailLevel = !processResult.FailedAgents.IsNullOrEmpty()
				? DetailLevel.Error
				: processResult.WarningAgents.IsNullOrEmpty() ? DetailLevel.Warning : DetailLevel.Info;

			return processResult;
		}




		public string ValidateWorkbook(IWorkbook workbook)
		{
			if (workbook.NumberOfSheets == 0)
			{
				return Resources.InvalidInput;
			}

			var sheet = workbook.GetSheetAt(0);

			if (sheet.LastRowNum == 0 && sheet.GetRow(0) == null)
			{
				return Resources.EmptyFile;
			}

			var errorMsg = "";
			if (!(errorMsg = this.ValidateSheetColumnHeader(workbook)).IsNullOrEmpty())
			{
				return errorMsg;
			}
			var numberOfRecords = GetNumberOfRecordsInSheet(sheet);
			if (numberOfRecords == 0)
			{
				return Resources.NoDataAvailable;
			}

			if (numberOfRecords > MAXIMUM_NUMBER_OF_AGENT_ROWS_REACH)
			{
				return string.Format(Resources.MaximumNumberOfAgentRowsReach, MAXIMUM_NUMBER_OF_AGENT_ROWS_REACH);
			}

			return string.Empty;

		}

		public string ValidateSheetColumnHeader(IWorkbook workbook)
		{

			var missingCols = _workbookHandler.ValidateSheetColumnHeader(workbook);
			if (missingCols.IsNullOrEmpty())
			{
				return string.Empty;
			}
			return string.Format(Resources.MissingColumnX, string.Join(", ", missingCols));
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
			var columnsCount = agentTemplate.ColumnHeaders.Length;
			var errorMessageColumnIndex = columnsCount;
			var warningMessageColumnIndex = columnsCount + 1;

			var hasErrorMessages = agents.Any(a => !a.Feedback.ErrorMessages.IsNullOrEmpty());
			var hasWarningMessages = agents.Any(a => !a.Feedback.WarningMessages.IsNullOrEmpty());

			if (hasErrorMessages)
			{
				newSheet.GetRow(0).CreateCell(errorMessageColumnIndex).SetCellValue("ErrorMessage");
			}
			if (hasWarningMessages)
			{
				newSheet.GetRow(0).CreateCell(warningMessageColumnIndex).SetCellValue("WarningMessage");
			}

			for (var i = 0; i < agents.Count; i++)
			{
				var sourceRow = agents[i].Row;
				var newRow = newSheet.CreateRow(i + 1);
				sourceRow.CopyTo(newRow, 0, columnsCount - 1);
				if (hasErrorMessages)
				{
					newRow.CreateCell(errorMessageColumnIndex).SetCellValue(string.Join(";", agents[i].Feedback.ErrorMessages));
				}
				if (hasWarningMessages)
				{
					newRow.CreateCell(warningMessageColumnIndex).SetCellValue(string.Join(";", agents[i].Feedback.WarningMessages));
				}
			}
			returnedFile.Write(ms);
			return ms;
		}


	}

	public interface IFileProcessor
	{
		IList<AgentExtractionResult> ProcessSheet(ISheet sheet, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null);
		int GetNumberOfRecordsInSheet(ISheet sheet);
		string ValidateSheetColumnHeader(IWorkbook workbook);
		IWorkbook ParseFile(FileData fileData);
		MemoryStream CreateFileForInvalidAgents(IList<AgentExtractionResult> agents, bool isXlsx);
		string ValidateWorkbook(IWorkbook workbook);
		AgentFileProcessResult Process(FileData fileData, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null);
	}
}