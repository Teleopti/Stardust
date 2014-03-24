using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
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
        private IProjectionService _projectionService1;
        private IProjectionService _projectionService2;
        private IVisualLayerCollection _visualLayerCollection;
        private IPermissionInformation _permissionInformaion;

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
            _projectionService1 = _mock.StrictMock<IProjectionService>();
            _projectionService2 = _mock.StrictMock<IProjectionService>();
            _permissionInformaion = _mock.StrictMock<IPermissionInformation>();
            _visualLayerCollection = _mock.StrictMock<IVisualLayerCollection>();
            _workTimeStartEndExtractor = _mock.StrictMock<IWorkTimeStartEndExtractor>();
            _dayOffMaxFlexCalculator = _mock.StrictMock<IDayOffMaxFlexCalculator>();
            _target = new EnsureWeeklyRestRule(_workTimeStartEndExtractor, _dayOffMaxFlexCalculator);
        }

        [Test]
        public void ReturnTrueIfNoScheduleDayFound()
        {
            var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
            var personWeek = new PersonWeek(_person, week);

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
            var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
            var personWeek = new PersonWeek(_person, week);

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
            var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
            var personWeek = new PersonWeek(_person, week);

            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week))
                    .IgnoreArguments()
                    .Return(new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 });
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
                Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment2);

                Expect.Call(_person.PermissionInformation).Return(_permissionInformaion).Repeat.Twice() ;
                Expect.Call(_permissionInformaion.DefaultTimeZone()).Return(TimeZoneInfo.Utc).Repeat.Twice() ;

                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 18))).Return(_scheduleDay3);
                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 17))).Return(_scheduleDay4);
                Expect.Call(_dayOffMaxFlexCalculator.MaxFlex(_scheduleDay3, _scheduleDay4))
                    .IgnoreArguments()
                    .Return(null);
                mockForPersonAssignment(_personAssignment1, _projectionService1, new DateTime(2014, 03, 19, 5, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 19, 16, 0, 0, DateTimeKind.Utc));
                mockForPersonAssignment(_personAssignment2, _projectionService2, new DateTime(2014, 03, 20, 5, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 20, 16, 0, 0, DateTimeKind.Utc));

                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 21))).Return(_scheduleDay3);
                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 22))).Return(_scheduleDay4);
                Expect.Call(_dayOffMaxFlexCalculator.MaxFlex(_scheduleDay3, _scheduleDay4))
                    .IgnoreArguments()
                    .Return(null);
            }
            using (_mock.Playback())
            {
                var result = _target.HasMinWeeklyRest(personWeek, _currentSchedules, TimeSpan.FromHours(40));
                Assert.IsFalse(result);
            }
        }

        [Test]
        public void ReturnTrueIfTheWeekHasValidWeeklyRest()
        {
            var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
            var personWeek = new PersonWeek(_person, week);

            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week))
                    .IgnoreArguments()
                    .Return(new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 });
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
                Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment2);

                Expect.Call(_person.PermissionInformation).Return(_permissionInformaion);
                Expect.Call(_permissionInformaion.DefaultTimeZone()).Return(TimeZoneInfo.Utc);

                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 18))).Return(_scheduleDay3);
                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 17))).Return(_scheduleDay4);
                Expect.Call(_dayOffMaxFlexCalculator.MaxFlex(_scheduleDay3, _scheduleDay4))
                    .IgnoreArguments()
                    .Return(null);
                mockForPersonAssignment(_personAssignment1, _projectionService1, new DateTime(2014, 03, 19, 5, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 19, 16, 0, 0, DateTimeKind.Utc));

                Expect.Call(_personAssignment2.ProjectionService()).Return(_projectionService2);
                Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection);
                Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(_visualLayerCollection)).IgnoreArguments().Return(new DateTime(2014, 03, 20, 05, 0, 0));
            }
            using (_mock.Playback())
            {
                var result = _target.HasMinWeeklyRest(personWeek, _currentSchedules, TimeSpan.FromHours(10));
                Assert.IsTrue( result);
            }
        }

        [Test]
        public void ReturnTrueIfTheWeekHasUnscheduleDay()
        {
            var week = new DateOnlyPeriod(2014, 03, 19, 2014, 03, 20);
            var personWeek = new PersonWeek(_person, week);

            using (_mock.Record())
            {
                Expect.Call(_currentSchedules.ScheduledDayCollection(week))
                    .IgnoreArguments()
                    .Return(new List<IScheduleDay>() { _scheduleDay1, _scheduleDay2 });
                Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
                Expect.Call(_scheduleDay2.PersonAssignment()).Return(null);

                Expect.Call(_person.PermissionInformation).Return(_permissionInformaion).Repeat.Twice();
                Expect.Call(_permissionInformaion.DefaultTimeZone()).Return(TimeZoneInfo.Utc).Repeat.Twice();

                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 18))).Return(_scheduleDay3);
                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 17))).Return(_scheduleDay4);
                Expect.Call(_dayOffMaxFlexCalculator.MaxFlex(_scheduleDay3, _scheduleDay4))
                    .IgnoreArguments()
                    .Return(null);
                //iterating on PA
                mockForPersonAssignment(_personAssignment1, _projectionService1, new DateTime(2014, 03, 19, 5, 0, 0, DateTimeKind.Utc), new DateTime(2014, 03, 19, 16, 0, 0, DateTimeKind.Utc));

                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 21))).Return(_scheduleDay3);
                Expect.Call(_currentSchedules.ScheduledDay(new DateOnly(2014, 03, 22))).Return(_scheduleDay4);
                Expect.Call(_dayOffMaxFlexCalculator.MaxFlex(_scheduleDay3, _scheduleDay4))
                    .IgnoreArguments()
                    .Return(null);
            }
            using (_mock.Playback())
            {
                var result = _target.HasMinWeeklyRest(personWeek, _currentSchedules, TimeSpan.FromHours(40));
                Assert.IsTrue(result);
            }
        }

        private void mockForPersonAssignment(IPersonAssignment personAssignment, IProjectionService projectionService, DateTime? dateTimeFrom, DateTime? dateTimeTo)
        {
            Expect.Call(personAssignment.ProjectionService()).Return(projectionService);
            Expect.Call(projectionService.CreateProjection()).Return(_visualLayerCollection);
            Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(_visualLayerCollection))
                .IgnoreArguments()
                .Return(dateTimeFrom);
            Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(_visualLayerCollection))
                .IgnoreArguments()
                .Return(dateTimeTo);
        }
    }

   
}
