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
			
			//
			IApplicationRole administratorRole, unitRole, siteRole, teamRole, agentRole;
			//
			var shippedRoles = ApplicationRoleFactory.CreateShippedRoles(out administratorRole, out agentRole, out unitRole, out siteRole, out teamRole);
			shippedRoles.ForEach(r => r.Name += "Shipped");
			var shippedRolesWithFunctions = from role in shippedRoles
											let functions = (role == agentRole ? agentRoleApplicationFunctions : allApplicationFunctions)
											let availableDataRangeOption = (role == agentRole ? AvailableDataRangeOption.MyTeam :
																			(role == administratorRole ? AvailableDataRangeOption.MyBusinessUnit : AvailableDataRangeOption.None)
																			)
											let availableData = new AvailableData{AvailableDataRange = availableDataRangeOption}
											let businessUnit = TestData.BusinessUnit
											select new { role, functions, businessUnit, availableData };


			var applicationRoleRepository = new ApplicationRoleRepository(uow);
			var availableDataRepository = new AvailableDataRepository(uow);
			shippedRolesWithFunctions.ToList().ForEach(r =>
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

	}
}