using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.ReadModel;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Matrix;


namespace Teleopti.Analytics.Etl.Common.Interfaces.Transformer
{
	public interface IRaptorRepository
	{
		//Schedule
		IList<IActivity> LoadActivity();
		int PersistActivity(DataTable dataTable);
		int FillActivityDataMart(IBusinessUnit businessUnit);
        int FillDimensionsNotDefined();
        IList<IShiftCategory> LoadShiftCategory();
		int PersistShiftCategory(DataTable dataTable);
		int FillShiftCategoryDataMart(IBusinessUnit businessUnit);
		IList<IScenario> LoadScenario();
		int PersistScenario(DataTable dataTable);
		int FillScenarioDataMart(IBusinessUnit businessUnit);
		IList<IPerson> LoadPerson(ICommonStateHolder stateHolder);
		int PersistPerson(DataTable dataTable);
		int PersistAcdLogOnPerson(DataTable dataTable);
		int FillPersonDataMart(IBusinessUnit currentBusinessUnit);
		IScheduleDictionary LoadSchedule(DateTimePeriod period, IScenario scenario, ICommonStateHolder stateHolder);
		IScheduleDictionary LoadSchedule(DateTimePeriod period, IScenario scenario, IList<IPerson> persons);
		void TruncateSchedule();
		int PersistSchedule(DataTable scheduleDataTable, DataTable absenceDayCountDataTable);

		int FillScheduleDataMart(DateTimePeriod period, IBusinessUnit businessUnit);
		int PersistDate(DataTable dataTable);
		int FillDateDataMart();
		int PersistInterval(DataTable dataTable);
		int FillShiftLengthDataMart(IBusinessUnit businessUnit);
		int FillBusinessUnitDataMart(IBusinessUnit businessUnit);
		int FillSiteDataMart(IBusinessUnit businessUnit);
		int FillTeamDataMart(IBusinessUnit businessUnit);
		IList<IAbsence> LoadAbsence();
		int PersistAbsence(DataTable dataTable);
		int FillAbsenceDataMart(IBusinessUnit businessUnit);
		IList<ISkill> LoadSkill(IList<IActivity> activities);
		IList<ISkill> LoadSkillWithSkillDays(DateOnlyPeriod period);
		int PersistScheduleForecastSkill(DataTable dataTable);
		int FillScheduleForecastSkillDataMart(DateTimePeriod period, IBusinessUnit businessUnit);
		IList<IBusinessUnit> LoadBusinessUnit();
		int PersistBusinessUnit(DataTable dataTable);
		int FillScheduleDayCountDataMart(DateTimePeriod period, IBusinessUnit businessUnit);
		int FillDayOffDataMart(IBusinessUnit businessUnit);
		int PersistScheduleDayOffCount(DataTable dataTable);

		/// <summary>
		/// Persists the schedule preferences.
		/// </summary>
		/// <param name="dataTable">The data table.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Henryg
		/// Created date: 2009-11-18
		/// </remarks>
		int PersistSchedulePreferences(DataTable dataTable);

		/// <summary>
		/// Fills the fact schedule preference mart.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="defaultTimeZone">The default time zone.</param>
		/// <param name="businessUnit"></param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Henryg
		/// Created date: 2009-11-27
		/// </remarks>
		int FillFactSchedulePreferenceMart(DateOnlyPeriod period, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);

		//Queue stats
		int FillDimQueueDataMart(int dataSourceId, IBusinessUnit businessUnit);
		int FillFactQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);

		//Date and Time Zone
		int PersistTimeZoneDim(DataTable dataTable);
		int FillTimeZoneDimDataMart(IBusinessUnit businessUnit);
		int PersistTimeZoneBridge(DataTable dataTable, bool doTruncateTable);
		int FillTimeZoneBridgeDataMart();
		IEnumerable<TimeZoneInfo> LoadTimeZonesInUse();
		DateTime GetMaxDateInDimDate(bool isInitial);
		IList<TimeZonePeriod> GetBridgeTimeZoneLoadPeriod(TimeZoneInfo timeZone);

