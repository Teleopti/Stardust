using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

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

		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPreferenceDayRepository _preferenceDayRepository;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;
		private readonly IAnalyticsScheduleRepository _analyticsScheduleRepository;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IAnalyticsPreferenceRepository _analyticsPreferenceRepository;

		public PreferenceChangedHandler(ICurrentUnitOfWorkFactory unitOfWorkFactory,
			IScenarioRepository scenarioRepository,
			IPreferenceDayRepository preferenceDayRepository,
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository,
			IAnalyticsBusinessUnitRepository analyticsBusinessUnitRepository,
			IAnalyticsScheduleRepository analyticsScheduleRepository,
			IAnalyticsDateRepository analyticsDateRepository,
			IScheduleStorage scheduleStorage, 
			IAnalyticsPreferenceRepository analyticsPreferenceRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
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

			var preferenceDay = _preferenceDayRepository.Find(@event.PreferenceDayId);
			var restrictionChecker = new RestrictionChecker();

			var personPeriod = preferenceDay.Person.Period(preferenceDay.RestrictionDate);
			var analyticsPersonPeriodId = _analyticsPersonPeriodRepository.PersonPeriod(personPeriod.Id.GetValueOrDefault()).PersonId;

			var dateId = _analyticsDateRepository.Date(preferenceDay.RestrictionDate.Date);
			var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, false, true) { LoadDaysAfterLeft = true };

			var scenarios = _scenarioRepository.FindEnabledForReportingSorted();
			var period = new DateOnlyPeriod(new DateOnly(preferenceDay.RestrictionDate.Date), new DateOnly(preferenceDay.RestrictionDate.Date));
			var person = preferenceDay.Person;
			var analyticsScenarios = _analyticsScheduleRepository.Scenarios();

			var resultFactSchedulePreference = new List<AnalyticsFactSchedulePreference>();

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
						var shiftCategory = _analyticsScheduleRepository.ShiftCategories()
							.FirstOrDefault(a => a.Code == (preferenceRestriction.ShiftCategory?.Id ?? Guid.Empty));
						var absence = _analyticsScheduleRepository.Absences()
							.FirstOrDefault(a => a.AbsenceCode == (preferenceRestriction.Absence?.Id ?? Guid.Empty));

						var preferenceItem = new AnalyticsFactSchedulePreference
						{
							DateId = dateId.Value,
							IntervalId = 0,
							PersonId = analyticsPersonPeriodId,
							ScenarioId = scenarioId,
							PreferenceTypeId = SchedulePreferenceTransformerHelper.GetPreferenceTypeId(preferenceRestriction),
							ShiftCategoryId = shiftCategory?.Id ?? -1,
							DayOffId = 0, //vad? NOT NULL
							PreferencesRequested = 1,
							PreferencesFulfilled = permissionState == PermissionState.Satisfied ? 1 : 0,
							PreferencesUnfulFilled = permissionState == PermissionState.Satisfied ? 0 : 1,
							BusinessUnitId = businessUnitId.BusinessUnitId,
							DatasourceId = 1,
							DatasourceUpdateDate = preferenceDay.UpdatedOn.GetValueOrDefault(DateTime.Now),
							MustHaves = preferenceRestriction.MustHave ? 1 : 0,
							AbsenceId = absence?.AbsenceId ?? -1
						};
						resultFactSchedulePreference.Add(preferenceItem);
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
}
