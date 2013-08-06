﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo.Requests
{
    [TestFixture]
    public class ShiftTradeRequestSetChecksumTest
    {
        private ShiftTradeRequestSetChecksum _target;
        private MockRepository _mockRepository;
        private IScenario _scenario;
        private IScheduleRepository _scheduleRepository;
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

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _scenario = _mockRepository.StrictMock<IScenario>();
				var scenarioRepository = MockRepository.GenerateMock<IScenarioRepository>();
				scenarioRepository.Expect(s => s.LoadDefaultScenario()).Return(_scenario).Repeat.Any();
            _scheduleRepository = _mockRepository.StrictMock<IScheduleRepository>();
			_target = new ShiftTradeRequestSetChecksum(new DefaultScenarioFromRepository(scenarioRepository), _scheduleRepository);

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
            _personAssignment1 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person1,
                                                                                                  _personRequest2.
                                                                                                  Request.Period.ChangeEndTime(TimeSpan.FromHours(1)));
            _personAssignment2 = PersonAssignmentFactory.CreateAssignmentWithMainShift(_scenario, _person2,
                                                                                                  _personRequest2.
                                                                                                  Request.Period.ChangeEndTime(TimeSpan.FromHours(1)));
            
        }

        [Test]
        public void VerifyCanFindScheduleAndSetChecksums()
        {
            Expect.Call(_scheduleRepository.FindSchedulesOnlyInGivenPeriod(new PersonProvider(new[] { _person2, _person1 }), new ScheduleDictionaryLoadOptions(true, true), _personRequest2.Request.Period.ChangeEndTime(TimeSpan.FromHours(25)).ChangeStartTime(TimeSpan.FromHours(-25)), _scenario)).Return(_scheduleDictionary).IgnoreArguments();
            Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRangePerson1).Repeat.AtLeastOnce();
            Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRangePerson2).Repeat.AtLeastOnce();
            Expect.Call(_scheduleRangePerson1.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart1).Repeat.
                AtLeastOnce();
            Expect.Call(_scheduleRangePerson2.ScheduledDay(new DateOnly(2009, 9, 21))).Return(_schedulePart2).Repeat.
                AtLeastOnce();
            Expect.Call(_schedulePart1.PersonAssignmentCollectionDoNotUse()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignment1 })).Repeat.AtLeastOnce();
            Expect.Call(_schedulePart2.PersonAssignmentCollectionDoNotUse()).Return(new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { _personAssignment2 })).Repeat.AtLeastOnce();
            Expect.Call(_schedulePart1.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();
            Expect.Call(_schedulePart2.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.AtLeastOnce();

            _mockRepository.ReplayAll();
            Assert.AreEqual(0,((IShiftTradeRequest)_personRequest2.Request).ShiftTradeSwapDetails[0].ChecksumFrom);
            Assert.AreEqual(0, ((IShiftTradeRequest)_personRequest2.Request).ShiftTradeSwapDetails[0].ChecksumTo);
            _target.SetChecksum(_personRequest2.Request);
            Assert.AreNotEqual(0, ((IShiftTradeRequest)_personRequest2.Request).ShiftTradeSwapDetails[0].ChecksumFrom);
            Assert.AreNotEqual(0, ((IShiftTradeRequest)_personRequest2.Request).ShiftTradeSwapDetails[0].ChecksumTo);
            Assert.AreEqual(_schedulePart2, ((IShiftTradeRequest)_personRequest2.Request).ShiftTradeSwapDetails[0].SchedulePartFrom);
            Assert.AreEqual(_schedulePart1, ((IShiftTradeRequest)_personRequest2.Request).ShiftTradeSwapDetails[0].SchedulePartTo);
            _mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyCannotSupplyNullOrWrongRequestType()
        {
            _mockRepository.ReplayAll();
            _target.SetChecksum(null);
            _target.SetChecksum(_personRequest1.Request);
            _mockRepository.VerifyAll();
        }
    }
}