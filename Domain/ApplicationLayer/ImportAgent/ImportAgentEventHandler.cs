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
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentEventHandler : IHandleEvent<ImportAgentEvent>,
		IRunOnStardust
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IWorkbookHandler _workbookHandler;
		private readonly IFileProcessor _fileProcessor;
		private readonly IStardustJobFeedback _feedback;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public ImportAgentEventHandler(
			IJobResultRepository jobResultRepository,
			IWorkbookHandler workbookHandler,
			IFileProcessor fileProcessor,
			IStardustJobFeedback feedback,
			ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_jobResultRepository = jobResultRepository;
			_workbookHandler = workbookHandler;
			_fileProcessor = fileProcessor;
			_feedback = feedback;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		[AsSystem]
		[TenantScope]
		public virtual void Handle(ImportAgentEvent @event)
		{
			if (IfNeedRejectJob(@event))
			{
				return;
			}
			try
			{
				HandleJob(@event);
			}
			catch (Exception e)
			{
				SaveJobResultDetail(@event, e);
			}
		}


		[UnitOfWork]
		protected virtual bool IfNeedRejectJob(ImportAgentEvent @event)
		{
			var jobResult = getJobResult(@event);
			if (jobResult == null)
			{
				_feedback.SendProgress("Can not found the job.");
				return true;
			}
			var version = jobResult.Version;
			if (version.GetValueOrDefault() > 1)
			{
				return true;
			}
			var currentVersion = jobResult.Version.GetValueOrDefault();
			jobResult.SetVersion(++currentVersion);
			return false;
		}


		protected virtual void HandleJob(ImportAgentEvent @event)
		{
			var inputFile = GetAndCheckInputArtifact(@event);
			if (inputFile == null) return;
			var processResult = processFile(@event, inputFile);
			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.Feedback.ErrorMessages);
				_feedback.SendProgress($"Extract file has error:{msg}.");
				SaveJobResultDetail(@event, msg, DetailLevel.Error);
				return;
			}
			SaveJobArtifactsAndUpdateJobResultDetail(@event, inputFile, processResult);
		}

		[UnitOfWork]
		protected virtual JobResultArtifact GetAndCheckInputArtifact(ImportAgentEvent @event)
		{
			var jobResult = getJobResult(@event);
			JobResultArtifact inputFile;
			_feedback.SendProgress($"Start to do input artifact validation!");
			var errorMsg = validateJobInputArtifact(jobResult, out inputFile);
			_feedback.SendProgress($"Done input artifact validation!");
			if (!errorMsg.IsNullOrEmpty())
			{
				saveJobResultDetail(jobResult, errorMsg, DetailLevel.Error);
				_feedback.SendProgress($"Input artifact validate has error:{errorMsg}");
				return null;
			}

			return inputFile;
		}

		[UnitOfWork]
		protected virtual void SaveJobArtifactsAndUpdateJobResultDetail(ImportAgentEvent @event,
			JobResultArtifact inputArtifact, AgentFileProcessResult processResult)
		{
			var jobResult = getJobResult(@event);
			_feedback.SendProgress($"Start to save job artifact!");


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
			_feedback.SendProgress($"Save job artifact success!");

			saveJobResultDetail(jobResult,
				processResult.GetSummaryMessage(),
				processResult.DetailLevel);

		}

		private AgentFileProcessResult processFile(ImportAgentEvent @event, JobResultArtifact inputFile)
		{

			var timeZoneInfo = GetTimezoneInfo(@event);

			var processResult = _fileProcessor.Process(new FileData
			{
				Data = inputFile.Content,
				FileName = inputFile.Name
			}, timeZoneInfo, @event.Defaults, _feedback.SendProgress);

			if (processResult.HasError)
			{
				return processResult;
			}
			_feedback.SendProgress($"Done agent persist {processResult.RawResults.Count}.");
			return processResult;
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
			if ((inputFile?.Content?.Length ?? 0) == 0)
			{
				return Resources.InvalidInput;
			}
			return string.Empty;
		}


		private IJobResult getJobResult(ImportAgentEvent @event)
		{
			return _jobResultRepository.FindWithNoLock(@event.JobResultId);
		}

		[UnitOfWork]
		protected virtual TimeZoneInfo GetTimezoneInfo(ImportAgentEvent @event)
		{
			var jobResult = getJobResult(@event);
			return jobResult.Owner.PermissionInformation.DefaultTimeZone();
		}

		[UnitOfWork]
		protected virtual void SaveJobResultDetail(ImportAgentEvent @event, string message, DetailLevel level = DetailLevel.Info,
			Exception exception = null)
		{
			saveJobResultDetail(getJobResult(@event), message, level, exception);
		}


		private void saveJobResultDetail(IJobResult result, string message, DetailLevel level = DetailLevel.Info, Exception exception = null)
		{
			var detail = new JobResultDetail(level, message, DateTime.UtcNow, exception);
			result.AddDetail(detail);
			result.FinishedOk = true;

			_feedback.SendProgress($"Done with adding job process detail, detail level:{level}, message:{detail.Message}.");
		}

		[UnitOfWork]
		protected virtual void SaveJobResultDetail(ImportAgentEvent @event, Exception ex)
		{
			var jobResult = getJobResult(@event);
			saveJobResultDetail(jobResult, ex.Message, DetailLevel.Error, ex);
		}
		
	}
}
