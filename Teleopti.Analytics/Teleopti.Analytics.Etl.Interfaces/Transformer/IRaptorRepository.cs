using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Interfaces.Transformer
{
    public interface IRaptorRepository
    {
        //Schedule
        IList<IActivity> LoadActivity();
        int PersistActivity(DataTable dataTable);
        int FillActivityDataMart();
        IList<IShiftCategory> LoadShiftCategory();
        int PersistShiftCategory(DataTable dataTable);
        int FillShiftCategoryDataMart();
        IList<IScenario> LoadScenario();
        int PersistScenario(DataTable dataTable);
        int FillScenarioDataMart();
        IList<IPerson> LoadPerson(ICommonStateHolder stateHolder);
        int PersistPerson(DataTable dataTable);
        int PersistAcdLogOnPerson(DataTable dataTable);
        int FillPersonDataMart(IBusinessUnit currentBusinessUnit);
        IScheduleDictionary LoadSchedule(DateTimePeriod period, IScenario scenario, ICommonStateHolder stateHolder);
        void TruncateSchedule();
        int PersistSchedule(DataTable scheduleDataTable, DataTable absenceDayCountDataTable);

        int FillScheduleDataMart(DateTimePeriod period);
        int FillScheduleContractDataMart(DateTimePeriod period);
        int PersistDate(DataTable dataTable);
        int FillDateDataMart();
        int PersistInterval(DataTable dataTable);
        int FillShiftLengthDataMart();
        int FillBusinessUnitDataMart();
        int FillSiteDataMart();
        int FillTeamDataMart();
        IList<IAbsence> LoadAbsence();
        int PersistAbsence(DataTable dataTable);
        int FillAbsenceDataMart();
        IList<ISkill> LoadSkill(IList<IActivity> activities);
        IList<ISkill> LoadSkillWithSkillDays(DateOnlyPeriod period);
        int PersistScheduleForecastSkill(DataTable dataTable);
        int FillScheduleForecastSkillDataMart(DateTimePeriod period);
        IList<IBusinessUnit> LoadBusinessUnit();
        int PersistBusinessUnit(DataTable dataTable);
        int FillScheduleDayCountDataMart(DateTimePeriod period, IBusinessUnit businessUnit);
        int FillDayOffDataMart();
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
        /// <returns></returns>
        /// <remarks>
        /// Created by: Henryg
        /// Created date: 2009-11-27
        /// </remarks>
        int FillFactSchedulePreferenceMart(DateTimePeriod period, TimeZoneInfo defaultTimeZone);

        //Queue stats
        int FillDimQueueDataMart(int dataSourceId);
        int FillFactQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone);

        //Date and Time Zone
        int PersistTimeZoneDim(DataTable dataTable);
        int FillTimeZoneDimDataMart();
        int PersistTimeZoneBridge(DataTable dataTable, bool doTruncateTable);
        int FillTimeZoneBridgeDataMart(DateTimePeriod period);
        IEnumerable<TimeZoneInfo> LoadTimeZonesInUse();
        DateTime GetMaxDateInDimDate();
        DateTimePeriod? GetBridgeTimeZoneLoadPeriod(TimeZoneInfo timeZone);

        //Forecast data
        IList<IWorkload> LoadWorkload();
        int PersistWorkload(DataTable dataTable);
        int PersistQueueWorkload(DataTable dataTable);
        int FillWorkloadDataMart();
        IDictionary<ISkill, IList<ISkillDay>> LoadSkillDays(DateTimePeriod period, IList<ISkill> skills, IScenario scenario);
        int PersistForecastWorkload(DataTable dataTable);
        int FillForecastWorkloadDataMart(DateTimePeriod period);
        int FillSkillDataMart();

        //Agent stats
        int FillFactAgentDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone);
        int FillFactAgentQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone);
        int FillAcdLogOnDataMart(int dataSourceId);
        int FillScheduleDeviationDataMart(DateTimePeriod period, IBusinessUnit businessUnit, TimeZoneInfo defaultTimeZone);

        //KPI
        IList<IKeyPerformanceIndicator> LoadKpi();
        int PersistKpi(DataTable dataTable);
        int FillKpiDataMart();
        IList<IScorecard> LoadScorecard();
        int PersistScorecard(DataTable dataTable);
        int FillScorecardDataMart();
        int PersistScorecardKpi(DataTable dataTable);
        int FillScorecardKpiDataMart();
        IList<IKpiTarget> LoadKpiTargetTeam();
        int PersistKpiTargetTeam(DataTable dataTable);
        int FillKpiTargetTeamDataMart();
        
        //Permission
        int PersistPermissionReport(DataTable dataTable);
        void TruncatePermissionReportTable();
		int FillPermissionDataMart(IBusinessUnit businessUnit);
        IList<MatrixPermissionHolder> LoadReportPermissions();

        int FillBridgeAcdLogOnPerson(IBusinessUnit businessUnit);
        int FillBridgeWorkloadQueue();

        //Person Skill
        int PersistSkill(DataTable dataTable);
        int PersistAgentSkill(DataTable dataTable);
        int FillSkillSetDataMart();
        int FillBridgeAgentSkillSetDataMart();

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
        int DimPersonDeleteData();
        int DimPersonTrimData();
        int DimScenarioDeleteData();
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
    	int FillOvertimeDataMart();
		IList<TimeZoneInfo> LoadTimeZonesInUseByDataSource();

    	ITimeZoneDim DefaultTimeZone { get; }
		IList<IPersonRequest> LoadRequest(DateTimePeriod period);
		int PersistRequest(DataTable dataTable);
    	int FillFactRequestMart(DateTimePeriod period);
    	int PerformPurge();
    }
}
