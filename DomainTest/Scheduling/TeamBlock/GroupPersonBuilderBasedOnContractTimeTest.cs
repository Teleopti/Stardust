using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class GroupPersonBuilderBasedOnContractTimeTest
    {
        private MockRepository _mock;
        private IGroupPersonBuilderBasedOnContractTime _target;
        private DateOnly _dateOnly = DateOnly.Today;
        private IPerson _person1;
        private IPerson _person2;
        private IGroupPersonFactory _groupPersonFactory;
        private IPersonPeriod _personPeriod1;
        private IPersonPeriod _personPeriod2;
        private IPersonContract _personContract1;
        private IPersonContract _personContract2;
        private IContract _contract1;
        private IContract _contract2;
        private IGroupPerson _groupPerson;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _person1 = _mock.StrictMock<IPerson>();
            _person2 = _mock.StrictMock<IPerson>();
            _personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            _personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            _personContract1 = _mock.StrictMock<IPersonContract>();
            _personContract2 = _mock.StrictMock<IPersonContract>();
            _contract1 = _mock.StrictMock<IContract>();
            _contract2 = _mock.StrictMock<IContract>();
            _groupPerson = _mock.StrictMock<IGroupPerson>();
            _groupPersonFactory = _mock.StrictMock<IGroupPersonFactory>();
            _target = new GroupPersonBuilderBasedOnContractTime(_groupPersonFactory);
        }

        [Test ]
        public void ShouldSplitTeams()
        {
            var personList = new List<IPerson> {_person1, _person2};
            var contract1WorkTime = new WorkTime(new TimeSpan(0, 7, 0, 0));
            var contract2WorkTime = new WorkTime(new TimeSpan(0, 8, 0, 0));
            using(_mock.Record() )
            {
                Expect.Call(_groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(personList ));
                Expect.Call(_person1.Period(_dateOnly)).IgnoreArguments().Return(_personPeriod1);
                Expect.Call(_personPeriod1.PersonContract).Return(_personContract1);
                Expect.Call(_personContract1.Contract).Return(_contract1);
                Expect.Call(_contract1.WorkTimeSource).Return(WorkTimeSource.FromContract);
                Expect.Call(_contract1.WorkTime).Return(contract1WorkTime);

                Expect.Call(_person2.Period(_dateOnly)).IgnoreArguments().Return(_personPeriod2);
                Expect.Call(_personPeriod2.PersonContract).Return(_personContract2);
                Expect.Call(_personContract2.Contract).Return(_contract2);
                Expect.Call(_contract2.WorkTimeSource).Return(WorkTimeSource.FromContract);
                Expect.Call(_contract2.WorkTime).Return(contract2WorkTime);

                Expect.Call(_groupPersonFactory.CreateGroupPerson(personList, _dateOnly, "", new Guid())).IgnoreArguments() .Return(_groupPerson ).Repeat.Twice();
            }

            using(_mock.Playback())
            {
                Assert.AreEqual(_target.SplitTeams(_groupPerson, _dateOnly).Count, 2);
            }
        }

    }

   
}
