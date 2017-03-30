using System;
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
			var analyticsScenario = _analyticsScenarioRepository.Get(@event.ScenarioId);
			if (analyticsScenario == null)
			{
				logger.Warn($"Scenario with code {@event.ScenarioId} has not been inserted in analytics yet. Schedule changes for agent {@event.PersonId} is not saved into Analytics database.");
				throw new ScenarioMissingInAnalyticsException();
			}
			var scenarioId = analyticsScenario.ScenarioId;
			var person = _personRepository.Get(@event.PersonId);
			if (person == null)
				throw new PersonNotFoundException($"Could not load person {@event.PersonId} from application database.");
			var scenario = _scenarioRepository.Get(@event.ScenarioId);
			if (scenario == null)
				throw new Exception($"Could not load scenario {@event.ScenarioId} from application database.");

			foreach (var eventScheduleDay in @event.ScheduleDays)
			{
				var dateOnly = new DateOnly(eventScheduleDay.Date);
				
				int dateId;
				if (!_factScheduleDateMapper.MapDateId(dateOnly, out dateId))
				{
					logger.Warn($"Date {eventScheduleDay.Date} could not be mapped to Analytics date_id. Schedule changes for agent {@event.PersonId} is not saved into Analytics database.");
					//throw new DateMissingInAnalyticsException(scheduleDay.Date);
					continue;
				}

				var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
					new ScheduleDictionaryLoadOptions(true, true, true), new DateOnlyPeriod(dateOnly, dateOnly), scenario);
				var scheduleDay = schedule.SchedulesForDay(dateOnly).FirstOrDefault();
				if (scheduleDay == null)
					continue;
				var currentEventScheduleDay = _projectionChangedEventBuilder.BuildEventScheduleDay(scheduleDay);
				if (currentEventScheduleDay == null)
					throw new Exception("No schedule");
				var personPart = _factSchedulePersonMapper.Map(currentEventScheduleDay.PersonPeriodId);
				if (personPart.PersonId == -1)
				{
					logger.Warn($"PersonPeriodId {currentEventScheduleDay.PersonPeriodId} could not be found. Schedule changes for agent {@event.PersonId} is not saved into Analytics database.");
					throw new PersonPeriodMissingInAnalyticsException(currentEventScheduleDay.PersonPeriodId);
				}

				_analyticsScheduleRepository.DeleteFactSchedule(dateId, @event.PersonId, scenarioId);

				if (!currentEventScheduleDay.NotScheduled)
				{
					var shiftCategoryId = -1;
					if (currentEventScheduleDay.Shift != null)
						shiftCategoryId = getCategory(currentEventScheduleDay.ShiftCategoryId);

					var dayCount = _factScheduleDayCountMapper.Map(currentEventScheduleDay, personPart, scenarioId, shiftCategoryId);
					var agentDaySchedule = _factScheduleMapper.AgentDaySchedule(currentEventScheduleDay, scheduleDay, personPart, @event.Timestamp, shiftCategoryId, scenarioId, @event.ScenarioId, @event.PersonId);

					if (agentDaySchedule == null)
						continue;

					if (dayCount != null)
						_analyticsScheduleRepository.PersistFactScheduleDayCountRow(dayCount);

					_analyticsScheduleRepository.PersistFactScheduleBatch(agentDaySchedule);
				}

				if (@event.IsDefaultScenario)
				{
					if (currentEventScheduleDay.Date < DateTime.Now.AddDays(1))
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