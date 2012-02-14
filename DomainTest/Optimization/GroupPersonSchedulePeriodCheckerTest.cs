using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupPersonSchedulePeriodCheckerTest
    {
        private MockRepository _mock;
        private GroupPersonSchedulePeriodChecker _target;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new GroupPersonSchedulePeriodChecker();
        }

        [Test]
        public void ShouldReturnFalseIfMemberIsNull()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var members = new  ReadOnlyCollection<IPerson>(new List<IPerson> {null});
            Expect.Call(groupPerson.GroupMembers).Return(members);
            _mock.ReplayAll();
            Assert.That(_target.AllInSameGroupHasSameSchedulePeriod(groupPerson, new List<DateOnly>()),Is.False);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfLessThanTwoDays()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var person = _mock.StrictMock<IPerson>();
            var members = new ReadOnlyCollection<IPerson>(new List<IPerson> { person });
            Expect.Call(groupPerson.GroupMembers).Return(members);
            _mock.ReplayAll();
            Assert.That(_target.AllInSameGroupHasSameSchedulePeriod(groupPerson, new List<DateOnly>()), Is.False);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfNotSameSchedulePeriod()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var person = _mock.StrictMock<IPerson>();
            var members = new ReadOnlyCollection<IPerson>(new List<IPerson> { person });
            var dateOne = new DateOnly(2011, 9, 20);
            var dateTwo = new DateOnly(2011, 9, 21);
            var schedulePeriodOne = _mock.StrictMock<ISchedulePeriod>();
            var schedulePeriodTwo = _mock.StrictMock<ISchedulePeriod>();

            Expect.Call(groupPerson.GroupMembers).Return(members);
            Expect.Call(person.SchedulePeriod(dateOne)).Return(schedulePeriodOne);
            Expect.Call(person.SchedulePeriod(dateTwo)).Return(schedulePeriodTwo);
            Expect.Call(schedulePeriodTwo.Equals(schedulePeriodOne)).Return(false);
            _mock.ReplayAll();
            Assert.That(_target.AllInSameGroupHasSameSchedulePeriod(groupPerson, new List<DateOnly>{dateOne, dateTwo}), Is.False);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfSameSchedulePeriod()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var person = _mock.StrictMock<IPerson>();
            var members = new ReadOnlyCollection<IPerson>(new List<IPerson> { person });
            var dateOne = new DateOnly(2011, 9, 20);
            var dateTwo = new DateOnly(2011, 9, 21);
            var schedulePeriodOne = _mock.StrictMock<ISchedulePeriod>();
            
            Expect.Call(groupPerson.GroupMembers).Return(members);
            Expect.Call(person.SchedulePeriod(dateOne)).Return(schedulePeriodOne);
            Expect.Call(person.SchedulePeriod(dateTwo)).Return(schedulePeriodOne);
            Expect.Call(schedulePeriodOne.Equals(schedulePeriodOne)).Return(true);
            _mock.ReplayAll();
            Assert.That(_target.AllInSameGroupHasSameSchedulePeriod(groupPerson, new List<DateOnly> { dateOne, dateTwo }), Is.True);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfSchedulePeriodNull()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var person = _mock.StrictMock<IPerson>();
            var members = new ReadOnlyCollection<IPerson>(new List<IPerson> { person });
            var dateOne = new DateOnly(2011, 9, 20);
            var dateTwo = new DateOnly(2011, 9, 21);
            
            Expect.Call(groupPerson.GroupMembers).Return(members);
            Expect.Call(person.SchedulePeriod(dateOne)).Return(null);

            _mock.ReplayAll();
            Assert.That(_target.AllInSameGroupHasSameSchedulePeriod(groupPerson, new List<DateOnly> { dateOne, dateTwo }), Is.False);
            _mock.VerifyAll();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfNoGroupPerson()
        {
           _target.AllInSameGroupHasSameSchedulePeriod(null, new List<DateOnly> ());
        }
    }

    
}