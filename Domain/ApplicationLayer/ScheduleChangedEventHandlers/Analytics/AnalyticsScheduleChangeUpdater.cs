using System;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsScheduleChangeUpdater:
		IHandleEvent<ProjectionChangedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsScheduleChangeUpdater));
		private readonly IAnalyticsFactScheduleMapper _factScheduleMapper;
		private readonly IAnalyticsFactSchedulePersonMapper _factSchedulePersonMapper;
		private readonly IAnalyticsFactScheduleDateMapper _factScheduleDateMapper;
		private readonly IAnalyticsFactScheduleDayCountMapper _factScheduleDayCountMapper;
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private readonly IAnalyticsScheduleChangeUpdaterFilter _analyticsScheduleChangeUpdaterFilter;
		private readonly IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private readonly IAnalyticsShiftCategoryRepository _analyticsShiftCategoryRepository;

		public AnalyticsScheduleChangeUpdater(
			IAnalyticsFactScheduleMapper factScheduleMapper,
			IAnalyticsFactScheduleDateMapper factScheduleDateMapper,
			IAnalyticsFactSchedulePersonMapper factSchedulePersonMapper,
			IAnalyticsFactScheduleDayCountMapper factScheduleDayCountMapper,
			IAnalyticsScheduleRepository analyticsScheduleRepository,
			IAnalyticsScheduleChangeUpdaterFilter analyticsScheduleChangeUpdaterFilter,
			IAnalyticsScenarioRepository analyticsScenarioRepository,
			IAnalyticsShiftCategoryRepository analyticsShiftCategoryRepository)
		{
			_factScheduleMapper = factScheduleMapper;
			_factScheduleDateMapper = factScheduleDateMapper;
			_factSchedulePersonMapper = factSchedulePersonMapper;
			_factScheduleDayCountMapper = factScheduleDayCountMapper;
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_analyticsScheduleChangeUpdaterFilter = analyticsScheduleChangeUpdaterFilter;
			_analyticsScenarioRepository = analyticsScenarioRepository;
			_analyticsShiftCategoryRepository = analyticsShiftCategoryRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			if (!_analyticsScheduleChangeUpdaterFilter.ContinueProcessingEvent(@event))
				return;
			var scenario = _analyticsScenarioRepository.Get(@event.ScenarioId);
			if (scenario == null)
			{
				logger.Warn($"Scenario with code {@event.ScenarioId} has not been inserted in analytics yet. Schedule changes for agent {@event.PersonId} is not saved into Analytics database.");
				throw new ScenarioMissingInAnalyticsException();
			}
			var scenarioId = scenario.ScenarioId;

			foreach (var scheduleDay in @event.ScheduleDays)
			{
				var dateOnly = new DateOnly(scheduleDay.Date);

				int dateId;
				if (!_factScheduleDateMapper.MapDateId(dateOnly, out dateId))
				{
					logger.Warn($"Date {scheduleDay.Date} could not be mapped to Analytics date_id. Schedule changes for agent {@event.PersonId} is not saved into Analytics database.");
					//throw new DateMissingInAnalyticsException(scheduleDay.Date);
					continue;
				}

				var personPart = _factSchedulePersonMapper.Map(scheduleDay.PersonPeriodId);
				if (personPart.PersonId == -1)
				{
					logger.Warn($"PersonPeriodId {scheduleDay.PersonPeriodId} could not be found. Schedule changes for agent {@event.PersonId} is not saved into Analytics database.");
					throw new PersonPeriodMissingInAnalyticsException(scheduleDay.PersonPeriodId);
				}

				_analyticsScheduleRepository.DeleteFactSchedule(dateId, personPart.PersonId, scenarioId);

				if (!scheduleDay.NotScheduled)
				{
					var shiftCategoryId = -1;
					if (scheduleDay.Shift != null)
						shiftCategoryId = getCategory(scheduleDay.ShiftCategoryId);

					var dayCount = _factScheduleDayCountMapper.Map(scheduleDay, personPart, scenarioId, shiftCategoryId);
					if (dayCount != null)
						_analyticsScheduleRepository.PersistFactScheduleDayCountRow(dayCount);

					var agentDaySchedule = _factScheduleMapper.AgentDaySchedule(scheduleDay, personPart, @event.Timestamp, shiftCategoryId, scenarioId, @event.ScenarioId, @event.PersonId);
					if (agentDaySchedule == null)
					{
						_analyticsScheduleRepository.DeleteFactSchedule(dateId, personPart.PersonId, scenarioId);
						continue;
					}
					_analyticsScheduleRepository.PersistFactScheduleBatch(agentDaySchedule);
				}

				if (@event.IsDefaultScenario)
				{
					if (scheduleDay.Date < DateTime.Now.AddDays(1))
					{
						_analyticsScheduleRepository.InsertStageScheduleChangedServicebus(dateOnly, @event.PersonId, @event.ScenarioId, @event.LogOnBusinessUnitId, DateTime.Now);
					}
				}
			}
		}

		private int getCategory(Guid shiftCategoryCode)
		{
			var cats = _analyticsShiftCategoryRepository.ShiftCategories();
			var cat = cats.FirstOrDefault(x => x.ShiftCategoryCode.Equals(shiftCategoryCode));
			if (cat == null)
				return -1;
			return cat.ShiftCategoryId;
		}
	}

	public interface IAnalyticsScheduleChangeUpdaterFilter
	{
		bool ContinueProcessingEvent(ProjectionChangedEvent @event);
	}
}