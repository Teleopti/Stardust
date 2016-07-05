﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[TestFixture]
	public class ShiftTradeRequestHandlerTest
	{
		private ShiftTradeRequestHandler target;
		private IShiftTradeValidator validator;
		private IPersonRequestRepository personRequestRepository;
		private IPersonRepository personRepository;
		private IPersonRequest personRequest;
		private Person fromPerson;
		private Person toPerson;
		private ICurrentScenario scenarioRepository;
		private IScheduleStorage scheduleStorage;
		private ISchedulingResultStateHolder schedulingResultState;
		private IScenario scenario;
		private IRequestFactory requestFactory;
		private IScheduleDifferenceSaver scheduleDictionarySaver;
		private IPersonRequestCheckAuthorization personRequestCheckAuthorization;
		private ILoadSchedulesForRequestWithoutResourceCalculation loader;
		private IDifferenceCollectionService<IPersistableScheduleData> differenceCollectionService;
		private IMessageBrokerComposite messageBroker;

		[SetUp]
		public void Setup()
		{
			createPersonAndScenario();
			createRequest();
			createRepositories();

			scheduleDictionarySaver = MockRepository.GenerateMock<IScheduleDifferenceSaver>();
			differenceCollectionService = MockRepository.GenerateMock<IDifferenceCollectionService<IPersistableScheduleData>>();
			requestFactory = MockRepository.GenerateMock<IRequestFactory>();
			personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerForTest();
			schedulingResultState = new SchedulingResultStateHolder();
			var scheduleRanges = new Dictionary<IPerson, IScheduleRange>();
			schedulingResultState.Schedules = new ScheduleDictionaryForTest(scenario,
				new ScheduleDateTimePeriod(new DateTimePeriod()), scheduleRanges);
			var permissionChecker = new PersistableScheduleDataPermissionChecker();
			scheduleRanges.Add(fromPerson,
				new ScheduleRange(schedulingResultState.Schedules,
					new ScheduleParameters(scenario, fromPerson, new DateTimePeriod()), permissionChecker));
			scheduleRanges.Add(toPerson,
				new ScheduleRange(schedulingResultState.Schedules, new ScheduleParameters(scenario, toPerson, new DateTimePeriod()),
					permissionChecker));
			loader = MockRepository.GenerateMock<ILoadSchedulesForRequestWithoutResourceCalculation>();
			messageBroker = new FakeMessageBrokerComposite();

			target = new ShiftTradeRequestHandler(schedulingResultState, validator, requestFactory,
				scenarioRepository, personRequestRepository, scheduleStorage,
				personRepository, personRequestCheckAuthorization, scheduleDictionarySaver,
				loader, differenceCollectionService, messageBroker);
		}

		private void createRepositories()
		{
			validator = MockRepository.GenerateMock<IShiftTradeValidator>();
			personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
			personRepository = MockRepository.GenerateMock<IPersonRepository>();
			scenarioRepository = MockRepository.GenerateMock<ICurrentScenario>();
			scheduleStorage = MockRepository.GenerateMock<IScheduleStorage>();
		}

		private void createRequest()
		{
			var shiftTradeSwapDetail = new ShiftTradeSwapDetail(fromPerson, toPerson,
				new DateOnly(2008, 12, 23), new DateOnly(2008, 12, 24))
			{
				ChecksumFrom = -349792281,
				ChecksumTo = -349792281
			};

			var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail>
			{
				shiftTradeSwapDetail
			});
			personRequest = new PersonRequest(fromPerson, shiftTradeRequest);
		}

		private void createPersonAndScenario()
		{
			fromPerson = new Person { Name = new Name("Janne", "Schaffer") };
			toPerson = new Person { Name = new Name("Staffan", "Ling") };
			var wfcl = new WorkflowControlSet("Mutex") { AutoGrantShiftTradeRequest = true };
			fromPerson.WorkflowControlSet = wfcl;
			toPerson.WorkflowControlSet = wfcl;
			scenario = new Scenario("Default");
		}

		[Test]
		public void CanConsumeNewValidateIsTrue()
		{
			var created = getNewShiftTradeRequestCreated();
			var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();

			validator.Stub(x => x.Validate((IShiftTradeRequest)personRequest.Request))
				.Return(new ShiftTradeRequestValidationResult(true));
			personRequestRepository.Stub(x => x.Get(created.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);

			target.Handle(created);
			Assert.AreEqual(false, personRequest.IsNew);
			Assert.AreEqual(true, personRequest.IsPending);
		}

		[Test]
		public void ShouldBeReferedAfterScheduleChanged()
		{
			personRequest = creatPersonRequest();

			var scheduleChangedEvent = new ProjectionChangedEvent
			{
				PersonId = personRequest.Person.Id.GetValueOrDefault(),
				ScheduleDays = new[]
				{
					new ProjectionChangedEventScheduleDay {Date = DateTime.Now},
					new ProjectionChangedEventScheduleDay {Date = DateTime.Now}
				}
			};

			target = getTargetWithAlwaysReferedChecker();

			target.Handle(scheduleChangedEvent);
			var shiftTrafeRequest = (IShiftTradeRequest)personRequest.Request;

			var mockRequestStatusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			Assert.AreEqual(ShiftTradeStatus.Referred, shiftTrafeRequest.GetShiftTradeStatus(mockRequestStatusChecker));
		}

		[Test]
		public void ShouldSendNotificationToAnotherPerson()
		{
			personRequest = creatPersonRequest();

			var scheduleChangedEvent = new ProjectionChangedEvent
			{
				PersonId = personRequest.Request.PersonFrom.Id.GetValueOrDefault(),
				ScheduleDays = new[]
				{
					new ProjectionChangedEventScheduleDay {Date = DateTime.Now},
					new ProjectionChangedEventScheduleDay {Date = DateTime.Now}
				}
			};

			target = getTargetWithAlwaysReferedChecker();

			target.Handle(scheduleChangedEvent);

			var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;
			var fakeMessageBroker = (FakeMessageBrokerComposite)messageBroker;
			fakeMessageBroker.ReferenceObjectId().Should()
				.Be(shiftTradeRequest.PersonTo.Id.GetValueOrDefault());
			fakeMessageBroker.SentCount().Should().Be(1);
			fakeMessageBroker.SentMessageType().Should().Be(typeof(IShiftTradeScheduleChangedInDefaultScenario));
		}

		private ShiftTradeRequestHandler getTargetWithAlwaysReferedChecker()
		{
			scenarioRepository.Stub(x => x.Current()).Return(scenario);

			var statusChecker = new ShiftTradeRequestStatusCheckerForTestAlwaysRefer();

			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);

			target = new ShiftTradeRequestHandler(schedulingResultState, validator, requestFactory,
				scenarioRepository, personRequestRepository, scheduleStorage,
				personRepository, personRequestCheckAuthorization, scheduleDictionarySaver,
				loader, differenceCollectionService, messageBroker);

			return target;
		}

		private IPersonRequest creatPersonRequest()
		{
			personRepository = new FakePersonRepository();
			personRequestRepository = new FakePersonRequestRepository();

			var personTo = PersonFactory.CreatePersonWithId();
			var personFrom = PersonFactory.CreatePersonWithId();
			var newPersonRequest = new PersonRequestFactory().CreatePersonShiftTradeRequest(personFrom, personTo, DateOnly.Today);
			var shiftTradeRequest = newPersonRequest.Request as IShiftTradeRequest;
			shiftTradeRequest.SetShiftTradeStatus(ShiftTradeStatus.OkByBothParts, new PersonRequestAuthorizationCheckerForTest());
			personRequestRepository.Add(newPersonRequest);
			personRepository.Add(shiftTradeRequest.PersonFrom);
			personRepository.Add(shiftTradeRequest.PersonTo);

			return newPersonRequest;
		}

		[Test]
		public void CanConsumeNewValidateIsFalse()
		{
			var created = getNewShiftTradeRequestCreated();
			var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();

			validator.Stub(x => x.Validate((IShiftTradeRequest)personRequest.Request))
				.Return(new ShiftTradeRequestValidationResult(false));
			personRequestRepository.Stub(x => x.Get(created.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);

			target.Handle(created);
			Assert.AreEqual(true, personRequest.IsDenied);
		}

		[Test]
		public void CanConsumeAcceptValidIsTrue()
		{
			var accept = getAcceptShiftTrade();

			var approvalService = MockRepository.GenerateMock<IRequestApprovalService>();
			var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;
			INewBusinessRuleCollection ruleCollection = null;

			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			validator.Stub(x => x.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
			personRepository.Stub(x => x.Get(accept.AcceptingPersonId)).Return(toPerson);

			requestFactory.Stub(x => x.GetRequestApprovalService(null, null, schedulingResultState))
				.Constraints(Is.Matching<NewBusinessRuleCollection>(
					b =>
					{
						ruleCollection = b;
						return true;
					}), Is.Equal(scenario), Is.Equal(schedulingResultState))
				.Return(approvalService);
			approvalService.Stub(x => x.ApproveShiftTrade(shiftTradeRequest)).Return(new List<IBusinessRuleResponse>());
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);

			target.Handle(accept);
			Assert.AreEqual(false, personRequest.IsNew);
			Assert.AreEqual(false, personRequest.IsPending);
			Assert.AreEqual(true, personRequest.IsApproved);
			Assert.AreEqual(ShiftTradeStatus.OkByBothParts,
				shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			Assert.AreEqual(accept.Message, personRequest.GetMessage(new NoFormatting()));
			ruleCollection.Item(typeof(NewPersonAccountRule)).HaltModify.Should().Be.False();
			statusChecker.AssertWasCalled(x => x.Check(shiftTradeRequest), o => o.Repeat.Twice());
			scheduleDictionarySaver.AssertWasCalled(x => x.SaveChanges(null, null), o => o.IgnoreArguments());
			loader.AssertWasCalled(x => x.Execute(scenario, new DateTimePeriod(), null, schedulingResultState),
				o => o.IgnoreArguments());
		}

		[Test]
		public void CanConsumeAcceptValidIsTrueAndShutdownNiceUponValidationExceptionWhenSavingSchedules()
		{
			var accept = getAcceptShiftTrade();

			var approvalService = MockRepository.GenerateMock<IRequestApprovalService>();
			var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;

			validator.Stub(x => x.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
			personRepository.Stub(x => x.Get(accept.AcceptingPersonId)).Return(toPerson);
			approvalService.Stub(x => x.ApproveShiftTrade(shiftTradeRequest)).Return(new List<IBusinessRuleResponse>());
			requestFactory.Stub(x => x.GetRequestApprovalService(null, scenario, schedulingResultState))
				.IgnoreArguments()
				.Return(approvalService);
			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);
			scheduleDictionarySaver.Stub(x => x.SaveChanges(null, null)).IgnoreArguments().Throw(new ValidationException());

			target.Handle(accept);
			Assert.AreEqual(false, personRequest.IsNew);
			Assert.AreEqual(false, personRequest.IsPending);
			Assert.AreEqual(true, personRequest.IsApproved);
			Assert.AreEqual(ShiftTradeStatus.OkByBothParts,
				shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			Assert.AreEqual(accept.Message, personRequest.GetMessage(new NoFormatting()));
			scheduleDictionarySaver.AssertWasCalled(x => x.SaveChanges(null, null), o => o.IgnoreArguments());
		}

		[Test]
		public void ShouldKeepAsPendingWhenBusinessRulesFail()
		{
			var accept = getAcceptShiftTrade();

			var approvalService = MockRepository.GenerateMock<IRequestApprovalService>();
			var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			var rule = MockRepository.GenerateMock<IBusinessRuleResponse>();
			var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;

			validator.Stub(x => x.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
			personRepository.Stub(x => x.Get(accept.AcceptingPersonId)).Return(toPerson);
			approvalService.Stub(x => x.ApproveShiftTrade(shiftTradeRequest))
				.Return(new List<IBusinessRuleResponse> { rule, rule });
			requestFactory.Stub(x => x.GetRequestApprovalService(null, scenario, schedulingResultState))
				.IgnoreArguments().Return(approvalService);
			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);
			rule.Stub(x => x.Message).Return("aja baja!");

			target.Handle(accept);
			Assert.AreEqual(false, personRequest.IsNew);
			Assert.AreEqual(true, personRequest.IsPending);
			Assert.AreEqual(false, personRequest.IsApproved);
			Assert.AreEqual(ShiftTradeStatus.OkByBothParts,
				shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			personRequest.GetMessage(new NoFormatting())
				.Should().Be.EqualTo("I want to trade!\r\nViolation of a Business Rule:\r\naja baja!\r\n");
		}

		[Test]
		public void DoNotProcessAlreadyHandledAcceptMessages()
		{
			var accept = getAcceptShiftTrade();

			personRequest.Pending();
			personRequest.Deny(null, "DenyReason", new PersonRequestAuthorizationCheckerForTest());
			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);

			target.Handle(accept);

			requestFactory.AssertWasNotCalled(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState));
		}

		[Test]
		public void DoNotProcessAlreadyHandledCreatedMessages()
		{
			var shiftTradeRequestCreated = getNewShiftTradeRequestCreated();

			personRequest.Pending();
			personRequest.Deny(null, "DenyReason", new PersonRequestAuthorizationCheckerForTest());
			personRequestRepository.Stub(x => x.Get(shiftTradeRequestCreated.PersonRequestId)).Return(personRequest);

			target.Handle(shiftTradeRequestCreated);
			requestFactory.AssertWasNotCalled(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState));
		}

		private static NewShiftTradeRequestCreatedEvent getNewShiftTradeRequestCreated()
		{
			var nstrc = new NewShiftTradeRequestCreatedEvent
			{
				LogOnDatasource = "V7Config",
				LogOnBusinessUnitId = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
				Timestamp = DateTime.UtcNow,
				PersonRequestId = new Guid("9AC8476B-9B8F-4330-9561-9D7A00BAA585")
			};
			return nstrc;
		}

		private static AcceptShiftTradeEvent getAcceptShiftTrade()
		{
			var ast = new AcceptShiftTradeEvent
			{
				LogOnDatasource = "V7Config",
				LogOnBusinessUnitId = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA"),
				Timestamp = DateTime.UtcNow,
				PersonRequestId = new Guid("9AC8476B-9B8F-4330-9561-9D7A00BAA585"),
				Message = "I want to trade!"
			};
			return ast;
		}
	}
}
