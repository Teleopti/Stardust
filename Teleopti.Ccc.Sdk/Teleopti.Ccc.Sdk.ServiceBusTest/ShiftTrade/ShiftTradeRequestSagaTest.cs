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
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Infrastructure.Persisters;
using Teleopti.Ccc.Sdk.ServiceBus;
using Teleopti.Ccc.Sdk.ServiceBus.ShiftTrade;
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
        private readonly MockRepository mocker = new MockRepository();
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
        private IScheduleRepository scheduleRepository;
        private ISchedulingResultStateHolder schedulingResultState;
        private IScenario scenario;
        private IRequestFactory requestFactory;
        private IScheduleDictionarySaver scheduleDictionarySaver;
        private IPersonRequestCheckAuthorization personRequestCheckAuthorization;
    	private ILoadSchedulesForRequestWithoutResourceCalculation loader;

    	[SetUp]
        public void Setup()
        {
            CreatePersonAndScenario();
            CreateRequest();
            CreateRepositories();

            scheduleDictionarySaver = mocker.StrictMock<IScheduleDictionarySaver>();
						unitOfWorkFactory = mocker.StrictMock<ICurrentUnitOfWorkFactory>();
            requestFactory = mocker.StrictMock<IRequestFactory>();
            personRequestCheckAuthorization = new PersonRequestAuthorizationCheckerForTest();
            schedulingResultState = new SchedulingResultStateHolder();
            schedulingResultState.Schedules = mocker.DynamicMock<IScheduleDictionary>();
            unitOfWork = mocker.StrictMock<IUnitOfWork>();
            loader = mocker.DynamicMock<ILoadSchedulesForRequestWithoutResourceCalculation>();
            
            target = new ShiftTradeRequestSaga(unitOfWorkFactory, schedulingResultState, validator, requestFactory, scenarioRepository, personRequestRepository, scheduleRepository, personRepository, personRequestCheckAuthorization, scheduleDictionarySaver, loader);
        }

        private void CreateRepositories()
        {
            validator = mocker.StrictMock<IShiftTradeValidator>();
            personRequestRepository = mocker.StrictMock<IPersonRequestRepository>();
            personRepository = mocker.StrictMock<IPersonRepository>();
            scenarioRepository = mocker.StrictMock<ICurrentScenario>();
            scheduleRepository = mocker.StrictMock<IScheduleRepository>();
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
            prepareUnitOfWork(1, true);
            var statusChecker = mocker.StrictMock<IShiftTradeRequestStatusChecker>();

            using (mocker.Record())
            {
                Expect.Call(validator.Validate((IShiftTradeRequest)personRequest.Request)).Return(new ShiftTradeRequestValidationResult(true));
                Expect.Call(personRequestRepository.Get(created.PersonRequestId)).Return(personRequest);
                Expect.Call(scenarioRepository.Current()).Return(scenario).Repeat.AtLeastOnce();
                Expect.Call(() => loader.Execute(scenario, new DateTimePeriod(), null)).IgnoreArguments();
                Expect.Call(requestFactory.GetShiftTradeRequestStatusChecker()).Return(
                          statusChecker);
                Expect.Call(() => statusChecker.Check((IShiftTradeRequest)personRequest.Request));
            }

            using (mocker.Playback())
            {
                target.Consume(created);
                Assert.AreEqual(false, personRequest.IsNew);
                Assert.AreEqual(true, personRequest.IsPending);
            }
        }

        [Test]
        public void CanConsumeNewValidateIsFalse()
        {
            var created = getNewShiftTradeRequestCreated();
            prepareUnitOfWork(1, true);
            var statusChecker = mocker.StrictMock<IShiftTradeRequestStatusChecker>();

            using (mocker.Record())
            {
                ExpectCreationOfCommonRepositories(created.PersonRequestId);
                Expect.Call(validator.Validate((IShiftTradeRequest)personRequest.Request)).Return(new ShiftTradeRequestValidationResult(false));
                Expect.Call(() => loader.Execute(scenario, new DateTimePeriod(), null)).IgnoreArguments();
                Expect.Call(requestFactory.GetShiftTradeRequestStatusChecker()).Return(
                    statusChecker);
                Expect.Call(() => statusChecker.Check((IShiftTradeRequest)personRequest.Request));
            }

            using (mocker.Playback())
            {
                target.Consume(created);
                Assert.AreEqual(true, personRequest.IsDenied);
            }
        }

        private void ExpectCreationOfCommonRepositories(Guid requestId)
        {
            Expect.Call(personRequestRepository.Get(requestId)).Return(personRequest);
            Expect.Call(scenarioRepository.Current()).Return(scenario).Repeat.AtLeastOnce();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanConsumeAcceptValidIsTrue()
        {
            var accept = getAcceptShiftTrade();

            var approvalService = mocker.StrictMock<IRequestApprovalService>();
            var statusChecker = mocker.StrictMock<IShiftTradeRequestStatusChecker>();
            var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;
            INewBusinessRuleCollection ruleCollection = null;

            using (mocker.Record())
            {
                prepareUnitOfWork(1, true);

                ExpectCreationOfCommonRepositories(accept.PersonRequestId);
                Expect.Call(validator.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
                Expect.Call(personRepository.Get(accept.AcceptingPersonId)).Return(toPerson);
                
                ////Argh can't beleve this mocking!!! This is so anoying, have to set up all this crap to make the Swap work
                Expect.Call(() => loader.Execute(scenario, new DateTimePeriod(), null)).IgnoreArguments();
                Expect.Call(requestFactory.GetRequestApprovalService(null, null)).Constraints(new[]{Is.Matching<NewBusinessRuleCollection>(
                    b =>
                        { ruleCollection = b;
                            return true;
                        }),Is.Equal(scenario)}).Return(approvalService);
                Expect.Call(approvalService.ApproveShiftTrade(shiftTradeRequest)).Return(
                    new List<IBusinessRuleResponse>());
				Expect.Call(scheduleDictionarySaver.MarkForPersist(unitOfWork, scheduleRepository, schedulingResultState.Schedules.DifferenceSinceSnapshot())).Return(new ScheduleDictionaryPersisterResult { ModifiedEntities = new IPersistableScheduleData[] { }, AddedEntities = new IPersistableScheduleData[] { }, DeletedEntities = new IPersistableScheduleData[] { } });
                Expect.Call(requestFactory.GetShiftTradeRequestStatusChecker()).Return(
                    statusChecker);
                Expect.Call(() => statusChecker.Check(shiftTradeRequest)).Repeat.Twice();
            }

            using (mocker.Playback())
            {
                target.Consume(accept);
                Assert.AreEqual(false, personRequest.IsNew);
                Assert.AreEqual(false, personRequest.IsPending);
                Assert.AreEqual(true, personRequest.IsApproved);
                Assert.AreEqual(ShiftTradeStatus.OkByBothParts, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
				Assert.AreEqual(accept.Message, personRequest.GetMessage(new NoFormatting()));
                ruleCollection.Item(typeof (NewPersonAccountRule)).HaltModify.Should().Be.False();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void CanConsumeAcceptValidIsTrueAndShutdownNiceUponValidationExceptionWhenSavingSchedules()
        {
            var accept = getAcceptShiftTrade();

            var approvalService = mocker.StrictMock<IRequestApprovalService>();
            var statusChecker = mocker.StrictMock<IShiftTradeRequestStatusChecker>();
            var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;

            using (mocker.Record())
            {
                prepareUnitOfWork(1, false);

                ExpectCreationOfCommonRepositories(accept.PersonRequestId);
                Expect.Call(validator.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
                Expect.Call(personRepository.Get(accept.AcceptingPersonId)).Return(toPerson);

                Expect.Call(() => loader.Execute(scenario, new DateTimePeriod(), null)).IgnoreArguments();
                Expect.Call(requestFactory.GetRequestApprovalService(null, scenario)).IgnoreArguments().Return(
                    approvalService);
                Expect.Call(approvalService.ApproveShiftTrade(shiftTradeRequest)).Return(
                    new List<IBusinessRuleResponse>());
				Expect.Call(scheduleDictionarySaver.MarkForPersist(unitOfWork, scheduleRepository, schedulingResultState.Schedules.DifferenceSinceSnapshot())).Throw(new ValidationException());
                Expect.Call(requestFactory.GetShiftTradeRequestStatusChecker()).Return(
                    statusChecker);
                Expect.Call(() => statusChecker.Check(shiftTradeRequest));
            }

            using (mocker.Playback())
            {
                target.Consume(accept);
                Assert.AreEqual(false, personRequest.IsNew);
                Assert.AreEqual(false, personRequest.IsPending);
                Assert.AreEqual(true, personRequest.IsApproved);
                Assert.AreEqual(ShiftTradeStatus.OkByBothParts, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
				Assert.AreEqual(accept.Message, personRequest.GetMessage(new NoFormatting()));
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldKeepAsPendingWhenBusinessRulesFail()
        {
            var accept = getAcceptShiftTrade();

            var approvalService = mocker.StrictMock<IRequestApprovalService>();
            var statusChecker = mocker.StrictMock<IShiftTradeRequestStatusChecker>();
            var rule = mocker.StrictMock<IBusinessRuleResponse>();
            var shiftTradeRequest = (IShiftTradeRequest)personRequest.Request;

            using (mocker.Record())
            {
                prepareUnitOfWork(1, true);

                Expect.Call(validator.Validate(shiftTradeRequest)).Return(new ShiftTradeRequestValidationResult(true));
                ExpectCreationOfCommonRepositories(accept.PersonRequestId);
                Expect.Call(personRepository.Get(accept.AcceptingPersonId)).Return(toPerson);
                
                ////Argh can't beleve this mocking!!! This is so anoying, have to set up all this crap to make the Swap work
                Expect.Call(() => loader.Execute(scenario, new DateTimePeriod(), null)).IgnoreArguments();
                Expect.Call(requestFactory.GetRequestApprovalService(null, scenario)).IgnoreArguments().Return(
                    approvalService);
                Expect.Call(approvalService.ApproveShiftTrade(shiftTradeRequest)).Return(
                    new List<IBusinessRuleResponse>{rule,rule});
				Expect.Call(scheduleDictionarySaver.MarkForPersist(unitOfWork, scheduleRepository, schedulingResultState.Schedules.DifferenceSinceSnapshot())).Return(new ScheduleDictionaryPersisterResult { ModifiedEntities = new IPersistableScheduleData[] { }, AddedEntities = new IPersistableScheduleData[] { }, DeletedEntities = new IPersistableScheduleData[] { } });
                Expect.Call(requestFactory.GetShiftTradeRequestStatusChecker()).Return(
                    statusChecker);
                Expect.Call(() => statusChecker.Check(shiftTradeRequest)).Repeat.Twice();
                Expect.Call(rule.Message).Return("aja baja!").Repeat.AtLeastOnce();
            }

            using (mocker.Playback())
            {
                target.Consume(accept);
                Assert.AreEqual(false, personRequest.IsNew);
                Assert.AreEqual(true, personRequest.IsPending);
                Assert.AreEqual(false, personRequest.IsApproved);
                Assert.AreEqual(ShiftTradeStatus.OkByBothParts, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
				personRequest.GetMessage(new NoFormatting()).Should().Be.EqualTo("I want to trade!\r\nViolation of a Business Rule:\r\naja baja!\r\n");
            }
        }

        [Test]
        public void DoNotProcessAlreadyHandledAcceptMessages()
        {
            var accept = getAcceptShiftTrade();

            personRequest.Pending();
            personRequest.Deny(null, "DenyReason", new PersonRequestAuthorizationCheckerForTest());

            using (mocker.Record())
            {
                prepareUnitOfWork(1, false);
                Expect.Call(personRequestRepository.Get(accept.PersonRequestId)).Return(personRequest);
            }

            using (mocker.Playback())
            {
                target.Consume(accept);
            }
        }

        [Test]
        public void DoNotProcessAlreadyHandledCreatedMessages()
        {
            var shiftTradeRequestCreated = getNewShiftTradeRequestCreated();
            
            personRequest.Pending();
            personRequest.Deny(null, "DenyReason", new PersonRequestAuthorizationCheckerForTest());

            using (mocker.Record())
            {
                prepareUnitOfWork(1, false);
                Expect.Call(personRequestRepository.Get(shiftTradeRequestCreated.PersonRequestId)).Return(personRequest);
            }

            using (mocker.Playback())
            {
                target.Consume(shiftTradeRequestCreated);
            }
        }

        private void prepareUnitOfWork(int times, bool persistAll)
        {
	        var uowFactory = mocker.DynamicMock<IUnitOfWorkFactory>();
	        Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(uowFactory);
					Expect.Call(uowFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork).Repeat.Times(times);
            Expect.Call(unitOfWork.Dispose).Repeat.Times(times);
            if (persistAll)
            {
                Expect.Call(unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>()).Repeat.Times(times);
            }
        }

        private static NewShiftTradeRequestCreated getNewShiftTradeRequestCreated()
        {
            var nstrc = new NewShiftTradeRequestCreated();
            nstrc.Datasource = "V7Config";
            nstrc.BusinessUnitId = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA");
            nstrc.Timestamp = DateTime.UtcNow;
            nstrc.PersonRequestId = new Guid("9AC8476B-9B8F-4330-9561-9D7A00BAA585");
            return nstrc;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Messages.Requests.AcceptShiftTrade.set_Message(System.String)")]
        private static AcceptShiftTrade getAcceptShiftTrade()
        {
            var ast = new AcceptShiftTrade();
            ast.Datasource = "V7Config";
            ast.BusinessUnitId = new Guid("928DD0BC-BF40-412E-B970-9B5E015AADEA");
            ast.Timestamp = DateTime.UtcNow;
            ast.PersonRequestId = new Guid("9AC8476B-9B8F-4330-9561-9D7A00BAA585");
            ast.Message = "I want to trade!";
            return ast;
        }
    }
}
