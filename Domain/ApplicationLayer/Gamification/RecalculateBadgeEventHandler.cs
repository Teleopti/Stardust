using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Gamification
{
	public class RecalculateBadgeEventHandler : IHandleEvent<RecalculateBadgeEvent>, IRunOnStardust
	{
		private readonly CalculateBadges _calculateBadges;
		private readonly IPerformBadgeCalculation _performBadgeCalculation;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IStardustJobFeedback _feedback;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(RecalculateBadgeEventHandler));

		public RecalculateBadgeEventHandler(CalculateBadges calculateBadges, IPerformBadgeCalculation performBadgeCalculation, IJobResultRepository jobResultRepository, IStardustJobFeedback feedback)
		{
			_calculateBadges = calculateBadges;
			_performBadgeCalculation = performBadgeCalculation;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
		}

		[AsSystem]
		public virtual void Handle(RecalculateBadgeEvent @event)
		{
			try
			{
				var period = new DateOnlyPeriod(new DateOnly(@event.StartDate), new DateOnly(@event.EndDate));
				RemoveExistingAgentBadges(period);
				HandleJob(@event, period);
			}
			catch (Exception e)
			{
				Logger.Error(e);
				SaveErrorJobResultDetail(@event.JobResultId, e);
				throw;
			}
		}

		[UnitOfWork]
		protected virtual void SaveErrorJobResultDetail(Guid jobResultId, Exception e)
		{
			saveJobResultDetail(jobResultId, DetailLevel.Error, e.Message, e);
		}

		[UnitOfWork]
		protected virtual void RemoveExistingAgentBadges(DateOnlyPeriod period)
		{
			_calculateBadges.RemoveAgentBadges(period);
		}

		[UnitOfWork]
		protected virtual void HandleJob(RecalculateBadgeEvent @event, DateOnlyPeriod period)
		{
			foreach (var date in period.DayCollection())
			{
				_performBadgeCalculation.Calculate(@event.LogOnBusinessUnitId, date.Date);
			}

			saveJobResultDetail(@event.JobResultId, DetailLevel.Info, "Recalculated Agent Badges", null);
		}

		private void saveJobResultDetail(Guid jobResultId, DetailLevel level, string message, Exception e)
		{
			var result = _jobResultRepository.FindWithNoLock(jobResultId);
			var detail = new JobResultDetail(level, message, DateTime.UtcNow, e);
			result.AddDetail(detail);
			result.FinishedOk = true;

			_feedback.SendProgress($"Added Job Result Detail. Detail level: {level}, message: {message}, exception: {e}");
		}
	}
}
