using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class GroupPersonSameForPersonOnDateCheckerTest
    {
        private MockRepository _mock;
        private GroupPersonSameForPersonOnDateChecker _target;
        private IGroupPersonsBuilder _groupPersonsBuilder;
       // private IGroupPagePerDate _groupPagePerDate;
        private IPerson _person;
        readonly DateOnly _dateOnlyOne = new DateOnly(2011,9,20);
        readonly DateOnly _dateOnlyTwo = new DateOnly(2011, 9, 21);
    	private ISchedulingOptions _schedulingOptions;

    	[SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _groupPersonsBuilder = _mock.StrictMock<IGroupPersonsBuilder>();
        	_schedulingOptions = new SchedulingOptions();
          //  _groupPagePerDate = _mock.StrictMock<IGroupPagePerDate>();
            _person = _mock.StrictMock<IPerson>();

            _target = new GroupPersonSameForPersonOnDateChecker(_groupPersonsBuilder);
        }

        [Test]
        public void ShouldReturnFalseNullIfAnyParameterIsNull()
        {
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(null, new List<DateOnly>(), new List<IPerson>(), _schedulingOptions), Is.Null);
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, null, new List<IPerson>(), _schedulingOptions), Is.Null);
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly>(), new List<IPerson>(), _schedulingOptions), Is.Null);
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly>(), null, _schedulingOptions), Is.Null);
        }

        [Test]
        public void ShouldReturnNullIfLessThanTwoDays()
        {
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly>(), new List<IPerson>(), _schedulingOptions), Is.Null);
        }

        [Test]
        public void ShouldReturnNullIfNoGroupPersons()
        {
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyOne, new List<IPerson>(),
                                                                     false,_schedulingOptions)).Return(
                                                                         new List<IGroupPerson>());
            _mock.ReplayAll();
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly> { _dateOnlyOne, _dateOnlyTwo }, new List<IPerson>(), _schedulingOptions), Is.Null);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnNullIfNoGroupPersonContainsPerson()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var groupPersons = new List<IGroupPerson> {groupPerson};
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyOne, new List<IPerson>(),
																	 false, _schedulingOptions)).Return(
                                                                         groupPersons);
            Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson>()));
            _mock.ReplayAll();
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly> { _dateOnlyOne, _dateOnlyTwo }, new List<IPerson>(), _schedulingOptions), Is.Null);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnNullIfOtherGroupPersonHasLessOrMoreMembers()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var groupPersonDate2 = _mock.StrictMock<IGroupPerson>();
            var groupPersons = new List<IGroupPerson> { groupPerson };
            var groupPersonsDate2 = new List<IGroupPerson> { groupPersonDate2 };
            var person2 = _mock.StrictMock<IPerson>();
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyOne, new List<IPerson>(),
																	 false, _schedulingOptions)).Return(
                                                                         groupPersons);
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyTwo, new List<IPerson>(),
																	 false, _schedulingOptions)).Return(
                                                                         groupPersonsDate2);
            Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson>{_person})).Repeat.AtLeastOnce();
            Expect.Call(groupPersonDate2.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person, person2 })).Repeat.AtLeastOnce();
            _mock.ReplayAll();
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly> { _dateOnlyOne, _dateOnlyTwo }, new List<IPerson>(), _schedulingOptions), Is.Null);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnNullIfOtherGroupPersonNotHavePerson()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var groupPersonDate2 = _mock.StrictMock<IGroupPerson>();
            var groupPersons = new List<IGroupPerson> { groupPerson };
            var groupPersonsDate2 = new List<IGroupPerson> { groupPersonDate2 };
            var person2 = _mock.StrictMock<IPerson>();
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyOne, new List<IPerson>(),
																	 false, _schedulingOptions)).Return(
                                                                         groupPersons);
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyTwo, new List<IPerson>(),
																	 false, _schedulingOptions)).Return(
                                                                         groupPersonsDate2);
            Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person })).Repeat.AtLeastOnce();
            Expect.Call(groupPersonDate2.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { person2 })).Repeat.AtLeastOnce();
            _mock.ReplayAll();
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly> { _dateOnlyOne, _dateOnlyTwo }, new List<IPerson>(), _schedulingOptions), Is.Null);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnNullIfOtherGroupPersonNotHaveSameMembers()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var groupPersonDate2 = _mock.StrictMock<IGroupPerson>();
            var groupPersons = new List<IGroupPerson> { groupPerson };
            var groupPersonsDate2 = new List<IGroupPerson> { groupPersonDate2 };
            var person2 = _mock.StrictMock<IPerson>();
            var person3 = _mock.StrictMock<IPerson>();
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyOne, new List<IPerson>(),
																	 false, _schedulingOptions)).Return(
                                                                         groupPersons);
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyTwo, new List<IPerson>(),
																	 false, _schedulingOptions)).Return(
                                                                         groupPersonsDate2);
            Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person, person3 })).Repeat.AtLeastOnce();
            Expect.Call(groupPersonDate2.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {_person, person2 })).Repeat.AtLeastOnce();
            _mock.ReplayAll();
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly> { _dateOnlyOne, _dateOnlyTwo }, new List<IPerson>(), _schedulingOptions), Is.Null);
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnGroupPersonIfSameMembersOnAllDates()
        {
            var groupPerson = _mock.StrictMock<IGroupPerson>();
            var groupPersonDate2 = _mock.StrictMock<IGroupPerson>();
            var groupPersons = new List<IGroupPerson> { groupPerson };
            var groupPersonsDate2 = new List<IGroupPerson> { groupPersonDate2 };
            var person2 = _mock.StrictMock<IPerson>();
           
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyOne, new List<IPerson>(),
																	 false, _schedulingOptions)).Return(
                                                                         groupPersons);
            Expect.Call(_groupPersonsBuilder.BuildListOfGroupPersons(_dateOnlyTwo, new List<IPerson>(),
																	 false, _schedulingOptions)).Return(
                                                                         groupPersonsDate2);
            Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person, person2 })).Repeat.AtLeastOnce();
            Expect.Call(groupPersonDate2.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _person, person2 })).Repeat.AtLeastOnce();
            _mock.ReplayAll();
			Assert.That(_target.FindCommonGroupPersonForPersonOnDates(_person, new List<DateOnly> { _dateOnlyOne, _dateOnlyTwo }, new List<IPerson>(), _schedulingOptions), Is.EqualTo(groupPerson));
            _mock.VerifyAll();
        }

        [Test]
        public void ShouldReturnGroupPersonBuilder()
        {
            Assert.That(_target.GroupPersonsBuilder, Is.EqualTo(_groupPersonsBuilder));
        }
    }

    
}