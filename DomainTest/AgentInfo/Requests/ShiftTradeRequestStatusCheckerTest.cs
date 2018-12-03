using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class ShiftTradeRequestStatusCheckerTest
    {
        private ShiftTradeRequestStatusChecker _target;
        private MockRepository _mockRepository;
        private ICurrentScenario _scenarioRepository;
        private IScheduleStorage _scheduleStorage;
        private IScheduleDictionary _scheduleDictionary;
        private IScheduleRange _scheduleRangePerson1;
        private IScheduleRange _scheduleRangePerson2;
        private IScheduleDay _schedulePart1;
        private IScheduleDay _schedulePart2;
        private IPerson _person1;
        private IPerson _person2;
        private IPersonRequest _personRequest1;
        private IPersonRequest _personRequest2;
        private IPersonAssignment _personAssignment1;
        private IPersonAssignment _personAssignment2;
        private IPersonRequestCheckAuthorization _authorization;
        private IScenario _scenario;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scenarioRepository = _mockRepository.StrictMock<ICurrentScenario>();
            _scheduleStorage = _mockRepository.StrictMock<IScheduleStorage>();
            _authorization = new PersonRequestAuthorizationCheckerForTest();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            _target = new ShiftTradeRequestStatusChecker(_scenarioRepository, _scheduleStorage, _authorization);

            _scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
            _scheduleRangePerson1 = _mockRepository.StrictMock<IScheduleRange>();
            _scheduleRangePerson2 = _mockRepository.StrictMock<IScheduleRange>();
            _schedulePart1 = _mockRepository.StrictMock<IScheduleDay>();
            _schedulePart2 = _mockRepository.StrictMock<IScheduleDay>();
            _person1 = PersonFactory.CreatePerson();
            _person2 = PersonFactory.CreatePerson();

            _personRequest1 = new PersonRequest(_person1,
                                                              new TextRequest(new DateTimePeriod(2007, 6, 3, 2007, 7, 1)));
            _personRequest2 = new PersonRequest(_person2,
                                                              new ShiftTradeRequest(new List<IShiftTradeSwapDetail>{new ShiftTradeSwapDetail(_person2, _person1, new DateOnly(2009, 9, 21), new DateOnly(2009, 9, 21))}));
            _personAssignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person1,
                                                                                                  _scenario, _personRequest2.
		            Request.Period.ChangeEndTime(TimeSpan.FromHours(1)));
            _personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person2,
                                                                                                  _scenario, _personRequest2.
		            Request.Period.ChangeEndTime(TimeSpan.FromHours(1)));
            
        }

        private void SetupSchedule()
        {
            Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRangePerson1).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRangePerson2).Repeat.AtLeastOnce();
            Expect.Call(_scheduleRangePerson1.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart1).Repeat.
                AtLeastOnce();
            Expect.Call(_scheduleRangePerson2.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart2).Repeat.
                AtLeastOnce();
            Expect.Call(_schedulePart1.PersonAssignment()).Return(_personAssignment1).Repeat.AtLeastOnce();
            Expect.Call(_schedulePart2.PersonAssignment()).Return(_personAssignment2).Repeat.AtLeastOnce();
        }

        [Test]
        public void VerifyChangedScheduleForOwnerRefersRequest()
        {
            Expect.Call(_scenarioRepository.Current()).Return(_scenario);
            Expect.Call(_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { _person2, _person1 }, new ScheduleDictionaryLoadOptions(true, true), new DateOnlyPeriod(new DateOnly(_personRequest2.Request.Period.StartDateTime), new DateOnly(_personRequest2.Request.Period.EndDateTime.AddDays(1))), _scenario)).Return(_scheduleDictionary).IgnoreArguments();
            SetupSchedule();

            _mockRepository.ReplayAll();

            _personRequest2.Pending();
            IShiftTradeRequest shiftTradeRequest = (IShiftTradeRequest) _personRequest2.Request;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumFrom = -1 ^ 
                new ShiftTradeChecksumCalculator(_schedulePart1).CalculateChecksum();
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumTo = 100;
            _target.Check(shiftTradeRequest);

            Assert.AreEqual(ShiftTradeStatus.Referred, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyChangedScheduleForRequestedRefersRequest()
        {
            Expect.Call(_scenarioRepository.Current()).Return(_scenario);
			Expect.Call(_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(new[] { _person2, _person1 }, new ScheduleDictionaryLoadOptions(true, true), new DateOnlyPeriod(new DateOnly(_personRequest2.Request.Period.StartDateTime), new DateOnly(_personRequest2.Request.Period.EndDateTime.AddDays(1))), _scenario)).Return(_scheduleDictionary).IgnoreArguments();
            SetupSchedule();

            _mockRepository.ReplayAll();

            _personRequest2.Pending();
            IShiftTradeRequest shiftTradeRequest = (IShiftTradeRequest)_personRequest2.Request;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumFrom = 200;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumTo = -1 ^
                new ShiftTradeChecksumCalculator(_schedulePart2).CalculateChecksum();
            _target.Check(shiftTradeRequest);

            Assert.AreEqual(ShiftTradeStatus.Referred, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyNotChangedScheduleMakesNoStatusChange()
        {
            Expect.Call(_scenarioRepository.Current()).Return(_scenario);
	        Expect.Call(_scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
		        new[] {_person2, _person1},
		        new ScheduleDictionaryLoadOptions(true, true),
		        new DateOnlyPeriod(new DateOnly(_personRequest2.Request.Period.StartDateTime), new DateOnly(_personRequest2.Request.Period.EndDateTime.AddDays(1))),
		        _scenario)).Return(_scheduleDictionary).IgnoreArguments();
            SetupSchedule();

            _mockRepository.ReplayAll();

            _personRequest2.Pending();
            IShiftTradeRequest shiftTradeRequest = (IShiftTradeRequest)_personRequest2.Request;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumFrom = new ShiftTradeChecksumCalculator(_schedulePart2).CalculateChecksum();
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumTo = new ShiftTradeChecksumCalculator(_schedulePart1).CalculateChecksum();
            _target.Check(shiftTradeRequest);

            Assert.AreEqual(_schedulePart2, shiftTradeRequest.ShiftTradeSwapDetails[0].SchedulePartFrom);
            Assert.AreEqual(_schedulePart1, shiftTradeRequest.ShiftTradeSwapDetails[0].SchedulePartTo);
            Assert.AreEqual(ShiftTradeStatus.OkByMe, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyNothingHappensToApprovedRequest()
        {
            IRequestApprovalService requestApprovalService = _mockRepository.StrictMock<IRequestApprovalService>();

            Expect.Call(_schedulePart2.PersonAssignment()).Return(_personAssignment2).Repeat.AtLeastOnce();
            Expect.Call(requestApprovalService.Approve(_personRequest2.Request)).Return(
                new List<IBusinessRuleResponse>());
            _mockRepository.ReplayAll();

            IShiftTradeRequest shiftTradeRequest = (IShiftTradeRequest)_personRequest2.Request;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumFrom = 100;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumTo = -1 ^
                new ShiftTradeChecksumCalculator(_schedulePart2).CalculateChecksum();
            ((IShiftTradeRequest)_personRequest2.Request).Accept(_person1, new EmptyShiftTradeRequestSetChecksum(), _authorization);
            _personRequest2.Pending();
            _personRequest2.Approve(requestApprovalService,_authorization);
            Assert.IsTrue(_personRequest2.IsApproved);
            _target.Check(shiftTradeRequest);

            Assert.AreEqual(ShiftTradeStatus.OkByBothParts, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
            Assert.IsTrue(_personRequest2.IsApproved);

            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyNothingDoneWhenNoShiftTradesInList()
        {
            _mockRepository.ReplayAll();
            _personRequest2.Request = new TextRequest(_personAssignment1.Period);
            _target.StartBatch(new List<IPersonRequest> { _personRequest1, _personRequest2 });
            _target.Check(null);
            _target.EndBatch();
            _mockRepository.VerifyAll();
        }
    }
}