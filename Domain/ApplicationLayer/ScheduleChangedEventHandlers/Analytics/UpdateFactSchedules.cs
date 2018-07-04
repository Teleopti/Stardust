using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class UpdateFactSchedules : IUpdateFactSchedules
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(UpdateFactSchedules));

		private readonly IAnalyticsFactScheduleMapper _factScheduleMapper;
		private readonly IAnalyticsFactScheduleDateMapper _factScheduleDateMapper;
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

		public UpdateFactSchedules(IAnalyticsFactScheduleMapper factScheduleMapper,
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

			var dates = dateTimes.Select(d => new DateOnly(d));
			var dateIds = dates.Distinct().ToDictionary(k => k, MapDate);

			DoWork(person, scenario, businessUnitId, timestamp, dateIds);
		}

		[AnalyticsUnitOfWork]
		protected virtual void DoWork(IPerson person, IScenario scenario, Guid businessUnitId, DateTime timestamp,
			Dictionary<DateOnly, int?> dateIds)
		{
			var analyticsScenario = _analyticsScenarioRepository.Get(scenario.Id.Value);
			if (analyticsScenario == null)
			{
				logger.Warn(
					$"Scenario with code {scenario.Id.Value} has not been inserted in analytics yet. Schedule changes for " +
					$"agent {person.Id.Value} is not saved into Analytics database.");
				throw new ScenarioMissingInAnalyticsException();
			}

			_analyticsScheduleRepository.DeleteFactSchedules(dateIds.Values.Where(v => v.HasValue).Select(v => v.Value), person.Id.GetValueOrDefault(),
				analyticsScenario.ScenarioId);


			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, true, false), new DateOnlyPeriod(dateIds.Keys.Min(), dateIds.Keys.Max()), scenario);

			Dictionary<Guid,int> dayOffs = null;
			foreach (var date in dateIds)
			{
				if (!date.Value.HasValue) continue;
				
				var scheduleDay = schedule.SchedulesForDay(date.Key).FirstOrDefault();
				if (scheduleDay == null)
				{
					continue;
				}

				var currentEventScheduleDay = _projectionChangedEventBuilder.BuildEventScheduleDay(scheduleDay);
				if (currentEventScheduleDay == null)
				{
					logger.Warn($"No schedule found for {person.Id.GetValueOrDefault()} {date.Key.Date:yyyy-MM-dd}");
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
					var dayCount = _factScheduleDayCountMapper.Map(currentEventScheduleDay, personPart, date.Value.Value, analyticsScenario.ScenarioId, shiftCategoryId);
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
								dayOffs = _analyticsDayOffRepository.DayOffs().ToDictionary(k => k.DayOffCode, v => v.DayOffId);
							}

							var dayOff = scheduleDay.PersonAssignment().DayOff();

							if (!dayOffs.TryGetValue(dayOff.DayOffTemplateId, out var dayOffId))
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
								dayOffs = _analyticsDayOffRepository.DayOffs().ToDictionary(k => k.DayOffCode, v => v.DayOffId);
								dayOffs.TryGetValue(dayOff.DayOffTemplateId, out dayOffId);
							}

							dayCount.DayOffId = dayOffId;
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
						_analyticsScheduleRepository.InsertStageScheduleChangedServicebus(date.Key, person.Id.GetValueOrDefault(),
							scenario.Id.GetValueOrDefault(), businessUnitId, DateTime.Now);
					}
				}
			}
		}

		[AnalyticsUnitOfWork]
		protected virtual int? MapDate(DateOnly date)
		{
			if (_factScheduleDateMapper.MapDateId(date, out var dateId))
			{
				return dateId;
			}

			return null;
		}

		private int getCategory(Guid shiftCategoryCode)
		{
			var cat = _analyticsShiftCategoryRepository.ShiftCategory(shiftCategoryCode);
			if (cat == null)
				return -1;
			return cat.ShiftCategoryId;
		}
	}


	[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_48769)]
	public interface IUpdateFactSchedules
	{
		void Execute(Guid personId, Guid scenarioId, Guid businessUnitId, Func<IPerson, IEnumerable<DateTime>> dates,
			DateTime timestamp);
	}

	[RemoveMeWithToggle(Toggles.ResourcePlanner_SpeedUpEvents_48769)]
	public class UpdateFactSchedulesOLD : IUpdateFactSchedules
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(UpdateFactSchedules));

		private readonly IAnalyticsFactScheduleMapper _factScheduleMapper;
		private readonly IAnalyticsFactScheduleDateMapper _factScheduleDateMapper;
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

		public UpdateFactSchedulesOLD(IAnalyticsFactScheduleMapper factScheduleMapper,
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

		public void Execute(Guid personId, Guid scenarioCode, Guid businessUnitId, Func<IPerson, IEnumerable<DateTime>> dates,
			DateTime timestamp)
		{
			var scenario = _scenarioRepository.Get(scenarioCode);
			if (scenario == null)
			{
				logger.Warn($"Could not load scenario {scenarioCode} from application database.");
				return;
			}

			if (!_analyticsScheduleChangeUpdaterFilter.ContinueProcessingEvent(scenario.DefaultScenario, scenarioCode))
				return;

			var analyticsScenarioId = GetScenario(personId, scenarioCode);

			var person = _personRepository.Get(personId);
			if (person == null)
				throw new PersonNotFoundException($"Could not load person {personId} from application database.");

			updateForDates(dates(person), person, scenario, analyticsScenarioId, businessUnitId, timestamp);
		}

		[AnalyticsUnitOfWork]
		protected virtual int GetScenario(Guid personId, Guid scenarioCode)
		{
			var analyticsScenario = _analyticsScenarioRepository.Get(scenarioCode);
			if (analyticsScenario == null)
			{
				logger.Warn($"Scenario with code {scenarioCode} has not been inserted in analytics yet. Schedule changes for " +
							$"agent {personId} is not saved into Analytics database.");
				throw new ScenarioMissingInAnalyticsException();
			}

			return analyticsScenario.ScenarioId;
		}

		private void updateForDates(IEnumerable<DateTime> dates, IPerson person, IScenario scenario, int scenarioId,
			Guid businessUnitId, DateTime timestamp)
		{
			if (!dates.Any()) return;

			var dateOnly = dates.Select(d => new DateOnly(d)).Distinct().ToArray();
			IList<AnalyticsDayOff> dayOffs = null;
			var schedule = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person,
				new ScheduleDictionaryLoadOptions(false, false, false), new DateOnlyPeriod(dateOnly.Min(), dateOnly.Max()), scenario);
			foreach (var date in dateOnly)
			{
				dayOffs = UpdateForDate(person, scenario, scenarioId, businessUnitId, timestamp, date, schedule, dayOffs);
			}
		}

		[AnalyticsUnitOfWork]
		protected virtual IList<AnalyticsDayOff> UpdateForDate(IPerson person, IScenario scenario, int scenarioId,
			Guid businessUnitId, DateTime timestamp,
			DateOnly date, IScheduleDictionary schedule, IList<AnalyticsDayOff> dayOffs)
		{
			if (!_factScheduleDateMapper.MapDateId(date, out var dateId))
			{
				logger.Warn($"Date {date} could not be mapped to Analytics date_id. Schedule changes for " +
							$"agent {person.Id.GetValueOrDefault()} is not saved into Analytics database.");
				return dayOffs;
			}

			var scheduleDay = schedule.SchedulesForDay(date).FirstOrDefault();
			if (scheduleDay == null)
			{
				_analyticsScheduleRepository.DeleteFactSchedule(dateId, person.Id.GetValueOrDefault(), scenarioId);
				return dayOffs;
			}

			var currentEventScheduleDay = _projectionChangedEventBuilder.BuildEventScheduleDay(scheduleDay);
			if (currentEventScheduleDay == null)
			{
				logger.Warn($"No schedule found for {person.Id.GetValueOrDefault()} {date.Date:yyyy-MM-dd}");
				return dayOffs;
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
				var dayCount = _factScheduleDayCountMapper.Map(currentEventScheduleDay, personPart, dateId, scenarioId, shiftCategoryId);
				var agentDaySchedule = _factScheduleMapper.AgentDaySchedule(currentEventScheduleDay, scheduleDay, personPart,
					timestamp, shiftCategoryId, scenarioId, scenario.Id.GetValueOrDefault(), person.Id.GetValueOrDefault());

				if (agentDaySchedule == null)
				{
					return dayOffs;
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

			return dayOffs;
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