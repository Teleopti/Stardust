using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restrictions
{
    [TestFixture]
    public class PeriodScheduledAndRestrictionDaysOffTest
    {
        private IPeriodScheduledAndRestrictionDaysOff _target;
        private MockRepository _mocks;
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


        [SetUp]
        public void Setup()
        {
            _target = new PeriodScheduledAndRestrictionDaysOff();
            _mocks = new MockRepository();
            _matrix = _mocks.StrictMock<IScheduleMatrixPro>();
            _extractor = _mocks.StrictMock<IRestrictionExtractor>();
            _day1 = _mocks.StrictMock<IScheduleDayPro>();
            _day2 = _mocks.StrictMock<IScheduleDayPro>();
            _day3 = _mocks.StrictMock<IScheduleDayPro>();
            _day4 = _mocks.StrictMock<IScheduleDayPro>();
            _scheduleDay1 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mocks.StrictMock<IScheduleDay>();
            _scheduleDay4 = _mocks.StrictMock<IScheduleDay>();
            _preferenceRestriction = _mocks.StrictMock<IPreferenceRestriction>();
            _rotationRestriction = _mocks.StrictMock<IRotationRestriction>();
        }

        [Test]
        public void NotUsingSchedulesAndRestrictionsShouldReturnZero()
        {
            using (_mocks.Record())
            {
                commonMocks();
            }

            int result;

            using (_mocks.Playback())
            {
                result = _target.CalculatedDaysOff(_extractor, _matrix, false, false, false);
            }
            Assert.AreEqual(0, result);

        }

        [Test]
        public void OnlyCalculateScheduledIfUseSchedulingOnly()
        {
            using(_mocks.Record())
            {
                commonMocks();
            }

            int result;

            using(_mocks.Playback())
            {
                result = _target.CalculatedDaysOff(_extractor, _matrix, true, false, false);
            }
            Assert.AreEqual(2, result);
        }

        [Test]
        public void HandleSchedulesAndPreferences()
        {
            using (_mocks.Record())
            {
                commonMocks();
            }

            int result;

            using (_mocks.Playback())
            {
                result = _target.CalculatedDaysOff(_extractor, _matrix, true, true, false);
            }
            Assert.AreEqual(3, result);
        }

        [Test]
        public void HandleSchedulesAndPreferencesAndRotations()
        {
            using (_mocks.Record())
            {
                commonMocks();
            }

            int result;

            using (_mocks.Playback())
            {
                result = _target.CalculatedDaysOff(_extractor, _matrix, true, true, true);
            }
            Assert.AreEqual(3, result);
        }

        [Test]
        public void HandleSchedulesAndRotations()
        {
            using (_mocks.Record())
            {
                commonMocks();
            }

            int result;

            using (_mocks.Playback())
            {
                result = _target.CalculatedDaysOff(_extractor, _matrix, true, false, true);
            }
            Assert.AreEqual(3, result);
        }

        [Test]
        public void HandlePreferencesAndRotationsWithoutSchedules()
        {
            using (_mocks.Record())
            {
                commonMocks();
            }

            int result;

            using (_mocks.Playback())
            {
                result = _target.CalculatedDaysOff(_extractor, _matrix, false, true, true);
            }
            Assert.AreEqual(4, result);
        }

        [Test]
        public void ShouldCountPreferenceAbsenceOnContractNoWorkdayAsDayOff()
        {
            var person = _mocks.StrictMock<IPerson>();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var personContract = _mocks.StrictMock<IPersonContract>();
            var contractSchedule = _mocks.StrictMock<IContractSchedule>();
            var dateOnlyAsPeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();

            var dateOnly = new DateOnly();
            IList<IScheduleDayPro> days = new List<IScheduleDayPro> {_day4};


            using (_mocks.Record())
            {
                Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(days));
                Expect.Call(_day4.DaySchedulePart()).Return(_scheduleDay4);
                Expect.Call(_scheduleDay4.SignificantPart()).Return(SchedulePartView.None);
                Expect.Call(_scheduleDay4.IsScheduled()).Return(false).Repeat.Any();
                Expect.Call(_matrix.Person).Return(person).Repeat.Any();
                Expect.Call(_day4.Day).Return(dateOnly).Repeat.Any();
                Expect.Call(() => _extractor.Extract(person, dateOnly)).Repeat.Any();
                Expect.Call(_extractor.PreferenceList).Return(new List<IPreferenceRestriction> {_preferenceRestriction}).Repeat.Any();
                Expect.Call(_preferenceRestriction.DayOffTemplate).Return(null).Repeat.Any();
                Expect.Call(_preferenceRestriction.Absence).Return(new Absence()).Repeat.Any();
                Expect.Call(_scheduleDay4.Person).Return(person);
                Expect.Call(_scheduleDay4.DateOnlyAsPeriod).Return(dateOnlyAsPeriod);
                Expect.Call(dateOnlyAsPeriod.DateOnly).Return(dateOnly);
                Expect.Call(person.Period(dateOnly)).IgnoreArguments().Return(personPeriod);
                Expect.Call(personPeriod.PersonContract).Return(personContract);
                Expect.Call(personContract.ContractSchedule).Return(contractSchedule);
                Expect.Call(contractSchedule.IsWorkday(dateOnly, dateOnly)).IgnoreArguments().Return(false);
                Expect.Call(personPeriod.StartDate).Return(dateOnly);
            }

            using(_mocks.Playback())
            {
                var result = _target.CalculatedDaysOff(_extractor, _matrix, false, true, false);
                Assert.AreEqual(1, result);
            }
        }

        private void commonMocks()
        {
            IPerson person = PersonFactory.CreatePerson();
            DateOnly dateOnly = new DateOnly();
            IList<IScheduleDayPro> days = new List<IScheduleDayPro>{ _day1, _day2, _day3, _day4};
            Expect.Call(_matrix.EffectivePeriodDays).Return(new ReadOnlyCollection<IScheduleDayPro>(days));
            Expect.Call(_day1.DaySchedulePart()).Return(_scheduleDay1);
            Expect.Call(_day2.DaySchedulePart()).Return(_scheduleDay2);
            Expect.Call(_day3.DaySchedulePart()).Return(_scheduleDay3);
            Expect.Call(_day4.DaySchedulePart()).Return(_scheduleDay4);
            Expect.Call(_scheduleDay1.SignificantPart()).Return(SchedulePartView.DayOff);
            Expect.Call(_scheduleDay2.SignificantPart()).Return(SchedulePartView.ContractDayOff);
            Expect.Call(_scheduleDay3.SignificantPart()).Return(SchedulePartView.MainShift);
            Expect.Call(_scheduleDay4.SignificantPart()).Return(SchedulePartView.None);
            Expect.Call(_scheduleDay1.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_scheduleDay2.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_scheduleDay3.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_scheduleDay4.IsScheduled()).Return(false).Repeat.Any();
            Expect.Call(_matrix.Person).Return(person).Repeat.Any();
            Expect.Call(_day1.Day).Return(dateOnly).Repeat.Any();
            Expect.Call(_day2.Day).Return(dateOnly).Repeat.Any();
            Expect.Call(_day3.Day).Return(dateOnly).Repeat.Any();
            Expect.Call(_day4.Day).Return(dateOnly).Repeat.Any();
            Expect.Call(() => _extractor.Extract(person, dateOnly)).Repeat.Any();
            Expect.Call(_extractor.PreferenceList).Return(new List<IPreferenceRestriction> {_preferenceRestriction}).Repeat.Any();
            Expect.Call(_extractor.RotationList).Return(new List<IRotationRestriction> { _rotationRestriction }).Repeat.Any();
            Expect.Call(_preferenceRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description())).Repeat.Any();
            Expect.Call(_rotationRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description())).Repeat.Any();
        }
    }
}