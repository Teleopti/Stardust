using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Badge;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
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

		public void Handle(RecalculateBadgeEvent @event)
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

		[UnitOfWork]
		protected virtual void HandleJob(RecalculateBadgeEvent @event)
		{
			_calculateBadges.RemoveAgentBadges(@event.Period);
			foreach (var date in @event.Period.DayCollection())
			{
				_performBadgeCalculation.Calculate(_currentBusinessUnit.Current().Id.GetValueOrDefault(), date.Date);
			}
		}

		[UnitOfWork]
		protected virtual void SaveJobResultDetail(RecalculateBadgeEvent @event, Exception e)
		{
			var result = _jobResultRepository.FindWithNoLock(@event.JobResultId);
			var detail = new JobResultDetail(DetailLevel.Error, e.Message, DateTime.UtcNow, e);
			result.AddDetail(detail);
			result.FinishedOk = true;

			_feedback.SendProgress($"Added Job Result Detail. Detail level: {DetailLevel.Error}, message: {e.Message}, exception: {e}");
		}
	}
}
