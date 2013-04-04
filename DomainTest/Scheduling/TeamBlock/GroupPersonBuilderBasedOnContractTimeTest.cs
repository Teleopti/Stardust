using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class GroupPersonBuilderBasedOnContractTimeTest
    {
        private MockRepository _mock;
        private IGroupPersonBuilderBasedOnContractTime _target;
        private readonly DateOnly _dateOnly = DateOnly.Today;
        private IPerson _person1;
        private IPerson _person2;
        private IGroupPersonFactory _groupPersonFactory;
        private IPersonPeriod _personPeriod;
        private IPersonContract _personContract;
        private BaseLineData _baselineData;
        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _person1 = _mock.StrictMock<IPerson>();
            _person2 = _mock.StrictMock<IPerson>();
            _personPeriod = _mock.StrictMock<IPersonPeriod>();
            _personContract = _mock.StrictMock<IPersonContract>();
            _groupPersonFactory = new GroupPersonFactory();
            _target = new GroupPersonBuilderBasedOnContractTime(_groupPersonFactory);
            _baselineData = new BaseLineData();
        }

        [Test]
        public void ReturnIfGroupPersonIsNull()
        {
            Assert.AreEqual(_target.SplitTeams(new List<IPerson>(), new DateOnly()).Count, 0);
        }

        [Test]
        public void ShouldReturnEmptyListIfPeriodIsNull()
        {
            DateOnly dateOnly = DateOnly.Today;

            var groupPersonList = _target.SplitTeams(_baselineData.PersonList, dateOnly);
            Assert.AreEqual(groupPersonList.Count, 0);
        }

        [Test]
        public void ShouldSplitTeamsBasedOnDifferentAveragContractTime()
        {
            var personList = new List<IPerson> { _person1, _person2 };

            var contract1 = createContracts(new TimeSpan(0, 7, 0, 0));
            var contract2 = createContracts(new TimeSpan(0, 8, 0, 0));

            using (_mock.Record())
            {

                expectCallsForAverageContractTime(contract1,_person1 );
                expectCallsForAverageContractTime(contract2,_person2 );
            }

            var groupPersonList = _target.SplitTeams(personList, _dateOnly);
            Assert.AreEqual(groupPersonList.Count, 2);
        }

       

        [Test]
        public void ShouldNotSplitTeamsBasedOnSameAverageContractTime()
        {
            var personList = new List<IPerson> { _person1, _person2 };

            var contract1 = createContracts(new TimeSpan(0, 7, 0, 0));
            var contract2 = createContracts(new TimeSpan(0, 7, 0, 0)); 


            using (_mock.Record())
            {

                expectCallsForAverageContractTime(contract1, _person1);
                expectCallsForAverageContractTime(contract2, _person2);
            }

            var groupPersonList = _target.SplitTeams(personList, _dateOnly);
            Assert.AreEqual(groupPersonList.Count, 1);
        }



        [Test]
        public void ShouldNotSplitTeamsBasedOnDifferentSchedulePeriodWhenSchedulePeriodIsNull()
        {
            var personList = new List<IPerson> { _person1, _person2 };

            var timeSpan1 = new TimeSpan(0, 7, 0, 0);
            var timeSpan2 = new TimeSpan(0, 8, 0, 0);
            var contract1 = createContracts(timeSpan1);
            contract1.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
            var contract2 = createContracts(timeSpan2);
            contract1.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;

            
            using (_mock.Record())
            {

                expectCallsForSchedulePeriodTime(contract1, _person1,null,timeSpan1 );
                expectCallsForSchedulePeriodTime(contract2, _person2, null, timeSpan2);
            }

            var groupPersonList = _target.SplitTeams(personList, _dateOnly);
            Assert.AreEqual(groupPersonList.Count, 1);
        }

        [Test]
        public void ShouldSplitTeamsBasedOnDifferentSchedulePeriodWhenSchedulePeriod()
        {
            var personList = new List<IPerson> { _person1, _person2 };

            var timeSpan1 = new TimeSpan(0, 7, 0, 0);
            var timeSpan2 = new TimeSpan(0, 8, 0, 0);
            var contract1 = createContracts(timeSpan1);
            contract1.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;
            var contract2 = createContracts(timeSpan2);
            contract1.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;

            IVirtualSchedulePeriod virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();

            using (_mock.Record())
            {

                expectCallsForSchedulePeriodTime(contract1, _person1, virtualSchedulePeriod, timeSpan1);
                expectCallsForSchedulePeriodTime(contract2, _person2, virtualSchedulePeriod, timeSpan2);
            }

            var groupPersonList = _target.SplitTeams(personList, _dateOnly);
            Assert.AreEqual(groupPersonList.Count, 2);
        }

        [Test]
        public void ShouldNotSplitTeamsBasedOnSameSchedulePeriodWhenSchedulePeriod()
        {
            var personList = new List<IPerson> { _person1, _person2 };

            var timeSpan1 = new TimeSpan(0, 7, 0, 0);
            var timeSpan2 = new TimeSpan(0, 7, 0, 0);
            var contract1 = createContracts(timeSpan1);
            contract1.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;

            var contract2 = createContracts(timeSpan2);
            contract1.WorkTimeSource = WorkTimeSource.FromSchedulePeriod;

            IVirtualSchedulePeriod virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();

            using (_mock.Record())
            {

                expectCallsForSchedulePeriodTime(contract1, _person1, virtualSchedulePeriod, timeSpan1);
                expectCallsForSchedulePeriodTime(contract2, _person2, virtualSchedulePeriod, timeSpan2);
            }

            var groupPersonList = _target.SplitTeams(personList, _dateOnly);
            Assert.AreEqual(groupPersonList.Count, 1);
        }

        private static Contract createContracts(TimeSpan timeSpan)
        {
            var contract1WorkTime = new WorkTime(timeSpan);
            var contract1 = new Contract("contract1");
            contract1.WorkTime = contract1WorkTime;
            return contract1;
        }

        private void expectCallsForSchedulePeriodTime(Contract contract, IPerson person,IVirtualSchedulePeriod virtualSchedulePeriod, TimeSpan timeSpan)
        {
            DateOnly dateOnly = DateOnly.Today;

            var permissionInfo = new PermissionInformation(person);
            permissionInfo.SetDefaultTimeZone(TimeZoneInfo.Utc);

            

            IList<IPersonSkill> personSkills = new List<IPersonSkill>() { _baselineData.SamplePersonSkill };

            Expect.Call(person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
            Expect.Call(_personPeriod.PersonContract).Return(_personContract);
            Expect.Call(_personContract.Contract).Return(contract);

            
            Expect.Call(_person1.VirtualSchedulePeriod(dateOnly)).Return(virtualSchedulePeriod);
            if (virtualSchedulePeriod != null)
            {
                Expect.Call(virtualSchedulePeriod.AverageWorkTimePerDay).Return(timeSpan);
            }
               
            
            Expect.Call(person.PermissionInformation).Return(permissionInfo).Repeat.AtLeastOnce();
            Expect.Call(_personPeriod.PersonSkillCollection).Return(personSkills);
            Expect.Call(_personPeriod.RuleSetBag).Return(_baselineData.SampleRuleSetBag);
        }
        
        private void expectCallsForAverageContractTime( Contract contract,IPerson person )
        {
            DateOnly dateOnly = DateOnly.Today;

            var permissionInfo = new PermissionInformation(person);
            permissionInfo.SetDefaultTimeZone(TimeZoneInfo.Utc);

            IList<IPersonSkill> personSkills = new List<IPersonSkill>() { _baselineData.SamplePersonSkill };

            Expect.Call(person.Period(dateOnly)).Return(_personPeriod).Repeat.AtLeastOnce();
            Expect.Call(_personPeriod.PersonContract).Return(_personContract);
            Expect.Call(_personContract.Contract).Return(contract);

            Expect.Call(person.PermissionInformation).Return(permissionInfo).Repeat.AtLeastOnce();
            Expect.Call(_personPeriod.PersonSkillCollection).Return(personSkills);
            Expect.Call(_personPeriod.RuleSetBag).Return(_baselineData.SampleRuleSetBag);
        }

       
        

        

    }

   
}
