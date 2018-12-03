using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;


namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	[DomainTest] 
	[WebTest] 
	[RequestsTest] 
	public class ShiftTradeRequestPersonToPermissionValidatorTest : IIsolateSystem
	{
		public FakePersonRepository PersonRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public IShiftTradeRequestPersonToPermissionValidator Target;

		private static DateTime baseDate = new DateTime(2016, 01, 01);
		
		[Test]
		public void ShouldNotBeSatisfiedIfRecipientHasNoPermissionForShiftTrade()
		{
			var loggedOnUser = PersonRepository.Has();
			var person = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = person.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = false }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			
			person.PermissionInformation.AddApplicationRole(applicationRole);
			
			var result = validateRequestWithPermission(loggedOnUser, person, Target);
			Assert.AreEqual(false, result);
		}

		[Test]
		public void ShouldNotBeSatisfiedIfRecipientHasNoPermissionForBothViewSchedulesAndViewUnpublishedSchedules()
		{
			var loggedOnUser = PersonRepository.Has();
			var person = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = person.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = false }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));
			
			person.PermissionInformation.AddApplicationRole(applicationRole);

			var result = validateRequestWithPermission(loggedOnUser, person, Target);
			Assert.AreEqual(false, result);
		}

		[Test]
		public void ShouldBeSatisfiedIfRecipientHasPermissionForShiftTradeAndViewSchedules()
		{
			var loggedOnUser = PersonRepository.Has();
			var person = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = person.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = false }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));

			person.PermissionInformation.AddApplicationRole(applicationRole);

			var result = validateRequestWithPermission(loggedOnUser, person, Target);
			Assert.AreEqual(true, result);
		}

		[Test]
		public void ShouldBeSatisfiedIfRecipientHasPermissionForShiftTradeAndViewUnpublishedSchedules()
		{
			var loggedOnUser = PersonRepository.Has();
			var person = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = person.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = false }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));

			person.PermissionInformation.AddApplicationRole(applicationRole);

			var result = validateRequestWithPermission(loggedOnUser, person, Target);
			Assert.AreEqual(true, result);
		}
		
		private static bool validateRequestWithPermission(IPerson personFrom, IPerson personTo, IShiftTradeRequestPersonToPermissionValidator validator)
		{
			var requestDateOnly = new DateOnly(baseDate.AddDays(1));
			var shiftTradeSwapDetails = new IShiftTradeSwapDetail[]
			{
				new ShiftTradeSwapDetail(personFrom, personTo, requestDateOnly, requestDateOnly)
			};
			var request = new ShiftTradeRequest(shiftTradeSwapDetails);
			
			var result = validator.IsSatisfied(request);
			return result;
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLicensedFunctionProvider>().For<ILicensedFunctionsProvider>();

			var currentBusinessUnit = new SpecificBusinessUnit(BusinessUnitFactory.BusinessUnitUsedInTest);
			isolate.UseTestDouble(currentBusinessUnit).For<ICurrentBusinessUnit>();
			var dataSource = new FakeCurrentDatasource("Test");
			isolate.UseTestDouble(dataSource).For<ICurrentDataSource>();
		}
	}
}