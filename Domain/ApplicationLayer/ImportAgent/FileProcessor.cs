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

		public FileProcessor(IAgentPersister agentPersister, IWorkbookHandler workbookHandler)
		{
			_agentPersister = agentPersister;
			_workbookHandler = workbookHandler;
		}

		public AgentFileProcessResult Process(FileData fileData, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null)
		{
			var workbook = ParseFile(fileData);
			return Process(workbook, timezone, defaultValues);
		}

		public AgentFileProcessResult Process(IWorkbook workbook, TimeZoneInfo timezone,
			ImportAgentDefaults defaultValues = null)
		{
		
			var processResult = new AgentFileProcessResult();
			if (workbook == null)
			{
				processResult.DetailLevel = DetailLevel.Error;
				processResult.ErrorMessages.Add(Resources.InvalidInput);
				return processResult;
			}
			var extractedResult = _workbookHandler.Process(workbook, defaultValues);

			if (extractedResult.HasError)
			{
				processResult.DetailLevel = DetailLevel.Error;
				processResult.ErrorMessages.AddRange(extractedResult.Feedback.ErrorMessages);
				return processResult;
			}

			var rawResults = extractedResult.RawResults;
			_agentPersister.Persist(rawResults, timezone);
			processResult.WarningAgents.AddRange(rawResults.Where(a => a.DetailLevel == DetailLevel.Warning));
			processResult.FailedAgents.AddRange(rawResults.Where(a => a.DetailLevel == DetailLevel.Error).ToList());
			processResult.SucceedAgents.AddRange(rawResults.Where(a => a.DetailLevel == DetailLevel.Info).ToList());

			processResult.DetailLevel = !processResult.FailedAgents.IsNullOrEmpty()
				? DetailLevel.Error
				: processResult.WarningAgents.IsNullOrEmpty() ? DetailLevel.Warning : DetailLevel.Info;

			return processResult;
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

	}

	public interface IFileProcessor
	{
		MemoryStream CreateFileForInvalidAgents(IList<AgentExtractionResult> agents, bool isXlsx);
		AgentFileProcessResult Process(FileData fileData, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null);
		AgentFileProcessResult Process(IWorkbook workbook, TimeZoneInfo timezone,
			ImportAgentDefaults defaultValues = null);
		IWorkbook ParseFile(FileData fileData);
	}
}