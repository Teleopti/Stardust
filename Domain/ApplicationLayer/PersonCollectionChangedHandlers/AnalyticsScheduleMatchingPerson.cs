using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class ConstraintViolationWrapperException : Exception
	{
		public ConstraintViolationWrapperException(Exception innerException) : base(innerException.Message, innerException)
		{
		}
	}

	[EnabledBy(Toggles.ETL_FixScheduleForPersonPeriod_41393)]
	public class AnalyticsScheduleMatchingPerson : IHandleEvent<AnalyticsPersonPeriodRangeChangedEvent>, IRunOnHangfire
	{
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IAnalyticsScenarioRepository _analyticsScenarioRepository;

		public AnalyticsScheduleMatchingPerson(IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository, IAnalyticsScheduleRepository analyticsScheduleRepository, IDistributedLockAcquirer distributedLockAcquirer, IEventPopulatingPublisher eventPublisher, IAnalyticsDateRepository analyticsDateRepository, IAnalyticsScenarioRepository analyticsScenarioRepository)
		{
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_distributedLockAcquirer = distributedLockAcquirer;
			_eventPublisher = eventPublisher;
			_analyticsDateRepository = analyticsDateRepository;
			_analyticsScenarioRepository = analyticsScenarioRepository;
		}

		[Attempts(10)]
		[LogInfo]
		public virtual void Handle(AnalyticsPersonPeriodRangeChangedEvent @event)
		{
			var didRun = false;
			_distributedLockAcquirer.TryLockForTypeOf(this, () =>
			{
				didRun = true;
				foreach (var personId in @event.PersonIdCollection)
					try
					{
						_analyticsScheduleRepository.RunWithExceptionHandling(() =>
						{
							UpdateUnlinked(personId);
						});
					}
					catch (ConstraintViolationWrapperException)
					{
						PublishFixSchedule(personId, @event);
						throw;
					}
					
			});
			if (!didRun)
				throw new Exception("Another handler is running currently, explicitly failing to retry!");
		}

		[AnalyticsUnitOfWork]
		protected virtual void UpdateUnlinked(Guid personId)
		{
			var analyticsPersonPeriods = _analyticsPersonPeriodRepository.GetPersonPeriods(personId);
			var personPeriodIds = analyticsPersonPeriods.Select(x => x.PersonId).ToArray();
			_analyticsScheduleRepository.UpdateUnlinkedPersonids(personPeriodIds);
		}

		[AnalyticsUnitOfWork]
		protected virtual void PublishFixSchedule(Guid personId, EventWithInfrastructureContext @event)
		{
			var datesWithDuplicates = _analyticsScheduleRepository.GetDuplicateDatesForPerson(personId);
			var dates = _analyticsDateRepository.GetAllPartial();
			var scenarios = _analyticsScenarioRepository.Scenarios();
			foreach (var datesForScenario in datesWithDuplicates.GroupBy(x => x.ScenarioId))
			{
				var scenario = scenarios.FirstOrDefault(s => s.ScenarioId == datesForScenario.Key);
				if (scenario == null) continue;
				_eventPublisher.Publish(new ReloadSchedules
				{
					Dates = mapDates(dates, datesForScenario.Select(s => s.DateId)),
					PersonId = personId,
					ScenarioId = scenario.ScenarioCode.GetValueOrDefault(),
					LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
					Timestamp = @event.Timestamp
				});
			}
		}

		private static List<DateTime> mapDates(IList<IAnalyticsDate> analyticDates, IEnumerable<int> dateIds)
		{
			return dateIds.Select(x =>
			{
				var analyticsDate = analyticDates.FirstOrDefault(date => date.DateId == x);

				return analyticsDate?.DateDate;
			}).Where(d => d != null).Select(d => d.Value).ToList();
		}
		
	}
}