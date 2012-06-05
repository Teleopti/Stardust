using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using log4net;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestDataSetup
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(TestDataSetup));

		private static NHibernateUnitOfWorkFactory _unitOfWorkFactory;
		private static TeleoptiPrincipal Principal;

		public static void Setup()
		{
			var testDataSetupStartTime = DateTime.Now;

			var createDataSourceStartTime = DateTime.Now;
			TestData.DataSource = DataSourceHelper.CreateDataSource();
			// no longer required since CreateDataSource above will drop and recreate the whole database
			//DataSourceHelper.CleanDatabase();
			Log.Write("Create data source took " + DateTime.Now.Subtract(createDataSourceStartTime));

			TestData.PersonThatCreatesTestData = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", TestData.CommonPassword);
			TestData.BusinessUnit = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			TestData.BusinessUnit.Name = "BusinessUnit";

			var createFakeStateStartTime = DateTime.Now;
			StateHolderProxyHelper.SetupFakeState(TestData.DataSource, TestData.PersonThatCreatesTestData, TestData.BusinessUnit);
			Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;
			Log.Write("Create fake state took " + DateTime.Now.Subtract(createFakeStateStartTime));

			_unitOfWorkFactory = UnitOfWorkFactory.Current as NHibernateUnitOfWorkFactory;

			var createAndPersistTestDataStartTime = DateTime.Now;
			CreateAndPersistTestData();
			Log.Write("Create and persist test data took " + DateTime.Now.Subtract(createAndPersistTestDataStartTime));

			Log.Write("Test data setup took totally " + DateTime.Now.Subtract(testDataSetupStartTime));
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void EnsureThreadPrincipal()
		{
			if (Thread.CurrentPrincipal.GetType() == typeof(GenericPrincipal))
				Thread.CurrentPrincipal = Principal;
		}

		public static void UnitOfWorkAction(Action<IUnitOfWork> action)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				action.Invoke(unitOfWork);
				unitOfWork.PersistAll();
			}
		}

		private static void CreateAndPersistTestData()
		{
			DataSourceHelper.PersistAuditSetting();
			UnitOfWorkAction(CreatePersonThatCreatesTestData);
			UnitOfWorkAction(CreateLicense);
			UnitOfWorkAction(CreateBusinessUnits);
			UnitOfWorkAction(CreateAllRaptorApplicationFunctions);
			UnitOfWorkAction(CreateMatrixApplicationFunctions);
			UnitOfWorkAction(CreateSites);
			UnitOfWorkAction(CreateApplicationRoles);
			UnitOfWorkAction(CreateShiftCategory);
			UnitOfWorkAction(CreateDayOffTemplate);
			UnitOfWorkAction(CreateAbsence);
			UnitOfWorkAction(CreateWorkflowControlSet);
			UnitOfWorkAction(CreateTeams);
			UnitOfWorkAction(CreateContractStuff);
			UnitOfWorkAction(CreateAgentPersons);
			UnitOfWorkAction(CreateGroupingActivity);
			UnitOfWorkAction(CreateActivities);
			UnitOfWorkAction(CreateScenario);
		}

	    private static void CreateLicense(IUnitOfWork uow)
		{
			var license = new License {XmlString = File.ReadAllText("License.xml")};
			var licenseRepository = new LicenseRepository(uow);
			licenseRepository.Add(license);
		}

		private static void CreateAllRaptorApplicationFunctions(IUnitOfWork uow)
		{
			var applicationFunctionRepository = new ApplicationFunctionRepository(uow);
			var definedRaptorApplicationFunctionFactory = new DefinedRaptorApplicationFunctionFactory();

			applicationFunctionRepository.AddRange(definedRaptorApplicationFunctionFactory.ApplicationFunctionList);
		}

		private static void CreateMatrixApplicationFunctions(IUnitOfWork uow)
		{
			var applicationFunctionRepository = new ApplicationFunctionRepository(uow);
			var matrixReportsParent = applicationFunctionRepository.LoadAll().First(x => x.FunctionCode == "Reports");
			var names = new[] {"ResReportAbandonmentAndSpeedOfAnswer","ResReportForecastvsActualWorkload", "ResReportServiceLevelAndAgentsReady"};

			applicationFunctionRepository.AddRange(
				names.Select(n => new ApplicationFunction(n, matrixReportsParent) { ForeignSource = DefinedForeignSourceNames.SourceMatrix }));
		}

		private static void CreateApplicationRoles(IUnitOfWork uow)
		{
			var applicationFunctionRepository = new ApplicationFunctionRepository(uow);
			var allApplicationFunctions = applicationFunctionRepository.GetAllApplicationFunctionSortedByCode().AsEnumerable();

			var agentRoleApplicationFunctions =
				from r in allApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.All &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewConfidential &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.Anywhere
				select r;
			var agentRoleWithoutStudentAvailabilityApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.StudentAvailability
				select r;
			var agentRoleWithoutPreferencesApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.StandardPreferences
				select r;
			var agentRoleWithoutRequestsApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.TextRequests
				select r;
			var agentRoleWithoutTextRequestsApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.TextRequests
				select r;
			var agentRoleWithoutAbsenceRequestsApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb
				select r;
			var agentRoleWithoutTeamScheduleApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.TeamSchedule
				select r;
			var supervisorRoleApplicationFunctions =
				from r in allApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.All &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewConfidential
				select r;

			var agentRoleWithoutMyTimeWebApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.MyTimeWeb
				select r;
			var agentRoleWithoutMobileReportsApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.Anywhere
				select r;
			var agentRoleWithoutAgentRoleWithoutResReportServiceLevelAndAgentsReadyMatrixFunction =
				from r in supervisorRoleApplicationFunctions
				where
					!(r.FunctionCode == "ResReportServiceLevelAndAgentsReady" && r.ForeignSource == DefinedForeignSourceNames.SourceMatrix)
				select r;
			
			

			var shippedRoles = ApplicationRoleFactory.CreateShippedRoles(out TestData.AdministratorRole, out TestData.AgentRole, out TestData.UnitRole, out TestData.SiteRole, out TestData.TeamRole);
			var shippedRolesWithFunctions = from role in shippedRoles
			                                let functions = (role == TestData.AgentRole ? agentRoleApplicationFunctions : allApplicationFunctions)
											let availableDataRangeOption = (role == TestData.AgentRole ? AvailableDataRangeOption.MyTeam : 
																			(role == TestData.AdministratorRole ? AvailableDataRangeOption.MyBusinessUnit : AvailableDataRangeOption.None)
																			)
											let availableData = new AvailableData{AvailableDataRange = availableDataRangeOption}
											let businessUnit = TestData.BusinessUnit
											select new { role, functions, businessUnit, availableData };
			TestData.AgentRoleSecondBusinessUnit = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "SecondBusinessUnit", null);
			TestData.AgentRoleSecondBusinessUnit.SetBusinessUnit(TestData.SecondBusinessUnit);
			TestData.AgentRoleWithoutStudentAvailability = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoStudentAvailability", null);
			TestData.AgentRoleWithoutPreferences = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoPreferences", null);
			TestData.AgentRoleWithoutRequests = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoRequests", null);
			TestData.AgentRoleWithoutTextRequests = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoTextRequests", null);
			TestData.AgentRoleWithoutAbsenceRequests = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoAbsenceRequests", null);
			TestData.AgentRoleWithoutTeamSchedule = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoTeamSchedule", null);
			TestData.AgentRoleOnlyWithOwnData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "OnlyWithOwnData", null);
			TestData.AgentRoleWithSiteData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "WithSiteData", null);
			TestData.AgentRoleWithAnotherSiteData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "WithAnotherSiteData", null);
			TestData.AgentRoleWithoutMobileReports = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoMobileReports", null);
			TestData.AgentRoleWithoutMyTimeWeb = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoMyTimeWeb", null);
			TestData.AgentRoleWithoutResReportServiceLevelAndAgentsReady = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoServiceLevelAndAgentsReady", null);
			TestData.AdministratorRoleWithEveryoneData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AdministratorRole + "WithEveryoneData", null);
			TestData.SupervisorRole = ApplicationRoleFactory.CreateRole("SupervisorRole", null);

			var test = new AvailableData();
			test.AddAvailableSite(TestData.AnotherSite);

			var customTestRoles = new[]
			                 	{
			                 		new { role = TestData.AgentRoleSecondBusinessUnit, functions = agentRoleApplicationFunctions, businessUnit = TestData.SecondBusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}}, 
									new { role = TestData.AgentRoleWithoutStudentAvailability, functions = agentRoleWithoutStudentAvailabilityApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutPreferences, functions = agentRoleWithoutPreferencesApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutRequests, functions = agentRoleWithoutRequestsApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutTextRequests, functions = agentRoleWithoutTextRequestsApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutAbsenceRequests, functions = agentRoleWithoutAbsenceRequestsApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutTeamSchedule, functions = agentRoleWithoutTeamScheduleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutMyTimeWeb, functions = agentRoleWithoutMyTimeWebApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutMobileReports, functions = agentRoleWithoutMobileReportsApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutResReportServiceLevelAndAgentsReady, functions = agentRoleWithoutAgentRoleWithoutResReportServiceLevelAndAgentsReadyMatrixFunction, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleOnlyWithOwnData, functions = agentRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyOwn}},
									new { role = TestData.AgentRoleWithSiteData, functions = agentRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MySite}},
									new { role = TestData.AgentRoleWithAnotherSiteData, functions = agentRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = test},
									new { role = TestData.AdministratorRoleWithEveryoneData, functions = allApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.Everyone}},
									new { role = TestData.SupervisorRole, functions = supervisorRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
			                 	};

			var allRoles = customTestRoles.Union(shippedRolesWithFunctions);

			var applicationRoleRepository = new ApplicationRoleRepository(uow);
			var availableDataRepository = new AvailableDataRepository(uow);
			allRoles.ToList().ForEach(r =>
			                          	{
			                          		r.role.AvailableData = r.availableData;
			                          		r.availableData.ApplicationRole = r.role;
											r.role.SetBusinessUnit(r.businessUnit);
											r.functions.ToList().ForEach(r.role.AddApplicationFunction);
			                       			applicationRoleRepository.Add(r.role);
											availableDataRepository.Add(r.availableData);
			                       		});
		}

		private static void CreatePersonThatCreatesTestData(IUnitOfWork uow)
		{
			var personRepository = new PersonRepository(uow);
			personRepository.Add(TestData.PersonThatCreatesTestData);
		}

		private static void CreateAgentPersons(IUnitOfWork uow)
		{
			UserTestData.PersonWindowsUser = PersonFactory.CreatePersonWithWindowsPermissionInfo(Environment.UserName, Environment.UserDomainName);
			UserTestData.PersonWindowsUser.PermissionInformation.AddApplicationRole(TestData.AgentRole);
			UserTestData.PersonWindowsUser.PermissionInformation.AddApplicationRole(TestData.AgentRoleSecondBusinessUnit);
			UserTestData.PersonWindowsUser.PermissionInformation.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));
			UserTestData.PersonWindowsUser.PermissionInformation.SetUICulture(CultureInfo.GetCultureInfo("sv-SE"));

			UserTestData.PersonApplicationUser = PersonFactory.CreatePersonWithBasicPermissionInfo(UserTestData.PersonApplicationUserName, TestData.CommonPassword);
			UserTestData.PersonApplicationUser.PermissionInformation.AddApplicationRole(TestData.AgentRole);
			UserTestData.PersonApplicationUser.PermissionInformation.AddApplicationRole(TestData.AgentRoleSecondBusinessUnit);

			UserTestData.PersonApplicationUserSingleBusinessUnit = PersonFactory.CreatePersonWithBasicPermissionInfo(UserTestData.PersonApplicationUserSingleBusinessUnitUserName, TestData.CommonPassword);
			UserTestData.PersonApplicationUserSingleBusinessUnit.PermissionInformation.AddApplicationRole(TestData.AgentRole);
			UserTestData.PersonApplicationUserSingleBusinessUnit.WorkflowControlSet = TestData.WorkflowControlSetPublished;

			UserTestData.PersonWithNoPermission = new Person();
		    UserTestData.PersonWithNoPermission.ApplicationAuthenticationInfo = new ApplicationAuthenticationInfo
		                                                                            {
		                                                                                ApplicationLogOnName =
		                                                                                    UserTestData.
		                                                                                    PersonWithNoPermissionUserName,
		                                                                                Password = TestData.CommonPassword
		                                                                            };
            var personRepository = new PersonRepository(uow);
			personRepository.Add(TestData.PersonThatCreatesTestData);
			personRepository.Add(UserTestData.PersonWindowsUser);
			personRepository.Add(UserTestData.PersonApplicationUser);
			personRepository.Add(UserTestData.PersonApplicationUserSingleBusinessUnit);
		}

		private static void CreateBusinessUnits(IUnitOfWork uow)
		{
			TestData.SecondBusinessUnit = BusinessUnitFactory.CreateSimpleBusinessUnit("SecondBusinessUnit");

			var businessUnitRepository = new BusinessUnitRepository(uow);
			businessUnitRepository.Add(TestData.BusinessUnit);
			businessUnitRepository.Add(TestData.SecondBusinessUnit);
		}

		private static void CreateSites(IUnitOfWork uow)
		{
			TestData.CommonSite = SiteFactory.CreateSimpleSite("Common Site");
			TestData.AnotherSite = SiteFactory.CreateSimpleSite("Another Site");
			TestData.BusinessUnit.AddSite(TestData.CommonSite);
			TestData.BusinessUnit.AddSite(TestData.AnotherSite);

			var siteRepository = new SiteRepository(uow);
			siteRepository.Add(TestData.CommonSite);
			siteRepository.Add(TestData.AnotherSite);
		}

		private static void CreateTeams(IUnitOfWork uow)
		{
			TestData.CommonTeam = TeamFactory.CreateSimpleTeam("Common Team");
			TestData.CommonSite.AddTeam(TestData.CommonTeam);

			var teamRepository = new TeamRepository(uow);
			teamRepository.Add(TestData.CommonTeam);
		}

		private static void CreateContractStuff(IUnitOfWork uow)
		{
			TestData.PartTimePercentageOne = PartTimePercentageFactory.CreatePartTimePercentage("PartTimePercentage One");

			TestData.DayOffTodayContractSchedule = new ContractSchedule("Day off today schedule");
			TestData.DayOffTodayContractSchedule.AddContractScheduleWeek(new ContractScheduleWeek());

			var repository = new Repository(uow);
			repository.Add(TestData.DayOffTodayContractSchedule);
			repository.Add(TestData.PartTimePercentageOne);
		}

		private static void CreateActivities(IUnitOfWork unitOfWork)
		{
			TestData.ActivityPhone = ActivityFactory.CreateActivity("Phone", Color.FromKnownColor(KnownColor.Green));
			TestData.ActivityLunch = ActivityFactory.CreateActivity("Lunch", Color.FromKnownColor(KnownColor.Yellow));
			TestData.ActivityTraining = ActivityFactory.CreateActivity("Training", Color.FromKnownColor(KnownColor.Purple));

			TestData.ActivityPhone.GroupingActivity = TestData.GroupingActivity;
			TestData.ActivityLunch.GroupingActivity = TestData.GroupingActivity;
			TestData.ActivityTraining.GroupingActivity = TestData.GroupingActivity;

			var activityRepository = new ActivityRepository(unitOfWork);
			activityRepository.Add(TestData.ActivityPhone);
			activityRepository.Add(TestData.ActivityLunch);
			activityRepository.Add(TestData.ActivityTraining);
		}

		private static void CreateGroupingActivity(IUnitOfWork unitOfWork)
		{
			TestData.GroupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity("ActivityGroup");

			var groupingActivityRepository = new GroupingActivityRepository(unitOfWork);
			groupingActivityRepository.Add(TestData.GroupingActivity);
		}

		private static void CreateScenario(IUnitOfWork unitOfWork)
		{
			TestData.Scenario = ScenarioFactory.CreateScenarioAggregate("Default", true, false);
			TestData.SecondScenario = ScenarioFactory.CreateScenarioAggregate("Default", true, false);

			TestData.SecondScenario.SetBusinessUnit(TestData.SecondBusinessUnit);

			var scenarioRepository = new ScenarioRepository(unitOfWork);
			scenarioRepository.Add(TestData.Scenario);
			scenarioRepository.Add(TestData.SecondScenario);
		}

		private static void CreateShiftCategory(IUnitOfWork unitOfWork)
		{
			TestData.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Late", "Purple");

			var shiftCategoryRepository = new ShiftCategoryRepository(unitOfWork);
			shiftCategoryRepository.Add(TestData.ShiftCategory);
		}

		private static void CreateDayOffTemplate(IUnitOfWork unitOfWork)
		{
			TestData.DayOffTemplate = DayOffFactory.CreateDayOff(new Description("Day off", "DO"));

			var dayOffRepository = new DayOffRepository(unitOfWork);
			dayOffRepository.Add(TestData.DayOffTemplate);
		}

		private static void CreateAbsence(IUnitOfWork unitOfWork)
		{
			TestData.Absence = AbsenceFactory.CreateAbsence("Illness", "IL", new Color());
			TestData.ConfidentialAbsence = AbsenceFactory.CreateAbsence("Confidential");
			TestData.ConfidentialAbsence.Confidential = true;
			TestData.ConfidentialAbsence.DisplayColor = Color.GreenYellow;
			TestData.AbsenceInContractTime = AbsenceFactory.CreateAbsence("Vacation", "VA", new Color());
			TestData.AbsenceInContractTime.InContractTime = true;

			var absenceRepository = new AbsenceRepository(unitOfWork);
			absenceRepository.Add(TestData.Absence);
			absenceRepository.Add(TestData.ConfidentialAbsence);
			absenceRepository.Add(TestData.AbsenceInContractTime);
		}

		private static void CreateWorkflowControlSet(IUnitOfWork unitOfWork)
		{
			TestData.WorkflowControlSetPublished = new WorkflowControlSet("Published 100 days")
			                        {
			                            SchedulePublishedToDate = DateOnly.Today.AddDays(100)
			                        };

			TestData.WorkflowControlSetPublishedUntilWednesday = new WorkflowControlSet("Published until wednesday")
			                        {
			                            SchedulePublishedToDate = findWednesdayOfCurrentWeek(),
										// why do these need to be set for not published to work!?
										PreferencePeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1),
										StudentAvailabilityPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1)
			                        };

			TestData.WorkflowControlSetNotPublished = new WorkflowControlSet("Not published")
			                        {
			                            SchedulePublishedToDate = DateOnly.Today.AddDays(-100),
										// why do these need to be set for not published to work!?
										PreferencePeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1),
										StudentAvailabilityPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1)
			                        };

			TestData.WorkflowControlSetStudentAvailabilityOpen = new WorkflowControlSet("Published 100 days, SA open")
			                         	{
			                         		SchedulePublishedToDate = DateOnly.Today.AddDays(100),
											StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
											StudentAvailabilityPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-100), DateOnly.Today.AddDays(100))
			                         	};

			TestData.WorkflowControlSetStudentAvailabilityClosed = new WorkflowControlSet("Published 100 days, SA closed")
			                    {
			                        SchedulePublishedToDate = DateOnly.Today.AddDays(100),
									StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-1), DateOnly.Today.AddDays(-1)),
									StudentAvailabilityPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-100), DateOnly.Today.AddDays(100))
								};

			TestData.WorkflowControlSetPreferenceOpen = new WorkflowControlSet("Published 100 days, Preference open")
			{
				SchedulePublishedToDate = DateOnly.Today.AddDays(100),
				PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
				PreferencePeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-100), DateOnly.Today.AddDays(100))
			};

			TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences = new WorkflowControlSet("Published 100 days, Pref. open, Allow std.pref.")
			{
				SchedulePublishedToDate = DateOnly.Today.AddDays(100),
				PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
				PreferencePeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-100), DateOnly.Today.AddDays(100))
			};
			TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences.AddAllowedPreferenceShiftCategory(TestData.ShiftCategory);
			TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences.AddAllowedPreferenceDayOff(TestData.DayOffTemplate);
			TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences.AddAllowedPreferenceAbsence(TestData.Absence);

			TestData.WorkflowControlSetStudentAvailabilityOpenNextMonth = new WorkflowControlSet("Published 100 days, SA open next month")
			        {
			            SchedulePublishedToDate = DateOnly.Today.AddDays(100),
			            StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
			            StudentAvailabilityPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(30), DateOnly.Today.AddDays(60))
			        };

			TestData.WorkflowControlSetExisting = new WorkflowControlSet("Simple existing wcs");

			TestData.WorkflowControlSetPreferencesOpenNextMonth = new WorkflowControlSet("Published 100 days, Preference open next month")
			{
				SchedulePublishedToDate = DateOnly.Today.AddDays(100),
				PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today),
				PreferencePeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(30), DateOnly.Today.AddDays(60))
			};

			TestData.WorkflowControlSetPreferenceClosed = new WorkflowControlSet("Published 100 days, preference period closed")
			{
				SchedulePublishedToDate = DateOnly.Today.AddDays(100),
				PreferenceInputPeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-1), DateOnly.Today.AddDays(-1)),
				PreferencePeriod = new DateOnlyPeriod(DateOnly.Today.AddDays(-100), DateOnly.Today.AddDays(100))
			};

			var workflowControlSetRepository = new WorkflowControlSetRepository(unitOfWork);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetPublished);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetPublishedUntilWednesday);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetNotPublished);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetStudentAvailabilityOpen);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetStudentAvailabilityOpenNextMonth);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetStudentAvailabilityClosed);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetPreferenceOpen);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetExisting);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetPreferencesOpenNextMonth);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetPreferenceClosed);
		}




		private static DateTime findWednesdayOfCurrentWeek()
		{
			var currentDay = DateHelper.GetFirstDateInWeek(DateOnly.Today.Date, CultureInfo.CurrentCulture);
			while (true)
			{
				if (currentDay.DayOfWeek == DayOfWeek.Wednesday) break;

				currentDay = currentDay.AddDays(1);
			}
			return currentDay;
		}



		public static DateTime FirstDayOfCurrentWeek(CultureInfo culture)
		{
			return DateHelper.GetFirstDateInWeek(DateTime.Now.Date, culture);
		}

		public static DateTime LastDayOfCurrentWeek(CultureInfo culture)
		{
			return FirstDayOfCurrentWeek(culture).AddDays(6);
		}
		
		public static DateTime ThirdDayOfPreviousWeek(CultureInfo culture)
		{
			return FirstDayOfCurrentWeek(culture).AddDays(-7).AddDays(2);
		}

		public static DateTime ThirdDayOfNextWeek(CultureInfo culture)
		{
			return LastDayOfCurrentWeek(culture).AddDays(1).AddDays(2);
		}

		public static DateTime ThirdDayOfOtherThanCurrentWeekInCurrentMonth(CultureInfo culture)
		{
			return ThirdDayOfNextWeek(culture).Month != DateTime.Now.Month ? ThirdDayOfPreviousWeek(culture) : ThirdDayOfNextWeek(culture);
		}

		public static DateTime FirstDayOfPreviousWeek(CultureInfo culture)
		{
			return FirstDayOfCurrentWeek(culture).AddDays(-7);
		}

		public static DateTime FirstDayOfNextWeek(CultureInfo culture)
		{
			return LastDayOfCurrentWeek(culture).AddDays(1);
		}

		public static DateTime FirstDayOfAnyWeekInCurrentMonth(CultureInfo culture)
		{
			return FirstDayOfCurrentWeek(culture).Month == DateTime.Now.Month ? FirstDayOfCurrentWeek(culture) : FirstDayOfNextWeek(culture);
		}

	}

	public static class Extensions
	{

		public static void SetBusinessUnit(this IBelongsToBusinessUnit aggregateRootWithBusinessUnit, IBusinessUnit businessUnit)
		{
			var type = typeof(AggregateRootWithBusinessUnit);
			var privateField = type.GetField("_businessUnit", BindingFlags.NonPublic | BindingFlags.Instance);
			privateField.SetValue(aggregateRootWithBusinessUnit, businessUnit);
		}

	}
}