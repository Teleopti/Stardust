using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{

	public static class TestDataSetup
	{
		private static DatabaseHelper.Backup _Ccc7DataBackup;

		public static void CreateDataSource()
		{
			TestData.DataSource = DataSourceHelper.CreateDataSource(new[] { new EventsMessageSender(new SyncEventsPublisher(new EventPublisher(new HardCodedResolver(), new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))))) }, "TestData");
		}

		public static void SetupFakeState()
		{
			TestData.PersonThatCreatesTestData = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", TestData.CommonPassword);
			CommonBusinessUnit.BusinessUnitFromFakeState = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			CommonBusinessUnit.BusinessUnitFromFakeState.Name = "BusinessUnit";

			StateHolderProxyHelper.SetupFakeState(TestData.DataSource, TestData.PersonThatCreatesTestData, CommonBusinessUnit.BusinessUnitFromFakeState, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));

			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;
			GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();
		}


		public static void CreateMinimumTestData()
		{
			DataSourceHelper.PersistAuditSetting();// TODO: Remove, its done in DataSourceHelper.CreateDataSource();
			GlobalUnitOfWorkState.UnitOfWorkAction(CreatePersonThatCreatesTestData);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateLicense);
		}

		public static void CreateLegacyTestData()
		{
			CreateAndPersistTestData();
		}

		public static void ClearCcc7Data()
		{
			DataSourceHelper.ClearCcc7Data();
		}

		public static void ClearAnalyticsData()
		{
			DataSourceHelper.ClearAnalyticsData();
		}

		public static void BackupCcc7Data()
		{
			_Ccc7DataBackup = DataSourceHelper.BackupCcc7DataByFileCopy("Teleopti.Ccc.WebBehaviorTest");
		}

		public static void RestoreCcc7Data()
		{
			DataSourceHelper.RestoreCcc7DataByFileCopy(_Ccc7DataBackup);
		}







		private static void CreateAndPersistTestData()
		{
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateAllRaptorApplicationFunctions);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateMatrixApplicationFunctions);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateApplicationRoles);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateShiftCategory);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateDayOffTemplate);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateAbsence);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateWorkflowControlSet);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateAgentPersons);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateGroupingActivity);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateActivities);
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
			var names = new[] { "ResReportAbandonmentAndSpeedOfAnswer", "ResReportForecastvsActualWorkload", "ResReportServiceLevelAndAgentsReady", "ResReportRequestsPerAgent" };

			applicationFunctionRepository.AddRange(
				names.Select(n => new ApplicationFunction(n, matrixReportsParent) { ForeignSource = DefinedForeignSourceNames.SourceMatrix }));
		}

		private static void CreateApplicationRoles(IUnitOfWork uow)
		{
			var applicationFunctionRepository = new ApplicationFunctionRepository(uow);
			var allApplicationFunctions = applicationFunctionRepository.LoadAll().AsEnumerable();

			var agentRoleApplicationFunctions =
				from r in allApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.All &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewConfidential &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.MobileReports &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.Anywhere &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages
				select r;
			var agentNoReportsRoleApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.ForeignSource != DefinedForeignSourceNames.SourceMatrix &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.MyReportWeb
				select r;
			var agentRoleWithoutStudentAvailabilityApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.StudentAvailability
				select r;
			var agentRoleWithoutPreferencesApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.StandardPreferences
				select r;
			var agentRoleWithoutExtendedPreferencesApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ExtendedPreferencesWeb
				select r;
			var agentRoleWithoutRequestsApplicationFunctions =
				from r in agentRoleApplicationFunctions
				where
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.TextRequests &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb
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
			var agentRoleWithoutAgentRoleWithoutResReportScheduledAndActualAgentsMatrixFunction =
				from r in supervisorRoleApplicationFunctions
				where
					!(r.FunctionCode == "ResReportServiceLevelAndAgentsReady" && r.ForeignSource == DefinedForeignSourceNames.SourceMatrix)
				select r;


			var secondBusinessUnit = GlobalDataMaker.Data().Data<SecondBusinessUnit>().BusinessUnit;
			var anotherSite = GlobalDataMaker.Data().Data<AnotherSite>().Site;

			var shippedRoles = ApplicationRoleFactory.CreateShippedRoles(out TestData.AdministratorRole, out TestData.AgentRole, out TestData.UnitRole, out TestData.SiteRole, out TestData.TeamRole);
			shippedRoles.ForEach(r => r.Name += "Shipped");
			var shippedRolesWithFunctions = from role in shippedRoles
			                                let functions = (role == TestData.AgentRole ? agentRoleApplicationFunctions : allApplicationFunctions)
											let availableDataRangeOption = (role == TestData.AgentRole ? AvailableDataRangeOption.MyTeam : 
																			(role == TestData.AdministratorRole ? AvailableDataRangeOption.MyBusinessUnit : AvailableDataRangeOption.None)
																			)
											let availableData = new AvailableData{AvailableDataRange = availableDataRangeOption}
											let businessUnit = TestData.BusinessUnit
											select new { role, functions, businessUnit, availableData };
			TestData.AgentRoleSecondBusinessUnit = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "SecondBusinessUnit", null);
			TestData.AgentRoleSecondBusinessUnit.SetBusinessUnit(secondBusinessUnit);
			TestData.AgentRoleWithoutStudentAvailability = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoStudentAvailability", null);
			TestData.AgentRoleWithoutPreferences = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoPreferences", null);
			TestData.AgentRoleWithoutExtendedPreferences = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoExtendedPreferences", null);
			TestData.AgentRoleWithoutRequests = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoRequests", null);
			TestData.AgentRoleWithoutAbsenceRequests = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoAbsenceRequests", null);
			TestData.AgentRoleWithoutTeamSchedule = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoTeamSchedule", null);
			TestData.AgentRoleOnlyWithOwnData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "OnlyWithOwnData", null);
			TestData.AgentRoleWithSiteData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "WithSiteData", null);
			TestData.AgentRoleWithAnotherSiteData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "WithAnotherSiteData", null);
			TestData.AgentRoleWithoutMyTimeWeb = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoMyTimeWeb", null);
			TestData.AgentRoleWithoutResReportScheduledAndActualAgents = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoServiceLevelAndAgentsReady", null);
			TestData.AgentRoleWithoutAnyReport = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "WithoutAnyReport", null);
			TestData.AdministratorRoleWithEveryoneData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AdministratorRole + "WithEveryoneData", null);
			TestData.SupervisorRole = ApplicationRoleFactory.CreateRole("SupervisorRole", null);
			TestData.SupervisorRoleSecondBusinessUnit = ApplicationRoleFactory.CreateRole("SupervisorRole", null);
			TestData.SupervisorRoleSecondBusinessUnit.SetBusinessUnit(secondBusinessUnit);

			var availableDataAnotherSite = new AvailableData();
			availableDataAnotherSite.AddAvailableSite(anotherSite);

			var customTestRoles = new[]
			                 	{
			                 		new { role = TestData.AgentRoleSecondBusinessUnit, functions = agentRoleApplicationFunctions, businessUnit = secondBusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}}, 
									new { role = TestData.AgentRoleWithoutStudentAvailability, functions = agentRoleWithoutStudentAvailabilityApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutPreferences, functions = agentRoleWithoutPreferencesApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutExtendedPreferences, functions = agentRoleWithoutExtendedPreferencesApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutRequests, functions = agentRoleWithoutRequestsApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutAbsenceRequests, functions = agentRoleWithoutAbsenceRequestsApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutTeamSchedule, functions = agentRoleWithoutTeamScheduleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutMyTimeWeb, functions = agentRoleWithoutMyTimeWebApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutResReportScheduledAndActualAgents, functions = agentRoleWithoutAgentRoleWithoutResReportScheduledAndActualAgentsMatrixFunction, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleOnlyWithOwnData, functions = agentRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyOwn}},
									new { role = TestData.AgentRoleWithSiteData, functions = agentRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MySite}},
									new { role = TestData.AgentRoleWithAnotherSiteData, functions = agentRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = availableDataAnotherSite},
									new { role = TestData.AgentRoleWithoutAnyReport, functions = agentNoReportsRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MySite}},
									new { role = TestData.AdministratorRoleWithEveryoneData, functions = agentRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.Everyone}},
									new { role = TestData.SupervisorRole, functions = supervisorRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.SupervisorRoleSecondBusinessUnit, functions = supervisorRoleApplicationFunctions, businessUnit = secondBusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
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
            var personRepository = new PersonRepository(uow);
			personRepository.Add(TestData.PersonThatCreatesTestData);
		}

		private static void CreateActivities(IUnitOfWork unitOfWork)
		{
			TestData.ActivityPhone = new Activity("Legacy activity Phone"){DisplayColor = Color.FromKnownColor(KnownColor.Green)};
			TestData.ActivityShortBreak = new Activity("Legacy activity ShortBreak"){DisplayColor = Color.FromKnownColor(KnownColor.Red)};
			TestData.ActivityLunch = new Activity("Legacy activity Lunch"){DisplayColor = Color.FromKnownColor(KnownColor.Yellow)};
			TestData.ActivityTraining = new Activity("Legacy activity Training"){DisplayColor =  Color.FromKnownColor(KnownColor.Purple)};

			TestData.ActivityPhone.GroupingActivity = TestData.GroupingActivity;
			TestData.ActivityShortBreak.GroupingActivity = TestData.GroupingActivity;
			TestData.ActivityLunch.GroupingActivity = TestData.GroupingActivity;
			TestData.ActivityTraining.GroupingActivity = TestData.GroupingActivity;

			var activityRepository = new ActivityRepository(unitOfWork);
			activityRepository.Add(TestData.ActivityPhone);
			activityRepository.Add(TestData.ActivityShortBreak);
			activityRepository.Add(TestData.ActivityLunch);
			activityRepository.Add(TestData.ActivityTraining);
		}

		private static void CreateGroupingActivity(IUnitOfWork unitOfWork)
		{
			TestData.GroupingActivity = GroupingActivityFactory.CreateSimpleGroupingActivity("ActivityGroup");

			var groupingActivityRepository = new GroupingActivityRepository(unitOfWork);
			groupingActivityRepository.Add(TestData.GroupingActivity);
		}

		private static void CreateShiftCategory(IUnitOfWork unitOfWork)
		{
			TestData.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Legacy common shift category", "Purple");

			var shiftCategoryRepository = new ShiftCategoryRepository(unitOfWork);
			shiftCategoryRepository.Add(TestData.ShiftCategory);
		}

		private static void CreateDayOffTemplate(IUnitOfWork unitOfWork)
		{
			TestData.DayOffTemplate = DayOffFactory.CreateDayOff(new Description("Legacy common day off", "LCDO"));

			var dayOffRepository = new DayOffTemplateRepository(unitOfWork);
			dayOffRepository.Add(TestData.DayOffTemplate);
		}

		private static void CreateAbsence(IUnitOfWork unitOfWork)
		{
			TestData.Absence = AbsenceFactory.CreateAbsence("Legacy common absence", "LCA", Color.FromArgb(210, 150, 150));
			TestData.ConfidentialAbsence = AbsenceFactory.CreateAbsence("Legacy common confidential absence");
			TestData.ConfidentialAbsence.Confidential = true;
			TestData.ConfidentialAbsence.DisplayColor = Color.GreenYellow;
			TestData.AbsenceInContractTime = AbsenceFactory.CreateAbsence("Legacy common vacation absence", "LCA2", Color.FromArgb(200, 150, 150));
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
			                            SchedulePublishedToDate = DateOnlyForBehaviorTests.TestToday.AddDays(100)
			                        };

			TestData.WorkflowControlSetPublishedUntilWednesday = new WorkflowControlSet("Published until wednesday")
			                        {
			                            SchedulePublishedToDate = findWednesdayOfCurrentWeek(),
										// why do these need to be set for not published to work!?
										PreferencePeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1),
										StudentAvailabilityPeriod = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 1)
			                        };

			TestData.WorkflowControlSetStudentAvailabilityOpen = new WorkflowControlSet("Published 100 days, SA open")
			                         	{
			                         		SchedulePublishedToDate = DateOnlyForBehaviorTests.TestToday.AddDays(100),
											StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday, DateOnlyForBehaviorTests.TestToday.AddDays(1)),
											StudentAvailabilityPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday.AddDays(-100), DateOnlyForBehaviorTests.TestToday.AddDays(100))
			                         	};

			TestData.WorkflowControlSetStudentAvailabilityClosed = new WorkflowControlSet("Published 100 days, SA closed")
			                    {
			                        SchedulePublishedToDate = DateOnlyForBehaviorTests.TestToday.AddDays(100),
									StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday.AddDays(-1), DateOnlyForBehaviorTests.TestToday.AddDays(-1)),
									StudentAvailabilityPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday.AddDays(-100), DateOnlyForBehaviorTests.TestToday.AddDays(100))
								};

			TestData.WorkflowControlSetPreferenceOpen = new WorkflowControlSet("Published 100 days, Preference open")
			{
				SchedulePublishedToDate = DateOnlyForBehaviorTests.TestToday.AddDays(100),
				PreferenceInputPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday, DateOnlyForBehaviorTests.TestToday.AddDays(1)),
				PreferencePeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday.AddDays(-100), DateOnlyForBehaviorTests.TestToday.AddDays(100))
			};

			TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences = new WorkflowControlSet("Published 100 days, Pref. open, Allow std.pref.")
			{
				SchedulePublishedToDate = DateOnlyForBehaviorTests.TestToday.AddDays(100),
				PreferenceInputPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday, DateOnlyForBehaviorTests.TestToday.AddDays(1)),
				PreferencePeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday.AddDays(-100), DateOnlyForBehaviorTests.TestToday.AddDays(100))
			};
			TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences.AddAllowedPreferenceShiftCategory(TestData.ShiftCategory);
			TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences.AddAllowedPreferenceDayOff(TestData.DayOffTemplate);
			TestData.WorkflowControlSetPreferenceOpenWithAllowedPreferences.AddAllowedPreferenceAbsence(TestData.Absence);

			TestData.WorkflowControlSetStudentAvailabilityOpenNextMonth = new WorkflowControlSet("Published 100 days, SA open next month")
			        {
			            SchedulePublishedToDate = DateOnlyForBehaviorTests.TestToday.AddDays(100),
			            StudentAvailabilityInputPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday, DateOnlyForBehaviorTests.TestToday.AddDays(1)),
			            StudentAvailabilityPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday.AddDays(30), DateOnlyForBehaviorTests.TestToday.AddDays(60))
			        };

			TestData.WorkflowControlSetExisting = new WorkflowControlSet("Simple existing wcs");

			TestData.WorkflowControlSetPreferencesOpenNextMonth = new WorkflowControlSet("Published 100 days, Preference open next month")
			{
				SchedulePublishedToDate = DateOnlyForBehaviorTests.TestToday.AddDays(100),
				PreferenceInputPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday, DateOnlyForBehaviorTests.TestToday.AddDays(1)),
				PreferencePeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday.AddDays(30), DateOnlyForBehaviorTests.TestToday.AddDays(60))
			};

			TestData.WorkflowControlSetPreferenceClosed = new WorkflowControlSet("Published 100 days, preference period closed")
			{
				SchedulePublishedToDate = DateOnlyForBehaviorTests.TestToday.AddDays(100),
				PreferenceInputPeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday.AddDays(-2), DateOnlyForBehaviorTests.TestToday.AddDays(-2)),
				PreferencePeriod = new DateOnlyPeriod(DateOnlyForBehaviorTests.TestToday.AddDays(-100), DateOnlyForBehaviorTests.TestToday.AddDays(100))
			};

			var workflowControlSetRepository = new WorkflowControlSetRepository(unitOfWork);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetPublished);
			workflowControlSetRepository.Add(TestData.WorkflowControlSetPublishedUntilWednesday);
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
			var currentDay = DateHelper.GetFirstDateInWeek(DateOnlyForBehaviorTests.TestToday.Date, CultureInfo.CurrentCulture);
			while (true)
			{
				if (currentDay.DayOfWeek == DayOfWeek.Wednesday) break;

				currentDay = currentDay.AddDays(1);
			}
			return currentDay;
		}



		public static DateTime FirstDayOfCurrentWeek(CultureInfo culture)
		{
			return DateHelper.GetFirstDateInWeek(DateOnlyForBehaviorTests.TestToday.Date, culture);
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
            return ThirdDayOfNextWeek(culture).Month != DateOnlyForBehaviorTests.TestToday.Date.Month ? ThirdDayOfPreviousWeek(culture) : ThirdDayOfNextWeek(culture);
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
            return FirstDayOfCurrentWeek(culture).Month == DateOnlyForBehaviorTests.TestToday.Date.Month ? FirstDayOfCurrentWeek(culture) : FirstDayOfNextWeek(culture);
		}

	}
}