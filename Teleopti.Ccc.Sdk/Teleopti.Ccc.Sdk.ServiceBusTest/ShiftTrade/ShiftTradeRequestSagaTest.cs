using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.Persisters.Schedules;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.ShiftTrade;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Requests;
using Is = Rhino.Mocks.Constraints.Is;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.ShiftTrade
{
    [TestFixture]
    public class ShiftTradeRequestSagaTest
    {
        private ShiftTradeRequestSaga target;
        private IShiftTradeValidator validator;
        private IUnitOfWork unitOfWork;
        private IPersonRequestRepository personRequestRepository;
        private IPersonRepository personRepository;
        private IPersonRequest personRequest;
	    private ICurrentUnitOfWorkFactory unitOfWorkFactory;
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

	    [SetUp]
	    public void Setup()
	    {
		    CreatePersonAndScenario();
		    CreateRequest();
		    CreateRepositories();

		    scheduleDictionarySaver = MockRepository.GenerateMock<IScheduleDifferenceSaver>();
		    differenceCollectionService = MockRepository.GenerateMock<IDifferenceCollectionService<IPersistableScheduleData>>();
		    unitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
		    requestFactory = MockRepository.GenerateMock<IRequestFactory>();
		    personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerForTest();
		    schedulingResultState = new SchedulingResultStateHolder();
		    var scheduleRanges = new Dictionary<IPerson, IScheduleRange>();
		    schedulingResultState.Schedules = new ScheduleDictionaryForTest(scenario,new ScheduleDateTimePeriod(new DateTimePeriod()),scheduleRanges);
			scheduleRanges.Add(fromPerson,new ScheduleRange(schedulingResultState.Schedules,new ScheduleParameters(scenario,fromPerson,new DateTimePeriod())));
			scheduleRanges.Add(toPerson,new ScheduleRange(schedulingResultState.Schedules,new ScheduleParameters(scenario,toPerson,new DateTimePeriod())));
		    unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
		    loader = MockRepository.GenerateMock<ILoadSchedulesForRequestWithoutResourceCalculation>();

		    target = new ShiftTradeRequestSaga(unitOfWorkFactory, schedulingResultState, validator, requestFactory,
		                                       scenarioRepository, personRequestRepository, scheduleStorage,
		                                       personRepository, personRequestCheckAuthorization, scheduleDictionarySaver,
		                                       loader, differenceCollectionService, null);
		    prepareUnitOfWork();
	    }

	    private void CreateRepositories()
        {
            validator = MockRepository.GenerateMock<IShiftTradeValidator>();
            personRequestRepository = MockRepository.GenerateMock<IPersonRequestRepository>();
            personRepository = MockRepository.GenerateMock<IPersonRepository>();
            scenarioRepository = MockRepository.GenerateMock<ICurrentScenario>();
            scheduleStorage = MockRepository.GenerateMock<IScheduleStorage>();
        }

        private void CreateRequest()
        {
            var shiftTradeSwapDetail = new ShiftTradeSwapDetail(fromPerson, toPerson, new DateOnly(2008, 12, 23), new DateOnly(2008, 12, 24));
            shiftTradeSwapDetail.ChecksumFrom = -349792281;
            shiftTradeSwapDetail.ChecksumTo = -349792281;

            var shiftTradeRequest = new ShiftTradeRequest(new List<IShiftTradeSwapDetail> { shiftTradeSwapDetail });
            personRequest = new PersonRequest(fromPerson, shiftTradeRequest);
        }

        private void CreatePersonAndScenario()
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

		    validator.Stub(x=> x.Validate((IShiftTradeRequest) personRequest.Request)).Return(new ShiftTradeRequestValidationResult(true));
		    personRequestRepository.Stub(x => x.Get(created.PersonRequestId)).Return(personRequest);
		    scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker()).Return(statusChecker);
			
		    target.Consume(created);
		    Assert.AreEqual(false, personRequest.IsNew);
		    Assert.AreEqual(true, personRequest.IsPending);
			unitOfWork.AssertWasCalled(x => x.PersistAll());
	    }

	    [Test]
	    public void CanConsumeNewValidateIsFalse()
	    {
		    var created = getNewShiftTradeRequestCreated();
		    var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();

			validator.Stub(x => x.Validate((IShiftTradeRequest)personRequest.Request)).Return(new ShiftTradeRequestValidationResult(false));
			personRequestRepository.Stub(x => x.Get(created.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker()).Return(statusChecker);
			
		    target.Consume(created);
		    Assert.AreEqual(true, personRequest.IsDenied);
			unitOfWork.AssertWasCalled(x => x.PersistAll());
	    }

	    [Test]
	    public void CanConsumeAcceptValidIsTrue()
	    {
		    var accept = getAcceptShiftTrade();

		    var approvalService = MockRepository.GenerateMock<IRequestApprovalService>();
		    var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
		    var shiftTradeRequest = (IShiftTradeRequest) personRequest.Request;
		    INewBusinessRuleCollection ruleCollection = null;

			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
		    validator.Stub(x => x.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
		    personRepository.Stub(x => x.Get(accept.AcceptingPersonId)).Return(toPerson);

		    requestFactory.Stub(x => x.GetRequestApprovalService(null, null)).Constraints(new[]
			    {
				    Is.Matching<NewBusinessRuleCollection>(
					    b =>
						    {
							    ruleCollection = b;
							    return true;
						    }),
				    Is.Equal(scenario)
			    }).Return(approvalService);
		    approvalService.Stub(x => x.ApproveShiftTrade(shiftTradeRequest)).Return(new List<IBusinessRuleResponse>());
		    requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker()).Return(statusChecker);

		    target.Consume(accept);
		    Assert.AreEqual(false, personRequest.IsNew);
		    Assert.AreEqual(false, personRequest.IsPending);
		    Assert.AreEqual(true, personRequest.IsApproved);
		    Assert.AreEqual(ShiftTradeStatus.OkByBothParts, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
		    Assert.AreEqual(accept.Message, personRequest.GetMessage(new NoFormatting()));
		    ruleCollection.Item(typeof (NewPersonAccountRule)).HaltModify.Should().Be.False();
			unitOfWork.AssertWasCalled(x => x.PersistAll());
			statusChecker.AssertWasCalled(x => x.Check(shiftTradeRequest), o => o.Repeat.Twice());
			scheduleDictionarySaver.AssertWasCalled(x => x.SaveChanges(null, null), o => o.IgnoreArguments());
			loader.AssertWasCalled(x => x.Execute(scenario, new DateTimePeriod(), null), o => o.IgnoreArguments());
	    }

	    [Test]
	    public void CanConsumeAcceptValidIsTrueAndShutdownNiceUponValidationExceptionWhenSavingSchedules()
	    {
		    var accept = getAcceptShiftTrade();

		    var approvalService = MockRepository.GenerateMock<IRequestApprovalService>();
		    var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
		    var shiftTradeRequest = (IShiftTradeRequest) personRequest.Request;

			validator.Stub(x => x.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
			personRepository.Stub(x => x.Get(accept.AcceptingPersonId)).Return(toPerson);
			approvalService.Stub(x => x.ApproveShiftTrade(shiftTradeRequest)).Return(new List<IBusinessRuleResponse>());
			requestFactory.Stub(x => x.GetRequestApprovalService(null, scenario)).IgnoreArguments().Return(approvalService);
			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker()).Return(statusChecker);
			scheduleDictionarySaver.Stub(x => x.SaveChanges(null, null)).IgnoreArguments().Throw(new ValidationException());

		    target.Consume(accept);
		    Assert.AreEqual(false, personRequest.IsNew);
		    Assert.AreEqual(false, personRequest.IsPending);
		    Assert.AreEqual(true, personRequest.IsApproved);
		    Assert.AreEqual(ShiftTradeStatus.OkByBothParts,
		                    shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
			Assert.AreEqual(accept.Message, personRequest.GetMessage(new NoFormatting()));
			scheduleDictionarySaver.AssertWasCalled(x => x.SaveChanges(null, null), o => o.IgnoreArguments());
			unitOfWork.AssertWasNotCalled(x => x.PersistAll());
	    }

	    [Test]
	    public void ShouldKeepAsPendingWhenBusinessRulesFail()
	    {
		    var accept = getAcceptShiftTrade();

		    var approvalService = MockRepository.GenerateMock<IRequestApprovalService>();
		    var statusChecker = MockRepository.GenerateMock<IShiftTradeRequestStatusChecker>();
		    var rule = MockRepository.GenerateMock<IBusinessRuleResponse>();
		    var shiftTradeRequest = (IShiftTradeRequest) personRequest.Request;

		    validator.Stub(x => x.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
			personRepository.Stub(x => x.Get(accept.AcceptingPersonId)).Return(toPerson);
		    approvalService.Stub(x => x.ApproveShiftTrade(shiftTradeRequest))
		                   .Return(new List<IBusinessRuleResponse> {rule, rule});
			requestFactory.Stub(x => x.GetRequestApprovalService(null, scenario)).IgnoreArguments().Return(approvalService);
			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);
			scenarioRepository.Stub(x => x.Current()).Return(scenario);
			requestFactory.Stub(x => x.GetShiftTradeRequestStatusChecker()).Return(statusChecker);
			rule.Stub(x => x.Message).Return("aja baja!");

			target.Consume(accept);
		    Assert.AreEqual(false, personRequest.IsNew);
		    Assert.AreEqual(true, personRequest.IsPending);
		    Assert.AreEqual(false, personRequest.IsApproved);
		    Assert.AreEqual(ShiftTradeStatus.OkByBothParts,
		                    shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
		    personRequest.GetMessage(new NoFormatting())
		                 .Should()
		                 .Be.EqualTo("I want to trade!\r\nViolation of a Business Rule:\r\naja baja!\r\n");
			unitOfWork.AssertWasCalled(x => x.PersistAll());
	    }

	    [Test]
	    public void DoNotProcessAlreadyHandledAcceptMessages()
	    {
		    var accept = getAcceptShiftTrade();

		    personRequest.Pending();
		    personRequest.Deny(null, "DenyReason", new PersonRequestAuthorizationCheckerForTest());
			personRequestRepository.Stub(x => x.Get(accept.PersonRequestId)).Return(personRequest);

		    target.Consume(accept);

			unitOfWork.AssertWasNotCalled(x => x.PersistAll());
	    }

	    [Test]
	    public void DoNotProcessAlreadyHandledCreatedMessages()
	    {
		    var shiftTradeRequestCreated = getNewShiftTradeRequestCreated();

		    personRequest.Pending();
		    personRequest.Deny(null, "DenyReason", new PersonRequestAuthorizationCheckerForTest());
		    personRequestRepository.Stub(x => x.Get(shiftTradeRequestCreated.PersonRequestId)).Return(personRequest);

		    target.Consume(shiftTradeRequestCreated);

			unitOfWork.AssertWasNotCalled(x => x.PersistAll());
	    }

	    private void prepareUnitOfWork()
        {
	        var uowFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
	        unitOfWorkFactory.Stub(x => x.Current()).Return(uowFactory);
		    uowFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
        }

        private static NewShiftTradeRequestCreated getNewShiftTradeRequestCreated()
        {
            var nstrc = new NewShiftTradeRequestCreated();
            nstrc.LogOnDatasource = "V7Config";
            nstrc.LogOnBusinessUnitId = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA");
            nstrc.Timestamp = DateTime.UtcNow;
            nstrc.PersonRequestId = new Guid("9AC8476B-9B8F-4330-9561-9D7A00BAA585");
            return nstrc;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Messages.Requests.AcceptShiftTrade.set_Message(System.String)")]
        private static AcceptShiftTrade getAcceptShiftTrade()
        {
            var ast = new AcceptShiftTrade();
            ast.LogOnDatasource = "V7Config";
            ast.LogOnBusinessUnitId = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA");
            ast.Timestamp = DateTime.UtcNow;
            ast.PersonRequestId = new Guid("9AC8476B-9B8F-4330-9561-9D7A00BAA585");
            ast.Message = "I want to trade!";
            return ast;
        }
    }
}
