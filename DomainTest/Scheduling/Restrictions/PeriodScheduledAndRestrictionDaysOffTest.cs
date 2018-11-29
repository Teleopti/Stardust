using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class PeriodScheduledAndRestrictionDaysOffTest
    {
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;
        private IPreferenceRestriction _preferenceRestriction;
        private IRotationRestriction _rotationRestriction;
    	private IPerson _person;
		private IEnumerable<IScheduleDay> _scheduleDays;


    	[SetUp]
        public void Setup()
        {
			_person = PersonFactory.CreatePerson();

			_preferenceRestriction = MockRepository.GenerateMock<IPreferenceRestriction>();
			_rotationRestriction = MockRepository.GenerateMock<IRotationRestriction>();
			_preferenceRestriction.Stub(x => x.DayOffTemplate).Return(new DayOffTemplate(new Description())).Repeat.Any();
			_rotationRestriction.Stub(x => x.DayOffTemplate).Return(new DayOffTemplate(new Description())).Repeat.Any();
		
			var restrictions = new IRestrictionBase[] { _preferenceRestriction, _rotationRestriction };

			var stubs = new StubFactory();


    		var personAssignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(_person,
    		                                                                         new DateTimePeriod(2013, 1, 1, 8,  2013, 1, 1, 9));

        	_scheduleDay1 = stubs.ScheduleDayStub(DateTime.Today, _person, SchedulePartView.DayOff, PersonAssignmentFactory.CreateAssignmentWithDayOff());
			_scheduleDay1.Stub(x => x.RestrictionCollection()).Return(restrictions);
			_scheduleDay1.Stub(x => x.IsScheduled()).Return(true);
			_scheduleDay2 = stubs.ScheduleDayStub(DateTime.Today, _person, SchedulePartView.ContractDayOff, personAssignment);
			_scheduleDay2.Stub(x => x.RestrictionCollection()).Return(restrictions);
			_scheduleDay2.Stub(x => x.IsScheduled()).Return(true);
			_scheduleDay3 = stubs.ScheduleDayStub(DateTime.Today, _person, SchedulePartView.MainShift, personAssignment);
			_scheduleDay3.Stub(x => x.RestrictionCollection()).Return(restrictions);
			_scheduleDay3.Stub(x => x.IsScheduled()).Return(true);
			_scheduleDay4 = stubs.ScheduleDayStub(DateTime.Today, _person);
			_scheduleDay4.Stub(x => x.RestrictionCollection()).Return(restrictions);
			_scheduleDay4.Stub(x => x.IsScheduled()).Return(false);

    		_scheduleDays = new[] {_scheduleDay1, _scheduleDay2, _scheduleDay3, _scheduleDay4};
        }

        [Test]
        public void NotUsingSchedulesAndRestrictionsShouldReturnZero()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
            var result = target.CalculatedDaysOff(_scheduleDays, false, false, false);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void OnlyCalculateScheduledIfUseSchedulingOnly()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			var result = target.CalculatedDaysOff(_scheduleDays, true, false, false);
            Assert.AreEqual(2, result);
        }

        [Test]
        public void HandleSchedulesAndPreferences()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			var result = target.CalculatedDaysOff(_scheduleDays, true, true, false);
            Assert.AreEqual(3, result);
        }

        [Test]
        public void HandleSchedulesAndPreferencesAndRotations()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			var result = target.CalculatedDaysOff(_scheduleDays, true, true, true);
            Assert.AreEqual(3, result);
        }

        [Test]
        public void HandleSchedulesAndRotations()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			var result = target.CalculatedDaysOff(_scheduleDays, true, false, true);
            Assert.AreEqual(3, result);
        }

        [Test]
        public void HandlePreferencesAndRotationsWithoutSchedules()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			var result = target.CalculatedDaysOff(_scheduleDays, false, true, true);
            Assert.AreEqual(4, result);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void ShouldCountPreferenceAbsenceOnContractNoWorkdayAsDayOff()
        {
			var contractSchedule = ContractScheduleFactory.CreateContractScheduleWithoutWorkDays("contractSchedule");
        	var personContract = new PersonContract(new Contract("contract"), new PartTimePercentage("PTP"), contractSchedule);
			var personPeriod = new PersonPeriod(DateOnly.Today, personContract, new Team());
			_person.AddPersonPeriod(personPeriod);

			var preferenceRestriction = MockRepository.GenerateMock<IPreferenceRestriction>();
			var rotationRestriction = MockRepository.GenerateMock<IRotationRestriction>();
			preferenceRestriction.Stub(x => x.DayOffTemplate).Return(null).Repeat.Any();
			preferenceRestriction.Stub(x => x.Absence).Return(new Absence()).Repeat.Any();

			var scheduleDay = new StubFactory().ScheduleDayStub(DateTime.Today, _person);
			scheduleDay.Stub(x => x.RestrictionCollection()).Return(new IRestrictionBase[] { preferenceRestriction, rotationRestriction });
			scheduleDay.Stub(x => x.IsScheduled()).Return(false);

			var target = new PeriodScheduledAndRestrictionDaysOff();

			var result = target.CalculatedDaysOff(new[] { scheduleDay }, false, true, false);

            Assert.AreEqual(1, result);
        }
    }
}