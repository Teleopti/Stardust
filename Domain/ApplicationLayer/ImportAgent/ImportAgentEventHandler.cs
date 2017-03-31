using System;
using System.Linq;
using NPOI.HSSF.UserModel;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentEventHandler : IHandleEvent<ImportAgentEvent>,
		IRunOnStardust
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IFileProcessor _fileProcessor;
		public ImportAgentEventHandler(
			IJobResultRepository jobResultRepository,
			IFileProcessor fileProcessor,
			INow now
		 )
		{
			_jobResultRepository = jobResultRepository;
			_fileProcessor = fileProcessor;
		}


		public void Handle(ImportAgentEvent @event)
		{
			var jobResult = _jobResultRepository.Get(@event.JobResultId);
			UpdateJobVersion(jobResult);
			if (IfNeedRejectJob(jobResult))
			{
				return;
			}
			HandleJob(jobResult, @event.Defaults);
		}

		[UnitOfWork]
		protected virtual void UpdateJobVersion(IJobResult jobResult)
		{
			var currentVersion = jobResult.Version.GetValueOrDefault();
			jobResult.SetVersion(++currentVersion);
		}

		[AsSystem]
		[UnitOfWork]
		protected virtual void HandleJob(IJobResult jobResult, ImportAgentDefaults defaults)
		{
			JobResultArtifact inputFile;
			var errorMsg = validateJobInputArtifact(jobResult, out inputFile);
			if (!errorMsg.IsNullOrEmpty())
			{
				saveJobResultDetail(jobResult, errorMsg, DetailLevel.Error);
				return;
			}
			var fileData = new FileData
			{
				Data = inputFile.Content,
				FileName = inputFile.Name
			};

			var processResult = _fileProcessor.Process(fileData, defaults);
			if (!processResult.ErrorMessages.IsNullOrEmpty())
			{
				saveJobResultDetail(jobResult, string.Join(", ", processResult.ErrorMessages), DetailLevel.Error);
				return;
			}

			saveJobArtifacts(jobResult, fileData, processResult);
			saveJobResultDetail(jobResult,
			$"success count:{processResult.SucceedAgents?.Count ?? 0}, failed count:{processResult.FaildAgents?.Count ?? 0}, warning count:{processResult.WarningAgents?.Count ?? 0}",
			processResult.DetailLevel);
		}

		private string validateJobInputArtifact(IJobResult jobResult, out JobResultArtifact inputFile)
		{
			inputFile = null;
			if (jobResult.Artifacts.IsNullOrEmpty())
			{
				return Resources.NoInput;
			}

			if (jobResult.Artifacts.Count > 1)
			{
				return Resources.InvalidInput;
			}

			inputFile = jobResult.Artifacts.FirstOrDefault(a => a.Category == JobResultArtifactCategory.Input);
			if (inputFile?.Content == null || inputFile.Content.Length == 0)
			{
				return Resources.InvalidInput;
			}
			return string.Empty;
		}

		private void saveJobArtifacts(IJobResult jobResult, FileData fileData, AgentFileProcessResult processResult)
		{
			var fileNameLength = fileData.FileName.Length;
			var fileTypeIndex = fileData.FileName.LastIndexOf(".");
			string fileName = fileData.FileName.Substring(0, fileNameLength - fileTypeIndex);
			string fileType = fileData.FileName.Substring(fileTypeIndex + 1, fileNameLength - fileTypeIndex - 1);
			var isXlsx = fileType.Equals("xlsx", StringComparison.OrdinalIgnoreCase);

			if (!processResult.FaildAgents.IsNullOrEmpty())
			{
				var faildFile = _fileProcessor.CreateFileForInvalidAgents(processResult.FaildAgents, isXlsx);

				jobResult.AddArtifact(new JobResultArtifact(
					JobResultArtifactCategory.OutputError,
					$"{fileName}_error.{fileType}",
					faildFile.ToArray()));

			}
			if (!processResult.WarningAgents.IsNullOrEmpty())
			{
				var warningFile = _fileProcessor.CreateFileForInvalidAgents(processResult.WarningAgents, isXlsx);

				jobResult.AddArtifact(new JobResultArtifact(
					JobResultArtifactCategory.OutputWarning,
					$"{fileName}_warning.{fileType}",
					warningFile.ToArray()));
			}


		}

		private bool IfNeedRejectJob(IJobResult jobResult)
		{
			var version = jobResult.Version;
			if (version.GetValueOrDefault() > 1)
			{
				return true;
			}
			return false;
		}


		private void saveJobResultDetail(IJobResult result, string message, DetailLevel level = DetailLevel.Info, Exception exception = null)
		{
			var detail = new JobResultDetail(level, message, DateTime.UtcNow, exception);
			result.AddDetail(detail);
			result.FinishedOk = true;
		}

	}
}
