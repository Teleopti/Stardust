using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class TextRequestPersisterTest
	{
		[Test]
		public void ShouldAddTextRequest()
		{ 
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var timeZone = new FakeUserTimeZone();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new TextRequestPersister(personRequestRepository,
				new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new TextRequestFormMapper(loggedOnUser, timeZone,
					new DateTimePeriodFormMapper(timeZone)));
			var form = new TextRequestForm();
			
			target.Persist(form);

			personRequestRepository.AssertWasCalled(x => x.Add(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldDelete()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var timeZone = new FakeUserTimeZone();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new TextRequestPersister(personRequestRepository, new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new TextRequestFormMapper(loggedOnUser, timeZone,
					new DateTimePeriodFormMapper(timeZone)));
			var personRequest = new PersonRequest(new Person());
			personRequest.SetId(Guid.NewGuid());

			personRequestRepository.Stub(x => x.Find(personRequest.Id.Value)).Return(personRequest);

			target.Delete(personRequest.Id.Value);

			personRequestRepository.AssertWasCalled(x => x.Remove(personRequest));
		}		
		
		[Test]
		public void ShouldSetExchangeOfferStatusWhenDelete()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var timeZone = new FakeUserTimeZone();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new TextRequestPersister(personRequestRepository, new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new TextRequestFormMapper(loggedOnUser, timeZone,
					new DateTimePeriodFormMapper(timeZone)));
			var personRequest = new PersonRequest(new Person());
			var shiftTradeRequest = MockRepository.GenerateMock<IShiftTradeRequest>();
			var currentShift = ScheduleDayFactory.Create(DateOnly.Today.AddDays(1));
			var offer = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.PendingAdminApproval);
			shiftTradeRequest.Stub(x => x.Offer).Return(offer);
			personRequest.SetId(Guid.NewGuid());
			personRequest.Request = shiftTradeRequest;

			personRequestRepository.Stub(x => x.Find(personRequest.Id.Value)).Return(personRequest);

			target.Delete(personRequest.Id.Value);

			offer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Pending);
			personRequestRepository.AssertWasCalled(x => x.Remove(personRequest));
		}		
		
		[Test]
		public void ShouldNotSetCompletedExchangeOfferStatusWhenDelete()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var timeZone = new FakeUserTimeZone();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new TextRequestPersister(personRequestRepository, new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new TextRequestFormMapper(loggedOnUser, timeZone,
					new DateTimePeriodFormMapper(timeZone)));
			var personRequest = new PersonRequest(new Person());
			var shiftTradeRequest = MockRepository.GenerateMock<IShiftTradeRequest>();
			var currentShift = ScheduleDayFactory.Create(DateOnly.Today.AddDays(1));
			var offer = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Completed);
			shiftTradeRequest.Stub(x => x.Offer).Return(offer);
			personRequest.SetId(Guid.NewGuid());
			personRequest.Request = shiftTradeRequest;

			personRequestRepository.Stub(x => x.Find(personRequest.Id.Value)).Return(personRequest);

			target.Delete(personRequest.Id.Value);

			offer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Completed);
			personRequestRepository.AssertWasCalled(x => x.Remove(personRequest));
		}		
		
		[Test]
		public void ShouldNotSetInvalidExchangeOfferStatusWhenDelete()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var timeZone = new FakeUserTimeZone();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new TextRequestPersister(personRequestRepository, new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new TextRequestFormMapper(loggedOnUser, timeZone,
					new DateTimePeriodFormMapper(timeZone)));
			var personRequest = new PersonRequest(new Person());
			var shiftTradeRequest = MockRepository.GenerateMock<IShiftTradeRequest>();
			var currentShift = ScheduleDayFactory.Create(DateOnly.Today.AddDays(1));
			var offer = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Invalid);
			shiftTradeRequest.Stub(x => x.Offer).Return(offer);
			personRequest.SetId(Guid.NewGuid());
			personRequest.Request = shiftTradeRequest;

			personRequestRepository.Stub(x => x.Find(personRequest.Id.Value)).Return(personRequest);

			target.Delete(personRequest.Id.Value);

			offer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.Invalid);
			personRequestRepository.AssertWasCalled(x => x.Remove(personRequest));
		}

		[Test]
		public void ShouldThrowHttp404OIfTextRequestDoesNotExists()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var timeZone = new FakeUserTimeZone();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new TextRequestPersister(personRequestRepository, new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new TextRequestFormMapper(loggedOnUser, timeZone,
					new DateTimePeriodFormMapper(timeZone)));
			var id = Guid.NewGuid();

			personRequestRepository.Stub(x => x.Find(id)).Return(null);

			var exception = Assert.Throws<RequestPersistException>(() => target.Delete(id));
			exception.GetHttpCode().Should().Be(404);
		}

		[Test]
		public void ShouldThrowHttp404OIfRequestDeniedOrApproved()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var timeZone = new FakeUserTimeZone();
			var loggedOnUser = new FakeLoggedOnUser();
			var target = new TextRequestPersister(personRequestRepository, new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new TextRequestFormMapper(loggedOnUser, timeZone,
					new DateTimePeriodFormMapper(timeZone)));
			var personRequest = new PersonRequest(new Person());
			personRequest.SetId(Guid.NewGuid());

			personRequestRepository.Stub(x => x.Find(personRequest.Id.Value)).Return(personRequest);

			personRequestRepository.Stub(x => x.Remove(personRequest)).Throw(new DataSourceException());

			var exception = Assert.Throws<RequestPersistException>(() => target.Delete(personRequest.Id.Value));
			exception.GetHttpCode().Should().Be(404);
		}

		[Test]
		public void ShouldUpdateExistingTextRequest()
		{
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var timeZone = new FakeUserTimeZone();
			var loggedOnUser = new FakeLoggedOnUser();
			var personRequest = new PersonRequest(loggedOnUser.CurrentUser(),
				new TextRequest(new DateTimePeriod(2017, 1, 1, 2017, 1, 1)));
			var target = new TextRequestPersister(personRequestRepository, new RequestsViewModelMapper(timeZone, new FakeLinkProvider(), loggedOnUser,
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()),
				new TextRequestFormMapper(loggedOnUser, timeZone,
					new DateTimePeriodFormMapper(timeZone)));
			var form = new TextRequestForm {Message = "test"};
			var id = Guid.NewGuid();
			form.EntityId = id;

			personRequestRepository.Stub(x => x.Find(id)).Return(personRequest);

			target.Persist(form);
		}
	}
}