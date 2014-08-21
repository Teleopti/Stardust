using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public static class TestDataSetup
	{
		private static DatabaseHelper.Backup _Ccc7DataBackup;
		private static IDataSource datasource;
		private static IPerson personThatCreatesTestData;

		public static void CreateDataSource()
		{
			datasource = DataSourceHelper.CreateDataSource(new[] { new EventsMessageSender(new SyncEventsPublisher(new EventPublisher(new HardCodedResolver(), new EventContextPopulator(new CurrentIdentity(), new CurrentInitiatorIdentifier(CurrentUnitOfWork.Make()))))) }, "TestData");
		}

		public static void SetupFakeState()
		{
			personThatCreatesTestData = PersonFactory.CreatePersonWithBasicPermissionInfo("UserThatCreatesTestData", TestData.CommonPassword);
			CommonBusinessUnit.BusinessUnitFromFakeState = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
			CommonBusinessUnit.BusinessUnitFromFakeState.Name = "BusinessUnit";

			StateHolderProxyHelper.SetupFakeState(datasource, personThatCreatesTestData, CommonBusinessUnit.BusinessUnitFromFakeState, new ThreadPrincipalContext(new TeleoptiPrincipalFactory()));

			GlobalPrincipalState.Principal = Thread.CurrentPrincipal as TeleoptiPrincipal;
			GlobalUnitOfWorkState.CurrentUnitOfWorkFactory = UnitOfWorkFactory.CurrentUnitOfWorkFactory();
		}

		public static void CreateMinimumTestData()
		{
			GlobalUnitOfWorkState.UnitOfWorkAction(CreatePersonThatCreatesTestData);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateLicense);
		}

		public static void CreateLegacyTestData()
		{
			CreateAndPersistTestData();
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
			Navigation.GoToWaitForUrlAssert("Test/ClearConnections", "Test/ClearConnections", new ApplicationStartupTimeout());
			DataSourceHelper.RestoreCcc7DataByFileCopy(_Ccc7DataBackup);
		}

		private static void CreateAndPersistTestData()
		{
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateAllRaptorApplicationFunctions);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateMatrixApplicationFunctions);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateApplicationRoles);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateShiftCategory);
			GlobalUnitOfWorkState.UnitOfWorkAction(CreateAbsence);
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
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.Anywhere &&
					r.FunctionPath != DefinedRaptorApplicationFunctionPaths.ViewAllGroupPages
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

			var anotherSite = GlobalDataMaker.Data().Data<AnotherSite>().Site;

			//
			IApplicationRole administratorRole, unitRole, siteRole, teamRole;
			//
			var shippedRoles = ApplicationRoleFactory.CreateShippedRoles(out administratorRole, out TestData.AgentRole, out unitRole, out siteRole, out teamRole);
			shippedRoles.ForEach(r => r.Name += "Shipped");
			var shippedRolesWithFunctions = from role in shippedRoles
			                                let functions = (role == TestData.AgentRole ? agentRoleApplicationFunctions : allApplicationFunctions)
											let availableDataRangeOption = (role == TestData.AgentRole ? AvailableDataRangeOption.MyTeam :
																			(role == administratorRole ? AvailableDataRangeOption.MyBusinessUnit : AvailableDataRangeOption.None)
																			)
											let availableData = new AvailableData{AvailableDataRange = availableDataRangeOption}
											let businessUnit = TestData.BusinessUnit
											select new { role, functions, businessUnit, availableData };

			TestData.AgentRoleWithoutStudentAvailability = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoStudentAvailability", null);
			TestData.AgentRoleWithoutPreferences = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoPreferences", null);
			TestData.AgentRoleWithoutExtendedPreferences = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoExtendedPreferences", null);
			TestData.AgentRoleWithoutRequests = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoRequests", null);
			TestData.AgentRoleWithoutAbsenceRequests = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "NoAbsenceRequests", null);
			TestData.AgentRoleOnlyWithOwnData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "OnlyWithOwnData", null);
			TestData.AgentRoleWithSiteData = ApplicationRoleFactory.CreateRole(ShippedApplicationRoleNames.AgentRole + "WithSiteData", null);

			var availableDataAnotherSite = new AvailableData();
			availableDataAnotherSite.AddAvailableSite(anotherSite);

			var customTestRoles = new[]
			                 	{
									new { role = TestData.AgentRoleWithoutStudentAvailability, functions = agentRoleWithoutStudentAvailabilityApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutPreferences, functions = agentRoleWithoutPreferencesApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutExtendedPreferences, functions = agentRoleWithoutExtendedPreferencesApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutRequests, functions = agentRoleWithoutRequestsApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleWithoutAbsenceRequests, functions = agentRoleWithoutAbsenceRequestsApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyTeam}},
									new { role = TestData.AgentRoleOnlyWithOwnData, functions = agentRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MyOwn}},
									new { role = TestData.AgentRoleWithSiteData, functions = agentRoleApplicationFunctions, businessUnit = TestData.BusinessUnit, availableData = new AvailableData{AvailableDataRange = AvailableDataRangeOption.MySite}},
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
			personRepository.Add(personThatCreatesTestData);
		}

		private static void CreateShiftCategory(IUnitOfWork unitOfWork)
		{
			TestData.ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("Legacy common shift category", "Purple");

			var shiftCategoryRepository = new ShiftCategoryRepository(unitOfWork);
			shiftCategoryRepository.Add(TestData.ShiftCategory);
		}

		private static void CreateAbsence(IUnitOfWork unitOfWork)
		{
			TestData.ConfidentialAbsence = AbsenceFactory.CreateAbsence("Legacy common confidential absence");
			TestData.ConfidentialAbsence.Confidential = true;
			TestData.ConfidentialAbsence.DisplayColor = Color.GreenYellow;
			TestData.AbsenceInContractTime = AbsenceFactory.CreateAbsence("Legacy common vacation absence", "LCA2", Color.FromArgb(200, 150, 150));
			TestData.AbsenceInContractTime.InContractTime = true;

			var absenceRepository = new AbsenceRepository(unitOfWork);
			
			absenceRepository.Add(TestData.ConfidentialAbsence);
			absenceRepository.Add(TestData.AbsenceInContractTime);
		}
	}
}