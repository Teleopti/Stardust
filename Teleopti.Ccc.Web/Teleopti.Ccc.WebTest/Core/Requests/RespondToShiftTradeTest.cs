using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.WebTest.Core.Requests.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests
{
	[TestFixture]
	public class RespondToShiftTradeTest
	{
		[Test, Ignore("Don't know how to write with that eventsync thing")]
		public void OkByMeWhenCalledShouldLoadTheShiftTradeFromTheRepositoryAndAcceptItUsingPublisher()
		{
			//setup
			var shiftTradeId = Guid.NewGuid();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var loggedOnPerson = MockRepository.GenerateStub<IPerson>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var shiftTrade = MockRepository.GenerateMock<IShiftTradeRequest>();
			var shiftTradeRequestCheckSum = MockRepository.GenerateMock<IShiftTradeRequestSetChecksum>();
			var personRequestCheckAuthorization = MockRepository.GenerateMock<IPersonRequestCheckAuthorization>();
			var eventPublisher = MockRepository.GenerateMock<IEventPublisher>();
			var unitOfWorkFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateStub<IUnitOfWorkFactory>();
			uowFactory.Expect(x => x.Name).Return("gegga");
			unitOfWorkFactoryProvider.Expect(x => x.Current()).Return(uowFactory);
			var bsProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			bsProvider.Expect(x => x.Current()).Return(new BusinessUnit("sdf"));
			var dataSource = MockRepository.GenerateMock<ICurrentDataSource>();
			var currentBu = MockRepository.GenerateMock<ICurrentBusinessUnit>();
		    var eventSync = MockRepository.GenerateMock<IEventSyncronization>();
			var target = new RespondToShiftTrade(personRequestRepository, shiftTradeRequestCheckSum,
				personRequestCheckAuthorization, loggedOnUser, eventPublisher, MockRepository.GenerateMock<INow>(),
				dataSource, currentBu, eventSync, null, new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), loggedOnUser, new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));
			
			currentBu.Stub(x => x.Current()).Return(new BusinessUnit("new"));
			dataSource.Stub(x => x.CurrentName()).Return("name");
			personRequestRepository.Expect(p => p.Find(shiftTradeId)).Return(personRequest);
			personRequest.Expect(p => p.Request).Return(shiftTrade);
			loggedOnUser.Expect(l => l.CurrentUser()).Return(loggedOnPerson);
            eventSync.Stub(e => e.WhenDone(() => eventPublisher.Publish())).IgnoreArguments();
			//execute
			target.OkByMe(shiftTradeId, "");

			//verify expectation:
			eventPublisher.AssertWasCalled(s => s.Publish(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldDenyShiftTradeOnRequest()
		{
			var shiftTradeId = Guid.NewGuid();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var loggedOnPerson = new Person();
			var personRequest = new PersonRequest(loggedOnPerson,new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {new ShiftTradeSwapDetail(loggedOnPerson,loggedOnPerson,DateOnly.Today, DateOnly.Today)}));
			var personRequestCheckAuthorization = MockRepository.GenerateMock<IPersonRequestCheckAuthorization>();
			var eventSync = MockRepository.GenerateMock<IEventSyncronization>();
            var target = new RespondToShiftTrade(personRequestRepository, null, personRequestCheckAuthorization, loggedOnUser,
				null, null, MockRepository.GenerateMock<ICurrentDataSource>(),
				MockRepository.GenerateMock<ICurrentBusinessUnit>(), eventSync, null, new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), loggedOnUser, new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));

			personRequestRepository.Expect(p => p.Find(shiftTradeId)).Return(personRequest);
			loggedOnUser.Expect(l => l.CurrentUser()).Return(loggedOnPerson);
			
			target.Deny(shiftTradeId, "");

			personRequest.DenyReason.Should().Be.EqualTo("RequestDenyReasonOtherPart");
		}

		[Test]
		public void ApproveShouldReturnEmptyViewModelIfPersonrequestDoesntExist()
		{
			var id = Guid.Empty;
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			personRequestRepository.Expect(p => p.Find(id)).Return(null);
			var target = new RespondToShiftTrade(personRequestRepository, null, null, null, null, null,
				MockRepository.GenerateMock<ICurrentDataSource>(), MockRepository.GenerateMock<ICurrentBusinessUnit>(), MockRepository.GenerateMock<IEventSyncronization>(), null, new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(), new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));
			Assert.That(target.OkByMe(id, ""),Is.Not.Null);
		}

		[Test]
		public void DenyShouldReturnEmptyViewModelIfPersonrequestDoesntExist()
		{
			var id = Guid.Empty;
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			personRequestRepository.Expect(p => p.Find(id)).Return(null);
			var target = new RespondToShiftTrade(personRequestRepository, null, null, null, null, null,
				MockRepository.GenerateMock<ICurrentDataSource>(), MockRepository.GenerateMock<ICurrentBusinessUnit>(), MockRepository.GenerateMock<IEventSyncronization>(), null, new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(), new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));
			Assert.That(target.Deny(id, ""), Is.Not.Null);
		}
	}
}
