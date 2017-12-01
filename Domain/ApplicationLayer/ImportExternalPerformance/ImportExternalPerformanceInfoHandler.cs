using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance
{
	public class ImportExternalPerformanceInfoHandler : IHandleEvent<ImportExternalPerformanceInfoEvent>, IRunOnStardust
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IStardustJobFeedback _feedback;
		private readonly IImportJobArtifactValidator _validator;
		private readonly IExternalPerformanceInfoFileProcessor _externalPerformanceInfoFileProcessor;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportExternalPerformanceInfoHandler));

		public ImportExternalPerformanceInfoHandler(IJobResultRepository jobResultRepository, IStardustJobFeedback feedback, 
			IImportJobArtifactValidator validator, IExternalPerformanceInfoFileProcessor externalPerformanceInfoFileProcessor)
		{
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_validator = validator;
			_externalPerformanceInfoFileProcessor = externalPerformanceInfoFileProcessor;
		}

		[AsSystem]
		[TenantScope]
		public virtual void Handle(ImportExternalPerformanceInfoEvent @event)
		{
			try
			{
				HandleJob(@event);
			}
			catch (Exception e)
			{
				Logger.Error(e);
				saveErrorJobResultDetail(@event, e.Message, e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void HandleJob(ImportExternalPerformanceInfoEvent @event)
		{
			var jobResult = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			var inputFile = _validator.ValidateJobArtifact(jobResult, _feedback.SendProgress);

			if (inputFile == null)
			{
				_feedback.SendProgress("ImportExternalPerformanceInfoHandler: Job artifact validation has error, give up!");
				return;
			}

			var processResult = _externalPerformanceInfoFileProcessor.Process(
				new ImportFileData
				{
					Data = inputFile.Content,
					FileName = inputFile.Name
				}, 
				_feedback.SendProgress);
			//processResult.TimezoneForCreator = jobResult.Owner.PermissionInformation.DefaultTimeZone();
			//processResult.InputArtifactInfo = new { inputFile.FileName, inputFile.FileType };

			_feedback.SendProgress($"Done agent persist {processResult.TotalRecordCount}.");

			if (processResult.HasError)
			{
				var msg = string.Join(", ", processResult.ErrorMessages);
				_feedback.SendProgress($"Extract file has error:{msg}.");
				saveErrorJobResultDetail(@event, msg, null);
				return;
			}
			//SaveJobArtifactsAndUpdateJobResultDetail(@event, processResult.InputArtifactInfo, processResult);
		}

		private void saveErrorJobResultDetail(ImportExternalPerformanceInfoEvent @event, string message, Exception exception)
		{
			var result = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			var detail = new JobResultDetail(DetailLevel.Error, message, DateTime.UtcNow, exception);
			result.AddDetail(detail);
			result.FinishedOk = true;

			_feedback.SendProgress($"ImportExternalPerformanceInfoHandler Done with adding job process detail, detail level:{DetailLevel.Error}, message:{message}, exception: {exception}.");
		}
	}
}