		//Forecast data
		IList<IWorkload> LoadWorkload();
		int PersistWorkload(DataTable dataTable);
		int PersistQueueWorkload(DataTable dataTable);
		int FillWorkloadDataMart(IBusinessUnit businessUnit);
		IDictionary<ISkill, IEnumerable<ISkillDay>> LoadSkillDays(DateTimePeriod period, IList<ISkill> skills, IScenario scenario);
		IEnumerable<ISkillDay> LoadSkillDays(IScenario scenario, DateTime lastCheck);
		int PersistForecastWorkload(DataTable dataTable);
		int FillForecastWorkloadDataMart(DateTimePeriod period, IBusinessUnit businessUnit);
		int FillSkillDataMart(IBusinessUnit businessUnit);

		//Agent stats
		int FillFactAgentDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);
		int FillFactAgentQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);
		int FillAcdLogOnDataMart(int dataSourceId);
		int FillScheduleDeviationDataMart(DateTimePeriod period, IBusinessUnit businessUnit, TimeZoneInfo defaultTimeZone, int isIntraday, DateTime? nowUtc);

		int FillScheduleDeviationDataMart_Story79646(DateTimePeriod period, IBusinessUnit businessUnit, TimeZoneInfo defaultTimeZone, int isIntraday, DateTime? nowUtc);

		//KPI
		IList<IKeyPerformanceIndicator> LoadKpi();
		int PersistKpi(DataTable dataTable);
		int FillKpiDataMart(IBusinessUnit businessUnit);
		IList<IScorecard> LoadScorecard();
		int PersistScorecard(DataTable dataTable);
		int FillScorecardDataMart();
		int PersistScorecardKpi(DataTable dataTable);
		int FillScorecardKpiDataMart(IBusinessUnit businessUnit);
		IList<IKpiTarget> LoadKpiTargetTeam();
		int PersistKpiTargetTeam(DataTable dataTable);
		int FillKpiTargetTeamDataMart(IBusinessUnit businessUnit);

		//Permission
		int PersistPermissionReport(DataTable dataTable);
		void TruncatePermissionReportTable();
		int FillPermissionDataMart(IBusinessUnit businessUnit);
		IList<MatrixPermissionHolder> LoadReportPermissions();

		int FillBridgeAcdLogOnPerson(IBusinessUnit businessUnit);
		int FillBridgeWorkloadQueue(IBusinessUnit businessUnit);

		//Person Skill
		int PersistSkill(DataTable dataTable);
		int PersistAgentSkill(DataTable dataTable);
		int FillSkillSetDataMart(IBusinessUnit businessUnit);
		int FillBridgeAgentSkillSetDataMart(IBusinessUnit businessUnit);
		int FillFactAgentSkillDataMart(IBusinessUnit businessUnit);
		
		//Users
		IList<IPerson> LoadUser();
		int PersistUser(DataTable dataTable);

		//Raptor synchronization
		int SynchronizeQueues(IList<IQueueSource> matrixQueues);
		int SynchronizeAgentLogOns(IList<IExternalLogOn> matrixAgentLogins);
		ReadOnlyCollection<IQueueSource> LoadQueues();
		ReadOnlyCollection<IExternalLogOn> LoadAgentLogins();

		// Cleanup
		int DimPersonDeleteData(IBusinessUnit businessUnit);
		int DimPersonTrimData(IBusinessUnit businessUnit);
		int DimTimeZoneDeleteData(IBusinessUnit businessUnit);
		int PerformMaintenance();
		int RunDelayedJob();
		int SqlServerUpdateStatistics();

		/// <summary>
		/// Loads the schedule parts per person and date.
		/// </summary>
		/// <param name="period">The period.</param>
		/// <param name="dictionary">The dictionary.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Henryg
		/// Created date: 2009-11-24
		/// </remarks>
		IList<IScheduleDay> LoadSchedulePartsPerPersonAndDate(DateTimePeriod period, IScheduleDictionary dictionary);

		/// <summary>
		/// Loads day offs from Raptor.
		/// </summary>
		/// <returns></returns>
		IList<IDayOffTemplate> LoadDayOff();

		/// <summary>
		/// Truncates stage table and then Persists all day offs found in scheduled preferences.
		/// </summary>
		/// <returns></returns>
		int PersistDayOffFromSchedulePreference();

		/// <summary>
		/// Truncates stage table and then Persists all day offs found in scheduled day offs.
		/// </summary>
		/// <returns></returns>
		int PersistDayOffFromScheduleDayOffCount();

		IList<IApplicationFunction> LoadApplicationFunction();
		IList<IApplicationRole> LoadApplicationRole(ICommonStateHolder stateHolder);
		IList<IAvailableData> LoadAvailableData();
		int PersistPmUser(DataTable dataTable);

		// Group Page dataprovider
		IList<IContract> LoadContract();
		IList<IContractSchedule> LoadContractSchedule();
		IList<IPartTimePercentage> LoadPartTimePercentage();
		IList<IRuleSetBag> LoadRuleSetBag();
		IList<IGroupPage> LoadUserDefinedGroupings();
		int PersistGroupPagePerson(DataTable dataTable);
		int FillGroupPagePersonDataMart(IBusinessUnit currentBusinessUnit);
		int FillGroupPagePersonBridgeDataMart(IBusinessUnit currentBusinessUnit);

		ICommonNameDescriptionSetting CommonAgentNameDescriptionSetting { get; }

		IList<IMultiplicatorDefinitionSet> LoadMultiplicatorDefinitionSet();
		int PersistOvertime(DataTable bulkInsertDataTable1);
		int FillOvertimeDataMart(IBusinessUnit businessUnit);
		IList<TimeZoneInfo> LoadTimeZonesInUseByDataSource();

		ITimeZoneDim DefaultTimeZone { get; }
		IList<IPersonRequest> LoadRequest(DateTimePeriod period);
		int PersistRequest(DataTable dataTable);
		void TruncateRequest();
		int FillFactRequestMart(DateTimePeriod period, IBusinessUnit businessUnit);
		int PerformPurge();
		int FillFactRequestedDaysMart(DateTimePeriod period, IBusinessUnit businessUnit);
		ILicenseStatusUpdater LicenseStatusUpdater { get; }
		int LoadQualityQuestDataMart(int dataSourceId, IBusinessUnit currentBusinessUnit);
		int FillFactQualityDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit currentBusinessUnit);

		ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName);
		IList<IScheduleChangedReadModel> ChangedDataOnStep(DateTime afterDate, IBusinessUnit currentBusinessUnit, string stepName);
		int PersistScheduleChanged(DataTable dataTable);
		void UpdateLastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTime thisTime);
		IEnumerable<IPreferenceDay> ChangedPreferencesOnStep(DateTime lastTime, IBusinessUnit currentBusinessUnit);
		IEnumerable<IStudentAvailabilityDay> ChangedAvailabilityOnStep(DateTime lastTime, IBusinessUnit currentBusinessUnit);
		int FillIntradayFactSchedulePreferenceMart(IBusinessUnit currentBusinessUnit, IScenario scenario);
		int PersistAvailability(DataTable dataTable);
		int FillFactAvailabilityMart(DateTimePeriod period, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);
		ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTimePeriod period);

		int FillIntradayFactQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);
		int FillIntradayFactAgentDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);
		int FillIntradayFactAgentQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);
		int PerformIndexMaintenance(DatabaseEnum database);
		int GetValidToDateIdLocalForEternity();
		int GetValidToDateIdMaxDateForEternity();
		IEnumerable<WindowsLogonInfo> GetWindowsLogonInfos();
		void FillJobIntradaySettingsMart();
		IList<IOptionalColumn> LoadOptionalColumnAvailableAsGroupPage();

		IEnumerable<ExternalStaff> GetExternalStaff(IEnumerable<ISkill> skills, DateTimePeriod period, ExternalStaffProvider externalStaffProvider);
	}
}
