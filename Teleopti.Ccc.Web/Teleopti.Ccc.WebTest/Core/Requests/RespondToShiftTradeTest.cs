using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Requests
{
	[TestFixture]
	public class RespondToShiftTradeTest
	{
		[Test]
		public void OkByMeWhenCalledShouldLoadTheShiftTradeFromTheRepositoryAndAcceptIt()
		{
			//setup
			var shiftTradeId = Guid.NewGuid();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var loggedOnPerson = MockRepository.GenerateStub<IPerson>();
			var personrequest = MockRepository.GenerateMock<IPersonRequest>();
			var shiftTrade = MockRepository.GenerateMock<IShiftTradeRequest>();
			var shiftTradeRequestCheckSum = MockRepository.GenerateMock<IShiftTradeRequestSetChecksum>();
			var personRequestCheckAuthorization = MockRepository.GenerateMock<IPersonRequestCheckAuthorization>();
			var target = new RespondToShiftTrade(personRequestRepository,shiftTradeRequestCheckSum,personRequestCheckAuthorization,loggedOnUser);

			personRequestRepository.Expect(p => p.Find(shiftTradeId)).Return(personrequest);
			personrequest.Expect(p => p.Request).Return(shiftTrade);
			loggedOnUser.Expect(l => l.CurrentUser()).Return(loggedOnPerson);

			//execute
			target.OkByMe(shiftTradeId);

			//verify expectation:
			shiftTrade.AssertWasCalled(s => s.Accept(loggedOnPerson, shiftTradeRequestCheckSum, personRequestCheckAuthorization));
		}

		[Test]
		public void ShouldDenyShiftTradeOnRequest()
		{
			var shiftTradeId = Guid.NewGuid();
			var personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var loggedOnPerson = MockRepository.GenerateStub<IPerson>();
			var personrequest = MockRepository.GenerateMock<IPersonRequest>();
			var shiftTrade = MockRepository.GenerateMock<IShiftTradeRequest>();
			var target = new RespondToShiftTrade(personRequestRepository, null, null, loggedOnUser);

			personRequestRepository.Expect(p => p.Find(shiftTradeId)).Return(personrequest);
			personrequest.Expect(p => p.Request).Return(shiftTrade);
			loggedOnUser.Expect(l => l.CurrentUser()).Return(loggedOnPerson);

			//execute
			target.Deny(shiftTradeId);

			//verify expectation:
			shiftTrade.AssertWasCalled(s => s.Deny(loggedOnPerson));
		}
	}
}
