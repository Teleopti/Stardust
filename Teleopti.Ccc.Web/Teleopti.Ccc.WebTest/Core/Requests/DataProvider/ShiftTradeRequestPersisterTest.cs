using System;
using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Requests.DataProvider
{
	[TestFixture]
	public class ShiftTradeRequestPersisterTest
	{
		private IShiftTradeRequestMapper mapper;
		private IMappingEngine autoMapper;
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
			autoMapper = MockRepository.GenerateMock<IMappingEngine>();
			autoMapper.Stub(x => x.Map<IPersonRequest, RequestViewModel>(new PersonRequest(new Person())))
				.IgnoreArguments()
				.Do((Func<IPersonRequest, RequestViewModel>)(request => new RequestViewModel
				{
					Status = request.StatusText,
					DenyReason = request.DenyReason,
					ExchangeOffer = new ShiftExchangeOfferRequestViewModel
					{
						IsOfferAvailable = true
					}
				}));
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
			var viewModel = new RequestViewModel();
			var shiftTradeRequest = new PersonRequest(new Person());
			var newAutoMapper = MockRepository.GenerateMock<IMappingEngine>();
			newAutoMapper.Stub(x => x.Map<IPersonRequest, RequestViewModel>(shiftTradeRequest))
				.Return(viewModel);

			var form = new ShiftTradeRequestForm();
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, newAutoMapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization);

			var result = target.Persist(form);

			result.Should().Be.SameInstanceAs(viewModel);
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

			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, publisher, null,
				null, null, currentUnitOfWork, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, null);

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

			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, publisher, null,
				null, null, currentUnitOfWork, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, null);

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
			var shiftTradeRequest = new PersonRequest(new Person());
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization);

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
			var shiftTradeRequest = new PersonRequest(new Person());
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(false);

			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization);
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
			var shiftTradeRequest = new PersonRequest(new Person());
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization);

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

			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = Guid.Empty};
			var shiftTradeRequest = new PersonRequest(new Person());
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			var offer = MockRepository.GenerateMock<IShiftExchangeOffer>();
			offer.Stub(x => x.Status).Return(ShiftExchangeOfferStatus.Pending);
			var personRequests = new PersonRequest(new Person()) {Request = offer};
			repository.Stub(x => x.FindPersonRequestByRequestId(Guid.Empty)).Return(personRequests);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization);
			var result = target.Persist(form);

			result.Status.Should().Be.EqualTo(Resources.New);
		}

		[Test]
		public void ShouldWaitingApprovedByAnnouncerWhenShiftTradeFromNormalRequestBoard()
		{
			shiftTradeRequestProvider.Stub(x => x.RetrieveUserWorkflowControlSet())
				.Return(new WorkflowControlSet("bla") {LockTrading = true});

			var form = new ShiftTradeRequestForm {ShiftExchangeOfferId = null};
			var shiftTradeRequest = new PersonRequest(new Person());
			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);

			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization);
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
			mapper.Stub(x => x.Map(form)).Return(new PersonRequest(new Person()));

			mockPermissionValidatorPassed(true);

			var uow = MockRepository.GenerateMock<IUnitOfWork>();
			currentUnitOfWork.Expect(x => x.Current()).Return(uow);

			var target = new ShiftTradeRequestPersister(MockRepository.GenerateMock<IPersonRequestRepository>(),
				mapper,
				autoMapper,
				publisher,
				now,
				dataSourceProvider,
				businessUnitProvider,
				currentUnitOfWork,
				shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization);

			target.Persist(form);

			uow.AssertWasCalled(x => x.AfterSuccessfulTx(Arg<Action>.Is.Anything));
		}

		[Test]
		public void ShouldSetChecksumOnRequest()
		{
			//elände - borde inte behöva anropa setchecksum explicit
			var form = new ShiftTradeRequestForm();
			var shiftTradeRequest = new PersonRequest(new Person());

			mapper.Stub(x => x.Map(form)).Return(shiftTradeRequest);
			mockPermissionValidatorPassed(true);

			var target = new ShiftTradeRequestPersister(repository, mapper, autoMapper, publisher, null,
				null, null, null, shiftTradeSetChecksum, shiftTradeRequestProvider, null,
				shiftTradeRequestPermissionValidator, personRequestCheckAuthorization);
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