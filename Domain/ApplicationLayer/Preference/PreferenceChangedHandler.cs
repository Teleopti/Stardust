using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Preference
{
	[UseOnToggle(Toggles.ETL_SpeedUpIntradayPreference_37124)]
	public class PreferenceChangedHandler :
		IHandleEvent<PreferenceChangedEvent>,
		IRunOnHangfire
	{
		private readonly static ILog logger = LogManager.GetLogger(typeof(PreferenceChangedHandler));
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IAnalyticsPreferenceRepository _analyticsPreferenceRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IAnalyticsDayOffRepository _analyticsDayOffRepository;
		private readonly RestrictionChecker restrictionChecker;
		private readonly ScheduleDictionaryLoadOptions scheduleDictionaryLoadOptions;

		public PreferenceChangedHandler(IScenarioRepository scenarioRepository,
			IPreferenceDayRepository preferenceDayRepository,
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository,
			IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsScheduleRepository analyticsScheduleRepository,
			IAnalyticsDateRepository analyticsDateRepository,
			IScheduleStorage scheduleStorage,
			IAnalyticsPreferenceRepository analyticsPreferenceRepository, 
			IPersonRepository personRepository, 
			IAnalyticsDayOffRepository analyticsDayOffRepository)
		{

			_scenarioRepository = scenarioRepository;
			_preferenceDayRepository = preferenceDayRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_analyticsDateRepository = analyticsDateRepository;
			_scheduleStorage = scheduleStorage;
			_analyticsPreferenceRepository = analyticsPreferenceRepository;
			_personRepository = personRepository;
			_analyticsDayOffRepository = analyticsDayOffRepository;


			restrictionChecker = new RestrictionChecker();
			scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, false, true) { LoadDaysAfterLeft = true };

			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of handler was created");
			}
		}

		[AsSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		public virtual void Handle(PreferenceChangedEvent @event)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Consuming event for preference Id = {0}. (Message timestamp = {1})",
								   @event.PreferenceDayId, @event.Timestamp);
			}

			// Preference, person look ups, mapping
			var preferenceDay = _preferenceDayRepository.Find(@event.PreferenceDayId);
			var dateId = _analyticsDateRepository.Date(@event.RestrictionDate.Date);
			var person = _personRepository.FindPeople(new[] { @event.PersonId }).First();
			var personPeriod = person.Period(new DateOnly(@event.RestrictionDate.Date));
			var analyticsPersonPeriodId = _analyticsPersonPeriodRepository.PersonPeriod(personPeriod.Id.GetValueOrDefault()).PersonId;

			if (preferenceDay == null)
			{
				// Preference day does not exists anymore so it has been deleted.
				_analyticsPreferenceRepository.DeletePreferences(dateId.Value, analyticsPersonPeriodId);
				return;
			}

			IList<IScenario> scenarios;
			if (@event.ScenarioId == Guid.Empty)
			{
				scenarios = _scenarioRepository.FindEnabledForReportingSorted();
			}
			else
			{
				scenarios = _scenarioRepository.FindEnabledForReportingSorted().Where(a => a.Id.Equals(@event.ScenarioId)).ToList();
				if (scenarios.IsEmpty())
				{
					logger.DebugFormat("Nothing to do with preference id {0} because scenario {1} was not found as reportable.",
						@event.PreferenceDayId, @event.ScenarioId);
					return;
				}
			}

			// General look ups
			var analyticsScenarios = _analyticsScheduleRepository.Scenarios();
			var analyticsDayOffs = _analyticsDayOffRepository.DayOffs();
			var analyticsAbsences = _analyticsScheduleRepository.Absences();
			var analyticsShiftCategories = _analyticsScheduleRepository.ShiftCategories();

			var period = new DateOnlyPeriod(new DateOnly(preferenceDay.RestrictionDate.Date), new DateOnly(preferenceDay.RestrictionDate.Date));
			List<AnalyticsFactSchedulePreference> resultFactSchedulePreference = new List<AnalyticsFactSchedulePreference>();

			foreach (var scenario in scenarios)
			{
				var schedulesDictionary = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, scheduleDictionaryLoadOptions, period, scenario);
				var scheduleParts = schedulesDictionary[person].ScheduledDayCollection(period);
				var businessUnitId = _analyticsBusinessUnitRepository.Get(scenario.BusinessUnit.Id.GetValueOrDefault());

				foreach (var schedulePart in scheduleParts)
				{
					var restrictionBases = schedulePart.RestrictionCollection();
					foreach (var restrictionBase in restrictionBases)
					{
						var preferenceRestriction = restrictionBase as IPreferenceRestriction;
						if (preferenceRestriction == null)
						{
							logger.ErrorFormat("Could not get restrictions for preference day id '{0}' for person with person code {1}",
								@event.PreferenceDayId, person.Id);
							continue;
						}
						if (!SchedulePreferenceTransformerHelper.CheckIfPreferenceIsValid(preferenceRestriction))
							continue;

						var preferenceItem = mapSchedulePreference(schedulePart, analyticsScenarios, scenario, analyticsShiftCategories, preferenceRestriction, analyticsAbsences, analyticsDayOffs, businessUnitId, dateId, analyticsPersonPeriodId, preferenceDay);
						resultFactSchedulePreference.Add(preferenceItem);
					}
				}
			}

			// Delete
			if (@event.ScenarioId == Guid.Empty)
			{
				_analyticsPreferenceRepository.DeletePreferences(dateId.Value, analyticsPersonPeriodId);
			}
			else
			{
				var scenarioId = analyticsScenarios.First(a => a.Code == @event.ScenarioId).Id;
				_analyticsPreferenceRepository.DeletePreferences(dateId.Value, analyticsPersonPeriodId, scenarioId);
			}

			// Insert
			foreach (var analyticsFactSchedulePreference in resultFactSchedulePreference)
			{
				_analyticsPreferenceRepository.AddPreference(analyticsFactSchedulePreference);
			}
		}

		private AnalyticsFactSchedulePreference mapSchedulePreference(IScheduleDay schedulePart, IList<IAnalyticsGeneric> analyticsScenarios, IScenario scenario, IList<IAnalyticsGeneric> analyticsShiftCategories,
			IPreferenceRestriction preferenceRestriction, IList<IAnalyticsAbsence> analyticsAbsences, IList<AnalyticsDayOff> analyticsDayOffs,
			AnalyticBusinessUnit businessUnitId, KeyValuePair<DateOnly, int> dateId, int analyticsPersonPeriodId, IPreferenceDay preferenceDay)
		{
			var permissionState = restrictionChecker.CheckPreference(schedulePart);
			var scenarioId = analyticsScenarios.First(a => a.Code == scenario.Id.GetValueOrDefault()).Id;
			var shiftCategory =
				analyticsShiftCategories.FirstOrDefault(
					a => a.Code == ((preferenceRestriction.ShiftCategory != null ? preferenceRestriction.ShiftCategory.Id : null) ?? Guid.Empty));
			var absence = analyticsAbsences.FirstOrDefault(a => a.AbsenceCode == ((preferenceRestriction.Absence != null ? preferenceRestriction.Absence.Id : null) ?? Guid.Empty));
			var dayOffId = preferenceRestriction.DayOffTemplate == null ? -1 :
				analyticsDayOffs.First(a => a.DayOffName == preferenceRestriction.DayOffTemplate.Description.Name && a.BusinessUnitId == businessUnitId.BusinessUnitId).DayOffId;

			var preferenceItem = new AnalyticsFactSchedulePreference
			{
				DateId = dateId.Value,
				IntervalId = 0,
				PersonId = analyticsPersonPeriodId,
				ScenarioId = scenarioId,
				PreferenceTypeId = SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preferenceRestriction),
				ShiftCategoryId = shiftCategory != null ? shiftCategory.Id : -1,
				DayOffId = dayOffId,
				PreferencesRequested = 1,
				PreferencesFulfilled = permissionState == PermissionState.Satisfied ? 1 : 0,
				PreferencesUnfulfilled = permissionState == PermissionState.Satisfied ? 0 : 1,
				BusinessUnitId = businessUnitId.BusinessUnitId,
				DatasourceId = 1,
				DatasourceUpdateDate = preferenceDay.UpdatedOn.GetValueOrDefault(DateTime.Now),
				MustHaves = preferenceRestriction.MustHave ? 1 : 0,
				AbsenceId = absence != null ? absence.AbsenceId : -1
			};
			return preferenceItem;
		}
	}
}
