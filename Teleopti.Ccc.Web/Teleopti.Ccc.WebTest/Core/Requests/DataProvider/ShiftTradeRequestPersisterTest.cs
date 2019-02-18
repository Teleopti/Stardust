using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.WebTest.Areas.Requests.Core.IOC;


namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[DomainTest] 
	[WebTest] 
	[RequestsTest] 
	public class ShiftTradeRequestPersisterTest : IIsolateSystem
	{
		public IShiftTradeRequestPersister Target;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public IScheduleDayProvider ScheduleDayProvider;
		public ICurrentScenario CurrentScenario;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		
		[Test]
		public void ShouldPersistMappedData()
		{
			LoggedOnUser.SetFakeLoggedOnUser(PersonRepository.Has());
			var person = PersonRepository.Has();
			var form = new ShiftTradeRequestForm{Dates = new []{new DateOnly(2001,1,1)},Message = "test", PersonToId = person.Id.GetValueOrDefault(),Subject = "test"};
			
			Target.Persist(form);

			PersonRequestRepository.LoadAll().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPersistMappedDataWhenOfferCompleted()
		{
			var loggedOnUser = PersonRepository.Has();
			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = new WorkflowControlSet("bla") {LockTrading = true}.WithId();

			var person = PersonRepository.Has();
			
			var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			offer.Stub(x => x.Status).Return(ShiftExchangeOfferStatus.Completed);

			var personRequest = new PersonRequest(person) {Request = offer}.WithId();
			PersonRequestRepository.Add(personRequest);

			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = Guid.Empty};
			
			var result = Target.Persist(form);

			result.ExchangeOffer.IsOfferAvailable.Should().Be.False();
			PersonRequestRepository.FindAllRequestsForAgent(person).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPersistMappedDataWhenOfferPendingForAdminApproval()
		{
			var loggedOnUser = PersonRepository.Has();
			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = true }.WithId();

			var person = PersonRepository.Has();

			var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			offer.Stub(x => x.Status).Return(ShiftExchangeOfferStatus.PendingAdminApproval);

			var personRequest = new PersonRequest(person) { Request = offer }.WithId();
			PersonRequestRepository.Add(personRequest);
			
			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = Guid.Empty};
			
			var result = Target.Persist(form);

			result.ExchangeOffer.IsOfferAvailable.Should().Be.False();
			PersonRequestRepository.FindAllRequestsForAgent(person).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldAutoApprovedByAnnouncerWhenLockShiftTradeFromBulletinBoard()
		{
			var person = PersonRepository.Has();
			var loggedOnUser = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = true }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData {AvailableDataRange = AvailableDataRangeOption.Everyone};
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions,DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));
			
			loggedOnUser.PermissionInformation.AddApplicationRole(applicationRole);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			
			var offer = new ShiftExchangeOffer(ScheduleDayProvider.GetScheduleDay(new DateOnly(2001,1,1),person),new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);
			
			var personRequest = new PersonRequest(person) {Request = offer};
			PersonRequestRepository.Add(personRequest);

			var form = new ShiftTradeRequestForm { ShiftExchangeOfferId = Guid.Empty, Dates = new []{new DateOnly(2001,1,1)},PersonToId = person.Id.GetValueOrDefault()};
			
			var result = Target.Persist(form);

			result.ExchangeOffer.IsOfferAvailable.Should().Be.True();
			result.Status.Should().Be.EqualTo(Resources.WaitingThreeDots);
		}

		[Test]
		public void ShouldAutoDeniedIfRecipientHasNoPermissionForShiftTrade()
		{
			var person = PersonRepository.Has();
			var loggedOnUser = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = true }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));

			var applicationRole2 = new ApplicationRole();
			applicationRole2.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole2.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole2.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			
			loggedOnUser.PermissionInformation.AddApplicationRole(applicationRole);
			person.PermissionInformation.AddApplicationRole(applicationRole2);

			var offer = new ShiftExchangeOffer(ScheduleDayProvider.GetScheduleDay(new DateOnly(2001, 1, 1), person), new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);

			var personRequest = new PersonRequest(person) { Request = offer };
			PersonRequestRepository.Add(personRequest);

			var form = new ShiftTradeRequestForm { ShiftExchangeOfferId = Guid.Empty, Dates = new[] { new DateOnly(2001, 1, 1) }, PersonToId = person.Id.GetValueOrDefault() };
			var result = Target.Persist(form);

			result.Status.Should().Be.EqualTo(Resources.Denied);
			result.DenyReason.Should().Be.EqualTo(Resources.RecipientHasNoShiftTradePermission);
		}

		[Test]
		public void ShouldSetPendingAdminApprovalWhenLockShiftTradeFromBulletinBoard()
		{
			var person = PersonRepository.Has();
			var loggedOnUser = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = person.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = true, AutoGrantShiftTradeRequest = false }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));

			loggedOnUser.PermissionInformation.AddApplicationRole(applicationRole);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			
			var offer = new ShiftExchangeOffer(ScheduleDayProvider.GetScheduleDay(DateOnly.Today.AddDays(1), person), new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);
			var personRequest = new PersonRequest(person) {Request = offer};
			PersonRequestRepository.Add(personRequest);

			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = Guid.Empty, Dates = new[] { DateOnly.Today.AddDays(1) }, PersonToId = person.Id.GetValueOrDefault() };
			
			var result = Target.Persist(form);

			offer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.PendingAdminApproval);
			result.ExchangeOffer.IsOfferAvailable.Should().Be.True();
			result.Status.Should().Be.EqualTo(Resources.WaitingThreeDots);
		}

		[Test]
		public void ShouldWaitingApprovedByAnnouncerWhenShiftTradeFromBulletinBoard()
		{
			var person = PersonRepository.Has();
			var loggedOnUser = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = person.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = false }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));

			loggedOnUser.PermissionInformation.AddApplicationRole(applicationRole);
			person.PermissionInformation.AddApplicationRole(applicationRole);

			var offer = new ShiftExchangeOffer(ScheduleDayProvider.GetScheduleDay(new DateOnly(2017, 1, 1), person), new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);
			var personRequest = new PersonRequest(person) { Request = offer };
			PersonRequestRepository.Add(personRequest);

			var form = new ShiftTradeRequestForm { ShiftExchangeOfferId = Guid.Empty, Dates = new[] { new DateOnly(2017, 1, 1) }, PersonToId = person.Id.GetValueOrDefault() };
			var result = Target.Persist(form);

			result.Status.Should().Be.EqualTo(Resources.New);
		}

		[Test]
		public void ShouldWaitingApprovedByAnnouncerWhenShiftTradeFromNormalRequestBoard()
		{
			var person = PersonRepository.Has();
			var loggedOnUser = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = person.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = false }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));

			loggedOnUser.PermissionInformation.AddApplicationRole(applicationRole);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			
			var form = new ShiftTradeRequestForm { ShiftExchangeOfferId = null, Dates = new[] { new DateOnly(2017, 1, 1) }, PersonToId = person.Id.GetValueOrDefault() };
			var result = Target.Persist(form);

			result.Status.Should().Be.EqualTo(Resources.New);
		}

		[Test]
		public void ShouldSetChecksumOnRequest()
		{
			var person = PersonRepository.Has();
			var loggedOnUser = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = person.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = false }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));

			loggedOnUser.PermissionInformation.AddApplicationRole(applicationRole);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			PersonAssignmentRepository.Has(person, CurrentScenario.Current(), ActivityFactory.CreateActivity("Phone"),
				ShiftCategoryFactory.CreateShiftCategory(), new DateOnly(2007, 1, 1), new TimePeriod(8, 17));

			var form = new ShiftTradeRequestForm { ShiftExchangeOfferId = null, Dates = new[] { new DateOnly(2017, 1, 1) }, PersonToId = person.Id.GetValueOrDefault() };
			Target.Persist(form);

			((IShiftTradeRequest) PersonRequestRepository.LoadAll().Last().Request).ShiftTradeSwapDetails.First().ChecksumTo
				.Should().Be.EqualTo(-1);
		}

		[Test]
		public void ShouldSetChecksumOnRequestFromOffer()
		{
			var person = PersonRepository.Has();
			var loggedOnUser = PersonRepository.Has();

			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			loggedOnUser.WorkflowControlSet = person.WorkflowControlSet = new WorkflowControlSet("bla") { LockTrading = false }.WithId();
			var applicationRole = new ApplicationRole();
			var functions = new DefinedRaptorApplicationFunctionFactory();
			applicationRole.AvailableData = new AvailableData { AvailableDataRange = AvailableDataRangeOption.Everyone };
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules));
			applicationRole.AddApplicationFunction(ApplicationFunction.FindByPath(functions.ApplicationFunctions, DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb));

			loggedOnUser.PermissionInformation.AddApplicationRole(applicationRole);
			person.PermissionInformation.AddApplicationRole(applicationRole);
			PersonAssignmentRepository.Has(person, CurrentScenario.Current(), ActivityFactory.CreateActivity("Phone"),
				ShiftCategoryFactory.CreateShiftCategory(), new DateOnly(2007, 1, 1), new TimePeriod(8, 17));
			
			var offer = new ShiftExchangeOffer(ScheduleDayProvider.GetScheduleDay(new DateOnly(2007, 1, 1), person), new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);
			var personRequest = new PersonRequest(person) { Request = offer };
			PersonRequestRepository.Add(personRequest);
			
			var form = new ShiftTradeRequestForm { ShiftExchangeOfferId = Guid.Empty, Dates = new[] { new DateOnly(2017, 1, 1) }, PersonToId = person.Id.GetValueOrDefault() };
			Target.Persist(form);

			((IShiftTradeRequest)PersonRequestRepository.LoadAll().Last().Request).ShiftTradeSwapDetails.First().ChecksumTo
				.Should().Not.Be.EqualTo(-1);
		}

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLinkProvider>().For<ILinkProvider>();
			isolate.UseTestDouble<FakePersonalSettingDataRepository>().For<IPersonalSettingDataRepository>();
			isolate.UseTestDouble<FakeLicensedFunctionProvider>().For<ILicensedFunctionsProvider>();

			var currentBusinessUnit = new SpecificBusinessUnit(BusinessUnitUsedInTests.BusinessUnit);
			isolate.UseTestDouble(currentBusinessUnit).For<ICurrentBusinessUnit>();
			var dataSource = new FakeCurrentDatasource("Test");
			isolate.UseTestDouble(dataSource).For<ICurrentDataSource>();
		}
	}
}