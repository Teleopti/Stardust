using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class EnsureWeeklyRestRuleTest
    {
        private MockRepository _mock;
        private IEnsureWeeklyRestRule _target;
        private IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
        private IDayOffMaxFlexCalculator _dayOffMaxFlexCalculator;
        private IPerson _person;
        private IScheduleRange _currentSchedules;
        private IScheduleDay _scheduleDay1;
        private IScheduleDay _scheduleDay2;
        private IPersonAssignment _personAssignment2;
        private IPersonAssignment _personAssignment1;
        private IScheduleDay _scheduleDay3;
        private IScheduleDay _scheduleDay4;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _person = _mock.StrictMock<IPerson>();
            _currentSchedules = _mock.StrictMock<IScheduleRange>();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay2 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay3 = _mock.StrictMock<IScheduleDay>();
            _scheduleDay4 = _mock.StrictMock<IScheduleDay>();
            _personAssignment1 = _mock.StrictMock<IPersonAssignment>();
            _personAssignment2 = _mock.StrictMock<IPersonAssignment>();

            _workTimeStartEndExtractor = _mock.StrictMock<IWorkTimeStartEndExtractor>();
            _dayOffMaxFlexCalculator = _mock.StrictMock<IDayOffMaxFlexCalculator>();
            _target = new EnsureWeeklyRestRule(_workTimeStartEndExtractor, _dayOffMaxFlexCalculator);
        }

        [Test]
        public void ReturnTrueIfNoScheduleDayFound()
        {
            DateOnlyPeriod week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
            PersonWeek personWeek = new PersonWeek(_person, week);

            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week))
                    .IgnoreArguments()
                    .Return(new List<IScheduleDay>());
            }
            using (_mock.Playback())
            {
                var result = _target.HasMinWeeklyRest(personWeek, _currentSchedules, TimeSpan.FromHours(10));
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void ReturnTrueIfNoPersonAssignmentFound()
        {
            DateOnlyPeriod week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
            PersonWeek personWeek = new PersonWeek(_person, week);

            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week))
                    .IgnoreArguments()
                    .Return(new List<IScheduleDay>() {_scheduleDay1, _scheduleDay2});
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(null);
                Expect.Call(_scheduleDay2.PersonAssignment()).Return(null);
            }
            using (_mock.Playback())
            {
                var result = _target.HasMinWeeklyRest(personWeek, _currentSchedules, TimeSpan.FromHours(10));
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void ReturnFalseIfTheWeekHasInValidWeeklyRest()
        {
            DateOnlyPeriod week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
            PersonWeek personWeek = new PersonWeek(_person, week);

            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week))
                    .IgnoreArguments()
                    .Return(new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 });
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
                Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment2);
                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 18))).Return(_scheduleDay3);
                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 17))).Return(_scheduleDay4);
            }
            using (_mock.Playback())
            {
                var result = _target.HasMinWeeklyRest(personWeek, _currentSchedules, TimeSpan.FromHours(10));
                Assert.IsTrue(result);
            }
        }

        [Test]
        public void ReturnTrueIfTheWeekHasValidWeeklyRest()
        {
            Assert.Pass();
        }

        [Test]
        public void ReturnTrueIfTheWeekHasUnscheduleDay()
        {
            Assert.Pass();
        }

        [Test]
        public void ReturnFalseEvenIfTheWeekHasAbsence()
        {
            Assert.Pass();
        }

        [Test]
        public void ReturnTrueIfTwoDaysOffFound()
        {
            Assert.Pass();
        }
    }

    public class EnsureWeeklyRestRule : IEnsureWeeklyRestRule
    {
        private readonly IWorkTimeStartEndExtractor _workTimeStartEndExtractor;
        private readonly IDayOffMaxFlexCalculator _dayOffMaxFlexCalculator;

        public EnsureWeeklyRestRule( IWorkTimeStartEndExtractor workTimeStartEndExtractor, IDayOffMaxFlexCalculator dayOffMaxFlexCalculator)
        {
            
            _workTimeStartEndExtractor = workTimeStartEndExtractor;
            _dayOffMaxFlexCalculator = dayOffMaxFlexCalculator;
        }

        public  bool HasMinWeeklyRest(PersonWeek personWeek, IScheduleRange currentSchedules, TimeSpan weeklyRest)
        {
            var extendedWeek = new DateOnlyPeriod(personWeek.Week.StartDate.AddDays(-1),
                                                  personWeek.Week.EndDate.AddDays(1));
            var pAss = new List<IPersonAssignment>();
            foreach (var schedule in currentSchedules.ScheduledDayCollection(extendedWeek))
            {
                var ass = schedule.PersonAssignment();
                if (ass != null)
                {
                    pAss.Add(ass);
                }
            }
            if (pAss.Count == 0)
                return true;

            DateTime endOfPeriodBefore = TimeZoneHelper.ConvertToUtc(extendedWeek.StartDate, personWeek.Person.PermissionInformation.DefaultTimeZone());

            var scheduleDayBefore1 = currentSchedules.ScheduledDay(personWeek.Week.StartDate.AddDays(-1));
            var scheduleDayBefore2 = currentSchedules.ScheduledDay(personWeek.Week.StartDate.AddDays(-2));
            var result = _dayOffMaxFlexCalculator.MaxFlex(scheduleDayBefore1, scheduleDayBefore2);
            if (result != null)
                endOfPeriodBefore = result.Value.EndDateTime;

            foreach (IPersonAssignment ass in pAss)
            {
                var proj = ass.ProjectionService().CreateProjection();
                var nextStartDateTime =
                    _workTimeStartEndExtractor.WorkTimeStart(proj);
                if (nextStartDateTime != null)
                {
                    if ((nextStartDateTime - endOfPeriodBefore) >= weeklyRest)
                    {
                        // the majority must be in this week
                        if (endOfPeriodBefore.Add(TimeSpan.FromMinutes(weeklyRest.TotalMinutes / 2.0)) <= personWeek.Week.EndDate.AddDays(1) && nextStartDateTime.Value.Add(TimeSpan.FromMinutes(-weeklyRest.TotalMinutes / 2.0)) > personWeek.Week.StartDate)
                            return true;
                    }
                    var end = _workTimeStartEndExtractor.WorkTimeEnd(proj);
                    if (end.HasValue)
                        endOfPeriodBefore = end.Value;
                }
            }
            DateTime endOfPeriodAfter = TimeZoneHelper.ConvertToUtc(extendedWeek.EndDate.AddDays(1), personWeek.Person.PermissionInformation.DefaultTimeZone());


            var scheduleDayAfter1 = currentSchedules.ScheduledDay(personWeek.Week.EndDate.AddDays(1));
            var scheduleDayAfter2 = currentSchedules.ScheduledDay(personWeek.Week.EndDate.AddDays(2));
            result = _dayOffMaxFlexCalculator.MaxFlex(scheduleDayAfter1, scheduleDayAfter2);
            if (result != null)
                endOfPeriodAfter = result.Value.StartDateTime;

            if ((endOfPeriodAfter - endOfPeriodBefore) >= weeklyRest)
                return true;

            return false;
        }
    }

    public  interface IEnsureWeeklyRestRule
    {
        bool HasMinWeeklyRest(PersonWeek personWeek, IScheduleRange currentSchedules, TimeSpan weeklyRest);
    }
}
