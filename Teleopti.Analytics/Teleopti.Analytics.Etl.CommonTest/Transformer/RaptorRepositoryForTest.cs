using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Reflection;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.ReadModel;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Repositories;


namespace Teleopti.Analytics.Etl.CommonTest.Transformer
{
	public class RaptorRepositoryForTest : IRaptorRepository
	{
		public IList<IActivity> LoadActivity()
		{
			return new List<IActivity>();
		}

		public int PersistActivity(DataTable dataTable)
		{
			return dataTable.Rows.Count;
		}

		public int FillActivityDataMart(IBusinessUnit businessUnit)
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

		public int FillShiftCategoryDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		private IScenario _setScenario;
		public IList<IScenario> LoadScenario()
		{
			if (_setScenario != null)
				return new[] {_setScenario };
			// Create default scenario
			IScenario scenario1 = CreateScenario("My Default Scenario", true);

			// Create another...
			IScenario scenario2 = CreateScenario("Another Scenario", false);

			var scenarios = new List<IScenario>();
			scenarios.Add(scenario1);
			scenarios.Add(scenario2);

			return scenarios;
		}

		public void SetLoadScenario(IScenario scenario)
		{
			_setScenario = scenario;
		}

		private static IScenario CreateScenario(string name, bool isDefaultScenario)
		{
			IScenario scenario = new Scenario(name);
			scenario.SetId(Guid.NewGuid());
			scenario.DefaultScenario = isDefaultScenario;
			Type rootType = typeof(AggregateRoot_Events_ChangeInfo);
			rootType.GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(
				 scenario, new DateTime(1900, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			return scenario;
		}

		public int PersistScenario(DataTable dataTable)
		{
			return dataTable.Rows.Count;
		}

		public int FillScenarioDataMart(IBusinessUnit businessUnit)
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

		public int SqlServerUpdateStatistics()
		{
			return 0;
		}

		private IScheduleDictionary _scheduleDictionary;
		public IScheduleDictionary LoadSchedule(DateTimePeriod period, IScenario scenario, ICommonStateHolder stateHolder)
		{
			if (_scheduleDictionary != null)
				return _scheduleDictionary;
			IScheduleDateTimePeriod scheduleDateTimePeriod = new ScheduleDateTimePeriod(new DateTimePeriod());
			return new ScheduleDictionary(scenario, scheduleDateTimePeriod, new PersistableScheduleDataPermissionChecker(CurrentAuthorization.Make()), CurrentAuthorization.Make());
		}
		public void SetLoadSchedule(IScheduleDictionary scheduleDictionary)
		{
			_scheduleDictionary = scheduleDictionary;
		}

		public IScheduleDictionary LoadSchedule(DateTimePeriod period, IScenario scenario, IList<IPerson> persons)
		{
			throw new NotImplementedException();
		}

		public void TruncateSchedule()
		{
		}

		public int PersistSchedule(DataTable scheduleDataTable, DataTable absenceDayCountDataTable)
		{
			return scheduleDataTable.Rows.Count + absenceDayCountDataTable.Rows.Count;
		}

		public int FillScheduleDataMart(DateTimePeriod period, IBusinessUnit businessUnit)
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

		public int FillShiftLengthDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int FillBusinessUnitDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int FillSiteDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int FillTeamDataMart(IBusinessUnit businessUnit)
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

		public int FillAbsenceDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public IList<ISkill> LoadSkill(IList<IActivity> activities)
		{
			return new List<ISkill>();
		}

		private IList<ISkill> _loadSkillWithSkillDays = new List<ISkill>();
		public IList<ISkill> LoadSkillWithSkillDays(DateOnlyPeriod period)
		{
			return _loadSkillWithSkillDays;
		}
		public void SetLoadSkillWithSkillDays(IList<ISkill> skills)
		{
			_loadSkillWithSkillDays = skills;
		}

		public int PersistScheduleForecastSkill(DataTable dataTable)
		{
			return dataTable.Rows.Count;
		}

		public int FillScheduleForecastSkillDataMart(DateTimePeriod period, IBusinessUnit businessUnit)
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

		public int FillDayOffDataMart(IBusinessUnit businessUnit)
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

		public int FillFactSchedulePreferenceMart(DateOnlyPeriod period, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int FillDimQueueDataMart(int dataSourceId, IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int FillFactQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int PersistTimeZoneDim(DataTable dataTable)
		{
			return dataTable.Rows.Count;
		}

		public int FillTimeZoneDimDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int PersistTimeZoneBridge(DataTable dataTable, bool doTruncateTable)
		{
			return dataTable.Rows.Count;
		}

		public int FillTimeZoneBridgeDataMart()
		{
			return 0;
		}

		public IEnumerable<TimeZoneInfo> LoadTimeZonesInUse()
		{
			return new List<TimeZoneInfo>();
		}

		public IList<TimeZonePeriod> GetBridgeTimeZoneLoadPeriod(TimeZoneInfo timeZone)
		{
			return new List<TimeZonePeriod>();
		}

		public IList<IWorkload> LoadWorkload()
		{
			return new List<IWorkload>();
		}

		public int PersistWorkload(DataTable dataTable)
		{
			return dataTable.Rows.Count;
		}
        public int FillDimensionsNotDefined()
        {
            return 0;
        }
        public int PersistQueueWorkload(DataTable dataTable)
		{
			return 0;
		}

		public int FillWorkloadDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> LoadSkillDays(DateTimePeriod period, IList<ISkill> skills, IScenario scenario)
		{
			return _skillDays;
		}

		public IEnumerable<ISkillDay> LoadSkillDays(IScenario scenario, DateTime lastCheck)
		{
			throw new NotImplementedException();
		}

		private IDictionary<ISkill, IEnumerable<ISkillDay>> _skillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
		private List<ExternalStaff> _externalStaff;

		public void SetLoadSkillDays(IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays)
		{
			_skillDays = skillDays;
		}

		public int PersistForecastWorkload(DataTable dataTable)
		{
			return dataTable.Rows.Count;
		}

		public int FillForecastWorkloadDataMart(DateTimePeriod period, IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int FillSkillDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int FillAcdLogOnDataMart(int dataSourceId)
		{
			return 0;
		}

		public int FillScheduleDeviationDataMart(DateTimePeriod period, IBusinessUnit businessUnit, TimeZoneInfo defaultTimeZone, int isIntraday, DateTime? nowUtc)
		{
			return 0;
		}

		public int FillScheduleDeviationDataMart_Story79646(DateTimePeriod period, IBusinessUnit businessUnit,
			TimeZoneInfo defaultTimeZone, int isIntraday, DateTime? nowUtc)
		{
			throw new NotImplementedException();
		}

		public int FillFactAgentDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int FillFactAgentQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
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

		public int FillKpiDataMart(IBusinessUnit businessUnit)
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

		public int FillScorecardKpiDataMart(IBusinessUnit businessUnit)
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

		public int FillKpiTargetTeamDataMart(IBusinessUnit businessUnit)
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

		public int FillBridgeWorkloadQueue(IBusinessUnit businessUnit)
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

		public int FillSkillSetDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int FillBridgeAgentSkillSetDataMart(IBusinessUnit businessUnit)
		{
			return Ret;
		}

		public int FillFactAgentSkillDataMart(IBusinessUnit businessUnit)
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

		public int DimPersonDeleteData(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int DimPersonTrimData(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int DimTimeZoneDeleteData(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int PerformMaintenance()
		{
			return 0;
		}

		public int RunDelayedJob()
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

		public int PersistDayOffFromSchedulePreference()
		{
			return 0;
		}

		public int PersistDayOffFromScheduleDayOffCount()
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

		public int FillOvertimeDataMart(IBusinessUnit businessUnit)
		{
			return 0;
		}

		public IList<TimeZoneInfo> LoadTimeZonesInUseByDataSource()
		{
			return new List<TimeZoneInfo>();
		}

		public ITimeZoneDim DefaultTimeZone
		{
			get { return new TimeZoneDim(1, "UTC", "UTC", false, 0, 0, false); }
		}

		public IList<IPersonRequest> LoadRequest(DateTimePeriod period)
		{
			return new List<IPersonRequest>();
		}

		public IList<IPersonRequest> LoadIntradayRequest(DateTime lastTime)
		{
			return new List<IPersonRequest>();
		}

		public int PersistRequest(DataTable dataTable)
		{
			return 0;
		}

		public int FillFactRequestMart(DateTimePeriod period, IBusinessUnit businessUnit)
		{
			return 0;
		}

		public int PerformPurge()
		{
			return 0;
		}

		public int FillFactRequestedDaysMart(DateTimePeriod period, IBusinessUnit businessUnit)
		{
			return 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public ILicenseStatusUpdater LicenseStatusUpdater
		{
			get { throw new NotImplementedException(); }
		}

		public int LoadQualityQuestDataMart(int dataSourceId, IBusinessUnit currentBusinessUnit)
		{
			return 0;
		}

		public int FillFactQualityDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit currentBusinessUnit)
		{
			return 0;
		}

		public ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName)
		{
			return new LastChangedReadModel();
		}

		public IList<IScheduleChangedReadModel> ChangedDataOnStep(DateTime afterDate, IBusinessUnit currentBusinessUnit, string stepName)
		{
			throw new NotImplementedException();
		}

		public int PersistScheduleChanged(DataTable dataTable)
		{
			return 0;
		}

		public void UpdateLastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTime thisTime)
		{ }

		public IEnumerable<IPreferenceDay> ChangedPreferencesOnStep(DateTime lastTime, IBusinessUnit currentBusinessUnit)
		{
			return new List<IPreferenceDay>();
		}

		public IEnumerable<IStudentAvailabilityDay> ChangedAvailabilityOnStep(DateTime lastTime, IBusinessUnit currentBusinessUnit)
		{
			return new List<IStudentAvailabilityDay>();
		}

		public int FillIntradayFactSchedulePreferenceMart(IBusinessUnit currentBusinessUnit, IScenario scenario)
		{
			return 0;
		}

		public int PersistAvailability(DataTable dataTable)
		{
			return 0;
		}

		public int FillFactAvailabilityMart(DateTimePeriod period, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			return 0;
		}

		public ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTimePeriod period)
		{
			return new LastChangedReadModel();
		}

		public int FillIntradayFactQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone,
			IBusinessUnit businessUnit)
		{
			throw new NotImplementedException();
		}

		public int FillIntradayFactAgentDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone,
			IBusinessUnit businessUnit)
		{
			throw new NotImplementedException();
		}

		public int FillIntradayFactAgentQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone,
			IBusinessUnit businessUnit)
		{
			throw new NotImplementedException();
		}

		public int PerformIndexMaintenance(DatabaseEnum database)
		{
			throw new NotImplementedException();
		}

		public int GetValidToDateIdLocalForEternity()
		{
			throw new NotImplementedException();
		}

		public int GetValidToDateIdMaxDateForEternity()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<WindowsLogonInfo> GetWindowsLogonInfos()
		{
			throw new NotImplementedException();
		}

		public void FillJobIntradaySettingsMart()
		{
			throw new NotImplementedException();
		}

		public IList<IOptionalColumn> LoadOptionalColumnAvailableAsGroupPage()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ExternalStaff> GetExternalStaff(IEnumerable<ISkill> skills, DateTimePeriod period, ExternalStaffProvider externalStaffProvider)
		{
			return _externalStaff;
		}

		public DateTime GetMaxDateInDimDate(bool isInitial = false)
		{
			return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
		}

		public void TruncateRequest()
		{
		}

		public void SetLoadExternalStaffData(List<ExternalStaff> externalStaff)
		{
			_externalStaff = externalStaff;
		}
	}
}
