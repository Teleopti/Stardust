using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon.Aspects;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportAgent
{
	public class ImportAgentEventHandler : IHandleEvent<ImportAgentEvent>,
		IRunOnStardust
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IFileProcessor _fileProcessor;
		private readonly IStardustJobFeedback _feedback;
		private readonly IImportJobArtifactValidator _validator;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportAgentEventHandler));
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public ImportAgentEventHandler(
			IJobResultRepository jobResultRepository,
			IFileProcessor fileProcessor,
			IStardustJobFeedback feedback, IImportJobArtifactValidator validator, ICurrentUnitOfWorkFactory currentUnitOfWork)
		{
			_jobResultRepository = jobResultRepository;
			_fileProcessor = fileProcessor;
			_feedback = feedback;
			_validator = validator;
			_currentUnitOfWorkFactory = currentUnitOfWork;
		}

		[AsSystem]
		[TenantScope]
		public virtual void Handle(ImportAgentEvent @event)
		{
			try
			{
				HandleJob(@event);
			}
			catch (Exception e)
			{
				Logger.Error(e);
				SaveJobResultDetail(@event, e);
				throw;
			}
		}

		protected virtual void HandleJob(ImportAgentEvent @event)
		{
			IJobResult jobResult = null;
			JobResultArtifact inputFile = null;
			AgentFileProcessResult processResult = null;
			using (_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				jobResult = _jobResultRepository.FindWithNoLock(@event.JobResultId);
				inputFile = _validator.ValidateJobArtifact(jobResult, _feedback.SendProgress);
				if (inputFile == null)
				{
					_feedback.SendProgress("Job artifact validation has error, give up!");
					return;
				}

				processResult = _fileProcessor.Process(new ImportFileData
				{
					Data = inputFile.Content,
					FileName = inputFile.Name
				}, @event.Defaults, _feedback.SendProgress);
				processResult.TimezoneForCreator = jobResult.Owner.PermissionInformation.DefaultTimeZone();
				processResult.InputArtifactInfo = new { inputFile.FileName, inputFile.FileType };
			}
			

			_fileProcessor.BatchPersist(processResult.TimezoneForCreator, _feedback.SendProgress, processResult.RawResults.ToArray());

			_feedback.SendProgress($"Done agent persist {processResult.RawResults.Count}.");

			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.Feedback.ErrorMessages);
				_feedback.SendProgress($"Extract file has error:{msg}.");
				SaveJobResultDetail(@event, msg, DetailLevel.Error);
				return;
			}
			SaveJobArtifactsAndUpdateJobResultDetail(@event, processResult.InputArtifactInfo, processResult);
		}

		[UnitOfWork]
		protected virtual void SaveJobArtifactsAndUpdateJobResultDetail(ImportAgentEvent @event,
			dynamic inputArtifactInfo, AgentFileProcessResult processResult)
		{
			var jobResult = getJobResult(@event);
			_feedback.SendProgress($"Start to save job artifact!");

			string fileName = inputArtifactInfo.FileName;
			string fileType = inputArtifactInfo.FileType;
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

		[UnitOfWork]
		protected virtual void SaveJobResultDetail(ImportAgentEvent @event, string message, DetailLevel level = DetailLevel.Info,
			Exception exception = null)
		{
			saveJobResultDetail(getJobResult(@event), message, level, exception);
		}

		[UnitOfWork]
		protected virtual void SaveJobResultDetail(ImportAgentEvent @event, Exception ex)
		{
			var jobResult = getJobResult(@event);
			saveJobResultDetail(jobResult, ex.Message, DetailLevel.Error, ex);
		}

		private void saveJobResultDetail(IJobResult result, string message, DetailLevel level = DetailLevel.Info, Exception exception = null)
		{
			var detail = new JobResultDetail(level, message, DateTime.UtcNow, exception);
			result.AddDetail(detail);
			result.FinishedOk = true;
			if (exception == null)
			{
				_feedback.SendProgress($"Done with adding job process detail, detail level:{level}, message:{detail.Message}.");
			}
			else
			{
				_feedback.SendProgress($"Done with adding job process detail, detail level:{level}, message:{detail.Message}, exception: {exception}.");
			}
		}

		private IJobResult getJobResult(ImportAgentEvent @event)
		{
			return _jobResultRepository.FindWithNoLock(@event.JobResultId);
		}
	}
}
