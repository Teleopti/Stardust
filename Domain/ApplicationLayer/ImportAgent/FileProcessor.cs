using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class FileProcessor : IFileProcessor
	{
		private readonly IAgentPersister _agentPersister;
		private readonly IWorkbookHandler _workbookHandler;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private const int BATCH_SIZE = 200;
		private bool hasException = false;
		public FileProcessor(
			IAgentPersister agentPersister,
			IWorkbookHandler workbookHandler,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_agentPersister = agentPersister;
			_workbookHandler = workbookHandler;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public AgentFileProcessResult Process(FileData fileData, ImportAgentDefaults defaultValues = null, Action<string> sendProgress = null)
		{
			var workbook = ParseFile(fileData);

			if (sendProgress == null)
			{
				sendProgress = (msg) => { };
			}
			var fileProcessResult = extractFileWithoutSeperateSession(workbook, defaultValues, sendProgress);
			return fileProcessResult;
		}

		public void BatchPersist(TimeZoneInfo timezone, Action<string> sendProgress, AgentExtractionResult[] extractedAgents)
		{
			var result = new
			{
				AgentIds = new List<Guid>(),
				TenantUserIds = new List<Guid>()
			};
			try
			{
				var i = 0;
				foreach (var agents in extractedAgents.Batch(BATCH_SIZE))
				{
					sendProgress($"Start to persist agent {BATCH_SIZE * i} - {BATCH_SIZE * i + agents.Count()}.");
					using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
					{
						var batchResult = _agentPersister.Persist(agents, timezone);
						uow.PersistAll();
						result.AgentIds.AddRange(batchResult.AgentIds);
						result.TenantUserIds.AddRange(batchResult.TenantUserIds);
					}
					i++;
				}
				if (hasException)
				{
					throw new Exception("only for test");
				}
			}
			catch (Exception)
			{
				sendProgress($"An unexpected exception happened, rollback persisted agents:{string.Join(",", result.AgentIds)}, tenant user:{string.Join(",", result.TenantUserIds)}");
				rollbackAllPersisted(result);
				throw;
			}
		}

		public AgentFileProcessResult Process(FileData fileData, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null, Action<string> sendProgress = null)
		{
			var workbook = ParseFile(fileData);
			return Process(workbook, timezone, defaultValues, sendProgress);
		}


		public AgentFileProcessResult Process(IWorkbook workbook, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null,
			Action<string> sendProgress = null)
		{
			if (sendProgress == null)
			{
				sendProgress = (msg) => { };
			}
			var processResult = extractFile(workbook, defaultValues, sendProgress);
			if (processResult.HasError)
			{
				return processResult;
			}

			BatchPersist(timezone, sendProgress, processResult.RawResults.ToArray());

			return processResult;

		}

		private AgentFileProcessResult extractFileWithoutSeperateSession(IWorkbook workbook, ImportAgentDefaults defaultValues,
			Action<string> sendProgress)
		{
			sendProgress($"Start to extract file.");
			var processResult = _workbookHandler.Process(workbook, defaultValues);
			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.Feedback.ErrorMessages);
				sendProgress($"Extract file has error:{msg}.");
				return processResult;
			}
			sendProgress($"Extract file succeed.");
			return processResult;
		}

		private AgentFileProcessResult extractFile(IWorkbook workbook, ImportAgentDefaults defaultValues, Action<string> sendProgress)
		{
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				sendProgress($"Start to extract file.");
				var processResult = _workbookHandler.Process(workbook, defaultValues);
				if (processResult.HasError)
				{
					var msg = string.Join(", ", processResult.Feedback.ErrorMessages);
					sendProgress($"Extract file has error:{msg}.");
					return processResult;
				}
				sendProgress($"Extract file succeed.");
				return processResult;
			}
		}


		private void rollbackAllPersisted(dynamic persisted)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_agentPersister.Rollback(persisted);
				uow.PersistAll();
			}
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
				var endColumnIndex = sourceRow.LastCellNum < columnsCount ? sourceRow.LastCellNum - 1 : columnsCount - 1;
				sourceRow.CopyTo(newRow, 0, endColumnIndex);
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

		public void HasException()
		{
			hasException = true;
		}
		public void ResetException()
		{
			hasException = false;
		}
	}

	public interface IFileProcessor
	{
		MemoryStream CreateFileForInvalidAgents(IList<AgentExtractionResult> agents, bool isXlsx);
		AgentFileProcessResult Process(FileData fileData, ImportAgentDefaults defaultValues = null, Action<string> sendProgress = null);
		AgentFileProcessResult Process(FileData fileData, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null, Action<string> sendProgress = null);
		AgentFileProcessResult Process(IWorkbook workbook, TimeZoneInfo timezone, ImportAgentDefaults defaultValues = null, Action<string> sendProgress = null);
		IWorkbook ParseFile(FileData fileData);
		void BatchPersist(TimeZoneInfo timezone, Action<string> sendProgress, AgentExtractionResult[] extractedAgents);
	}
}