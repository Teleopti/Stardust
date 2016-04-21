using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Preference
{
#pragma warning disable 618
	[UseOnToggle(Toggles.ETL_SpeedUpIntradayPreference_37124)]
	public class PreferenceChangedHandler :
		IHandleEvent<PreferenceChangedEvent>,
		IRunOnServiceBus
#pragma warning restore 618
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

		public PreferenceChangedHandler(IScenarioRepository scenarioRepository,
			IPreferenceDayRepository preferenceDayRepository,
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository,
			IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsScheduleRepository analyticsScheduleRepository,
			IAnalyticsDateRepository analyticsDateRepository,
			IScheduleStorage scheduleStorage,
			IAnalyticsPreferenceRepository analyticsPreferenceRepository)
		{
			_scenarioRepository = scenarioRepository;
			_preferenceDayRepository = preferenceDayRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsBusinessUnitRepository = analyticsBusinessUnitRepository;
			_analyticsScheduleRepository = analyticsScheduleRepository;
			_analyticsDateRepository = analyticsDateRepository;
			_scheduleStorage = scheduleStorage;
			_analyticsPreferenceRepository = analyticsPreferenceRepository;

			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of handler was created");
			}
		}

		public void Handle(PreferenceChangedEvent @event)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Consuming event for preference Id = {0}. (Message timestamp = {1})",
								   @event.PreferenceDayId, @event.Timestamp);
			}

			// General init
			var restrictionChecker = new RestrictionChecker();
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, false, true) { LoadDaysAfterLeft = true };
			var resultFactSchedulePreference = new List<AnalyticsFactSchedulePreference>();

			// General look ups
			var scenarios = _scenarioRepository.FindEnabledForReportingSorted();
			var analyticsScenarios = _analyticsScheduleRepository.Scenarios();
			var analyticsDayOffs = _analyticsScheduleRepository.DayOffs();
			var analyticsAbsences = _analyticsScheduleRepository.Absences();
			var analyticsShiftCategories = _analyticsScheduleRepository.ShiftCategories();

			// Preference and person look ups
			var preferenceDay = _preferenceDayRepository.Find(@event.PreferenceDayId);
			var personPeriod = preferenceDay.Person.Period(preferenceDay.RestrictionDate);
			var analyticsPersonPeriodId = _analyticsPersonPeriodRepository.PersonPeriod(personPeriod.Id.GetValueOrDefault()).PersonId;

			// Mapping
			var dateId = _analyticsDateRepository.Date(preferenceDay.RestrictionDate.Date);
			var period = new DateOnlyPeriod(new DateOnly(preferenceDay.RestrictionDate.Date), new DateOnly(preferenceDay.RestrictionDate.Date));
			var person = preferenceDay.Person;

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

						var permissionState = restrictionChecker.CheckPreference(schedulePart);
						var scenarioId = analyticsScenarios.First(a => a.Code == scenario.Id.GetValueOrDefault()).Id;
						var shiftCategory = analyticsShiftCategories.FirstOrDefault(a => a.Code == ((preferenceRestriction.ShiftCategory != null ? preferenceRestriction.ShiftCategory.Id : (Guid?) null) ?? Guid.Empty));
						var absence = analyticsAbsences.FirstOrDefault(a => a.AbsenceCode == ((preferenceRestriction.Absence != null ? preferenceRestriction.Absence.Id : (Guid?) null) ?? Guid.Empty));
						var dayOffId = preferenceRestriction.DayOffTemplate == null ? -1 : analyticsDayOffs.First(
											   a => a.DayOffName == preferenceRestriction.DayOffTemplate.Description.Name &&
													a.BusinessUnitId == businessUnitId.BusinessUnitId).DayOffId;

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
						resultFactSchedulePreference.Add(preferenceItem);
					}
				}
			}

			// Delete
			_analyticsPreferenceRepository.DeletePreferences(dateId.Value, analyticsPersonPeriodId);

			// Insert
			foreach (var analyticsFactSchedulePreference in resultFactSchedulePreference)
			{
				_analyticsPreferenceRepository.AddPreference(analyticsFactSchedulePreference);
			}
		}
	}
}
