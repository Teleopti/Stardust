﻿using System;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
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
			var target = new RespondToShiftTrade(personRequestRepository, shiftTradeRequestCheckSum, personRequestCheckAuthorization, loggedOnUser, null);

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
			var personRequest = MockRepository.GenerateMock<IPersonRequest>();
			var personRequestCheckAuthorization = MockRepository.GenerateMock<IPersonRequestCheckAuthorization>();
			var mapper = MockRepository.GenerateMock<IMappingEngine>();
			var target = new RespondToShiftTrade(personRequestRepository, null, personRequestCheckAuthorization, loggedOnUser, mapper);
			var requestViewModel = new RequestViewModel();

			personRequestRepository.Expect(p => p.Find(shiftTradeId)).Return(personRequest);
			personRequest.Expect(p => p.GetMessage(Arg<ITextFormatter>.Is.Anything)).Return("message");
			personRequest.Expect(p => p.TrySetMessage("message")).Return(true);
			loggedOnUser.Expect(l => l.CurrentUser()).Return(loggedOnPerson);
			personRequest.Expect(p => p.TrySetMessage("message"));
			mapper.Expect(m => m.Map<IPersonRequest, RequestViewModel>(personRequest)).Return(requestViewModel);

			var result = target.Deny(shiftTradeId);

			personRequest.AssertWasCalled(s => s.Deny(loggedOnPerson, "RequestDenyReasonOtherPart", personRequestCheckAuthorization));
			result.Should().Be.SameInstanceAs(requestViewModel);
		}
	}
}
