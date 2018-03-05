using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
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
	public class AnalyticsScheduleChangeUpdater :
		IHandleEvent<ScheduleChangedEvent>,
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
		private readonly IAnalyticsDayOffRepository _analyticsDayOffRepository;

		public AnalyticsScheduleChangeUpdater(
			IAnalyticsFactScheduleMapper factScheduleMapper,
			IAnalyticsFactScheduleDateMapper factScheduleDateMapper,
			IAnalyticsFactSchedulePersonMapper factSchedulePersonMapper,
			IAnalyticsFactScheduleDayCountMapper factScheduleDayCountMapper,
			IAnalyticsScheduleRepository analyticsScheduleRepository,
			IAnalyticsScheduleChangeUpdaterFilter analyticsScheduleChangeUpdaterFilter,
			IAnalyticsScenarioRepository analyticsScenarioRepository,
			IAnalyticsShiftCategoryRepository analyticsShiftCategoryRepository,
			IScheduleStorage scheduleStorage,
			IPersonRepository personRepository,
			IScenarioRepository scenarioRepository,
			IProjectionChangedEventBuilder projectionChangedEventBuilder,
			IAnalyticsDayOffRepository analyticsDayOffRepository)
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
			_analyticsDayOffRepository = analyticsDayOffRepository;
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(ScheduleChangedEvent @event)
		{
			Func<IPerson, IEnumerable<DateTime>> period = p =>
				new DateTimePeriod(@event.StartDateTime, @event.EndDateTime)
					.ToDateOnlyPeriod(p.PermissionInformation.DefaultTimeZone()).DayCollection().Select(d => d.Date);
			updateForPersonAndDates(@event.PersonId, @event.ScenarioId, @event.LogOnBusinessUnitId,
				period, @event.Timestamp);
		}

		[ImpersonateSystem]
		[UnitOfWork]
		[AnalyticsUnitOfWork]
		[Attempts(2)]
		public virtual void Handle(ReloadSchedules @event)
		{
			updateForPersonAndDates(@event.PersonId, @event.ScenarioId, @event.LogOnBusinessUnitId,
				p=>@event.Dates, @event.Timestamp);
		}

		private void updateForPersonAndDates(Guid personId, Guid scenarioId, Guid businessUnitId, Func<IPerson,IEnumerable<DateTime>> dates,
			DateTime timestamp)
		{
			var scenario = _scenarioRepository.Get(scenarioId);
			if (scenario == null)
				throw new Exception($"Could not load scenario {scenarioId} from application database.");
			if (!_analyticsScheduleChangeUpdaterFilter.ContinueProcessingEvent(scenario.DefaultScenario, scenarioId))
				return;

			var analyticsScenario = _analyticsScenarioRepository.Get(scenarioId);
			if (analyticsScenario == null)
			{
				logger.Warn($"Scenario with code {scenarioId} has not been inserted in analytics yet. Schedule changes for " +
							$"agent {personId} is not saved into Analytics database.");
				throw new ScenarioMissingInAnalyticsException();
			}

			var person = _personRepository.Get(personId);
			if (person == null)
				throw new PersonNotFoundException($"Could not load person {personId} from application database.");

			updateForDates(dates(person), person, scenario, analyticsScenario.ScenarioId, businessUnitId, timestamp);
		}

		private void updateForDates(IEnumerable<DateTime> dates, IPerson person, IScenario scenario, int scenarioId,
			Guid businessUnitId, DateTime timestamp)
		{
			if (!dates.Any()) return;

			var dateOnly = dates.Select(d => new DateOnly(d)).ToArray();
			var dayOffs = _analyticsDayOffRepository.DayOffs();
			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(true, true, true), new DateOnlyPeriod(dateOnly.Min(), dateOnly.Max()), scenario);
			foreach (var date in dateOnly)
			{
				if (!_factScheduleDateMapper.MapDateId(date, out var dateId))
				{
					logger.Warn($"Date {date} could not be mapped to Analytics date_id. Schedule changes for " +
								$"agent {person.Id.GetValueOrDefault()} is not saved into Analytics database.");
					continue;
				}
				
				var scheduleDay = schedule.SchedulesForDay(date).FirstOrDefault();
				if (scheduleDay == null)
				{
					_analyticsScheduleRepository.DeleteFactSchedule(dateId, person.Id.GetValueOrDefault(), scenarioId);
					continue;
				}

				var currentEventScheduleDay = _projectionChangedEventBuilder.BuildEventScheduleDay(scheduleDay);
				if (currentEventScheduleDay == null)
				{
					logger.Warn($"No schedule found for {person.Id.GetValueOrDefault()} {date.Date:yyyy-MM-dd}");
					continue;
				}

				var personPart = _factSchedulePersonMapper.Map(currentEventScheduleDay.PersonPeriodId);
				if (personPart.PersonId == -1)
				{
					logger.Warn($"PersonPeriodId {currentEventScheduleDay.PersonPeriodId} could not be found. Schedule " +
								$"changes for agent {person.Id.GetValueOrDefault()} is not saved into Analytics database.");
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
					{
						continue;
					}

					if (dayCount != null)
					{
						if (dayCount.DayOffName != null)
						{
							var dayOff = scheduleDay.PersonAssignment().DayOff();

							if (dayOffs.All(x => x.DayOffCode != dayOff.DayOffTemplateId))
							{
								_analyticsDayOffRepository.AddOrUpdate(new AnalyticsDayOff
								{
									DayOffCode = dayOff.DayOffTemplateId,
									BusinessUnitId = dayCount.BusinessUnitId,
									DayOffName = dayOff.Description.Name,
									DayOffShortname = dayOff.Description.ShortName,
									DisplayColor = dayOff.DisplayColor.ToArgb(),
									DisplayColorHtml = ColorTranslator.ToHtml(dayOff.DisplayColor),
									DatasourceUpdateDate = DateHelper.GetSmallDateTime(DateTime.UtcNow),
									DatasourceId = 1
								});
							}

							dayOffs = _analyticsDayOffRepository.DayOffs();
							dayCount.DayOffId = dayOffs.Single(x => x.DayOffCode == dayOff.DayOffTemplateId).DayOffId;
						}
						else
						{
							dayCount.DayOffId = -1;
						}

						_analyticsScheduleRepository.PersistFactScheduleDayCountRow(dayCount);
					}

					_analyticsScheduleRepository.PersistFactScheduleBatch(agentDaySchedule);
				}

				if (scenario.DefaultScenario)
				{
					if (currentEventScheduleDay.Date < DateTime.Now.AddDays(1))
					{
						_analyticsScheduleRepository.InsertStageScheduleChangedServicebus(date, person.Id.GetValueOrDefault(),
							scenario.Id.GetValueOrDefault(), businessUnitId, DateTime.Now);
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
		bool ContinueProcessingEvent(bool isDefaultScenario, Guid scenarioId);
	}
}