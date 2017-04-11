using System;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentEventHandler : IHandleEvent<ImportAgentEvent>,
		IRunOnStardust
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IFileProcessor _fileProcessor;
		public ImportAgentEventHandler(
			IJobResultRepository jobResultRepository,
			IFileProcessor fileProcessor
		 )
		{
			_jobResultRepository = jobResultRepository;
			_fileProcessor = fileProcessor;
		}

		[AsSystem]
		[TenantScope]
		[UnitOfWork]
		public virtual void Handle(ImportAgentEvent @event)
		{
			if (IfNeedRejectJob(@event))
			{
				return;
			}
			HandleJob(@event);
		}

		protected bool IfNeedRejectJob(ImportAgentEvent @event)
		{
			var jobResult = getJobResult(@event);
			var version = jobResult.Version;
			if (version.GetValueOrDefault() > 1)
			{
				return true;
			}
			var currentVersion = jobResult.Version.GetValueOrDefault();
			jobResult.SetVersion(++currentVersion);
			return false;
		}
		
		protected void HandleJob(ImportAgentEvent @event)
		{
			var defaults = @event.Defaults;
			var jobResult = getJobResult(@event);
			var owner = jobResult.Owner;
			try
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
				var processResult = _fileProcessor.Process(fileData, owner.PermissionInformation.DefaultTimeZone(), defaults);
				if (!processResult.ErrorMessages.IsNullOrEmpty())
				{
					saveJobResultDetail(jobResult, string.Join(", ", processResult.ErrorMessages), DetailLevel.Error);
					return;
				}
				saveJobArtifacts(jobResult, inputFile, processResult);
				saveJobResultDetail(jobResult,
				processResult.GetSummaryMessage(),
				processResult.DetailLevel);
			}
			catch (Exception ex)
			{
				saveJobResultDetail(jobResult, ex.Message, DetailLevel.Error, ex);

			}
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
			if ((inputFile?.Content?.Length ?? 0 ) == 0)
			{
				return Resources.InvalidInput;
			}
			return string.Empty;
		}

		private void saveJobArtifacts(IJobResult jobResult, JobResultArtifact inputArtifact, AgentFileProcessResult processResult)
		{
			string fileName = inputArtifact.FileName;
			string fileType = inputArtifact.FileType;
			var isXlsx = fileType.Equals("xlsx", StringComparison.OrdinalIgnoreCase);

			if (!processResult.FailedAgents.IsNullOrEmpty())
			{
				var faildFile = _fileProcessor.CreateFileForInvalidAgents(processResult.FailedAgents, isXlsx);

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
		private IJobResult getJobResult(ImportAgentEvent @event)
		{
			return _jobResultRepository.Get(@event.JobResultId);
		}
		private void saveJobResultDetail(IJobResult result, string message, DetailLevel level = DetailLevel.Info, Exception exception = null)
		{
			var detail = new JobResultDetail(level, message, DateTime.UtcNow, exception);
			result.AddDetail(detail);
			result.FinishedOk = true;
		}

	}
}
