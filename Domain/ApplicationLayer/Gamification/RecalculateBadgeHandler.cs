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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Gamification
{
	public class RecalculateBadgeHandler : IHandleEvent<RecalculateBadgeEvent>, IRunOnStardust
	{
		private readonly CalculateBadges _calculateBadges;
		private readonly IPerformBadgeCalculation _performBadgeCalculation;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;
		private readonly IJobResultRepository _jobResultRepository;
		private readonly IStardustJobFeedback _feedback;

		private static readonly ILog Logger = LogManager.GetLogger(typeof(RecalculateBadgeHandler));

		public RecalculateBadgeHandler(CalculateBadges calculateBadges, IPerformBadgeCalculation performBadgeCalculation, ICurrentBusinessUnit currentBusinessUnit, IJobResultRepository jobResultRepository, IStardustJobFeedback feedback)
		{
			_calculateBadges = calculateBadges;
			_performBadgeCalculation = performBadgeCalculation;
			_currentBusinessUnit = currentBusinessUnit;
			_jobResultRepository = jobResultRepository;
			_feedback = feedback;
		}

		[AsSystem, UnitOfWork]
		public virtual void Handle(RecalculateBadgeEvent @event)
		{
			try
			{
				HandleJob(@event);
			}
			catch (Exception e)
			{
				Logger.Error(e);
				saveJobResultDetail(@event.JobResultId, DetailLevel.Error, e.Message, e);
			}
		}

		protected void HandleJob(RecalculateBadgeEvent @event)
		{
			var result = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			_calculateBadges.RemoveAgentBadges(result.Period);
			foreach (var date in result.Period.DayCollection())
			{
				_performBadgeCalculation.Calculate(_currentBusinessUnit.Current().Id.GetValueOrDefault(), date.Date);
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
