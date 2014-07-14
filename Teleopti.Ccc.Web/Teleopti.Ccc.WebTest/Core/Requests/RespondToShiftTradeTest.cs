using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Core.Requests
{
	[TestFixture]
	public class RespondToShiftTradeTest
	{
		[Test]
		public void OkByMeWhenCalledShouldLoadTheShiftTradeFromTheRepositoryAndAcceptItUsingNoBus()
		{
			//setup
			var shiftTradeId = Guid.NewGuid();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var loggedOnPerson = MockRepository.GenerateStub<IPerson>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var shiftTrade = MockRepository.GenerateMock<IShiftTradeRequest>();
			var shiftTradeRequestCheckSum = MockRepository.GenerateMock<IShiftTradeRequestSetChecksum>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestCheckAuthorization = MockRepository.GenerateMock<IPersonRequestCheckAuthorization>();
			var busSender = MockRepository.GenerateMock<IServiceBusEventPublisher>();
			busSender.Expect(x => x.EnsureBus()).Return(false);
			var target = new RespondToShiftTrade(personRequestRepository, shiftTradeRequestCheckSum, personRequestCheckAuthorization, loggedOnUser, mapper, busSender, null);
			var requestViewModel = new RequestViewModel();

			personRequestRepository.Expect(p => p.Find(shiftTradeId)).Return(personRequest);
			personRequest.Expect(p => p.Request).Return(shiftTrade);
			loggedOnUser.Expect(l => l.CurrentUser()).Return(loggedOnPerson);
			mapper.Expect(m => m.Map<IPersonRequest, RequestViewModel>(personRequest)).Return(requestViewModel);

			//execute
			var result = target.OkByMe(shiftTradeId, "");

			//verify expectation:
			shiftTrade.AssertWasCalled(s => s.Accept(loggedOnPerson, shiftTradeRequestCheckSum, personRequestCheckAuthorization));
			result.Should().Be.SameInstanceAs(requestViewModel);
		}

		[Test]
		public void OkByMeWhenCalledShouldLoadTheShiftTradeFromTheRepositoryAndAcceptItUsingBus()
		{
			//setup
			var shiftTradeId = Guid.NewGuid();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var loggedOnPerson = MockRepository.GenerateStub<IPerson>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var shiftTrade = MockRepository.GenerateMock<IShiftTradeRequest>();
			var shiftTradeRequestCheckSum = MockRepository.GenerateMock<IShiftTradeRequestSetChecksum>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var personRequestCheckAuthorization = MockRepository.GenerateMock<IPersonRequestCheckAuthorization>();
			var busSender = MockRepository.GenerateMock<IServiceBusEventPublisher>();
			var unitOfWorkFactoryProvider = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var uowFactory = MockRepository.GenerateStub<IUnitOfWorkFactory>();
			uowFactory.Expect(x => x.Name).Return("gegga");
			unitOfWorkFactoryProvider.Expect(x => x.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
			var bsProvider = MockRepository.GenerateMock<ICurrentBusinessUnit>();
			bsProvider.Expect(x => x.Current()).Return(new BusinessUnit("sdf"));
			busSender.Expect(x => x.EnsureBus()).Return(true);
			var target = new RespondToShiftTrade(personRequestRepository, shiftTradeRequestCheckSum, personRequestCheckAuthorization, loggedOnUser, mapper, busSender, MockRepository.GenerateMock<INow>());
			var requestViewModel = new RequestViewModel();

			personRequestRepository.Expect(p => p.Find(shiftTradeId)).Return(personRequest);
			personRequest.Expect(p => p.Request).Return(shiftTrade);
			loggedOnUser.Expect(l => l.CurrentUser()).Return(loggedOnPerson);
			mapper.Expect(m => m.Map<IPersonRequest, RequestViewModel>(personRequest)).Return(requestViewModel);

			//execute
			target.OkByMe(shiftTradeId, "");

			//verify expectation:
			busSender.AssertWasCalled(s => s.Publish(null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldDenyShiftTradeOnRequest()
		{
			var shiftTradeId = Guid.NewGuid();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var loggedOnPerson = MockRepository.GenerateStub<IPerson>();
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var personRequestCheckAuthorization = MockRepository.GenerateMock<IPersonRequestCheckAuthorization>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new RespondToShiftTrade(personRequestRepository, null, personRequestCheckAuthorization, loggedOnUser, mapper, null, null);
			var requestViewModel = new RequestViewModel();

			personRequestRepository.Expect(p => p.Find(shiftTradeId)).Return(personRequest);
			personRequest.Expect(p => p.GetMessage(Arg<ITextFormatter>.Is.Anything)).Return("message");
			personRequest.Expect(p => p.TrySetMessage("message")).Return(true);
			loggedOnUser.Expect(l => l.CurrentUser()).Return(loggedOnPerson);
			personRequest.Expect(p => p.TrySetMessage("message"));
			mapper.Expect(m => m.Map<IPersonRequest, RequestViewModel>(personRequest)).Return(requestViewModel);

			var result = target.Deny(shiftTradeId, "");

			personRequest.AssertWasCalled(s => s.Deny(loggedOnPerson, "RequestDenyReasonOtherPart", personRequestCheckAuthorization));
			result.Should().Be.SameInstanceAs(requestViewModel);
		}

		[Test]
		public void ApproveShouldReturnEmptyViewModelIfPersonrequestDoesntExist()
		{
			var id = new Guid();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			personRequestRepository.Expect(p => p.Find(id)).Return(null);
			var target = new RespondToShiftTrade(personRequestRepository, null, null, null, null, null, null);
			Assert.That(target.OkByMe(id, ""),Is.Not.Null);
		}

		[Test]
		public void DenyShouldReturnEmptyViewModelIfPersonrequestDoesntExist()
		{
			var id = new Guid();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			personRequestRepository.Expect(p => p.Find(id)).Return(null);
			var target = new RespondToShiftTrade(personRequestRepository, null, null, null, null, null, null);
			Assert.That(target.Deny(id, ""), Is.Not.Null);
		}
	}
}
