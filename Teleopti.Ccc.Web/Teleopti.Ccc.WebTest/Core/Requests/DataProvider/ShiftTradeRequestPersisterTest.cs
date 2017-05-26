using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class ShiftTradeRequestPersisterTest
	{
		private IShiftTradeRequestMapper mapper;
		private IPersonRequestRepository repository;
		private IEventPublisher publisher;
		private IShiftTradeRequestSetChecksum shiftTradeSetChecksum;
		private IShiftTradeRequestProvider shiftTradeRequestProvider;
		private ICurrentUnitOfWork currentUnitOfWork;
		private IShiftTradeRequestPersonToPermissionValidator shiftTradeRequestPermissionValidator;
		private IPersonRequestCheckAuthorization personRequestCheckAuthorization;

		[SetUp]
		public void Setup()
		{
			mapper = MockRepository.GenerateMock<IShiftTradeRequestMapper>();
			repository = MockRepository.GenerateMock<IPersonRequestRepository>();
			publisher = MockRepository.GenerateMock<IEventPublisher>();
			shiftTradeSetChecksum = MockRepository.GenerateMock<IShiftTradeRequestSetChecksum>();
			shiftTradeRequestProvider = MockRepository.GenerateMock<IShiftTradeRequestProvider>();
			currentUnitOfWork = MockRepository.GenerateMock<ICurrentUnitOfWork>();
			shiftTradeRequestPermissionValidator = MockRepository.GenerateMock<IShiftTradeRequestPersonToPermissionValidator>();
			personRequestCheckAuthorization = MockRepository.GenerateMock<IPersonRequestCheckAuthorization>();
		}

		[Test]
		public void ShouldPersistMappedData()
		{
			var shiftTradeRequest = new PersonRequest(new Person()) { Request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>()) };
			
			var form = new ShiftTradeRequestForm();
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));

			target.Persist(form);

			repository.AssertWasCalled(x => x.Add(shiftTradeRequest));
		}

		[Test]
		public void ShouldNotPersistMappedDataWhenOfferCompleted()
		{
			shiftTradeRequestProvider.Stub(x => x.RetrieveUserWorkflowControlSet())
				.Return(new WorkflowControlSet("bla") {LockTrading = true});

			var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			offer.Stub(x => x.Status).Return(ShiftExchangeOfferStatus.Completed);

			var personRequests = new PersonRequest(new Person()) {Request = offer};
			repository.Stub(x => x.FindPersonRequestByRequestId(Guid.Empty)).Return(personRequests);

			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = Guid.Empty};
			var shiftTradeRequest = new PersonRequest(new Person());
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWork.Expect(x => x.Current()).Return(uow);

			var target = new ShiftTradeRequestPersister(repository, mapper, publisher, null,
				null, null, currentUnitOfWork, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, null,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));

			var result = target.Persist(form);

			result.ExchangeOffer.IsOfferAvailable.Should().Be.False();
			repository.AssertWasNotCalled(x => x.Add(shiftTradeRequest));
			uow.AssertWasNotCalled(x => x.AfterSuccessfulTx(Arg<Action>.Is.Anything));
		}

		[Test]
		public void ShouldNotPersistMappedDataWhenOfferPendingForAdminApproval()
		{
			shiftTradeRequestProvider.Stub(x => x.RetrieveUserWorkflowControlSet())
				.Return(new WorkflowControlSet("bla") {LockTrading = true});

			var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			offer.Stub(x => x.Status).Return(ShiftExchangeOfferStatus.PendingAdminApproval);

			var personRequests = new PersonRequest(new Person()) {Request = offer};
			repository.Stub(x => x.FindPersonRequestByRequestId(Guid.Empty)).Return(personRequests);

			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = Guid.Empty};
			var shiftTradeRequest = new PersonRequest(new Person());
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWork.Expect(x => x.Current()).Return(uow);

			var target = new ShiftTradeRequestPersister(repository, mapper, publisher, null,
				null, null, currentUnitOfWork, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, null,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));

			var result = target.Persist(form);

			result.ExchangeOffer.IsOfferAvailable.Should().Be.False();
			repository.AssertWasNotCalled(x => x.Add(shiftTradeRequest));
			uow.AssertWasNotCalled(x => x.AfterSuccessfulTx(Arg<Action>.Is.Anything));
		}

		[Test]
		public void ShouldAutoApprovedByAnnouncerWhenLockShiftTradeFromBulletinBoard()
		{
			shiftTradeRequestProvider.Stub(x => x.RetrieveUserWorkflowControlSet())
				.Return(new WorkflowControlSet("bla") {LockTrading = true});

			var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			offer.Stub(x => x.Status).Return(ShiftExchangeOfferStatus.Pending);

			var personRequests = new PersonRequest(new Person()) {Request = offer};
			repository.Stub(x => x.FindPersonRequestByRequestId(Guid.Empty)).Return(personRequests);

			var form = new ShiftTradeRequestForm { ShiftExchangeOfferId = Guid.Empty };
			var shiftTradeRequest = new PersonRequest(new Person()) {Request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>())};
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));

			var result = target.Persist(form);

			result.ExchangeOffer.IsOfferAvailable.Should().Be.True();
			result.Status.Should().Be.EqualTo(Resources.WaitingThreeDots);
		}

		[Test]
		public void ShouldAutoDeniedIfRecipientHasNoPermissionForShiftTrade()
		{
			shiftTradeRequestProvider.Stub(x => x.RetrieveUserWorkflowControlSet())
				.Return(new WorkflowControlSet("bla") { LockTrading = true });

			var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			offer.Stub(x => x.Status).Return(ShiftExchangeOfferStatus.Pending);
			var personRequests = new PersonRequest(new Person()) { Request = offer };
			repository.Stub(x => x.FindPersonRequestByRequestId(Guid.Empty)).Return(personRequests);

			var form = new ShiftTradeRequestForm { ShiftExchangeOfferId = Guid.Empty };
			var shiftTradeRequest = new PersonRequest(new Person()) {Request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> {new ShiftTradeSwapDetail(new Person(), new Person(), DateOnly.Today, DateOnly.Today)})};
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(false);

			var target = new ShiftTradeRequestPersister(repository, mapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));
			var result = target.Persist(form);

			result.Status.Should().Be.EqualTo(Resources.Denied);
			result.DenyReason.Should().Be.EqualTo(Resources.RecipientHasNoShiftTradePermission);
		}

		[Test]
		public void ShouldSetPendingAdminApprovalWhenLockShiftTradeFromBulletinBoard()
		{
			shiftTradeRequestProvider.Stub(x => x.RetrieveUserWorkflowControlSet())
				.Return(new WorkflowControlSet("bla")
				{
					LockTrading = true,
					AutoGrantShiftTradeRequest = false
				});

			var currentShift = ScheduleDayFactory.Create(DateOnly.Today.AddDays(1));
			var offer = new ShiftExchangeOffer(currentShift, new ShiftExchangeCriteria(), ShiftExchangeOfferStatus.Pending);
			var personRequests = new PersonRequest(new Person()) {Request = offer};
			repository.Stub(x => x.FindPersonRequestByRequestId(Guid.Empty)).Return(personRequests);

			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = Guid.Empty};
			var shiftTradeRequest = new PersonRequest(new Person()) { Request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>())};
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));

			var result = target.Persist(form);

			offer.Status.Should().Be.EqualTo(ShiftExchangeOfferStatus.PendingAdminApproval);
			result.ExchangeOffer.IsOfferAvailable.Should().Be.True();
			result.Status.Should().Be.EqualTo(Resources.WaitingThreeDots);
		}

		[Test]
		public void ShouldWaitingApprovedByAnnouncerWhenShiftTradeFromBulletinBoard()
		{
			shiftTradeRequestProvider.Stub(x => x.RetrieveUserWorkflowControlSet())
				.Return(new WorkflowControlSet("bla") {LockTrading = false});

			var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			offer.Stub(x => x.Status).Return(ShiftExchangeOfferStatus.Pending);
			offer.Stub(x => x.Period).Return(new DateTimePeriod(2017, 1, 1, 2017, 1, 1));

			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = Guid.Empty};
			var shiftTradeRequest = new PersonRequest(new Person()) {Request = offer};
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);
			
			offer.Stub(x => x.Status).Return(ShiftExchangeOfferStatus.Pending);
			var personRequests = new PersonRequest(new Person()) {Request = offer};
			repository.Stub(x => x.FindPersonRequestByRequestId(Guid.Empty)).Return(personRequests);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));
			var result = target.Persist(form);

			result.Status.Should().Be.EqualTo(Resources.New);
		}

		[Test]
		public void ShouldWaitingApprovedByAnnouncerWhenShiftTradeFromNormalRequestBoard()
		{
			shiftTradeRequestProvider.Stub(x => x.RetrieveUserWorkflowControlSet())
				.Return(new WorkflowControlSet("bla") {LockTrading = true});

			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = null};
			var shiftTradeRequest = new PersonRequest(new Person()) { Request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>()) };
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));
			var result = target.Persist(form);

			result.Status.Should().Be.EqualTo(Resources.New);
		}

		[Test]
		public void ShouldSendMessageToBus()
		{
			//bajstest - calling bus shouldn't happen here at all I think...
			//therefore - just dummy test
			var now = MockRepository.GenerateMock<INow>();
			now.Expect(x => x.UtcDateTime()).Return(DateTime.Now);

			var dataSourceProvider = MockRepository.GenerateMock<ICurrentDataSource>();
			dataSourceProvider.Expect(x => x.Current()).Return(MockRepository.GenerateMock<IDataSource>());

			var businessUnitProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			businessUnitProvider.Expect(x => x.Current()).Return(new BusinessUnit("d"));

			var form = new ShiftTradeRequestForm();
			mapper.Stub(x => x.Map(form)).Return(new PersonRequest(new Person()) { Request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>()) });

			mockPermissionValidatorPassed(true);

			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWork.Expect(x => x.Current()).Return(uow);

			var target = new ShiftTradeRequestPersister(MockRepository.GenerateMock<IPersonRequestRepository>(),
				mapper,
				publisher,
				now,
				dataSourceProvider,
				businessUnitProvider,
				currentUnitOfWork,
				shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));

			target.Persist(form);

			uow.AssertWasCalled(x => x.AfterSuccessfulTx(Arg<Action>.Is.Anything));
		}

		[Test]
		public void ShouldSetChecksumOnRequest()
		{
			//elände - borde inte behöva anropa setchecksum explicit
			var form = new ShiftTradeRequestForm();
			var shiftTradeRequest = new PersonRequest(new Person()) { Request = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>()) };

			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);
			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization,
				new RequestsViewModelMapper(new FakeUserTimeZone(), new FakeLinkProvider(), new FakeLoggedOnUser(),
					new EmptyShiftTradeRequestChecker(), new PersonNameProvider(new NameFormatSettingsPersisterAndProvider(new FakePersonalSettingDataRepository())), new FakeToggleManager()));
			target.Persist(form);

			shiftTradeSetChecksum.AssertWasCalled(x => x.SetChecksum(shiftTradeRequest.Request));
		}

		private void mockPermissionValidatorPassed(bool result)
		{
			shiftTradeRequestPermissionValidator.Stub(
				x => x.IsSatisfied(new ShiftTradeRequest(new List<IShiftTradeSwapDetail>())))
				.IgnoreArguments().Return(result);
		}
	}
}