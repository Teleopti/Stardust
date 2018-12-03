using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class UpdateFactSchedules
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(UpdateFactSchedules));

		private readonly IAnalyticsFactScheduleMapper _factScheduleMapper;
		private readonly IAnalyticsFactSchedulePersonMapper _factSchedulePersonMapper;
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
		private readonly AnalyticsDateMapper _analyticsDateMapper;

		public UpdateFactSchedules(IAnalyticsFactScheduleMapper factScheduleMapper,
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
			IAnalyticsDayOffRepository analyticsDayOffRepository,
			AnalyticsDateMapper analyticsDateMapper)
		{
			_factScheduleMapper = factScheduleMapper;
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
			_analyticsDateMapper = analyticsDateMapper;
		}

		public void Execute(Guid personId, Guid scenarioId, Guid businessUnitId, Func<IPerson, IEnumerable<DateTime>> dates, DateTime timestamp)
		{
			var scenario = _scenarioRepository.Get(scenarioId);
			if (scenario == null)
			{
				logger.Warn($"Could not load scenario {scenarioId} from application database.");
				return;
			}

			if (!_analyticsScheduleChangeUpdaterFilter.ContinueProcessingEvent(scenario.DefaultScenario, scenarioId))
				return;

			var person = _personRepository.Get(personId);
			if (person == null)
				throw new PersonNotFoundException($"Could not load person {personId} from application database.");

			updateForDates(dates(person), person, scenario, businessUnitId, timestamp);
		}

		private void updateForDates(IEnumerable<DateTime> dateTimes, IPerson person, IScenario scenario, Guid businessUnitId, DateTime timestamp)
		{
			if (!dateTimes.Any()) return;

			var dates = dateTimes.Select(d => new DateOnly(d)).ToArray();

			var dateIds = new List<int>();
			foreach (var date in dates)
			{
				MapDate(date, dateIds);
			}

			DoWork(person, scenario, businessUnitId, timestamp, dateIds, dates);
		}

		[AnalyticsUnitOfWork]
		protected virtual void DoWork(IPerson person, IScenario scenario, Guid businessUnitId, DateTime timestamp, IEnumerable<int> dateIds, IEnumerable<DateOnly> dates)
		{
			var analyticsScenario = _analyticsScenarioRepository.Get(scenario.Id.Value);
			if (analyticsScenario == null)
			{
				logger.Warn(
					$"Scenario with code {scenario.Id.Value} has not been inserted in analytics yet. Schedule changes for " +
					$"agent {person.Id.Value} is not saved into Analytics database.");
				throw new ScenarioMissingInAnalyticsException();
			}

			foreach (var dateIdsBatch in dateIds.Batch(365))
			{
				_analyticsScheduleRepository.DeleteFactSchedules(dateIdsBatch, person.Id.GetValueOrDefault(),
					analyticsScenario.ScenarioId);
			}

			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(true, false, true), new DateOnlyPeriod(dates.Min(), dates.Max()), scenario);

			IList<AnalyticsDayOff> dayOffs = null;
			foreach (var date in dates)
			{
				var scheduleDay = schedule.SchedulesForDay(date).FirstOrDefault();
				if (scheduleDay == null)
				{
					continue;
				}

				var currentEventScheduleDay = _projectionChangedEventBuilder.BuildEventScheduleDay(scheduleDay);
				if (currentEventScheduleDay == null)
				{
					logger.Warn($"No schedule found for {person.Id.GetValueOrDefault()} {date.Date:yyyy-MM-dd}");
					continue;
				}

				if (currentEventScheduleDay.PersonPeriodId.Equals(Guid.Empty))
				{
					continue;
				}

				var personPart = _factSchedulePersonMapper.Map(currentEventScheduleDay.PersonPeriodId);
				if (personPart.PersonId == -1)
				{
					logger.Warn($"PersonPeriodId {currentEventScheduleDay.PersonPeriodId} could not be found. Schedule " +
								$"changes for agent {person.Id.GetValueOrDefault()} is not saved into Analytics database.");
					throw new PersonPeriodMissingInAnalyticsException(currentEventScheduleDay.PersonPeriodId);
				}

				if (!currentEventScheduleDay.NotScheduled)
				{
					var shiftCategoryId = -1;
					if (currentEventScheduleDay.Shift != null)
						shiftCategoryId = getCategory(currentEventScheduleDay.ShiftCategoryId);
					var dayCount = _factScheduleDayCountMapper.Map(currentEventScheduleDay, personPart, analyticsScenario.ScenarioId, shiftCategoryId);
					var agentDaySchedule = _factScheduleMapper.AgentDaySchedule(currentEventScheduleDay, scheduleDay, personPart,
						timestamp, shiftCategoryId, analyticsScenario.ScenarioId, scenario.Id.GetValueOrDefault(),
						person.Id.GetValueOrDefault());

					if (agentDaySchedule == null)
					{
						continue;
					}

					if (dayCount != null)
					{
						if (dayCount.DayOffName != null)
						{
							if (dayOffs == null)
							{
								dayOffs = _analyticsDayOffRepository.DayOffs();
							}

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

		[AnalyticsUnitOfWork]
		protected virtual void MapDate(DateOnly date, List<int> dateIds)
		{
			if (_analyticsDateMapper.MapDateId(date, out var dateId))
			{
				dateIds.Add(dateId);
			}
		}

		private int getCategory(Guid shiftCategoryCode)
		{
			var cat = _analyticsShiftCategoryRepository.ShiftCategory(shiftCategoryCode);
			if (cat == null)
				return -1;
			return cat.ShiftCategoryId;
		}
	}
}