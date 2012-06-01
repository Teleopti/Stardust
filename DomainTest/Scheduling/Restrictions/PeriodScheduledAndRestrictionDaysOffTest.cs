using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class PeriodScheduledAndRestrictionDaysOffTest
    {
        private IScheduleMatrixPro _matrix;
        private IRestrictionExtractor _extractor;
        private IScheduleDayPro _day1;
        private IScheduleDayPro _day2;
        private IScheduleDayPro _day3;
        private IScheduleDayPro _day4;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;
        private IPreferenceRestriction _preferenceRestriction;
        private IRotationRestriction _rotationRestriction;
    	private IPerson _person;


    	[SetUp]
        public void Setup()
        {
			_person = PersonFactory.CreatePerson();

			_matrix = MockRepository.GenerateMock<IScheduleMatrixPro>();
			_extractor = MockRepository.GenerateMock<IRestrictionExtractor>();
			_day1 = MockRepository.GenerateMock<IScheduleDayPro>();
			_day2 = MockRepository.GenerateMock<IScheduleDayPro>();
			_day3 = MockRepository.GenerateMock<IScheduleDayPro>();
			_day4 = MockRepository.GenerateMock<IScheduleDayPro>();
        	var stubs = new StubFactory();
			var personAssignment = new PersonAssignment(_person, new Scenario(" "));
			personAssignment.SetMainShift(new MainShift(new ShiftCategory(" ")));
        	_scheduleDay1 = stubs.ScheduleDayStub(DateOnly.Today, _person, SchedulePartView.DayOff, stubs.PersonDayOffStub());
			_scheduleDay2 = stubs.ScheduleDayStub(DateOnly.Today, _person, SchedulePartView.ContractDayOff, personAssignment);
			_scheduleDay3 = stubs.ScheduleDayStub(DateOnly.Today, _person, SchedulePartView.MainShift, personAssignment);
			_scheduleDay4 = stubs.ScheduleDayStub(DateOnly.Today, _person);
			_preferenceRestriction = MockRepository.GenerateMock<IPreferenceRestriction>();
			_rotationRestriction = MockRepository.GenerateMock<IRotationRestriction>();
        }

        [Test]
        public void NotUsingSchedulesAndRestrictionsShouldReturnZero()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			stubCommonData();
            var result = target.CalculatedDaysOff(_extractor, _matrix, false, false, false);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void OnlyCalculateScheduledIfUseSchedulingOnly()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			stubCommonData();
            var result = target.CalculatedDaysOff(_extractor, _matrix, true, false, false);
            Assert.AreEqual(2, result);
        }

        [Test]
        public void HandleSchedulesAndPreferences()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			stubCommonData();
            var result = target.CalculatedDaysOff(_extractor, _matrix, true, true, false);
            Assert.AreEqual(3, result);
        }

        [Test]
        public void HandleSchedulesAndPreferencesAndRotations()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			stubCommonData();
            var result = target.CalculatedDaysOff(_extractor, _matrix, true, true, true);
            Assert.AreEqual(3, result);
        }

        [Test]
        public void HandleSchedulesAndRotations()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			stubCommonData();
            var result = target.CalculatedDaysOff(_extractor, _matrix, true, false, true);
            Assert.AreEqual(3, result);
        }

        [Test]
        public void HandlePreferencesAndRotationsWithoutSchedules()
        {
			var target = new PeriodScheduledAndRestrictionDaysOff();
			stubCommonData();
			var result = target.CalculatedDaysOff(_extractor, _matrix, false, true, true);
            Assert.AreEqual(4, result);
        }

        [Test]
        public void ShouldCountPreferenceAbsenceOnContractNoWorkdayAsDayOff()
        {
			var contractSchedule = ContractScheduleFactory.CreateContractScheduleWithoutWorkDays(" ");
        	var personContract = new PersonContract(new Contract(" "), new PartTimePercentage(" "), contractSchedule);
			var personPeriod = new PersonPeriod(DateOnly.Today, personContract, new Team());
			_person.AddPersonPeriod(personPeriod);
			var dateOnlyAsPeriod = new DateOnlyAsDateTimePeriod(DateOnly.Today, CccTimeZoneInfoFactory.StockholmTimeZoneInfo());
            IList<IScheduleDayPro> days = new List<IScheduleDayPro> {_day4};

            _matrix.Stub(x => x.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(days));
            _day4.Stub(x => x.DaySchedulePart()).Return(_scheduleDay4);
            _scheduleDay4.Stub(x => x.SignificantPart()).Return(SchedulePartView.None);
            _scheduleDay4.Stub(x => x.IsScheduled()).Return(false).Repeat.Any();
            _matrix.Stub(x => x.Person).Return(_person).Repeat.Any();
            _day4.Stub(x => x.Day).Return(DateOnly.Today).Repeat.Any();
            _extractor.Stub(x => x.PreferenceList).Return(new List<IPreferenceRestriction> {_preferenceRestriction}).Repeat.Any();
            _preferenceRestriction.Stub(x => x.DayOffTemplate).Return(null).Repeat.Any();
            _preferenceRestriction.Stub(x => x.Absence).Return(new Absence()).Repeat.Any();
            _scheduleDay4.Stub(x => x.Person).Return(_person);
            _scheduleDay4.Stub(x => x.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);

			var target = new PeriodScheduledAndRestrictionDaysOff();

			var result = target.CalculatedDaysOff(_extractor, _matrix, false, true, false);

            Assert.AreEqual(1, result);
        }

        private void stubCommonData()
        {
        	var date = DateOnly.Today;
            IList<IScheduleDayPro> days = new List<IScheduleDayPro>{ _day1, _day2, _day3, _day4};
			_matrix.Stub(x => x.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(days));
            _matrix.Stub(x => x.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(days));
            _day1.Stub(x => x.DaySchedulePart()).Return(_scheduleDay1);
            _day2.Stub(x => x.DaySchedulePart()).Return(_scheduleDay2);
            _day3.Stub(x => x.DaySchedulePart()).Return(_scheduleDay3);
            _day4.Stub(x => x.DaySchedulePart()).Return(_scheduleDay4);
            _matrix.Stub(x => x.Person).Return(_person).Repeat.Any();
            _day1.Stub(x => x.Day).Return(date).Repeat.Any();
            _day2.Stub(x => x.Day).Return(date).Repeat.Any();
            _day3.Stub(x => x.Day).Return(date).Repeat.Any();
            _day4.Stub(x => x.Day).Return(date).Repeat.Any();
            _extractor.Stub(x => x.PreferenceList).Return(new List<IPreferenceRestriction> {_preferenceRestriction}).Repeat.Any();
            _extractor.Stub(x => x.RotationList).Return(new List<IRotationRestriction> { _rotationRestriction }).Repeat.Any();
            _preferenceRestriction.Stub(x => x.DayOffTemplate).Return(new DayOffTemplate(new Description())).Repeat.Any();
            _rotationRestriction.Stub(x => x.DayOffTemplate).Return(new DayOffTemplate(new Description())).Repeat.Any();
        }
    }
}