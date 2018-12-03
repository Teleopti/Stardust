using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;

using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[TestFixture]
	public class ShiftTradeRequestHandlerTest
	{
		private const string ruleMessage1 = "A rule broken!";
		private const string ruleMessage2 = "Another rule broken!";

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
			var currentAuthorization = CurrentAuthorization.Make();
			var permissionChecker = new PersistableScheduleDataPermissionChecker(currentAuthorization);
			scheduleRanges.Add(fromPerson,
				new ScheduleRange(schedulingResultState.Schedules,
					new ScheduleParameters(scenario, fromPerson, new DateTimePeriod()), permissionChecker, currentAuthorization));
			scheduleRanges.Add(toPerson,
				new ScheduleRange(schedulingResultState.Schedules, new ScheduleParameters(scenario, toPerson, new DateTimePeriod()),
					permissionChecker, currentAuthorization));
			loader = MockRepository.GenerateMock<ILoadSchedulesForRequestWithoutResourceCalculation>();
			businessRuleProvider = new FakeBusinessRuleProvider();
			newBusinessRuleCollection = new FakeNewBusinessRuleCollection();
			shiftTradePendingReasonsService = new ShiftTradePendingReasonsService();
			shiftTradeApproveService = new ShiftTradeApproveService(personRequestCheckAuthorization, differenceCollectionService,
				scheduleDictionarySaver, requestFactory, scenarioRepository);

			target = new ShiftTradeRequestHandler(schedulingResultState, validator, requestFactory, scenarioRepository,
				personRequestRepository, scheduleStorage, personRepository, personRequestCheckAuthorization,
				loader, businessRuleProvider,
				shiftTradePendingReasonsService, shiftTradeApproveService);
		}

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
		private IBusinessRuleProvider businessRuleProvider;
		private INewBusinessRuleCollection newBusinessRuleCollection;
		private IShiftTradePendingReasonsService shiftTradePendingReasonsService;
		private IShiftTradeApproveService shiftTradeApproveService;

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
			fromPerson = new Person().WithName(new Name("Janne", "Schaffer"));
			toPerson = new Person().WithName(new Name("Staffan", "Ling"));
			var wfcl = new WorkflowControlSet("Mutex") { AutoGrantShiftTradeRequest = true };
			fromPerson.WorkflowControlSet = wfcl;
			toPerson.WorkflowControlSet = wfcl;
			scenario = new Scenario("Default");
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

		[Test]
		public void CanConsumeAcceptValidIsTrue()
		{
			var accept = getAcceptShiftTrade();

			var approvalService = MockRepository.GenerateMock<IRequestApprovalService>();
			var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
			var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;
			var ruleResponse = new BusinessRuleResponse(typeof(NewPersonAccountRule), "no go", false, false,
				new DateTimePeriod(), PersonFactory.CreatePersonWithId(), new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), "tjillevippen");
			var rules = new List<IBusinessRuleResponse> { ruleResponse };
			((FakeNewBusinessRuleCollection)newBusinessRuleCollection).SetRuleResponse(rules);
			newBusinessRuleCollection.Add(new NewPersonAccountRule(null, null));

			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			validator.Stub(x => x.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
			personRepository.Stub(x => x.Get(accept.AcceptingPersonId)).Return(toPerson);

			requestFactory.Stub(x => x.GetRequestApprovalService(null, null, schedulingResultState, personRequest))
				.Constraints(Is.Matching<INewBusinessRuleCollection>(b => true), Is.Equal(scenario),
					Is.Equal(schedulingResultState), Is.Equal(personRequest))
				.Return(approvalService);
			approvalService.Stub(x => x.Approve(shiftTradeRequest)).Return(new List<IBusinessRuleResponse>());
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);
			((FakeBusinessRuleProvider)businessRuleProvider).SetBusinessRules(newBusinessRuleCollection);
			target.Handle(accept);
			Assert.AreEqual(false, personRequest.IsNew);
			Assert.AreEqual(false, personRequest.IsPending);
			Assert.AreEqual(true, personRequest.IsApproved);
			Assert.AreEqual(BusinessRuleFlags.None, personRequest.BrokenBusinessRules);
			Assert.AreEqual(ShiftTradeStatus.OkByBothParts,
				shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			Assert.AreEqual(accept.Message, personRequest.GetMessage(new NoFormatting()));
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
			approvalService.Stub(x => x.Approve(shiftTradeRequest)).Return(new List<IBusinessRuleResponse>());
			requestFactory.Stub(x => x.GetRequestApprovalService(null, scenario, schedulingResultState, personRequest))
				.IgnoreArguments()
				.Return(approvalService);
			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);
			scheduleDictionarySaver.Stub(x => x.SaveChanges(null, null)).IgnoreArguments().Throw(new ValidationException());
			var ruleResponse = new BusinessRuleResponse(typeof(NewPersonAccountRule), "no go", false, false,
				new DateTimePeriod(), PersonFactory.CreatePersonWithId(), new DateOnlyPeriod(DateOnly.Today, DateOnly.Today), "tjillevippen");
			var rules = new List<IBusinessRuleResponse> { ruleResponse };
			((FakeNewBusinessRuleCollection)newBusinessRuleCollection).SetRuleResponse(rules);
			newBusinessRuleCollection.Add(new NewPersonAccountRule(null, null));

			((FakeBusinessRuleProvider)businessRuleProvider).SetBusinessRules(newBusinessRuleCollection);
			target.Handle(accept);
			Assert.AreEqual(false, personRequest.IsNew);
			Assert.AreEqual(false, personRequest.IsPending);
			Assert.AreEqual(true, personRequest.IsApproved);
			Assert.AreEqual(BusinessRuleFlags.None, personRequest.BrokenBusinessRules.GetValueOrDefault());
			Assert.AreEqual(ShiftTradeStatus.OkByBothParts,
				shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			Assert.AreEqual(accept.Message, personRequest.GetMessage(new NoFormatting()));
			scheduleDictionarySaver.AssertWasCalled(x => x.SaveChanges(null, null), o => o.IgnoreArguments());
		}

		[Test]
		public void CanConsumeNewValidateIsFalse()
		{
			var created = getNewShiftTradeRequestCreated();
			var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();

			validator.Stub(x => x.Validate((IShiftTradeRequest)personRequest.Request))
				.Return(new ShiftTradeRequestValidationResult(false, true, string.Empty));
			personRequestRepository.Stub(x => x.Get(created.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);

			target.Handle(created);
			Assert.AreEqual(true, personRequest.IsDenied);
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
		public void DoNotProcessAlreadyHandledAcceptMessages()
		{
			var accept = getAcceptShiftTrade();

			personRequest.Pending();
			personRequest.Deny("DenyReason", new PersonRequestAuthorizationCheckerForTest());
			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);

			target.Handle(accept);

			requestFactory.AssertWasNotCalled(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState));
		}

		[Test]
		public void DoNotProcessAlreadyHandledCreatedMessages()
		{
			var shiftTradeRequestCreated = getNewShiftTradeRequestCreated();

			personRequest.Pending();
			personRequest.Deny("DenyReason", new PersonRequestAuthorizationCheckerForTest());
			personRequestRepository.Stub(x => x.Get(shiftTradeRequestCreated.PersonRequestId)).Return(personRequest);

			target.Handle(shiftTradeRequestCreated);
			requestFactory.AssertWasNotCalled(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState));
		}

		[Test]
		public void ShouldKeepAsPendingWhenBusinessRulesFailButNotSetToAutoDeny()
		{
			var responses = createRuleResponses();
			var accept = setupForConfigurableRuleTest(responses);
			var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;

			// Simulate no rule broken was configured to auto denied
			((FakeBusinessRuleProvider)businessRuleProvider).SetDeniableResponse(null);

			target.Handle(accept);
			Assert.AreEqual(false, personRequest.IsNew);
			Assert.AreEqual(true, personRequest.IsPending);
			Assert.AreEqual(false, personRequest.IsApproved);
			Assert.AreEqual(false, personRequest.IsDenied);
			Assert.AreEqual(BusinessRuleFlags.NewMaxWeekWorkTimeRule | BusinessRuleFlags.NewShiftCategoryLimitationRule,
				personRequest.BrokenBusinessRules);
			Assert.AreEqual(ShiftTradeStatus.OkByBothParts,
				shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			personRequest.DenyReason.Should().Be.Empty();
			personRequest.GetMessage(new NoFormatting())
				.Should().Be.EqualTo("I want to trade!\r\nViolation of a Business Rule:\r\n"
									 + ruleMessage1 + "\r\n" + ruleMessage2 + "\r\n");
		}

		[Test]
		public void ShouldDenyWhenBusinessRulesFail()
		{
			var responses = createRuleResponses();
			var accept = setupForConfigurableRuleTest(responses);
			var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;

			var deniableResponse = responses.First();
			// Simulate one rule broken was configured to auto denied
			((FakeBusinessRuleProvider)businessRuleProvider).SetDeniableResponse(deniableResponse);

			target.Handle(accept);
			Assert.AreEqual(false, personRequest.IsNew);
			Assert.AreEqual(false, personRequest.IsPending);
			Assert.AreEqual(false, personRequest.IsApproved);
			Assert.AreEqual(true, personRequest.IsDenied);
			Assert.AreEqual(BusinessRuleFlags.NewMaxWeekWorkTimeRule | BusinessRuleFlags.NewShiftCategoryLimitationRule,
				personRequest.BrokenBusinessRules);
			Assert.AreEqual(ShiftTradeStatus.OkByBothParts,
				shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			personRequest.DenyReason.Should().Be.EqualTo(deniableResponse.Message);
			personRequest.GetMessage(new NoFormatting())
				.Should().Be.EqualTo("I want to trade!\r\nViolation of a Business Rule:\r\n"
									 + ruleMessage1 + "\r\n" + ruleMessage2 + "\r\n");
		}

		private IList<IBusinessRuleResponse> createRuleResponses()
		{
			var ruleResponse1 = MockRepository.GenerateMock<IBusinessRuleResponse>();
			ruleResponse1.Stub(x => x.Message).Return(ruleMessage1);
			ruleResponse1.Stub(x => x.TypeOfRule).Return(typeof(NewMaxWeekWorkTimeRule));
			var ruleResponse2 = MockRepository.GenerateMock<IBusinessRuleResponse>();
			ruleResponse2.Stub(x => x.Message).Return(ruleMessage2);
			ruleResponse2.Stub(x => x.TypeOfRule).Return(typeof(NewShiftCategoryLimitationRule));

			return new[] { ruleResponse1, ruleResponse2 };
		}

		private AcceptShiftTradeEvent setupForConfigurableRuleTest(IList<IBusinessRuleResponse> responses)
		{
			var ruleResponse1 = responses.First();
			var ruleResponse2 = responses.Second();
			var accept = getAcceptShiftTrade();

			var approvalService = MockRepository.GenerateMock<IRequestApprovalService>();
			var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();

			var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;
			validator.Stub(x => x.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
			personRepository.Stub(x => x.Get(accept.AcceptingPersonId)).Return(toPerson);
			approvalService.Stub(x => x.Approve(shiftTradeRequest))
				.Return(new List<IBusinessRuleResponse> { ruleResponse1, ruleResponse1, ruleResponse2 });
			requestFactory.Stub(x => x.GetRequestApprovalService(null, scenario, schedulingResultState, personRequest))
				.IgnoreArguments().Return(approvalService);
			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker(schedulingResultState)).Return(statusChecker);

			newBusinessRuleCollection.Add(new NewPersonAccountRule(null, null));
			((FakeNewBusinessRuleCollection)newBusinessRuleCollection).SetRuleResponse(responses);
			((FakeBusinessRuleProvider)businessRuleProvider).SetBusinessRules(newBusinessRuleCollection);
			return accept;
		}
	}
}