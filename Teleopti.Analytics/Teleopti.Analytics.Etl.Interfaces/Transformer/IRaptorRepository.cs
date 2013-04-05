using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IRaptorRepository
    {
        //Schedule
        IList<IActivity> LoadActivity();
        int PersistActivity(DataTable dataTable);
        int FillActivityDataMart(IBusinessUnit businessUnit);
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
        void TruncateSchedule();
        int PersistSchedule(DataTable scheduleDataTable, DataTable absenceDayCountDataTable);

        int FillScheduleDataMart(DateTimePeriod period, IBusinessUnit businessUnit);
        int FillScheduleContractDataMart(DateTimePeriod period);
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
        int FillFactSchedulePreferenceMart(DateTimePeriod period, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);

        //Queue stats
        int FillDimQueueDataMart(int dataSourceId, IBusinessUnit businessUnit);
        int FillFactQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);

        //Date and Time Zone
        int PersistTimeZoneDim(DataTable dataTable);
        int FillTimeZoneDimDataMart(IBusinessUnit businessUnit);
        int PersistTimeZoneBridge(DataTable dataTable, bool doTruncateTable);
        int FillTimeZoneBridgeDataMart(DateTimePeriod period);
        IEnumerable<TimeZoneInfo> LoadTimeZonesInUse();
        DateTime GetMaxDateInDimDate();
        DateTimePeriod? GetBridgeTimeZoneLoadPeriod(TimeZoneInfo timeZone);

        //Forecast data
        IList<IWorkload> LoadWorkload();
        int PersistWorkload(DataTable dataTable);
        int PersistQueueWorkload(DataTable dataTable);
        int FillWorkloadDataMart(IBusinessUnit businessUnit);
        IDictionary<ISkill, IList<ISkillDay>> LoadSkillDays(DateTimePeriod period, IList<ISkill> skills, IScenario scenario);
        int PersistForecastWorkload(DataTable dataTable);
        int FillForecastWorkloadDataMart(DateTimePeriod period, IBusinessUnit businessUnit);
        int FillSkillDataMart(IBusinessUnit businessUnit);

        //Agent stats
        int FillFactAgentDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);
        int FillFactAgentQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit);
        int FillAcdLogOnDataMart(int dataSourceId);
        int FillScheduleDeviationDataMart(DateTimePeriod period, IBusinessUnit businessUnit, TimeZoneInfo defaultTimeZone);

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
		int FillPermissionDataMart(IBusinessUnit businessUnit, bool isFirstBusinessUnit, bool isLastBusinessUnit);
        IList<MatrixPermissionHolder> LoadReportPermissions();

        int FillBridgeAcdLogOnPerson(IBusinessUnit businessUnit);
        int FillBridgeWorkloadQueue(IBusinessUnit businessUnit);

        //Person Skill
        int PersistSkill(DataTable dataTable);
        int PersistAgentSkill(DataTable dataTable);
        int FillSkillSetDataMart(IBusinessUnit businessUnit);
        int FillBridgeAgentSkillSetDataMart(IBusinessUnit businessUnit);

        //Users
        IList<IPerson> LoadUser();
        int PersistUser(DataTable dataTable);
        int FillAspNetUsersDataMart();

        //Raptor synchronization
        int SynchronizeQueues(IList<IQueueSource> matrixQueues);
        int SynchronizeAgentLogOns(IList<IExternalLogOn> matrixAgentLogins);
        ReadOnlyCollection<IQueueSource> LoadQueues();
        ReadOnlyCollection<IExternalLogOn> LoadAgentLogins();

        // Cleanup
        int DimPersonDeleteData(IBusinessUnit businessUnit);
        int DimPersonTrimData(IBusinessUnit businessUnit);
        int DimScenarioDeleteData(IBusinessUnit businessUnit);
		int DimTimeZoneDeleteData(IBusinessUnit businessUnit);
        int PerformMaintenance();

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
        /// Truncates stage table and then Persists all day offs in data table.
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        /// <returns></returns>
        int PersistDayOff(DataTable dataTable);

        IList<IApplicationFunction> LoadApplicationFunction();
        IList<IApplicationRole> LoadApplicationRole(ICommonStateHolder stateHolder);
        IList<IAvailableData> LoadAvailableData();
        IsolationLevel IsolationLevel { get; }
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
    	int FillFactRequestMart(DateTimePeriod period, IBusinessUnit businessUnit);
    	int PerformPurge();
        int FillFactRequestedDaysMart(DateTimePeriod period, IBusinessUnit businessUnit);
        ILicenseStatusUpdater LicenseStatusUpdater { get; }
        int LoadQualityQuestDataMart(int dataSourceId, IBusinessUnit currentBusinessUnit);
        int FillFactQualityDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit currentBusinessUnit);
    }
}
