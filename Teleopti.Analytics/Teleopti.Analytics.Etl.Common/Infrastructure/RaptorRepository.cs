using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using log4net;
using NHibernate;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.ReadModel;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;


namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public class RaptorRepository : IRaptorRepository
	{
		private readonly string _dataMartConnectionString;
		private ILicenseStatusUpdater _licenseStatusUpdater;
		private readonly ILog _logger = LogManager.GetLogger(typeof(RaptorRepository));
		private readonly IIndexMaintenanceRepository _indexMaintenanceRepository;
		private readonly IAnalyticsPersonPeriodDateFixer _analyticsPersonPeriodDateFixer;

		public RaptorRepository(
			string dataMartConnectionString,
			IIndexMaintenanceRepository indexMaintenanceRepository,
			IAnalyticsPersonPeriodDateFixer analyticsPersonPeriodDateFixer)
		{
			_dataMartConnectionString = dataMartConnectionString;
			_indexMaintenanceRepository = indexMaintenanceRepository;
			_analyticsPersonPeriodDateFixer = analyticsPersonPeriodDateFixer;
		}

		public int PersistPmUser(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("mart.pm_user_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "mart.pm_user", _dataMartConnectionString);
		}

		public IList<IContract> LoadContract()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IContractRepository repository = new ContractRepository(uow);
				return repository.LoadAll().ToList();
			}
		}

		public IList<IContractSchedule> LoadContractSchedule()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IContractScheduleRepository repository = new ContractScheduleRepository(uow);
				return repository.LoadAll().ToList();
			}
		}

		public IList<IPartTimePercentage> LoadPartTimePercentage()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IPartTimePercentageRepository repository = new PartTimePercentageRepository(uow);
				return repository.LoadAll().ToList();
			}
		}

		public IList<IRuleSetBag> LoadRuleSetBag()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IRuleSetBagRepository repository = new RuleSetBagRepository(uow);
				return repository.LoadAll().ToList();
			}
		}

		public IList<IGroupPage> LoadUserDefinedGroupings()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//Avoid lazy load
				BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName();

				IGroupPageRepository repository = new GroupPageRepository(uow);
				return repository.LoadAllGroupPageBySortedByDescription();
			}
		}

		public int PersistGroupPagePerson(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_group_page_person_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_group_page_person", _dataMartConnectionString);
		}

		public int FillGroupPagePersonDataMart(IBusinessUnit currentBusinessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", currentBusinessUnit.Id) };
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_group_page_load", parameterList, _dataMartConnectionString);
		}

		public int FillGroupPagePersonBridgeDataMart(IBusinessUnit currentBusinessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", currentBusinessUnit.Id) };
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_bridge_group_page_person_load", parameterList, _dataMartConnectionString);
		}

		public ICommonNameDescriptionSetting CommonAgentNameDescriptionSetting
		{
			get
			{
				ICommonNameDescriptionSetting commonNameDescription;
				using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					ISettingDataRepository settingDataRepository = new GlobalSettingDataRepository(uow);

					commonNameDescription = settingDataRepository.FindValueByKey("CommonNameDescription",
																				 new CommonNameDescriptionSetting());
				}

				return commonNameDescription;
			}
		}

		public IList<IMultiplicatorDefinitionSet> LoadMultiplicatorDefinitionSet()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				IMultiplicatorDefinitionSetRepository repository = new MultiplicatorDefinitionSetRepository(uow);
				var multiplicatorDefninitionSets = repository.LoadAll().ToList();

				foreach (IMultiplicatorDefinitionSet multiplicatorDefinitionSet in multiplicatorDefninitionSets)
				{
					if (!LazyLoadingManager.IsInitialized(multiplicatorDefinitionSet.BusinessUnit))
					{
						LazyLoadingManager.Initialize(multiplicatorDefinitionSet.BusinessUnit);
					}
				}

				return multiplicatorDefninitionSets;
			}
		}

		public int PersistOvertime(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_overtime_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_overtime", _dataMartConnectionString);
		}

		public int FillOvertimeDataMart(IBusinessUnit businessUnit)
		{
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_overtime_load", null,
													_dataMartConnectionString);
		}

		public int FillActivityDataMart(IBusinessUnit businessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };
			return
					 HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_activity_load", parameterList,
											  _dataMartConnectionString);
		}

		public int FillAbsenceDataMart(IBusinessUnit businessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };

			return
					 HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_absence_load", parameterList,
											  _dataMartConnectionString);
		}

		public int FillDateDataMart()
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_date_load", null,
											  _dataMartConnectionString);
		}

		public int FillScheduleForecastSkillDataMart(DateTimePeriod period, IBusinessUnit businessUnit)
		{
			//Prepare sql parameters
			var parameterList = new[]
			{
				new SqlParameter("start_date", period.StartDateTime.Date),
				new SqlParameter("end_date", period.EndDateTime.Date),
				new SqlParameter("business_unit_code", businessUnit.Id)
			};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_schedule_forecast_skill_load", parameterList,
											  _dataMartConnectionString);
		}

		public IList<IBusinessUnit> LoadBusinessUnit()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				BusinessUnitRepository repository = new BusinessUnitRepository(uow);
				return repository.LoadAll().ToList();
			}
		}

		public int PersistBusinessUnit(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_business_unit_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_business_unit", _dataMartConnectionString);
		}

		public int FillScheduleDayCountDataMart(DateTimePeriod period, IBusinessUnit businessUnit)
		{
			//Prepare sql parameters
			var parameterList = new[]
			{
				new SqlParameter("start_date", period.StartDateTime.Date),
				new SqlParameter("end_date", period.EndDateTime.Date),
				new SqlParameter("business_unit_code", businessUnit.Id)
			};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_schedule_day_count_load", parameterList,
											  _dataMartConnectionString);
		}

		public int FillDayOffDataMart(IBusinessUnit businessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };
			return
					 HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_day_off_load", parameterList,
											  _dataMartConnectionString);
		}

		public int PersistScheduleDayOffCount(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_schedule_day_off_count_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_schedule_day_off_count", _dataMartConnectionString);
		}

		public int PersistSchedulePreferences(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_schedule_preference_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_schedule_preference", _dataMartConnectionString);
		}

		public int FillFactSchedulePreferenceMart(DateOnlyPeriod period, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			var startDate = period.StartDate;
			var endDate = period.EndDate;

			//Prepare sql parameters
			var parameterList = new[]
				{
					new SqlParameter("start_date", startDate.Date),
					new SqlParameter("end_date", endDate.Date),
					new SqlParameter("business_unit_code", businessUnit.Id)
				};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_schedule_preference_load", parameterList,
											  _dataMartConnectionString);

		}

		public int FillFactAvailabilityMart(DateTimePeriod period, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			//Convert time back to local time before sp call
			var startDate = TimeZoneInfo.ConvertTimeFromUtc(period.StartDateTime, defaultTimeZone);
			var endDate = TimeZoneInfo.ConvertTimeFromUtc(period.EndDateTime, defaultTimeZone);

			//Prepare sql parameters
			var parameterList = new[]
				{
					new SqlParameter("start_date", startDate.Date),
					new SqlParameter("end_date", endDate.Date),
					new SqlParameter("business_unit_code", businessUnit.Id)
				};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "[mart].[etl_fact_hourly_availability_load]", parameterList,
											  _dataMartConnectionString);

		}

		public ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTimePeriod period)
		{
			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
			{
				IEtlReadModelRepository rep = new EtlReadModelRepository(uow);
				return rep.LastChangedDate(currentBusinessUnit, stepName, period);
			}
		}

		public int FillIntradayFactSchedulePreferenceMart(IBusinessUnit currentBusinessUnit, IScenario scenario)
		{
			var parameterList = new[]
				{
					new SqlParameter("business_unit_code", currentBusinessUnit.Id),
					new SqlParameter("scenario_code", scenario.Id)
				};


			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_schedule_preference_intraday_load", parameterList,
											  _dataMartConnectionString);
		}

		public int PersistAvailability(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_availability_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "[stage].[stg_hourly_availability]", _dataMartConnectionString);
		}

		public int FillDimQueueDataMart(int dataSourceId, IBusinessUnit businessUnit)
		{
			//Prepare sql parameters
			var parameterList = new[] { new SqlParameter("datasource_id", dataSourceId) };

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_queue_load", parameterList,
											  _dataMartConnectionString);
		}

		public int FillFactQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			//Convert time back to local time before sp call
			var startDate = period.StartDateTimeLocal(defaultTimeZone);
			var endDate = period.EndDateTimeLocal(defaultTimeZone);

			//Prepare sql parameters
			var parameterList = new[]
			{
				new SqlParameter("start_date", startDate.Date),
				new SqlParameter("end_date", endDate.Date),
				new SqlParameter("datasource_id", dataSourceId)
			};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_queue_load", parameterList,
											  _dataMartConnectionString);
		}

		public int PersistAcdLogOnPerson(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_acd_login_person_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_acd_login_person", _dataMartConnectionString);
		}

		public int FillPersonDataMart(IBusinessUnit currentBusinessUnit)
		{
			if (currentBusinessUnit.Id == null)
			{
				throw new ArgumentNullException(nameof(currentBusinessUnit));
			}
			var parameterList = new[]
			{
				new SqlParameter("current_business_unit_code", currentBusinessUnit.Id.Value)
			};

			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure,
				"mart.etl_dim_person_load",
				parameterList,
				_dataMartConnectionString);
		}

		public int FillScenarioDataMart(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_scenario_load", null,
											  _dataMartConnectionString);
		}

		public int PersistSchedule(DataTable scheduleDataTable, DataTable absenceDayCountDataTable)
		{
			int affectedRows = HelperFunctions.BulkInsert(scheduleDataTable, "stage.stg_schedule", _dataMartConnectionString);
			affectedRows += HelperFunctions.BulkInsert(absenceDayCountDataTable, "stage.stg_schedule_day_absence_count", _dataMartConnectionString);

			return affectedRows;
		}

		public int FillScheduleDataMart(DateTimePeriod period, IBusinessUnit businessUnit)
		{
			var parameterList = new[]
			{
				new SqlParameter("start_date", period.StartDateTime.Date),
				new SqlParameter("end_date", period.EndDateTime.Date),
				new SqlParameter("business_unit_code", businessUnit.Id)
			};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_schedule_load", parameterList,
											  _dataMartConnectionString);
		}

		public int FillShiftCategoryDataMart(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_shift_category_load", null,
											  _dataMartConnectionString);
		}

		public int FillShiftLengthDataMart(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_shift_length_load", null,
											  _dataMartConnectionString);
		}

		public IList<IActivity> LoadActivity()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					//Avoid lazy load
					BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
					businessUnitRepository.LoadAllBusinessUnitSortedByName();

					ActivityRepository repository = new ActivityRepository(uow);

					return repository.LoadAll().ToList();
				}
			}
		}

		public IList<IApplicationFunction> LoadApplicationFunction()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//Avoid lazy load
				BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName();

				ApplicationFunctionRepository repository = new ApplicationFunctionRepository(uow);

				return repository.LoadAll().ToList();
			}
		}

		public IList<IAvailableData> LoadAvailableData()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//Avoid lazy load
				BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName();

				AvailableDataRepository repository = new AvailableDataRepository(uow);

				return repository.LoadAllAvailableData();
			}
		}

		public IList<IApplicationRole> LoadApplicationRole(ICommonStateHolder stateHolder)
		{
			var applicationFunctions = stateHolder.ApplicationFunctionCollection;
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(applicationFunctions);
				//Avoid lazy load
				BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName();

				ApplicationRoleRepository repository = new ApplicationRoleRepository(uow);
				return repository.LoadAll().ToList();
			}
		}

		public IList<IAbsence> LoadAbsence()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					//Avoid lazy load
					BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
					businessUnitRepository.LoadAllBusinessUnitSortedByName();

					AbsenceRepository repository = new AbsenceRepository(uow);
					return repository.LoadAll().ToList();
				}
			}
		}

		public IList<IPerson> LoadPerson(ICommonStateHolder stateHolder)
		{
			var activities = stateHolder.ActivityCollection;
			var skills = stateHolder.SkillCollection;
			var shiftCategories = stateHolder.ShiftCategoryCollection;

			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(activities);
				uow.Reassociate(skills);
				uow.Reassociate(shiftCategories);

				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					new ContractScheduleRepository(uow).LoadAllAggregate();
					new ContractRepository(uow).LoadAll();
					new PartTimePercentageRepository(uow).LoadAll();
					new RuleSetBagRepository(uow).LoadAll();
					new ScorecardRepository(uow).LoadAll();
					new SkillTypeRepository(uow).LoadAll();
				}

				PersonRepository repository = new PersonRepository(new ThisUnitOfWork(uow));
				//return repository.LoadAll();
				List<IPerson> retList = new List<IPerson>();
				// We want to load all persons, therefore we use a large date scope.
				var foreverPeriod = new DateOnlyPeriod(1900, 1, 1, 9999, 12, 31);
				retList.AddRange(repository.FindAllAgents(foreverPeriod, false));

				initializePersonPeriod(retList);

				return retList;
			}
		}

		private static void initializePersonPeriod(IEnumerable<IPerson> retList)
		{
			foreach (var person in retList)
			{
				foreach (var personPeriod in person.PersonPeriodCollection)
				{
					if (!LazyLoadingManager.IsInitialized(personPeriod.PersonContract.AverageWorkTimePerDay))
						LazyLoadingManager.Initialize(personPeriod.PersonContract.AverageWorkTimePerDay);
					foreach (IPersonSkill personSkill in personPeriod.PersonSkillCollection)
					{
						if (!LazyLoadingManager.IsInitialized(personSkill.Skill.Activity))
							LazyLoadingManager.Initialize(personSkill.Skill.Activity);
					}
					if (!LazyLoadingManager.IsInitialized(personPeriod.Team.Site.BusinessUnit))
						LazyLoadingManager.Initialize(personPeriod.Team.Site.BusinessUnit);
				}
			}
		}

		public IList<IPerson> LoadUser()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repository = new PersonRepository(new ThisUnitOfWork(uow));
				IList<IPerson> retList = repository.FindPersonsThatAlsoAreUsers();

				return retList;
			}
		}

		public IList<IScenario> LoadScenario()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					//Avoid lazy load
					BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
					businessUnitRepository.LoadAllBusinessUnitSortedByName();

					ScenarioRepository repository = new ScenarioRepository(uow);
					return repository.FindEnabledForReportingSorted();
				}
			}
		}

		public IScheduleDictionary LoadSchedule(DateTimePeriod period, IScenario scenario, ICommonStateHolder stateHolder)
		{
			var persons = stateHolder.PersonCollection;
			return LoadSchedule(period, scenario, persons.ToList());
		}

		public IScheduleDictionary LoadSchedule(DateTimePeriod period, IScenario scenario, IList<IPerson> persons)
		{
			RemoveDuplicatesWorkaroundFor27636();
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//Avoid lazy load error
				avoidLazyLoadForLoadSchedule(uow, persons);

				var currentUnitOfWork = new ThisUnitOfWork(uow);
				var currentAuthorization = CurrentAuthorization.Make();
				var personAssignmentRepository = new PersonAssignmentRepository(currentUnitOfWork);
				var personAbsenceRepository = new PersonAbsenceRepository(currentUnitOfWork);
				var agentDayScheduleTagRepository = new AgentDayScheduleTagRepository(currentUnitOfWork);
				var noteRepository = new NoteRepository(currentUnitOfWork);
				var publicNoteRepository = new PublicNoteRepository(currentUnitOfWork);
				var preferenceDayRepository = new PreferenceDayRepository(currentUnitOfWork);
				var studentAvailabilityDayRepository = new StudentAvailabilityDayRepository(currentUnitOfWork);
				var overtimeAvailabilityRepository = new OvertimeAvailabilityRepository(currentUnitOfWork);
				var scheduleRepository = new ScheduleStorage(currentUnitOfWork,
					personAssignmentRepository, personAbsenceRepository,
					new MeetingRepository(currentUnitOfWork), agentDayScheduleTagRepository,
					noteRepository, publicNoteRepository,
					preferenceDayRepository,
					studentAvailabilityDayRepository,
					new PersonAvailabilityRepository(currentUnitOfWork),
					new PersonRotationRepository(currentUnitOfWork),
					overtimeAvailabilityRepository,
					new PersistableScheduleDataPermissionChecker(currentAuthorization),
					new ScheduleStorageRepositoryWrapper(() => personAssignmentRepository,
						() => personAbsenceRepository,
						() => preferenceDayRepository, () => noteRepository,
						() => publicNoteRepository,
						() => studentAvailabilityDayRepository,
						() => agentDayScheduleTagRepository,
						() => overtimeAvailabilityRepository),
					currentAuthorization);
				var scheduleDictionaryLoadOptions = new ScheduleDictionaryLoadOptions(true, false, true)
					{LoadDaysAfterLeft = true};

				var schedulesDictionary = scheduleRepository.FindSchedulesForPersons(scenario, persons, scheduleDictionaryLoadOptions, period, persons, false);

				//Clean ScheduleDictionary from all persons not present in LoadedAgents
				IList<IPerson> personsToRemove = new List<IPerson>();
				foreach (var keyValuePair in schedulesDictionary)
				{
					if (!persons.Contains(keyValuePair.Key))
						personsToRemove.Add(keyValuePair.Key);
				}
				foreach (var person in personsToRemove)
				{
					schedulesDictionary.Remove(person);
				}

				return schedulesDictionary;
			}
		}

		public void RemoveDuplicatesWorkaroundFor27636()
		{
			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
			{
				IEtlReadModelRepository rep = new EtlReadModelRepository(uow);
				rep.WorkAroundFor27636();
			}

		}

		private static void avoidLazyLoadForLoadSchedule(IUnitOfWork uow, IEnumerable<IPerson> persons)
		{
			//just dirty fix for now
			using (uow.DisableFilter(QueryFilter.Deleted))
			{
				new ActivityRepository(uow).LoadAll();
				new AbsenceRepository(uow).LoadAll();
				new ShiftCategoryRepository(uow).LoadAll();
				new DayOffTemplateRepository(uow).LoadAll();
				new MultiplicatorDefinitionSetRepository(uow).LoadAll();
			}

			uow.Reassociate(persons);

			var businessUnitRepository = new BusinessUnitRepository(uow);
			var businessUnitList = businessUnitRepository.LoadAll();
			var scenarioRep = new ScenarioRepository(uow);
			IList<IScenario> scenarioList = scenarioRep.FindAllSorted();

			Trace.WriteLine("Lazy load business unit list: " + businessUnitList.Count());
			Trace.WriteLine("Lazy load scenario list: " + scenarioList.Count);
		}

		public int PerformMaintenance()
		{
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_data_mart_maintenance", null,
													_dataMartConnectionString);
		}

		public int RunDelayedJob()
		{
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_execute_delayed_job", null,
																_dataMartConnectionString);
		}
		public int SqlServerUpdateStatistics()
		{
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_data_mart_updatestat", null,
																_dataMartConnectionString);
		}

		public int PerformPurge()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var purgeCommand = new PurgeApplicationData(UnitOfWorkFactory.Current);
				purgeCommand.Execute();
				uow.PersistAll();
			}
			return 1;
		}

		public ILicenseStatusUpdater LicenseStatusUpdater
		{
			get
			{
				if (_licenseStatusUpdater == null)
				{
					_licenseStatusUpdater =
						 new LicenseStatusUpdater(new LicenseStatusRepositories(UnitOfWorkFactory.Current,
																								  new RepositoryFactory()));
				}
				return _licenseStatusUpdater;
			}
		}

		public int LoadQualityQuestDataMart(int dataSourceId, IBusinessUnit currentBusinessUnit)
		{
			//Prepare sql parameters
			var parameterList = new[] { new SqlParameter("datasource_id", dataSourceId) };

			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_quality_quest_load", parameterList,
																_dataMartConnectionString);
		}

		public int FillFactQualityDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit currentBusinessUnit)
		{
			//Convert time back to local time before sp call
			DateTime startDate = TimeZoneInfo.ConvertTimeFromUtc(period.StartDateTime, defaultTimeZone);
			DateTime endDate = TimeZoneInfo.ConvertTimeFromUtc(period.EndDateTime, defaultTimeZone);

			//Prepare sql parameters
			var parameterList = new[]
			{
				new SqlParameter("start_date", startDate.Date),
				new SqlParameter("end_date", endDate.Date),
				new SqlParameter("datasource_id", dataSourceId)
			};

			return
				 HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_quality_load", parameterList,
														 _dataMartConnectionString);
		}

		public ILastChangedReadModel LastChangedDate(IBusinessUnit currentBusinessUnit, string stepName)
		{
			return LastChangedDate(currentBusinessUnit, stepName, new DateTimePeriod(2000, 1, 1, 2100, 1, 1));
		}

		public IList<IScheduleChangedReadModel> ChangedDataOnStep(DateTime afterDate, IBusinessUnit currentBusinessUnit, string stepName)
		{
			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
			{
				IEtlReadModelRepository rep = new EtlReadModelRepository(uow);
				return rep.ChangedDataOnStep(afterDate, currentBusinessUnit, stepName);
			}
		}

		public int PersistScheduleChanged(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.stg_schedule_changed_delete", _dataMartConnectionString);
			int rowCount = HelperFunctions.BulkInsert(dataTable, "stage.[stg_schedule_changed]", _dataMartConnectionString);
			return rowCount;

		}

		private TResult repositoryActionWithRetry<TResult>(Func<IUnitOfWork, TResult> innerAction, int attempt = 0)
		{
			try
			{
				using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
				{
					var ret = innerAction(uow);
					uow.PersistAll();
					return ret;
				}
			}
			catch (DataSourceException ex)
			{
				if (ex.InnerException is TransactionException && attempt < 6)
				{
					_logger.Warn(String.Format("Retry - Count:{0}, Exception:{1}, StackTrace:{2}", attempt, ex, ex.StackTrace));
					return repositoryActionWithRetry(innerAction, ++attempt);
				}
				throw;
			}
		}

		public void UpdateLastChangedDate(IBusinessUnit currentBusinessUnit, string stepName, DateTime thisTime)
		{
			repositoryActionWithRetry(uow =>
			{
				IEtlReadModelRepository rep = new EtlReadModelRepository(uow);
				rep.UpdateLastChangedDate(currentBusinessUnit, stepName, thisTime);
				return 0;
			});
		}

		public IEnumerable<IPreferenceDay> ChangedPreferencesOnStep(DateTime lastTime, IBusinessUnit currentBusinessUnit)
		{
			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
			{
				var rep = new PreferenceDayRepository(new ThisUnitOfWork(uow));
				return rep.FindNewerThan(lastTime);
			}
		}

		public IEnumerable<IStudentAvailabilityDay> ChangedAvailabilityOnStep(DateTime lastTime,
																			IBusinessUnit currentBusinessUnit)
		{
			using (var uow = UnitOfWorkFactory.CurrentUnitOfWorkFactory().Current().CreateAndOpenUnitOfWork())
			{
				var rep = new StudentAvailabilityDayRepository(uow);
				return rep.FindNewerThan(lastTime);
			}
		}

		public IList<IScheduleDay> LoadSchedulePartsPerPersonAndDate(DateTimePeriod period, IScheduleDictionary dictionary)
		{
			return dictionary.Keys.GroupBy(p => p.PermissionInformation.DefaultTimeZone()).SelectMany(g =>
			{
				var dateOnlyPeriod = period.ToDateOnlyPeriod(g.Key);
				return dictionary.SchedulesForPeriod(dateOnlyPeriod, g.ToArray());
			}).ToArray();
		}

		public IList<IDayOffTemplate> LoadDayOff()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//Avoid lazy load
				BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName();

				var repository = new DayOffTemplateRepository(uow);
				return repository.FindAllDayOffsSortByDescription();
			}
		}

		public int PersistDayOffFromSchedulePreference()
		{
			HelperFunctions.TruncateTable("stage.etl_stg_day_off_delete", _dataMartConnectionString);
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "stage.etl_stg_day_off_load_from_schedule_preference", null, _dataMartConnectionString);
		}

		public int PersistDayOffFromScheduleDayOffCount()
		{
			HelperFunctions.TruncateTable("stage.etl_stg_day_off_delete", _dataMartConnectionString);
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "stage.etl_stg_day_off_load_from_schedule_day_off", null, _dataMartConnectionString);
		}


		public IList<IShiftCategory> LoadShiftCategory()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					//Avoid lazy load
					BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
					businessUnitRepository.LoadAllBusinessUnitSortedByName();

					ShiftCategoryRepository repository = new ShiftCategoryRepository(uow);
					return repository.LoadAll().ToList();
				}
			}
		}

		public int PersistActivity(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_activity_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_activity", _dataMartConnectionString);
		}

		public int PersistPermissionReport(DataTable dataTable)
		{
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_permission_report", _dataMartConnectionString);
		}

		public void TruncatePermissionReportTable()
		{
			HelperFunctions.TruncateTable("stage.etl_stg_permission_report_delete", _dataMartConnectionString);
		}

		public int PersistAbsence(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_absence_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_absence", _dataMartConnectionString);
		}

		public int PersistDate(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_date_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_date", _dataMartConnectionString);
		}

		public int PersistInterval(DataTable dataTable)
		{
			//Check if dim_interval contains rows. If so - donothing
			int rowCount = (int)HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.etl_dim_interval_check", _dataMartConnectionString);

			if (rowCount == 0)
				return HelperFunctions.BulkInsert(dataTable, "mart.dim_interval", _dataMartConnectionString);
			return 0;
		}

		public int PersistPerson(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_person_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_person", _dataMartConnectionString);
		}

		public int PersistUser(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_user_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_user", _dataMartConnectionString);
		}


		public int SynchronizeQueues(IList<IQueueSource> matrixQueues)
		{
			return repositoryActionWithRetry(uow => MatrixSync.SynchronizeQueueSources(uow, matrixQueues));
		}

		public int SynchronizeAgentLogOns(IList<IExternalLogOn> matrixAgentLogins)
		{
			return repositoryActionWithRetry(uow => MatrixSync.SynchronizeExternalLogOns(uow, matrixAgentLogins));
		}

		public ReadOnlyCollection<IQueueSource> LoadQueues()
		{
			IList<IQueueSource> queueSources = new List<IQueueSource>();
			DataSet dataSet = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.raptor_load_queues", null, _dataMartConnectionString);

			if (dataSet != null && dataSet.Tables.Count == 1)
			{
				foreach (DataRow row in dataSet.Tables[0].Rows)
				{
					string description = row["Description"] == DBNull.Value ? string.Empty : (string)row["Description"];
					string queueOriginalId = row["QueueOriginalId"] == DBNull.Value
											  ? "-1"
											  : (string)row["QueueOriginalId"];
					int queueAgglId = row["QueueAggId"] == DBNull.Value ? -1 : (int)row["QueueAggId"];
					int dsId = row["DataSourceId"] == DBNull.Value ? -1 : (short)row["DataSourceId"];

					IQueueSource queue = new QueueSource((string)row["Name"], description, queueOriginalId, queueAgglId, (int)row["QueueMartId"], dsId);
					queue.LogObjectName = row["LogObjectName"] == DBNull.Value ? string.Empty : (string)row["LogObjectName"];
					queueSources.Add(queue);
				}
			}

			return new ReadOnlyCollection<IQueueSource>(queueSources);
		}

		public ReadOnlyCollection<IExternalLogOn> LoadAgentLogins()
		{
			IList<IExternalLogOn> externalLogOns = new List<IExternalLogOn>();
			DataSet dataSet = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure, "mart.raptor_load_acd_logins", null, _dataMartConnectionString);

			if (dataSet != null && dataSet.Tables.Count == 1)
			{
				foreach (DataRow row in dataSet.Tables[0].Rows)
				{
					string acdLogOnOriginalId = row["AcdLogOnOriginalId"] == DBNull.Value ? null : (string)row["AcdLogOnOriginalId"];
					string acdLogOnName = row["AcdLogOnName"] == DBNull.Value ? null : (string)row["AcdLogOnName"];
					int acdLogOnAggId = row["AcdLogOnAggId"] == DBNull.Value ? -1 : (int)row["AcdLogOnAggId"];
					int dsId = row["DataSourceId"] == DBNull.Value ? -1 : (short)row["DataSourceId"];
					string dsName = row["DataSourceName"] == DBNull.Value ? null : (string)row["DataSourceName"];

					IExternalLogOn externalLogOn = new ExternalLogOn((int)row["AcdLogOnMartId"], acdLogOnAggId,
																	 acdLogOnOriginalId, acdLogOnName, true);
					externalLogOn.DataSourceId = dsId;
					externalLogOn.DataSourceName = dsName;
					externalLogOns.Add(externalLogOn);
				}
			}

			return new ReadOnlyCollection<IExternalLogOn>(externalLogOns);
		}

		public int DimPersonDeleteData(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_person_delete", null,
											  _dataMartConnectionString);
		}

		public int DimPersonTrimData(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_person_trim", null,
											  _dataMartConnectionString);
		}

		public int DimTimeZoneDeleteData(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_time_zone_delete", null,
											  _dataMartConnectionString);
		}

		public int PersistScenario(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_scenario_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_scenario", _dataMartConnectionString);
		}

		public void TruncateSchedule()
		{
			HelperFunctions.TruncateTable("stage.etl_stg_schedule_delete", _dataMartConnectionString);
			//HelperFunctions.TruncateTable("stage.etl_stg_contract_delete", _dataMartConnectionString);
			HelperFunctions.TruncateTable("stage.etl_stg_schedule_day_absence_count_delete", _dataMartConnectionString);
		}

		public int PersistShiftCategory(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_shift_category_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_shift_category", _dataMartConnectionString);
		}

		public int PersistTimeZoneDim(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_time_zone_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_time_zone", _dataMartConnectionString);
		}
		public int FillTimeZoneDimDataMart(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_time_zone_load", null,
												_dataMartConnectionString);
		}
		public int PersistTimeZoneBridge(DataTable dataTable, bool doTruncateTable)
		{
			if (doTruncateTable)
				HelperFunctions.TruncateTable("stage.etl_stg_time_zone_bridge_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_time_zone_bridge", _dataMartConnectionString);
		}

		public int FillTimeZoneBridgeDataMart()
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_bridge_time_zone_load", null,
												_dataMartConnectionString);
		}

		public IEnumerable<TimeZoneInfo> LoadTimeZonesInUse()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				BusinessUnitRepository bur = new BusinessUnitRepository(uow);
				return bur.LoadAllTimeZones();
			}
		}

		public IList<TimeZoneInfo> LoadTimeZonesInUseByDataSource()
		{
			DataTable dataTable = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure,
																 "mart.sys_get_timezones_used_by_datasource", null,
																 _dataMartConnectionString).Tables[0];

			return (from DataRow dataRow in dataTable.Rows
					select TimeZoneInfo.FindSystemTimeZoneById((string)dataRow["time_zone_code"])).ToList();
		}

		public ITimeZoneDim DefaultTimeZone
		{
			get
			{
				var generalInfrastructure = new GeneralInfrastructure(new BaseConfigurationRepository());
				generalInfrastructure.SetDataMartConnectionString(_dataMartConnectionString);
				return generalInfrastructure.DefaultTimeZone;
			}
		}

		public IList<IPersonRequest> LoadRequest(DateTimePeriod period)
		{
			IList<IPersonRequest> personRequests;
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var rep = new PersonRequestRepository(uow);
				uow.Reassociate(((ITeleoptiIdentityForLegacy)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity).BusinessUnit());
				personRequests = rep.FindPersonRequestWithinPeriod(period);
			}
			return personRequests;
		}

		public void TruncateRequest()
		{
			HelperFunctions.TruncateTable("stage.etl_stg_request_delete", _dataMartConnectionString);
		}

		public int PersistRequest(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_request_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_request", _dataMartConnectionString);
		}

		public int FillFactRequestMart(DateTimePeriod period, IBusinessUnit businessUnit)
		{
			// currently we are considering only UTC time instead of converting into Local if we need it in future we will change it here.

			//Prepare sql parameters
			var parameterList = new[]
			{
				new SqlParameter("start_date", period.StartDateTime.Date),
				new SqlParameter("end_date", period.EndDateTime.Date),
				new SqlParameter("business_unit_code", businessUnit.Id)
			};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_request_load", parameterList,
											  _dataMartConnectionString);
		}

		public int FillFactRequestedDaysMart(DateTimePeriod period, IBusinessUnit businessUnit)
		{
			var parameterList = new[]
			{
				new SqlParameter("start_date", period.StartDateTime.Date),
				new SqlParameter("end_date", period.EndDateTime.Date),
				new SqlParameter("business_unit_code", businessUnit.Id)
			};

			return
				 HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_requested_days_load", parameterList,
														 _dataMartConnectionString);
		}

		public IList<TimeZonePeriod> GetBridgeTimeZoneLoadPeriod(TimeZoneInfo timeZone)
		{
			DataTable dataTable = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure,
															 "mart.etl_bridge_time_zone_get_load_period", null,
															 _dataMartConnectionString).Tables[0];
			var timeZonePeriodList = new List<TimeZonePeriod>();
			foreach (DataRow row in dataTable.Rows)
			{
				if (row["period_start_date"] == DBNull.Value && row["period_end_date"] == DBNull.Value)
					return null;

				var startDate = DateTime.SpecifyKind((DateTime)row["period_start_date"], DateTimeKind.Utc);
				var endDate = DateTime.SpecifyKind((DateTime)row["period_end_date"], DateTimeKind.Utc);
				timeZonePeriodList.Add(new TimeZonePeriod
				{
					TimeZoneCode = row["time_zone_code"].ToString(),
					PeriodToLoad = new DateTimePeriod(startDate, endDate.AddDays(1).AddMinutes(-1))
				});
			}

			return timeZonePeriodList;
		}

		public IList<IWorkload> LoadWorkload()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//Avoid lazy load
				BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName();

				WorkloadRepository repository = new WorkloadRepository(uow);
				IList<IWorkload> workloadsIncludedDeleted;

				// Handle deleted
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					workloadsIncludedDeleted = repository.LoadAll().ToList();
					foreach (IWorkload workload in workloadsIncludedDeleted)
					{
						if (!LazyLoadingManager.IsInitialized(workload.Skill))
							LazyLoadingManager.Initialize(workload.Skill);
						if (!LazyLoadingManager.IsInitialized(workload.Skill.SkillType))
							LazyLoadingManager.Initialize(workload.Skill.SkillType);
						if (!LazyLoadingManager.IsInitialized(workload.Skill.WorkloadCollection))
							LazyLoadingManager.Initialize(workload.Skill.WorkloadCollection);
						foreach (var workload1 in workload.Skill.WorkloadCollection)
						{
							if (!LazyLoadingManager.IsInitialized(workload1.QueueSourceCollection))
								LazyLoadingManager.Initialize(workload1.QueueSourceCollection);
						}

					}
				}

				return workloadsIncludedDeleted;
			}
		}

		public int PersistWorkload(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_workload_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_workload", _dataMartConnectionString);
		}
		public int PersistQueueWorkload(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_workload_queue_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_queue_workload", _dataMartConnectionString);
		}
		public int FillWorkloadDataMart(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_workload_load", null,
												_dataMartConnectionString);
		}

		public IDictionary<ISkill, IEnumerable<ISkillDay>> LoadSkillDays(DateTimePeriod period, IList<ISkill> skills, IScenario scenario, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					//Avoid lazy load error
					WorkloadRepository workloadRep = new WorkloadRepository(uow);
					var workloadList = workloadRep.LoadAll();
					Trace.WriteLine("Lazy load Workload list: " + workloadList.Count());

					SkillTypeRepository skillTypeRepository = new SkillTypeRepository(uow);
					var skillTypeList = skillTypeRepository.LoadAll();
					Trace.WriteLine("Lazy load Skill Type list: " + skillTypeList.Count());
				}

				uow.Reassociate(skills);
				uow.Reassociate(scenario);
				foreach (ISkill skill in skills)
				{
					if (!LazyLoadingManager.IsInitialized(skill.BusinessUnit))
						LazyLoadingManager.Initialize(skill.BusinessUnit);
				}

				ISkillDayRepository skillDayRepository = new SkillDayRepository(uow);
				IMultisiteDayRepository multisiteDayRepository = new MultisiteDayRepository(uow);
				IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays = new SkillDayLoadHelper(skillDayRepository, multisiteDayRepository, staffingCalculatorServiceFacade).LoadSchedulerSkillDays(period.ToDateOnlyPeriod(TimeZoneInfo.Utc),
																											skills,
																											scenario);
				foreach (var keyValuePair in skillDays)
				{
					foreach (ISkillDay skillDay in keyValuePair.Value)
					{
						if (!LazyLoadingManager.IsInitialized(skillDay.Skill))
							LazyLoadingManager.Initialize(skillDay.Skill);
					}
				}
				return skillDays;
			}
		}

		public IEnumerable<ISkillDay> LoadSkillDays(IScenario scenario, DateTime lastCheck, IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			IEnumerable<ISkillDay> skillDayList;
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				SkillDayRepository skillDayRep = new SkillDayRepository(uow);
				skillDayList = skillDayRep.FindUpdatedSince(scenario, lastCheck);

			}

			var result = new List<ISkillDay>();
			var groupedBySkill = skillDayList.GroupBy(s => s.Skill);
			foreach (var item in groupedBySkill)
			{
				var skillDays = LoadSkillDays(new DateOnlyPeriod(item.Min(s => s.CurrentDate), item.Max(s => s.CurrentDate)).ToDateTimePeriod(item.Key.TimeZone), new List<ISkill> { item.Key }, scenario, staffingCalculatorServiceFacade);
				result.AddRange(skillDays[item.Key]);
			}

			return result;
		}

		public int PersistForecastWorkload(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_forecast_workload_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_forecast_workload", _dataMartConnectionString);
		}
		public int FillForecastWorkloadDataMart(DateTimePeriod period, IBusinessUnit businessUnit)
		{
			//Prepare sql parameters
			var parameterList = new[]
			{
				new SqlParameter("start_date", period.StartDateTime.Date),
				new SqlParameter("end_date", period.EndDateTime.Date),
				new SqlParameter("business_unit_code", businessUnit.Id)
			};


			var result = HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_forecast_workload_load", parameterList,
				_dataMartConnectionString);
			return result;

		}

		public int FillSkillDataMart(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_skill_load", null,
												_dataMartConnectionString);
		}

		public int FillBusinessUnitDataMart(IBusinessUnit businessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };

			//note: This step is BU-less, so businessUnit.Id could be removed

			int rows = HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_business_unit_load", parameterList, _dataMartConnectionString);

			FillJobIntradaySettingsMart();

			return rows;
		}

		public void FillJobIntradaySettingsMart()
		{
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_job_intraday_settings_load", null, _dataMartConnectionString);
			HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_job_intraday_settings_load_deviation", null, _dataMartConnectionString);
		}

		public IList<IOptionalColumn> LoadOptionalColumnAvailableAsGroupPage()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var optionalColumnRepo = new OptionalColumnRepository(uow);
				return optionalColumnRepo.LoadAll()
					.Where(x => x.AvailableAsGroupPage)
					.ToList();

			}
		}

		public IEnumerable<ExternalStaff> GetExternalStaff(IEnumerable<ISkill> skills, DateTimePeriod period, ExternalStaffProvider externalStaffProvider)
		{
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				return externalStaffProvider.Fetch(skills, period);
			}
		}


		public int FillSiteDataMart(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_site_load", null,
												_dataMartConnectionString);
		}

		public int FillTeamDataMart(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_team_load", null,
												_dataMartConnectionString);
		}

		public int FillAcdLogOnDataMart(int dataSourceId)
		{
			//Prepare sql parameters
			var parameterList = new[] { new SqlParameter("datasource_id", dataSourceId) };

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_acd_login_load", parameterList,
											  _dataMartConnectionString);
		}

		public int FillScheduleDeviationDataMart(DateTimePeriod period, IBusinessUnit businessUnit, TimeZoneInfo defaultTimeZone, int isIntraday, DateTime? nowUtc)
		{
			//Convert time back to local time before sp call
			DateTime startDate = TimeZoneInfo.ConvertTimeFromUtc(period.StartDateTime, defaultTimeZone);
			DateTime endDate = TimeZoneInfo.ConvertTimeFromUtc(period.EndDateTime, defaultTimeZone);

			//Prepare sql parameters
			var parameterList = new[]
				{
					new SqlParameter("start_date", startDate.Date),
					new SqlParameter("end_date", endDate.Date),
					new SqlParameter("business_unit_code", businessUnit.Id),
					new SqlParameter("isIntraday", isIntraday),
					new SqlParameter("is_delayed_job", 0),
					new SqlParameter("now_utc", nowUtc)
				};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_schedule_deviation_load",
												parameterList,
												_dataMartConnectionString);
		}


		public int FillScheduleDeviationDataMart_Story79646(DateTimePeriod period, IBusinessUnit businessUnit, TimeZoneInfo defaultTimeZone, int isIntraday, DateTime? nowUtc)
		{
			//Convert time back to local time before sp call
			DateTime startDate = TimeZoneInfo.ConvertTimeFromUtc(period.StartDateTime, defaultTimeZone);
			DateTime endDate = TimeZoneInfo.ConvertTimeFromUtc(period.EndDateTime, defaultTimeZone);

			//Prepare sql parameters
			var parameterList = new[]
			{
				new SqlParameter("start_date", startDate.Date),
				new SqlParameter("end_date", endDate.Date),
				new SqlParameter("business_unit_code", businessUnit.Id),
				new SqlParameter("isIntraday", isIntraday),
				new SqlParameter("is_delayed_job", 0),
				new SqlParameter("now_utc", nowUtc)
			};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_schedule_deviation_load_79646",
					parameterList,
					_dataMartConnectionString);
		}

		public int FillFactAgentDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			//Convert time back to local time before sp call
			DateTime startDate = period.StartDateTimeLocal(defaultTimeZone);
			DateTime endDate = period.EndDateTimeLocal(defaultTimeZone);

			//Prepare sql parameters
			var parameterList = new[]
			{
				new SqlParameter("start_date", startDate.Date),
				new SqlParameter("end_date", endDate.Date),
				new SqlParameter("datasource_id", dataSourceId)
			};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_agent_load", parameterList,
											  _dataMartConnectionString);
		}

		public int FillFactAgentQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			//Convert time back to local time before sp call
			DateTime startDate = TimeZoneInfo.ConvertTimeFromUtc(period.StartDateTime, defaultTimeZone);
			DateTime endDate = TimeZoneInfo.ConvertTimeFromUtc(period.EndDateTime, defaultTimeZone);

			//Prepare sql parameters
			var parameterList = new[]
			{
				new SqlParameter("start_date", startDate.Date),
				new SqlParameter("end_date", endDate.Date),
				new SqlParameter("datasource_id", dataSourceId)
			};

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_agent_queue_load", parameterList,
											  _dataMartConnectionString);
		}

		public IList<IKeyPerformanceIndicator> LoadKpi()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//Avoid lazy load
				BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName();

				KpiRepository repository = new KpiRepository(uow);
				return repository.LoadAll().ToList();
			}
		}

		public int FillKpiDataMart(IBusinessUnit businessUnit)
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_kpi_load", null,
											  _dataMartConnectionString);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public int FillPermissionDataMart(IBusinessUnit businessUnit)
		{
			//Prepare sql parameters
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };

			//fill data and return effected rows
			var numberOfRows = HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_permission_report_load", parameterList,
				_dataMartConnectionString);

			return numberOfRows;
		}

		public IList<MatrixPermissionHolder> LoadReportPermissions()
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var personRepository = new PersonRepository(new ThisUnitOfWork(uow));
				new ApplicationRoleRepository(uow).LoadAll();
				var permissionsResolver = new MatrixPermissionsResolver(personRepository,
					new ApplicationFunctionsForRole(
						new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory()),
						new ApplicationFunctionRepository(new ThisUnitOfWork(uow))
						),
					new SiteRepository(uow)
					);

				var permissionHolders = permissionsResolver.ResolvePermission(DateOnly.Today, UnitOfWorkFactory.Current);

				foreach (MatrixPermissionHolder permissionHolder in permissionHolders)
				{
					if (!LazyLoadingManager.IsInitialized(permissionHolder.Team.Site.BusinessUnit))
						LazyLoadingManager.Initialize(permissionHolder.Team.Site.BusinessUnit);
				}

				return permissionHolders;
			}
		}

		public int FillBridgeAcdLogOnPerson(IBusinessUnit businessUnit)
		{
			//Prepare sql parameters
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_bridge_acd_login_person_load",
												parameterList,
												_dataMartConnectionString);
		}

		public int FillBridgeWorkloadQueue(IBusinessUnit businessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_bridge_queue_workload_load", parameterList,
							 _dataMartConnectionString);
		}

		public int PersistAgentSkill(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_agent_skill_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_agent_skill", _dataMartConnectionString);
		}

		public int PersistSkill(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_skill_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_skill", _dataMartConnectionString);
		}

		public int FillSkillSetDataMart(IBusinessUnit businessUnit)
		{
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_skillset_load", null,
													_dataMartConnectionString);

		}

		public int FillBridgeAgentSkillSetDataMart(IBusinessUnit businessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };

			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_bridge_skillset_skill_load", parameterList,
								_dataMartConnectionString);
		}

		public int FillFactAgentSkillDataMart(IBusinessUnit businessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };

			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_agent_skill_load", parameterList,
									  _dataMartConnectionString);
		}

		public DateTime GetMaxDateInDimDate(bool isInitial)
		{
			//Prepare sql parameters
			var parameterList = new[] { new SqlParameter("isInitial", isInitial) };
			return
				DateTime.SpecifyKind(
					(DateTime)
					HelperFunctions.ExecuteScalar(CommandType.StoredProcedure, "mart.etl_dim_date_get_max_date", parameterList,
												  _dataMartConnectionString), DateTimeKind.Unspecified);
		}

		public int PersistKpi(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_kpi_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_kpi", _dataMartConnectionString);
		}

		public IList<IScorecard> LoadScorecard()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//Avoid lazy load
				BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName();

				ScorecardRepository repository = new ScorecardRepository(uow);
				var ret = repository.LoadAll().ToList();
				foreach (IScorecard scorecard in ret)
				{
					if (!LazyLoadingManager.IsInitialized(scorecard.KeyPerformanceIndicatorCollection))
						LazyLoadingManager.Initialize(scorecard.KeyPerformanceIndicatorCollection);
				}
				return ret;
			}
		}

		public int PersistScorecard(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_scorecard_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_scorecard", _dataMartConnectionString);
		}

		public int FillScorecardDataMart()
		{
			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_dim_scorecard_load", null,
											  _dataMartConnectionString);
		}

		public int PersistScorecardKpi(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_scorecard_kpi_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_scorecard_kpi", _dataMartConnectionString);
		}

		public int FillScorecardKpiDataMart(IBusinessUnit businessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };
			return
					 HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_scorecard_kpi_load", parameterList,
											  _dataMartConnectionString);
		}

		public IList<IKpiTarget> LoadKpiTargetTeam()
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				//Avoid lazy load
				BusinessUnitRepository businessUnitRepository = new BusinessUnitRepository(uow);
				businessUnitRepository.LoadAllBusinessUnitSortedByName();

				KpiTargetRepository repository = new KpiTargetRepository(uow);
				var ret = repository.LoadAll().ToList();
				foreach (IKpiTarget target in ret)
				{
					if (!LazyLoadingManager.IsInitialized(target.KeyPerformanceIndicator))
						LazyLoadingManager.Initialize(target.KeyPerformanceIndicator);
					if (!LazyLoadingManager.IsInitialized(target.Team))
						LazyLoadingManager.Initialize(target.Team);
				}
				return ret;
			}
		}

		public int PersistKpiTargetTeam(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_kpi_targets_team_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_kpi_targets_team", _dataMartConnectionString);
		}

		public int FillKpiTargetTeamDataMart(IBusinessUnit businessUnit)
		{
			var parameterList = new[] { new SqlParameter("business_unit_code", businessUnit.Id) };

			return
				HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_kpi_targets_team_load", parameterList,
											  _dataMartConnectionString);
		}

		public IList<ISkill> LoadSkill(IList<IActivity> activities)
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				uow.Reassociate(activities);

				var repository = new SkillRepository(uow);
				IList<ISkill> ret;
				using (uow.DisableFilter(QueryFilter.Deleted))
				{
					new SkillTypeRepository(uow).LoadAll();

					ret = repository.LoadAll().ToList();
					foreach (ISkill skill in ret)
					{
						if (!LazyLoadingManager.IsInitialized(skill.Activity))
							LazyLoadingManager.Initialize(skill.Activity);
						if (!LazyLoadingManager.IsInitialized(skill.SkillType))
							LazyLoadingManager.Initialize(skill.SkillType);
						if (!LazyLoadingManager.IsInitialized(skill.BusinessUnit))
							LazyLoadingManager.Initialize(skill.BusinessUnit);
					}
				}
				return ret;
			}
		}

		public IList<ISkill> LoadSkillWithSkillDays(DateOnlyPeriod period)
		{
			using (IUnitOfWork uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{

				var repository = new SkillRepository(uow);
				return new List<ISkill>(repository.FindAllWithSkillDays(period));
			}
		}

		public int PersistScheduleForecastSkill(DataTable dataTable)
		{
			HelperFunctions.TruncateTable("stage.etl_stg_schedule_forecast_skill_delete", _dataMartConnectionString);
			return HelperFunctions.BulkInsert(dataTable, "stage.stg_schedule_forecast_skill", _dataMartConnectionString);
		}

		private static SqlParameter[] parameterList(int dataSourceId)
		{
			var parameterList = new[]
			{
				new SqlParameter("datasource_id", dataSourceId)
			};
			return parameterList;
		}

		public int FillIntradayFactQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			var parameters = parameterList(dataSourceId);

			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_queue_load_intraday", parameters,
				_dataMartConnectionString);
		}

		public int FillIntradayFactAgentDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			var parameters = parameterList(dataSourceId);

			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_agent_load_intraday", parameters,
				_dataMartConnectionString);
		}

		public int FillIntradayFactAgentQueueDataMart(DateTimePeriod period, int dataSourceId, TimeZoneInfo defaultTimeZone, IBusinessUnit businessUnit)
		{
			var parameters = parameterList(dataSourceId);

			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_fact_agent_queue_load_intraday", parameters,
				_dataMartConnectionString);
		}
		public int FillDimensionsNotDefined()
		{
			return HelperFunctions.ExecuteNonQuery(CommandType.StoredProcedure, "mart.etl_all_dimensions_not_defined_load", null,
													_dataMartConnectionString);

		}
		public int PerformIndexMaintenance(DatabaseEnum database)
		{
			_indexMaintenanceRepository.PerformIndexMaintenance(database);
			return 0;
		}

		public int GetValidToDateIdLocalForEternity()
		{
			return _analyticsPersonPeriodDateFixer.ValidToDateIdLocal(AnalyticsDate.Eternity.DateId);
		}

		public int GetValidToDateIdMaxDateForEternity()
		{
			return _analyticsPersonPeriodDateFixer.GetValidToDateIdMaxDate(AnalyticsDate.Eternity.DateDate, AnalyticsDate.Eternity.DateId);
		}

		public IEnumerable<WindowsLogonInfo> GetWindowsLogonInfos()
		{
			var dataTable = HelperFunctions.ExecuteDataSet(CommandType.StoredProcedure,
				"mart.etl_dim_person_get_allwindowslogons", null,
				_dataMartConnectionString).Tables[0];
			var result = new List<WindowsLogonInfo>();
			for (var i = 0; i < dataTable.Rows.Count; i++)
			{
				if (dataTable.Rows[i]["person_code"] != DBNull.Value)
				{
					result.Add(new WindowsLogonInfo
					{
						PersonCode = (Guid)dataTable.Rows[i]["person_code"],
						WindowsDomain = dataTable.Rows[i]["windows_domain"].ToString(),
						WindowsUsername = dataTable.Rows[i]["windows_username"].ToString(),

					});
				}
			}
			return result;
		}
	}
}
