using System;
using log4net;
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
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportExternalPerformanceInfoHandler));

		public ImportExternalPerformanceInfoHandler(IJobResultRepository jobResultRepository, IStardustJobFeedback feedback, IImportJobArtifactValidator validator)
		{
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
			_validator = validator;
		}

		[AsSystem]
		[TenantScope]
		public virtual void Handle(ImportExternalPerformanceInfoEvent @event)
		{
			try
			{
				handleJob(@event);
			}
			catch (Exception e)
			{
				Logger.Error(e);
				saveErrorJobResultDetail(@event, e);
				throw;
			}
		}

		private void handleJob(ImportExternalPerformanceInfoEvent @event)
		{
			var jobResult = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			var processResult = _validator.ValidateJobArtifact(jobResult, _feedback.SendProgress);

			if (processResult == null)
			{
				_feedback.SendProgress("ImportExternalPerformanceInfoHandler: Job artifact validation has error, give up!");
				return;
			}

			//feedback error
			//save new and update exist
		}

		private void saveErrorJobResultDetail(ImportExternalPerformanceInfoEvent @event, Exception exception)
		{
			var result = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			var detail = new JobResultDetail(DetailLevel.Error, exception.Message, DateTime.UtcNow, exception);
			result.AddDetail(detail);
			result.FinishedOk = true;

			_feedback.SendProgress($"ImportExternalPerformanceInfoHandler Done with adding job process detail, detail level:{DetailLevel.Error}, message:{detail.Message}, exception: {exception}.");
		}
	}
}
