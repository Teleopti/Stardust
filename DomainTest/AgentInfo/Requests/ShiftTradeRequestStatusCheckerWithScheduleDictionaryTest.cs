using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.Services;


namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class ShiftTradeRequestStatusCheckerWithScheduleDictionaryTest
    {
        private ShiftTradeRequestStatusCheckerWithSchedule _target;
        private MockRepository _mockRepository;
        private IScenario _scenario;
        private IScheduleDictionary _scheduleDictionary;
        private IScheduleRange _scheduleRangePerson1;
        private IScheduleRange _scheduleRangePerson2;
        private IScheduleDay _schedulePart1;
        private IScheduleDay _schedulePart2;
        private IPerson _person1;
        private IPerson _person2;
        private IPersonRequest _personRequest2;
        private ISkill _skill;
        private IPersonRequestCheckAuthorization _authorization;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scenario = ScenarioFactory.CreateScenarioAggregate();
            
            _scheduleDictionary = _mockRepository.StrictMock<IScheduleDictionary>();
            _scheduleRangePerson1 = _mockRepository.StrictMock<IScheduleRange>();
            _scheduleRangePerson2 = _mockRepository.StrictMock<IScheduleRange>();
            _authorization = new PersonRequestAuthorizationCheckerForTest();
            _skill = SkillFactory.CreateSkill("test");
            _person1 = PersonFactory.CreatePerson();
            _person2 = PersonFactory.CreatePerson();
            
            _personRequest2 = new PersonRequest(_person2,
                                                              new ShiftTradeRequest(new List<IShiftTradeSwapDetail>{new ShiftTradeSwapDetail(_person2, _person1, new DateOnly(2009, 9, 21), new DateOnly(2009, 9, 21))}));
            var period = _personRequest2. Request.Period.ChangeEndTime(TimeSpan.FromHours(1));
            _schedulePart1 = new SchedulePartFactoryForDomain(_person1, _scenario, period, _skill).CreatePartWithMainShiftWithDifferentActivities();
            _schedulePart2 = new SchedulePartFactoryForDomain(_person2, _scenario, period, _skill).AddMeeting().CreatePartWithMainShiftWithDifferentActivities();

            _personRequest2.Pending();
            _target = new ShiftTradeRequestStatusCheckerWithSchedule(_scheduleDictionary,_authorization);
        }

        [Test]
        public void VerifyChangedScheduleForOwnerRefersRequest()
        {
            Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRangePerson1).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRangePerson2).Repeat.AtLeastOnce();
            Expect.Call(_scheduleRangePerson1.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart1).Repeat.
                AtLeastOnce();
            Expect.Call(_scheduleRangePerson2.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart2).Repeat.
                AtLeastOnce();

            _mockRepository.ReplayAll();

            IShiftTradeRequest shiftTradeRequest = (IShiftTradeRequest) _personRequest2.Request;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumFrom = new ShiftTradeChecksumCalculator(_schedulePart1).CalculateChecksum();
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumTo = 100;
            _target.Check(shiftTradeRequest);

            Assert.AreEqual(ShiftTradeStatus.Referred, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyChangedScheduleForRequestedRefersRequest()
        {
            Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRangePerson1).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRangePerson2).Repeat.AtLeastOnce();
            Expect.Call(_scheduleRangePerson1.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart1).Repeat.
                AtLeastOnce();
            Expect.Call(_scheduleRangePerson2.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart2).Repeat.
                AtLeastOnce();

            _mockRepository.ReplayAll();

            IShiftTradeRequest shiftTradeRequest = (IShiftTradeRequest)_personRequest2.Request;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumFrom = 200;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumTo = new ShiftTradeChecksumCalculator(_schedulePart2).CalculateChecksum();
            _target.Check(shiftTradeRequest);

            Assert.AreEqual(ShiftTradeStatus.Referred, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyNotChangedScheduleMakesNoStatusChange()
        {
            Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRangePerson1).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRangePerson2).Repeat.AtLeastOnce();
            Expect.Call(_scheduleRangePerson1.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart1).Repeat.
                AtLeastOnce();
            Expect.Call(_scheduleRangePerson2.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart2).Repeat.
                AtLeastOnce();

            _mockRepository.ReplayAll();

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

            Expect.Call(requestApprovalService.Approve(_personRequest2.Request)).Return(
                new List<IBusinessRuleResponse>());
            _mockRepository.ReplayAll();

            IShiftTradeRequest shiftTradeRequest = (IShiftTradeRequest)_personRequest2.Request;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumFrom = 100;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumTo = new ShiftTradeChecksumCalculator(_schedulePart2).CalculateChecksum();
            ((IShiftTradeRequest)_personRequest2.Request).Accept(_person1,new EmptyShiftTradeRequestSetChecksum(),_authorization);
            _personRequest2.Approve(requestApprovalService,_authorization);
            Assert.IsTrue(_personRequest2.IsApproved);
            _target.Check(shiftTradeRequest);

            Assert.AreEqual(ShiftTradeStatus.OkByBothParts, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));
            Assert.IsTrue(_personRequest2.IsApproved);

            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyAddRequestsToListForReset()
        {
            Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRangePerson1).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRangePerson2).Repeat.AtLeastOnce();
            Expect.Call(_scheduleRangePerson1.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart1).Repeat.
                AtLeastOnce();
            Expect.Call(_scheduleRangePerson2.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart2).Repeat.
                AtLeastOnce();

            _mockRepository.ReplayAll();

            IShiftTradeRequest shiftTradeRequest = (IShiftTradeRequest)_personRequest2.Request;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumFrom = 200;
            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumTo = new ShiftTradeChecksumCalculator(_schedulePart1).CalculateChecksum();
            shiftTradeRequest.Accept(_person1, new EmptyShiftTradeRequestSetChecksum(), _authorization);
            Assert.AreEqual(ShiftTradeStatus.OkByBothParts, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _target.Check(shiftTradeRequest);

            Assert.AreEqual(ShiftTradeStatus.Referred, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            shiftTradeRequest.ShiftTradeSwapDetails[0].ChecksumFrom = new ShiftTradeChecksumCalculator(_schedulePart2).CalculateChecksum();
            _target.Check(shiftTradeRequest);
            _target.ClearReferredShiftTradeRequests();

            Assert.AreEqual(ShiftTradeStatus.OkByBothParts, shiftTradeRequest.GetShiftTradeStatus(new ShiftTradeRequestStatusCheckerForTestDoesNothing()));

            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyNothingDoneWhenNoShiftTradesInList()
        {
            _mockRepository.ReplayAll();
            _target.Check(null);
            _mockRepository.VerifyAll();
        }
    }
}