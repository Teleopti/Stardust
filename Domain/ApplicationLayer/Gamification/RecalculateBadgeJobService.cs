using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ImportExternalPerformance;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Gamification
{
	public interface IRecalculateBadgeJobService
	{
		IJobResult CreateJob(DateTime start, DateTime end);
		IList<RecalculateBadgeJobResultDetail> GetJobsForCurrentBusinessUnit();
	}

	public class RecalculateBadgeJobService : IRecalculateBadgeJobService
	{
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly ILoggedOnUser _loggedOnUser;

		public RecalculateBadgeJobService(IJobResultRepository jobResultRepository, IEventPopulatingPublisher eventPublisher, ILoggedOnUser loggedOnUser)
		{
			_jobResultRepository = jobResultRepository;
			_eventPublisher = eventPublisher;
			_loggedOnUser = loggedOnUser;
		}

		public IJobResult CreateJob(DateTime start, DateTime end)
		{
			var jobResult = new JobResult(JobCategory.WebRecalculateBadge, new DateOnlyPeriod(new DateOnly(start), new DateOnly(end)), _loggedOnUser.CurrentUser(), DateTime.UtcNow);
			_jobResultRepository.Add(jobResult);
			_eventPublisher.Publish(new RecalculateBadgeEvent
			{
				JobResultId = jobResult.Id.GetValueOrDefault(),
				StartDate = start,
				EndDate = end
			});
			return jobResult;
		}

		public IList<RecalculateBadgeJobResultDetail> GetJobsForCurrentBusinessUnit()
		{
			var resultList = _jobResultRepository.LoadAllWithNoLock()
				.Where(r => r.JobCategory == JobCategory.WebRecalculateBadge)
				.OrderByDescending(r => r.Timestamp)
				.ToList();
			return resultList.Select(jobResult =>
			{
				var hasError = jobResult.HasError();
				var finished = jobResult.FinishedOk;
				var jobResultDetail = jobResult.Details.FirstOrDefault();
				var hasException = !(jobResultDetail?.ExceptionMessage.IsNullOrEmpty() ?? true)
								   || !(jobResultDetail?.InnerExceptionMessage.IsNullOrEmpty() ?? true);
				var errorMessage = hasError 
					? (hasException 
						? Resources.InternalErrorMsg
						: (jobResultDetail?.Message ?? Resources.TimedOutWhileWaitingforProcessing))
					: string.Empty;
				var status = GamificationJobStatus.InProgress;
				if (finished) status = GamificationJobStatus.Finished;
				if (hasError) status = GamificationJobStatus.Failed;
				
				return new RecalculateBadgeJobResultDetail
				{
					Id = jobResult.Id.GetValueOrDefault(),
					StartDate = jobResult.Period.StartDate.Utc(),
					EndDate = jobResult.Period.EndDate.Utc(),
					Owner = jobResult.Owner.Name.ToString(),
					CreateDateTime = jobResult.Timestamp.ToUniversalTime(),
					Status = status.ToString().ToLower(),
					HasError = hasError,
					ErrorMessage = errorMessage
				};
			}).ToList();
		}
	}
}
