using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.WeeklyRestSolver
{
    [TestFixture]
    public class ContractWeeklyRestForPersonWeekTest
    {

        private MockRepository _mock;
        private IContractWeeklyRestForPersonWeek _target;
        private IPerson _person;
        private IPersonPeriod _personPeriod;
        private IPersonContract _personContract;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new ContractWeeklyRestForPersonWeek();
            _person = _mock.StrictMock<IPerson>();
            _personPeriod = _mock.StrictMock<IPersonPeriod>();
            _personContract = _mock.StrictMock<IPersonContract>();
        }

        [Test]
        public void ReturnNothingIfNoPersonPeriodFound()
        {
            var dateOnlyPeriod= new DateOnlyPeriod(2014,03,17,2014,03,23);
            var personWeek = new PersonWeek(_person,dateOnlyPeriod);
           
            using (_mock.Record())
            {
                Expect.Call(_person.Period(personWeek.Week.StartDate)).Return(null);
                Expect.Call(_person.Period(personWeek.Week.EndDate)).Return(null);

            }
            using (_mock.Playback())
            {
                var weeklyRest = _target.GetWeeklyRestFromContract(personWeek);
                Assert.AreEqual(TimeSpan.Zero, weeklyRest);
            }
        }

        [Test]
        public void ReturnWeeklyRestOnStartDate()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 23);
            var personWeek = new PersonWeek(_person, dateOnlyPeriod);
            IContract contract = new Contract("asad");
			contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromHours(36));
            using (_mock.Record())
            {
                Expect.Call(_person.Period(personWeek.Week.StartDate)).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(_personContract);
                Expect.Call(_personContract.Contract).Return(contract);

            }
            using (_mock.Playback())
            {
                var weeklyRest = _target.GetWeeklyRestFromContract(personWeek);
                Assert.AreEqual(TimeSpan.FromHours(36), weeklyRest);
            }
        }

        [Test]
        public void ReturnWeeklyRestOnEndDate()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 17, 2014, 03, 23);
            var personWeek = new PersonWeek(_person, dateOnlyPeriod);
            IContract contract = new Contract("asad");
			contract.WorkTimeDirective = new WorkTimeDirective(TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromHours(18));
            using (_mock.Record())
            {
                Expect.Call(_person.Period(personWeek.Week.StartDate)).Return(null);
                Expect.Call(_person.Period(personWeek.Week.EndDate )).Return(_personPeriod);
                Expect.Call(_personPeriod.PersonContract).Return(_personContract);
                Expect.Call(_personContract.Contract).Return(contract);

            }
            using (_mock.Playback())
            {
                var weeklyRest = _target.GetWeeklyRestFromContract(personWeek);
                Assert.AreEqual(TimeSpan.FromHours(18), weeklyRest);
            }
        }
    }

    
}
