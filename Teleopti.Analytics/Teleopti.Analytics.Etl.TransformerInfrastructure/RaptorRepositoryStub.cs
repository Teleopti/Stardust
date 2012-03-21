using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Reflection;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Analytics.Etl.TransformerInfrastructure
{
    public class RaptorRepositoryStub : IRaptorRepository
    {
        public IList<IActivity> LoadActivity()
        {
            return new List<IActivity>();
        }

        public int PersistActivity(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillActivityDataMart()
        {
            return 0;
        }

        public IList<IShiftCategory> LoadShiftCategory()
        {
            return new List<IShiftCategory>();
        }

        public int PersistShiftCategory(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillShiftCategoryDataMart()
        {
            return 0;
        }

        public IList<IScenario> LoadScenario()
        {
            // Create default scenario
            IScenario scenario1 = CreateScenario("My Default Scenario", true);

            // Create another...
            IScenario scenario2 = CreateScenario("Another Scenario", false);

            var scenarios = new List<IScenario>();
            scenarios.Add(scenario1);
            scenarios.Add(scenario2);

            return scenarios;
        }

        private static IScenario CreateScenario(string name, bool isDefaultScenario)
        {
            IScenario scenario = new Scenario(name);
            scenario.SetId(Guid.NewGuid());
            scenario.DefaultScenario = isDefaultScenario;
            //Type rootType = typeof(AggregateRootWithBusinessUnit);
            Type rootType = typeof(AggregateRoot);
            rootType.GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(
                scenario, new DateTime(1900, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            return scenario;
        }

        public int PersistScenario(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillScenarioDataMart()
        {
            return 0;
        }

        public IList<IPerson> LoadPerson(ICommonStateHolder stateHolder)
        {
            return new List<IPerson>();
        }

        public int PersistPerson(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int PersistAcdLogOnPerson(DataTable dataTable)
        {
            return 0;
        }

        public int FillPersonDataMart(IBusinessUnit currentBusinessUnit)
        {
            return 0;
        }

        public IScheduleDictionary LoadSchedule(DateTimePeriod period, IScenario scenario, ICommonStateHolder stateHolder)
        {
            IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(new DateTimePeriod());
            return new ScheduleDictionary(scenario, scheduleDateTimePeriod);
        }

        public void TruncateSchedule()
        {
        }

        //public int PersistSchedule(DataTable scheduleDataTable, DataTable contractDataTable, DataTable absenceDayCountDataTable)
        public int PersistSchedule(DataTable scheduleDataTable, DataTable absenceDayCountDataTable)
        {
            //return scheduleDataTable.Rows.Count + contractDataTable.Rows.Count + absenceDayCountDataTable.Rows.Count;
            return scheduleDataTable.Rows.Count + absenceDayCountDataTable.Rows.Count;
        }

        public int FillScheduleDataMart(DateTimePeriod period)
        {
            return 0;
        }

        public int FillScheduleContractDataMart(DateTimePeriod period)
        {
            return 0;
        }

        public int PersistDate(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillDateDataMart()
        {
            return 0;
        }

        public int PersistInterval(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillShiftLengthDataMart()
        {
            return 0;
        }

        public int FillBusinessUnitDataMart()
        {
            return 0;
        }

        public int FillSiteDataMart()
        {
            return 0;
        }

        public int FillTeamDataMart()
        {
            return 0;
        }

        public IList<IAbsence> LoadAbsence()
        {
            return new List<IAbsence>();
        }

        public int PersistAbsence(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillAbsenceDataMart()
        {
            return 0;
        }

        public IList<ISkill> LoadSkill(IList<IActivity> activities)
        {
            return new List<ISkill>();
        }

        public IList<ISkill> LoadSkillWithSkillDays(DateOnlyPeriod period)
        {
            return new List<ISkill>();
        }

        public int PersistScheduleForecastSkill(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillScheduleForecastSkillDataMart(DateTimePeriod period)
        {
            return 0;
        }

        public IList<IBusinessUnit> LoadBusinessUnit()
        {
            return new List<IBusinessUnit>();
        }

        public int PersistBusinessUnit(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillScheduleDayCountDataMart(DateTimePeriod period, IBusinessUnit businessUnit)
        {
            return 0;
        }

        public int FillDayOffDataMart()
        {
            return 0;
        }

        public int PersistScheduleDayOffCount(DataTable dataTable)
        {
            return 0;
        }

        public int PersistSchedulePreferences(DataTable dataTable)
        {
            return 0;
        }

        public int FillFactSchedulePreferenceMart(DateTimePeriod period, TimeZoneInfo defaultTimeZone)
        {
            return 0;
        }

        public int FillDimQueueDataMart(int dataSourceId)
        {
            return 0;
        }

        public int FillFactQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone)
        {
            return 0;
        }

        public int PersistTimeZoneDim(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillTimeZoneDimDataMart()
        {
            return 0;
        }

        public int PersistTimeZoneBridge(DataTable dataTable, bool doTruncateTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillTimeZoneBridgeDataMart(DateTimePeriod period)
        {
            return 0;
        }

        public IEnumerable<TimeZoneInfo> LoadTimeZonesInUse()
        {
            return new List<TimeZoneInfo>();
        }

        public DateTimePeriod? GetBridgeTimeZoneLoadPeriod(TimeZoneInfo timeZone)
        {
            return new DateTimePeriod();
        }

        public IList<IWorkload> LoadWorkload()
        {
            return new List<IWorkload>();
        }

        public int PersistWorkload(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int PersistQueueWorkload(DataTable dataTable)
        {
            return 0;
        }

        public int FillWorkloadDataMart()
        {
            return 0;
        }

        //public ICollection<SkillDay> LoadSkillDays(DateTimePeriod period)
        //{
        //    return new Collection<SkillDay>();
        //}

        public IDictionary<ISkill, IList<ISkillDay>> LoadSkillDays(DateTimePeriod period, IList<ISkill> skills, IScenario scenario)
        {
            return new Dictionary<ISkill, IList<ISkillDay>>();
        }

        public int PersistForecastWorkload(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillForecastWorkloadDataMart(DateTimePeriod period)
        {
            return 0;
        }

        public int FillSkillDataMart()
        {
            return 0;
        }

        public int FillAcdLogOnDataMart(int dataSourceId)
        {
            return 0;
        }

        public int FillScheduleDeviationDataMart(DateTimePeriod period, IBusinessUnit businessUnit, TimeZoneInfo defaultTimeZone)
        {
            return 0;
        }

        public int FillFactAgentDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone)
        {
            return 0;
        }

        public int FillFactAgentQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone)
        {
            return 0;
        }

        public IList<IKeyPerformanceIndicator> LoadKpi()
        {
            return new List<IKeyPerformanceIndicator>();
        }

        public int PersistKpi(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillKpiDataMart()
        {
            return 0;
        }

        public IList<IScorecard> LoadScorecard()
        {
            return new List<IScorecard>();
        }

        public int PersistScorecard(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillScorecardDataMart()
        {
            return 0;
        }

        public int PersistScorecardKpi(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillScorecardKpiDataMart()
        {
            return 0;
        }

        public IList<IKpiTarget> LoadKpiTargetTeam()
        {
            return new List<IKpiTarget>();
        }

        public int PersistKpiTargetTeam(DataTable dataTable)
        {
            return dataTable.Rows.Count;
        }

        public int FillKpiTargetTeamDataMart()
        {
            return 0;
        }

        public int PersistPermissionReport(DataTable dataTable)
        {
            return 0;
        }

        public void TruncatePermissionReportTable()
        {
        }

		public int FillPermissionDataMart(IBusinessUnit businessUnit)
        {
            return 0;
        }

        public IList<MatrixPermissionHolder> LoadReportPermissions()
        {
            return new List<MatrixPermissionHolder>();
        }

        public int Ret { set; get; }

        public int FillBridgeAcdLogOnPerson(IBusinessUnit businessUnit)
        {
            return Ret;
        }

        public int FillBridgeWorkloadQueue()
        {
            return Ret;
        }

        public int PersistSkill(DataTable dataTable)
        {
            return 0;
        }

        public int PersistAgentSkill(DataTable dataTable)
        {
            return 0;
        }

        public int FillSkillSetDataMart()
        {
            return 0;
        }

        public int FillBridgeAgentSkillSetDataMart()
        {
            return Ret;
        }

        public IList<IPerson> LoadUser()
        {
            return new List<IPerson>();
        }

        public int PersistUser(DataTable dataTable)
        {
            return 0;
        }

        public int FillAspNetUsersDataMart()
        {
            return 0;
        }

        public int SynchronizeQueues(IList<IQueueSource> matrixQueues)
        {
            return 0;
        }

        public int SynchronizeAgentLogOns(IList<IExternalLogOn> matrixAgentLogins)
        {
            return 0;
        }

        public ReadOnlyCollection<IQueueSource> LoadQueues()
        {
            return new ReadOnlyCollection<IQueueSource>(new List<IQueueSource>());
        }

        public ReadOnlyCollection<IExternalLogOn> LoadAgentLogins()
        {
            return new ReadOnlyCollection<IExternalLogOn>(new List<IExternalLogOn>());
        }

        public int DimPersonDeleteData()
        {
            return 0;
        }

        public int DimPersonTrimData()
        {
            return 0;
        }

        public int DimScenarioDeleteData()
        {
            return 0;
        }

        public int PerformMaintenance()
        {
            return 0;
        }

        public IList<IScheduleDay> LoadSchedulePartsPerPersonAndDate(DateTimePeriod period, IScheduleDictionary dictionary)
        {
            return new List<IScheduleDay>();
        }

        public IList<IDayOffTemplate> LoadDayOff()
        {
            return new List<IDayOffTemplate>();
        }

        public int PersistDayOff(DataTable dataTable)
        {
            return 0;
        }

        public IList<IApplicationFunction> LoadApplicationFunction()
        {
            return new List<IApplicationFunction>();
        }

        public IList<IApplicationRole> LoadApplicationRole(ICommonStateHolder stateHolder)
        {
            return new List<IApplicationRole>();
        }

        public IList<IAvailableData> LoadAvailableData()
        {
            return new List<IAvailableData>();
        }

        public IsolationLevel IsolationLevel
        {
            get { return IsolationLevel.ReadUncommitted; }
        }

        public int PersistPmUser(DataTable dataTable)
        {
            return 0;
        }

        public IList<IContract> LoadContract()
        {
            return new List<IContract>();
        }

        public IList<IContractSchedule> LoadContractSchedule()
        {
            return new List<IContractSchedule>();
        }

        public IList<IPartTimePercentage> LoadPartTimePercentage()
        {
            return new List<IPartTimePercentage>();
        }

        public IList<IRuleSetBag> LoadRuleSetBag()
        {
            return new List<IRuleSetBag>();
        }

        public IList<IGroupPage> LoadUserDefinedGroupings()
        {
            return new List<IGroupPage>();
        }

        public int PersistGroupPagePerson(DataTable dataTable)
        {
            return 0;
        }

        public int FillGroupPagePersonDataMart(IBusinessUnit currentBusinessUnit)
        {
            return 0;
        }

        public int FillGroupPagePersonBridgeDataMart(IBusinessUnit currentBusinessUnit)
        {
            return 0;
        }

        public ICommonNameDescriptionSetting CommonAgentNameDescriptionSetting
        {
            get
            {
                string aliasFormat = string.Format(CultureInfo.InvariantCulture, "{0} {1}",
                                                   CommonNameDescriptionSetting.FirstName,
                                                   CommonNameDescriptionSetting.LastName);
                return new CommonNameDescriptionSetting(aliasFormat);
            }
        }

    	public IList<IMultiplicatorDefinitionSet> LoadMultiplicatorDefinitionSet()
    	{
    		return new List<IMultiplicatorDefinitionSet>();
    	}

    	public int PersistOvertime(DataTable bulkInsertDataTable1)
    	{
    		return 0;
    	}

    	public int FillOvertimeDataMart()
    	{
    		return 0;
    	}

    	public IList<TimeZoneInfo> LoadTimeZonesInUseByDataSource()
    	{
    		return new List<TimeZoneInfo>();
    	}

    	public ITimeZoneDim DefaultTimeZone
    	{
    		get { return new TimeZoneDim(1, "UTC", "UTC", false, 0, 0); }
    	}

    	public IList<IPersonRequest> LoadRequest(DateTimePeriod period)
    	{
    		return new List<IPersonRequest>();
    	}

		public int PersistRequest(DataTable dataTable)
    	{
    		return 0;
    	}

		public int FillFactRequestMart(DateTimePeriod period)
		{
			return 0;
		}

    	public int PerformPurge()
    	{
    		return 0;
    	}
    	
        public int FillFactRequestedDaysMart(DateTimePeriod period)
        {
            return 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public ILicenseStatusUpdater LicenseStatusUpdater
        {
            get { throw new NotImplementedException(); }
        }

        public DateTime GetMaxDateInDimDate()
        {
            return new DateTime(1900,1,1,0,0,0,DateTimeKind.Unspecified);
        }
    }
}
