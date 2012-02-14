using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class GapsInAssignmentRuleTest
    {
        private GapsInAssignmentRule _target;
        private MockRepository _mocks;
        private IGapsInAssignment _restrictionChecker;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _restrictionChecker = _mocks.StrictMock<IGapsInAssignment>();
            _target = new GapsInAssignmentRule(_restrictionChecker);
        }

        [Test]
        public void VerifySimpleProperties()
        {
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IsMandatory);
        }

        [Test]
        public void VerifyHaltModifyAlwaysTrueAndCannotBeChange()
        {
            Assert.IsTrue(_target.HaltModify);
            _target.HaltModify = false;
            Assert.IsTrue(_target.HaltModify);
        }

        [Test]
        public void VerifyForDeleteAlwaysTrueAndCannotBeChange()
        {
            Assert.IsTrue(_target.ForDelete);
            _target.ForDelete = false;
            Assert.IsTrue(_target.ForDelete);
        }

        [Test]
        public void ShouldReturnEmptyListWhenGapNotThrows()
        {
            var person = new Person();
            var dic = _mocks.StrictMock<IDictionary<IPerson, IScheduleRange>>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var ass = _mocks.StrictMock<IPersonAssignment>();
            var personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> {ass});
            var lstOfDays = new List<IScheduleDay> { scheduleDay };

            Expect.Call(scheduleDay.Person).Return(person);
            Expect.Call(scheduleDay.PersonAssignmentCollection()).Return(personAssignments);
            Expect.Call(() => _restrictionChecker.CheckEntity(ass));
            _mocks.ReplayAll();
            var result = _target.Validate(dic, lstOfDays);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, new List<IBusinessRuleResponse>(result).Count);
            _mocks.VerifyAll();
        }
        
        [Test]
        public void ShouldReturnListWhenGapThrows()
        {
            var person = new Person();
            var period = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            var dateonly = new DateOnly(2011, 5, 30);
            var dic = _mocks.StrictMock<IDictionary<IPerson, IScheduleRange>>();
            var scheduleDay = _mocks.StrictMock<IScheduleDay>();
            var ass = _mocks.StrictMock<IPersonAssignment>();
            var personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment> { ass });
            var lstOfDays = new List<IScheduleDay> { scheduleDay };

            Expect.Call(scheduleDay.Person).Return(person);
            Expect.Call(scheduleDay.PersonAssignmentCollection()).Return(personAssignments);
            Expect.Call(() => _restrictionChecker.CheckEntity(ass)).Throw(new ValidationException());
            Expect.Call(scheduleDay.DateOnlyAsPeriod).Return(period);
            Expect.Call(period.DateOnly).Return(dateonly);
            _mocks.ReplayAll();
            var result = _target.Validate(dic, lstOfDays);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, new List<IBusinessRuleResponse>(result).Count);
            _mocks.VerifyAll();
        }

    }
}
