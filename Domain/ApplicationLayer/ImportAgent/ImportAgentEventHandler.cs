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
		private readonly IFileProcessor _fileProcessor;
		private readonly ITenantUserPersister _tenantUserPersister;
		private static bool _hasException = false;
		private readonly IStardustJobFeedback _feedback;
		public ImportAgentEventHandler(
			IJobResultRepository jobResultRepository,
			IFileProcessor fileProcessor, ITenantUserPersister tenantUserPersister, IStardustJobFeedback feedback)
		{
			_jobResultRepository = jobResultRepository;
			_fileProcessor = fileProcessor;
			_tenantUserPersister = tenantUserPersister;
			_feedback = feedback;
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
				_feedback.SendProgress($"An unexpected exception happened.");
				SaveJobResultDetail(@event, e);
				_tenantUserPersister.RollbackAllPersistedTenantUsers();
				//throw;
			}
			

		}

		[UnitOfWork]
		protected virtual void SaveJobResultDetail(ImportAgentEvent @event, Exception ex)
		{
			var jobResult = getJobResult(@event);
			saveJobResultDetail(jobResult, ex.Message, DetailLevel.Error, ex);
		}

		[UnitOfWork]
		protected virtual bool IfNeedRejectJob(ImportAgentEvent @event)
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

		[UnitOfWork]
		protected virtual void HandleJob(ImportAgentEvent @event)
		{
			var defaults = @event.Defaults;
			var jobResult = getJobResult(@event);
			var owner = jobResult.Owner;

			JobResultArtifact inputFile;
			var errorMsg = validateJobInputArtifact(jobResult, out inputFile);
			_feedback.SendProgress($"Done input artifact validation!");
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
			_feedback.SendProgress($"Done input artifact process!");
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
			if (_hasException) throw new Exception(); //only for test

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
			_feedback.SendProgress($"Done job artifact preparation!");

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

			_feedback.SendProgress($"Done with adding job process detail, detail level:{level}, message:{detail.Message}.");
		}

		public static void HasException()
		{
			_hasException = true;
		}
		public static void ResetException()
		{
			_hasException = false;
		}
	}
}
