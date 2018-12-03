using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Preference
{
	public class AnalyticsPreferenceUpdater :
		IHandleEvent<PreferenceCreatedEvent>,
		IHandleEvent<PreferenceDeletedEvent>,
		IHandleEvent<PreferenceChangedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsPreferenceUpdater));
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IAnalyticsPreferenceRepository _analyticsPreferenceRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IAnalyticsDayOffRepository _analyticsDayOffRepository;
		private readonly RestrictionChecker restrictionChecker;
		private readonly ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions;
		private readonly IAnalyticsScenarioRepository _analyticsScenarioRepository;
		private readonly IAnalyticsAbsenceRepository _analyticsAbsenceRepository;
		private readonly IAnalyticsShiftCategoryRepository _analyticsShiftCategoryRepository;

		public AnalyticsPreferenceUpdater
			(IScenarioRepository scenarioRepository,
			IPreferenceDayRepository preferenceDayRepository,
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository,
			IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsDateRepository analyticsDateRepository,
			IScheduleStorage scheduleStorage,
			IAnalyticsPreferenceRepository analyticsPreferenceRepository,
			IPersonRepository personRepository,
			IAnalyticsDayOffRepository analyticsDayOffRepository,
			IAnalyticsScenarioRepository analyticsScenarioRepository,
			IAnalyticsAbsenceRepository analyticsAbsenceRepository,
			IAnalyticsShiftCategoryRepository analyticsShiftCategoryRepository)
		{
			_scenarioRepository = scenarioRepository;
			_preferenceDayRepository = preferenceDayRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsDateRepository = analyticsDateRepository;
			_scheduleStorage = scheduleStorage;
			_analyticsPreferenceRepository = analyticsPreferenceRepository;
			_personRepository = personRepository;
			_analyticsDayOffRepository = analyticsDayOffRepository;
			_analyticsScenarioRepository = analyticsScenarioRepository;
			_analyticsAbsenceRepository = analyticsAbsenceRepository;
			_analyticsShiftCategoryRepository = analyticsShiftCategoryRepository;

			restrictionChecker = new RestrictionChecker();
			scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, false, true) { LoadDaysAfterLeft = true };

			logger.Info("New instance of handler was created");
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PreferenceDeletedEvent @event)
		{
			logger.Debug($"Consuming deleted event for person Id = {@event.PersonId}. (Message timestamp = {@event.Timestamp})");
			var person = _personRepository.Get(@event.PersonId);
			if (person == null)
			{
				logger.Warn("Could not find person in Application database, aborting.");
				return;
			}

			foreach (var restrictionDate in @event.RestrictionDates)
			{
				var analyticsDate = getAnalyticsDate(restrictionDate);
				_analyticsPreferenceRepository.DeletePreferences(analyticsDate.DateId, @event.PersonId);
			}
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PreferenceCreatedEvent @event)
		{
			logger.Debug($"Consuming created event for person Id = {@event.PersonId}. (Message timestamp = {@event.Timestamp})");

			commonHandle(@event.RestrictionDates, @event.PersonId, @event.ScenarioId);
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PreferenceChangedEvent @event)
		{
			logger.Debug($"Consuming changed event for person Id = {@event.PersonId}. (Message timestamp = {@event.Timestamp})");

			commonHandle(@event.RestrictionDates, @event.PersonId, @event.ScenarioId);
		}

		private void commonHandle(IEnumerable<DateTime> restrictionDates, Guid personCode, Guid scenarioId)
		{
			var person = _personRepository.Get(personCode);
			if (person == null)
			{
				logger.Warn("Could not find person in Application database, aborting.");
				return;
			}

			// General look ups
			var analyticsScenarios = _analyticsScenarioRepository.Scenarios();
			var analyticsDayOffs = _analyticsDayOffRepository.DayOffs();
			var analyticsAbsences = _analyticsAbsenceRepository.Absences();
			var analyticsShiftCategories = _analyticsShiftCategoryRepository.ShiftCategories();

			// Remove duplicate day
			var uniqueRestrictionDates = restrictionDates.Distinct();

			int? analyticsScenarioId = null;
			var scenarios = _scenarioRepository.FindEnabledForReportingSorted();
			if (scenarioId != Guid.Empty)
			{
				scenarios = scenarios.Where(a => a.Id.Equals(scenarioId)).ToList();
				if (scenarios.IsEmpty())
				{
					var dates = uniqueRestrictionDates.Aggregate("", (current, date) => current + $"{date:yyyy-MM-dd},");
					logger.Debug($"Nothing to do with person id {personCode} and \"{dates}\" because scenario {scenarioId} was not found as reportable.");
					return;
				}

				analyticsScenarioId = getAnalyticsScenario(analyticsScenarios, scenarioId).ScenarioId;
			}

			foreach (var restrictionDate in uniqueRestrictionDates)
			{
				// Preference, person look ups, mapping
				var preferenceDays = _preferenceDayRepository.Find(new DateOnly(restrictionDate), person).ToList();
				if (!preferenceDays.Any())
				{
					// Preference day does not exists anymore so it has been deleted, will be handled by other event.
					continue;
				}

				IPreferenceDay preferenceDay;
				if (preferenceDays.Count > 1)
				{
					// Refer to bug #75976, there may exists multiple PreferenceDay for an agent in same day
					logger.Warn($"{preferenceDays.Count} preference days found for person with Id={personCode} and date {restrictionDate:yyyy-MM-dd}");
					// Keep the last updated
					preferenceDay = preferenceDays.OrderByDescending(x => x.UpdatedOn).First();
				}
				else
				{
					preferenceDay = preferenceDays.Single();
				}

				var dateId = getAnalyticsDate(restrictionDate);
				var analyticsPersonPeriod = getAnalyticsPersonPeriod(restrictionDate, person);
				if (analyticsPersonPeriod == null)
				{
					continue;
				}

				var period = new DateOnlyPeriod(new DateOnly(preferenceDay.RestrictionDate.Date),
					new DateOnly(preferenceDay.RestrictionDate.Date));
				var resultFactSchedulePreference = new List<AnalyticsFactSchedulePreference>();

				foreach (var scenario in scenarios)
				{
					var schedulesDictionary =
						_scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, scheduleDictionaryLoadOptions, period, scenario);
					var scheduleParts = schedulesDictionary[person].ScheduledDayCollection(period);
					var analyticsBusinessUnit = getBusinessUnit(scenario);

					foreach (var schedulePart in scheduleParts)
					{
						var restrictionBases = schedulePart.RestrictionCollection();
						foreach (var restrictionBase in restrictionBases)
						{
							if (!(restrictionBase is IPreferenceRestriction preferenceRestriction))
							{
								logger.Error($"Could not get restrictions for person with person code {person.Id}, date {restrictionDate:yyyy-MM-dd} and scenario {scenario.Id}");
								continue;
							}

							if (!SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(preferenceRestriction))
								continue;

							var preferenceItem = mapSchedulePreference(schedulePart, analyticsScenarios, scenario, analyticsShiftCategories,
								preferenceRestriction, analyticsAbsences, analyticsDayOffs, analyticsBusinessUnit, dateId,
								analyticsPersonPeriod.PersonId, preferenceDay);
							resultFactSchedulePreference.Add(preferenceItem);
						}
					}
				}

				// Delete
				_analyticsPreferenceRepository.DeletePreferences(dateId.DateId, personCode, analyticsScenarioId);

				// Insert
				foreach (var analyticsFactSchedulePreference in resultFactSchedulePreference)
				{
					_analyticsPreferenceRepository.AddPreference(analyticsFactSchedulePreference);
				}
			}
		}

		private AnalyticsFactSchedulePreference mapSchedulePreference(
			IScheduleDay schedulePart,
			IEnumerable<AnalyticsScenario> analyticsScenarios,
			IScenario scenario,
			IEnumerable<AnalyticsShiftCategory> analyticsShiftCategories,
			IPreferenceRestriction preferenceRestriction,
			IEnumerable<AnalyticsAbsence> analyticsAbsences,
			IEnumerable<AnalyticsDayOff> analyticsDayOffs,
			AnalyticBusinessUnit businessUnitId,
			IAnalyticsDate analyticsDate,
			int analyticsPersonPeriodId,
			IPreferenceDay preferenceDay)
		{
			var permissionState = restrictionChecker.CheckPreference(schedulePart);
			var analyticsScenario = getAnalyticsScenario(analyticsScenarios, scenario.Id.GetValueOrDefault());
			var dayOffId = getDayOff(preferenceRestriction, analyticsDayOffs, businessUnitId);
			var shiftCategory = analyticsShiftCategories.FirstOrDefault(a => a.ShiftCategoryCode == (preferenceRestriction.ShiftCategory?.Id ?? Guid.Empty));
			var absence = analyticsAbsences.FirstOrDefault(a => a.AbsenceCode == (preferenceRestriction.Absence?.Id ?? Guid.Empty));

			var preferenceItem = new AnalyticsFactSchedulePreference
			{
				DateId = analyticsDate.DateId,
				IntervalId = 0,
				PersonId = analyticsPersonPeriodId,
				ScenarioId = analyticsScenario.ScenarioId,
				PreferenceTypeId = SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preferenceRestriction),
				ShiftCategoryId = shiftCategory?.ShiftCategoryId ?? -1,
				DayOffId = dayOffId,
				PreferencesRequested = 1,
				PreferencesFulfilled = permissionState == PermissionState.Satisfied ? 1 : 0,
				PreferencesUnfulfilled = permissionState == PermissionState.Satisfied ? 0 : 1,
				BusinessUnitId = businessUnitId.BusinessUnitId,
				DatasourceId = 1,
				DatasourceUpdateDate = preferenceDay.UpdatedOn.GetValueOrDefault(DateTime.Now),
				MustHaves = preferenceRestriction.MustHave ? 1 : 0,
				AbsenceId = absence?.AbsenceId ?? -1
			};
			return preferenceItem;
		}

		private IAnalyticsDate getAnalyticsDate(DateTime restrictionDate)
		{
			var dateId = _analyticsDateRepository.Date(restrictionDate.Date);
			if (dateId == null)
				throw new DateMissingInAnalyticsException(restrictionDate.Date);
			return dateId;
		}

		private static AnalyticsScenario getAnalyticsScenario(IEnumerable<AnalyticsScenario> analyticsScenarios, Guid scenarioId)
		{
			var scenario = analyticsScenarios.FirstOrDefault(a => a.ScenarioCode.GetValueOrDefault() == scenarioId);
			if (scenario == null)
				throw new ScenarioMissingInAnalyticsException();
			return scenario;
		}

		private static int getDayOff(IPreferenceRestriction preferenceRestriction, IEnumerable<AnalyticsDayOff> analyticsDayOffs, AnalyticBusinessUnit businessUnitId)
		{
			if (preferenceRestriction.DayOffTemplate == null)
				return -1;
			var dayOff = analyticsDayOffs.FirstOrDefault(a => a.DayOffName == preferenceRestriction.DayOffTemplate.Description.Name && a.BusinessUnitId == businessUnitId.BusinessUnitId);
			if (dayOff == null)
				throw new DayOffMissingInAnalyticsException();
			return dayOff.DayOffId;
		}

		private AnalyticBusinessUnit getBusinessUnit(IScenario scenario)
		{
			var analyticBusinessUnit = _analyticsBusinessUnitRepository.Get(scenario.BusinessUnit.Id.GetValueOrDefault());
			if (analyticBusinessUnit == null)
				throw new BusinessUnitMissingInAnalyticsException();
			return analyticBusinessUnit;
		}

		private AnalyticsPersonPeriod getAnalyticsPersonPeriod(DateTime restrictionDate, IPerson person)
		{
			var personPeriod = person.Period(new DateOnly(restrictionDate.Date));
			if (personPeriod == null)
				return null;
			var analyticsPersonPeriod = _analyticsPersonPeriodRepository.PersonPeriod(personPeriod.Id.GetValueOrDefault());
			if (analyticsPersonPeriod == null)
				throw new PersonPeriodMissingInAnalyticsException(personPeriod.Id.GetValueOrDefault());
			return analyticsPersonPeriod;
		}
	}
}
