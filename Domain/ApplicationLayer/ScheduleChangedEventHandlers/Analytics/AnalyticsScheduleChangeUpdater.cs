using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsScheduleChangeUpdater:
		IHandleEvent<ProjectionChangedEvent>,
		IHandleEvent<ReloadSchedules>,
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
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IPersonRepository _personRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IProjectionChangedEventBuilder _projectionChangedEventBuilder;

		public AnalyticsScheduleChangeUpdater(
			IAnalyticsFactScheduleMapper factScheduleMapper,
			IAnalyticsFactScheduleDateMapper factScheduleDateMapper,
			IAnalyticsFactSchedulePersonMapper factSchedulePersonMapper,
			IAnalyticsFactScheduleDayCountMapper factScheduleDayCountMapper,
			IAnalyticsScheduleRepository analyticsScheduleRepository,
			IAnalyticsScheduleChangeUpdaterFilter analyticsScheduleChangeUpdaterFilter,
			IAnalyticsScenarioRepository analyticsScenarioRepository,
			IAnalyticsShiftCategoryRepository analyticsShiftCategoryRepository, IScheduleStorage scheduleStorage, IPersonRepository personRepository, IScenarioRepository scenarioRepository, IProjectionChangedEventBuilder projectionChangedEventBuilder)
		{
			_factScheduleMapper = factScheduleMapper;
			_factScheduleDateMapper = factScheduleDateMapper;
			_factSchedulePersonMapper = factSchedulePersonMapper;
			_factScheduleDayCountMapper = factScheduleDayCountMapper;
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_analyticsScheduleChangeUpdaterFilter = analyticsScheduleChangeUpdaterFilter;
			_analyticsScenarioRepository = analyticsScenarioRepository;
			_analyticsShiftCategoryRepository = analyticsShiftCategoryRepository;
			_scheduleStorage = scheduleStorage;
			_personRepository = personRepository;
			_scenarioRepository = scenarioRepository;
			_projectionChangedEventBuilder = projectionChangedEventBuilder;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ProjectionChangedEvent @event)
		{
			if (!_analyticsScheduleChangeUpdaterFilter.ContinueProcessingEvent(@event))
				return;
			updateForPersonAndDates(@event.PersonId, @event.ScenarioId, @event.LogOnBusinessUnitId,
				@event.ScheduleDays.Select(x => x.Date), @event.Timestamp);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(2)]
		public virtual void Handle(ReloadSchedules @event)
		{
			updateForPersonAndDates(@event.PersonId, @event.ScenarioId, @event.LogOnBusinessUnitId,
				@event.Dates, @event.Timestamp);
		}

		private void updateForPersonAndDates(Guid personId, Guid scenarioId, Guid businessUnitId, IEnumerable<DateTime> dates, DateTime timestamp)
		{
			var analyticsScenario = _analyticsScenarioRepository.Get(scenarioId);
			if (analyticsScenario == null)
			{
				logger.Warn($"Scenario with code {scenarioId} has not been inserted in analytics yet. Schedule changes for agent {personId} is not saved into Analytics database.");
				throw new ScenarioMissingInAnalyticsException();
			}
			var person = _personRepository.Get(personId);
			if (person == null)
				throw new PersonNotFoundException($"Could not load person {personId} from application database.");
			var scenario = _scenarioRepository.Get(scenarioId);
			if (scenario == null)
				throw new Exception($"Could not load scenario {scenarioId} from application database.");

			foreach (var date in dates)
			{
				updateDate(date, person, scenario, analyticsScenario.ScenarioId, businessUnitId, timestamp);
			}
		}
		private void updateDate(DateTime date, IPerson person, IScenario scenario, int scenarioId, Guid businessUnitId, DateTime timestamp)
		{
			var dateOnly = new DateOnly(date);

			int dateId;
			if (!_factScheduleDateMapper.MapDateId(dateOnly, out dateId))
			{
				logger.Warn($"Date {date} could not be mapped to Analytics date_id. Schedule changes for agent {person.Id.GetValueOrDefault()} is not saved into Analytics database.");
				return;
			}

			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(true, true, true), new DateOnlyPeriod(dateOnly, dateOnly), scenario);
			var scheduleDay = schedule.SchedulesForDay(dateOnly).FirstOrDefault();
			if (scheduleDay == null)
				return;
			var currentEventScheduleDay = _projectionChangedEventBuilder.BuildEventScheduleDay(scheduleDay);
			if (currentEventScheduleDay == null)
				throw new Exception("No schedule");
			var personPart = _factSchedulePersonMapper.Map(currentEventScheduleDay.PersonPeriodId);
			if (personPart.PersonId == -1)
			{
				logger.Warn(
					$"PersonPeriodId {currentEventScheduleDay.PersonPeriodId} could not be found. Schedule changes for agent {person.Id.GetValueOrDefault()} is not saved into Analytics database.");
				throw new PersonPeriodMissingInAnalyticsException(currentEventScheduleDay.PersonPeriodId);
			}

			_analyticsScheduleRepository.DeleteFactSchedule(dateId, person.Id.GetValueOrDefault(), scenarioId);

			if (!currentEventScheduleDay.NotScheduled)
			{
				var shiftCategoryId = -1;
				if (currentEventScheduleDay.Shift != null)
					shiftCategoryId = getCategory(currentEventScheduleDay.ShiftCategoryId);

				var dayCount = _factScheduleDayCountMapper.Map(currentEventScheduleDay, personPart, scenarioId, shiftCategoryId);
				var agentDaySchedule = _factScheduleMapper.AgentDaySchedule(currentEventScheduleDay, scheduleDay, personPart,
					timestamp, shiftCategoryId, scenarioId, scenario.Id.GetValueOrDefault(), person.Id.GetValueOrDefault());

				if (agentDaySchedule == null)
					return;

				if (dayCount != null)
					_analyticsScheduleRepository.PersistFactScheduleDayCountRow(dayCount);

				_analyticsScheduleRepository.PersistFactScheduleBatch(agentDaySchedule);
			}

			if (scenario.DefaultScenario)
			{
				if (currentEventScheduleDay.Date < DateTime.Now.AddDays(1))
				{
					_analyticsScheduleRepository.InsertStageScheduleChangedServicebus(dateOnly, person.Id.GetValueOrDefault(), scenario.Id.GetValueOrDefault(),
						businessUnitId, DateTime.Now);
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