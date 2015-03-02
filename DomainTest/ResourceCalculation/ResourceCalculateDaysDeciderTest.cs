using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class ResourceCalculateDaysDeciderTest
    {
        private IResourceCalculateDaysDecider _target;
        private MockRepository _mocks;
        private IScheduleDay _dayWithNotScheduled;
        private IScheduleDay _dayWithDayOff;
        private IScheduleDay _dayWithShift;
        private IScheduleDay _dayWithFullDayAbsence;
        private IScheduleDay _dayWithNightShift;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();

            _dayWithNotScheduled = _mocks.StrictMock<IScheduleDay>();
            _dayWithDayOff = _mocks.StrictMock<IScheduleDay>();
            _dayWithShift = _mocks.StrictMock<IScheduleDay>();
            _dayWithFullDayAbsence = _mocks.StrictMock<IScheduleDay>();
            _dayWithNightShift = _mocks.StrictMock<IScheduleDay>();

            _target = new ResourceCalculateDaysDecider();
        }

        private void mockExpectations()
        {
            TimeZoneInfo tz = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            ((IUnsafePerson)TeleoptiPrincipal.CurrentPrincipal).Person.PermissionInformation.SetDefaultTimeZone(tz);
            IPerson person = PersonFactory.CreatePerson();
            person.PermissionInformation.SetDefaultTimeZone(tz);
            //Winter time, this shift should not pass midnight in WesternEurope
            DateTimePeriod dayPeriod = new DateTimePeriod(new DateTime(2011, 1, 1, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 1, 1, 23, 0, 0, 0, DateTimeKind.Utc));
            //Summer time this shift should pass midnight
            DateTimePeriod nightPeriod = new DateTimePeriod(new DateTime(2011, 4, 1, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2011, 4, 1, 23, 0, 0, 0, DateTimeKind.Utc));
            IPersonAssignment dayAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, dayPeriod);
            IPersonAssignment nightAss = PersonAssignmentFactory.CreateAssignmentWithMainShift(person, nightPeriod);

            Expect.Call(_dayWithDayOff.SignificantPart()).Return(SchedulePartView.DayOff).Repeat.Any();
            Expect.Call(_dayWithDayOff.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_dayWithDayOff.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue, tz)).
                Repeat.Any();

            Expect.Call(_dayWithFullDayAbsence.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
            Expect.Call(_dayWithFullDayAbsence.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_dayWithFullDayAbsence.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue, tz)).
                Repeat.Any();

            Expect.Call(_dayWithShift.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_dayWithShift.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_dayWithShift.PersonAssignment()).Return(dayAss).Repeat.Any();
            Expect.Call(_dayWithShift.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue, tz)).
                Repeat.Any();

            Expect.Call(_dayWithNightShift.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
            Expect.Call(_dayWithNightShift.IsScheduled()).Return(true).Repeat.Any();
            Expect.Call(_dayWithNightShift.PersonAssignment()).Return(nightAss).Repeat.Any();
            Expect.Call(_dayWithNightShift.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue, tz)).
                Repeat.Any();

            Expect.Call(_dayWithNotScheduled.IsScheduled()).Return(false).Repeat.Any();
            Expect.Call(_dayWithNotScheduled.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
            Expect.Call(_dayWithNotScheduled.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(DateOnly.MinValue, tz)).
                Repeat.Any();
        }

        [Test]
        public void IfDayOffIsDeletedNoCalculationIsNeeded()
        {
            using(_mocks.Record())
            {
                mockExpectations();
            }
            IList<DateOnly> result;
            using(_mocks.Playback())
            {
                result = _target.DecideDates(_dayWithNotScheduled, _dayWithDayOff);
            }

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void IfMainShiftIsOverwrittenByDayOffAndMainShiftIsNotNightshiftOnlyOneCalculationIsNeeded()
        {
            using (_mocks.Record())
            {
                mockExpectations();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.DecideDates(_dayWithDayOff, _dayWithShift);
                Assert.AreEqual(1, result.Count);
                result = _target.DecideDates(_dayWithDayOff, _dayWithNightShift);
                Assert.AreEqual(2, result.Count);
            }
        }

        [Test]
        public void IfMainShiftIsOverwrittenByMainShiftAndNoMainShiftIsNightshiftOnlyOneCalculationIsNeeded()
        {
            using (_mocks.Record())
            {
                mockExpectations();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.DecideDates(_dayWithShift, _dayWithShift);
                Assert.AreEqual(1, result.Count);
                result = _target.DecideDates(_dayWithNightShift, _dayWithNightShift);
                Assert.AreEqual(2, result.Count);
                result = _target.DecideDates(_dayWithNightShift, _dayWithShift);
                Assert.AreEqual(2, result.Count);
                result = _target.DecideDates(_dayWithShift, _dayWithNightShift);
                Assert.AreEqual(2, result.Count);
            }
        }

        [Test]
        public void IfDayOffIsOverwrittenByShiftAndNoNightOnlyOneCalculationIsNeeded()
        {
            using (_mocks.Record())
            {
                mockExpectations();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.DecideDates(_dayWithShift, _dayWithDayOff);
                Assert.AreEqual(1, result.Count);
                result = _target.DecideDates(_dayWithNightShift, _dayWithNightShift);
                Assert.AreEqual(2, result.Count);
            }
        }

        [Test]
        public void IfNoNightshiftIsDeletedOnlyOneCalculationIsNeeded()
        {
            using (_mocks.Record())
            {
                mockExpectations();
            }
            IList<DateOnly> result;
            using (_mocks.Playback())
            {
                result = _target.DecideDates(_dayWithNotScheduled, _dayWithShift);
                Assert.AreEqual(1, result.Count);
                result = _target.DecideDates(_dayWithNotScheduled, _dayWithNightShift);
                Assert.AreEqual(2, result.Count);
            }
        }
    }
}