using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class NewOverlappingAssignmentRuleTest
    {
        private NewOverlappingAssignmentRule _target;
        private MockRepository _mocks;
        private TimeZoneInfo _timeZone;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new NewOverlappingAssignmentRule();
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _person = PersonFactory.CreatePerson();
            _person.PermissionInformation.SetDefaultTimeZone(_timeZone);
        }

        [Test]
        public void CanAccessSimpleProperties()
        {
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            // ska man inte kunna ändra
            _target.HaltModify = false;
            Assert.IsTrue(_target.HaltModify);
            Assert.AreEqual("", _target.ErrorMessage);
        }

        [Test]
        public void OverlappingAssignmentsReturnsListOfErrorsWithOne()
        {
            var assignmentStart = new DateTime(2010, 8, 23, 8, 0, 0, DateTimeKind.Utc);
            var assignmentPeriod = new DateTimePeriod(assignmentStart, assignmentStart.AddHours(8));
            var dateOnly = new DateOnly(2010, 8, 23);
            var dateOnlyBefore = new DateOnly(2010, 8, 22);
            var dateOnlyAfter = new DateOnly(2010, 8, 24);
            var day = _mocks.StrictMock<IScheduleDay>();
            var dayBefore = _mocks.StrictMock<IScheduleDay>();
            var dayAfter = _mocks.StrictMock<IScheduleDay>();
            var days = new List<IScheduleDay> { day };
            var threeDays = new List<IScheduleDay> {dayBefore, day, dayAfter };
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> {{_person, range}};

            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, _timeZone);
            var ass = _mocks.StrictMock<IPersonAssignment>();

            Expect.Call(day.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(day.Person).Return(_person);
            Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(dateOnlyBefore, dateOnlyAfter))).Return(threeDays);
            Expect.Call(day.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment>{ass});
            Expect.Call(dayBefore.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment>());
            Expect.Call(dayAfter.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment>());
            Expect.Call(ass.Period).Return(assignmentPeriod);

            _mocks.ReplayAll();
            var ret = _target.Validate(dic, days);
            Assert.AreEqual(1,ret.Count());
            _mocks.VerifyAll();
        }

        [Test]
        public void NoOverlappingAssignmentsReturnsEmptyListOfErrors()
        {
            var dateOnly = new DateOnly(2010, 8, 23);
            var dateOnlyBefore = new DateOnly(2010, 8, 22);
            var dateOnlyAfter = new DateOnly(2010, 8, 24);
            var day = _mocks.StrictMock<IScheduleDay>();
            var dayBefore = _mocks.StrictMock<IScheduleDay>();
            var dayAfter = _mocks.StrictMock<IScheduleDay>();
            var days = new List<IScheduleDay> { day };
            var threeDays = new List<IScheduleDay> { dayBefore, day, dayAfter };
            var range = _mocks.StrictMock<IScheduleRange>();
            var dic = new Dictionary<IPerson, IScheduleRange> { { _person, range } };

            var dateOnlyAsDateTimePeriod = new DateOnlyAsDateTimePeriod(dateOnly, _timeZone);

            Expect.Call(day.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.AtLeastOnce();
            Expect.Call(day.Person).Return(_person);
            Expect.Call(range.ScheduledDayCollection(new DateOnlyPeriod(dateOnlyBefore, dateOnlyAfter))).Return(threeDays);
            Expect.Call(day.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment> ());
            Expect.Call(dayBefore.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment>());
            Expect.Call(dayAfter.PersonAssignmentConflictCollection).Return(new List<IPersonAssignment>());

            _mocks.ReplayAll();
            var ret = _target.Validate(dic, days);
            Assert.AreEqual(0, ret.Count());
            _mocks.VerifyAll();
        }
    }
}
