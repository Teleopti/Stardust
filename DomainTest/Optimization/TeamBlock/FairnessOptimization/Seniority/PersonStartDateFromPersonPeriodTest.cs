using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    [TestFixture]
    public class PersonStartDateFromPersonPeriodTest
    {
        private IPersonStartDateFromPersonPeriod _target;
        private MockRepository _mock;
        private IPerson _person;
        private IPersonPeriod _personPeriod1;
        private IPersonPeriod _personPeriod2;
        private IPersonPeriod _personPeriod3;

        [SetUp]
        public void Setup()
        {
            _target = new PersonStartDateFromPersonPeriod();
            _mock = new MockRepository();
            _person = _mock.StrictMock<IPerson>();
            _personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            _personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            _personPeriod3 = _mock.StrictMock<IPersonPeriod>();
        }

        [Test]
        public void ShouldReturn0ForAgentStartingToday()
        {
            var period1StartDate = DateOnly.Today;
            IList<IPersonPeriod> personPeriodList = new List<IPersonPeriod>()
                {
                    _personPeriod1
                };
            using (_mock.Record())
            {
                Expect.Call(_person.PersonPeriodCollection).Return(personPeriodList);
                Expect.Call(_personPeriod1.StartDate).Return(period1StartDate);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.GetPersonStartDate(_person), period1StartDate);
            }
        }

        [Test]
        public void ShouldReturnNegative10ForAgentStartingInFuture()
        {
            const double daysInFuture = 10;
            var period1StartDate = DateOnly.Today.AddDays((int)daysInFuture);
            IList<IPersonPeriod> personPeriodList = new List<IPersonPeriod>()
                {
                    _personPeriod1
                };
            using (_mock.Record())
            {
                Expect.Call(_person.PersonPeriodCollection).Return(personPeriodList);
                Expect.Call(_personPeriod1.StartDate).Return(period1StartDate);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.GetPersonStartDate(_person), period1StartDate);
            }
        }

       
        [Test]
        public void ShouldReturnPossible20ForWithMultiplePersonPeriod()
        {
            var period1StartDate = DateOnly.Today; //0 days
            var period2StartDate = DateOnly.Today.AddDays(-20); //earliest
            var period3StartDate = DateOnly.Today.AddDays(-7);
            IList<IPersonPeriod> personPeriodList = new List<IPersonPeriod>()
                {
                    _personPeriod1,
                    _personPeriod2,
                    _personPeriod3 
                };
            using (_mock.Record())
            {
                Expect.Call(_person.PersonPeriodCollection).Return(personPeriodList);
                Expect.Call(_personPeriod1.StartDate).Return(period1StartDate);
                Expect.Call(_personPeriod2.StartDate).Return(period2StartDate);
                Expect.Call(_personPeriod3.StartDate).Return(period3StartDate);

            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.GetPersonStartDate(_person), period2StartDate);
            }
        }

        [Test]
        public void ShouldReturnPossibleNegative7ForWithMultiplePersonPeriod()
        {
            var period1StartDate = DateOnly.Today.AddDays(20); //0 days
            var period2StartDate = DateOnly.Today.AddDays(21); //earliest
            var period3StartDate = DateOnly.Today.AddDays(7);
            IList<IPersonPeriod> personPeriodList = new List<IPersonPeriod>()
                {
                    _personPeriod1,
                    _personPeriod2,
                    _personPeriod3 
                };
            using (_mock.Record())
            {
                Expect.Call(_person.PersonPeriodCollection).Return(personPeriodList);
                Expect.Call(_personPeriod1.StartDate).Return(period1StartDate);
                Expect.Call(_personPeriod2.StartDate).Return(period2StartDate);
                Expect.Call(_personPeriod3.StartDate).Return(period3StartDate);

            }
            using (_mock.Playback())
            {
                Assert.AreEqual(_target.GetPersonStartDate(_person), period3StartDate);
            }
        }
    }
}
