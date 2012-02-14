using System.Collections.ObjectModel;
using NUnit.Framework;
using System.Collections.Generic;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupPersonSameDayOffsCheckerTest
    {
        private MockRepository _mock;
        private GroupPersonSameDayOffsChecker _target;
        private IGroupPerson _groupPerson;
        private List<DateOnly> _daysOffToRemove;
        private List<DateOnly> _daysOffToAdd;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _daysOffToRemove = new List<DateOnly>();
            _daysOffToAdd = new List<DateOnly>();
            _mock = new MockRepository();
            _groupPerson = _mock.StrictMock<IGroupPerson>();
            _person = _mock.StrictMock<IPerson>();
            _target = new GroupPersonSameDayOffsChecker();
        }

        [Test]
        public void ShouldReturnFalseIfAnyParameterIsNull()
        {
            Assert.That(_target.CheckGroupPerson(null,_groupPerson, _daysOffToRemove, _daysOffToAdd), Is.False);
            var dic = new List<IScheduleMatrixPro>();
            Assert.That(_target.CheckGroupPerson(dic, null, _daysOffToRemove, _daysOffToAdd), Is.False);
            Assert.That(_target.CheckGroupPerson(dic, _groupPerson, null, _daysOffToAdd), Is.False);
            Assert.That(_target.CheckGroupPerson(dic, _groupPerson, _daysOffToRemove, null), Is.False);
        }

        [Test]
        public void ShouldReturnFalseIfDateListsCountDiffer()
        {
            var dic = new List<IScheduleMatrixPro>();
            _daysOffToAdd.Add(new DateOnly(2011,9,20));
            Assert.That(_target.CheckGroupPerson(dic, _groupPerson, _daysOffToRemove, _daysOffToAdd), Is.False);
        }

        [Test]
        public void ShouldReturnFalseIfOneDateListIsEmpty()
        {
            var dic = new List<IScheduleMatrixPro>();
            Assert.That(_target.CheckGroupPerson(dic, _groupPerson, _daysOffToRemove, _daysOffToAdd), Is.False);
        }

        [Test]
        public void ShouldReturnFalseIfOnePersonDoesNotHaveDayOff()
        {
            var dateToRemove = new DateOnly(2011, 9, 20);
            var dateToAdd = new DateOnly(2011, 9, 21);
            _daysOffToRemove.Add(dateToRemove);
            _daysOffToAdd.Add(dateToAdd);
            var matrix = _mock.StrictMock<IScheduleMatrixPro>();
            var scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            var scheduleDay = _mock.StrictMock<IScheduleDay>();
            var dic = new List<IScheduleMatrixPro> {matrix};
            var schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();

            Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person }));
            Expect.Call(matrix.Person).Return(_person);
            Expect.Call(matrix.SchedulePeriod).Return(schedulePeriod);
            Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateToRemove, dateToRemove));
            Expect.Call(matrix.GetScheduleDayByKey(dateToRemove)).Return(scheduleDayPro);
            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
            _mock.ReplayAll();
            Assert.That(_target.CheckGroupPerson(dic, _groupPerson, _daysOffToRemove, _daysOffToAdd), Is.False);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfOnePersonDoesHaveDayOffOnMoveTo()
        {
            var dateToRemove = new DateOnly(2011, 9, 20);
            var dateToAdd = new DateOnly(2011, 9, 21);
            _daysOffToRemove.Add(dateToRemove);
            _daysOffToAdd.Add(dateToAdd);
            var matrix = _mock.StrictMock<IScheduleMatrixPro>();
            var scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            var scheduleDay = _mock.StrictMock<IScheduleDay>();
            var dic = new List<IScheduleMatrixPro> { matrix };
            var schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();

            Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {_person}));
            Expect.Call(matrix.Person).Return(_person);
            Expect.Call(matrix.SchedulePeriod).Return(schedulePeriod);
            Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateToRemove, dateToRemove));
            Expect.Call(matrix.GetScheduleDayByKey(dateToRemove)).Return(scheduleDayPro);
            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);

            Expect.Call(matrix.GetScheduleDayByKey(dateToAdd)).Return(scheduleDayPro);
            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);
            _mock.ReplayAll();
            Assert.That(_target.CheckGroupPerson(dic, _groupPerson, _daysOffToRemove, _daysOffToAdd), Is.False);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfMatrixNotFound()
        {
            var dateToRemove = new DateOnly(2011, 9, 20);
            var dateToAdd = new DateOnly(2011, 9, 21);
            _daysOffToRemove.Add(dateToRemove);
            _daysOffToAdd.Add(dateToAdd);
            var matrix = _mock.StrictMock<IScheduleMatrixPro>();
            var dic = new List<IScheduleMatrixPro> { matrix };
            var schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();

            Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person }));
            Expect.Call(matrix.Person).Return(_person);
            Expect.Call(matrix.SchedulePeriod).Return(schedulePeriod);
            Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2000, 1, 1), new DateOnly(2000, 1, 1)));

            _mock.ReplayAll();
            Assert.That(_target.CheckGroupPerson(dic, _groupPerson, _daysOffToRemove, _daysOffToAdd), Is.False);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueWhenAllGood()
        {
            var dateToRemove = new DateOnly(2011, 9, 20);
            var dateToAdd = new DateOnly(2011, 9, 21);
            _daysOffToRemove.Add(dateToRemove);
            _daysOffToAdd.Add(dateToAdd);
            var matrix = _mock.StrictMock<IScheduleMatrixPro>();
            var scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
            var scheduleDay = _mock.StrictMock<IScheduleDay>();
            var dic = new List<IScheduleMatrixPro> { matrix };
            var schedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            

            Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person }));
            Expect.Call(matrix.Person).Return(_person);
            Expect.Call(matrix.SchedulePeriod).Return(schedulePeriod);
            Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(dateToRemove, dateToRemove));
            Expect.Call(matrix.GetScheduleDayByKey(dateToRemove)).Return(scheduleDayPro);
            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.DayOff);

            Expect.Call(matrix.GetScheduleDayByKey(dateToAdd)).Return(scheduleDayPro);
            Expect.Call(scheduleDayPro.DaySchedulePart()).Return(scheduleDay);
            Expect.Call(scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
            _mock.ReplayAll();
            Assert.That(_target.CheckGroupPerson(dic, _groupPerson, _daysOffToRemove, _daysOffToAdd), Is.True);
            _mock.VerifyAll();
        }
    }

    
}