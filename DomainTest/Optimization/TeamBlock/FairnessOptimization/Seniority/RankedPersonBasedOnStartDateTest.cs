using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
    [TestFixture]
    public class RankedPersonBasedOnStartDateTest
    {
        private IRankedPersonBasedOnStartDate _target;
        private MockRepository _mock;
        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;
        private IPersonStartDateFromPersonPeriod _personStartDateFromPersonPeriod;

        [SetUp]
        public void Setup()
        {
            
            _mock = new MockRepository();
            _personStartDateFromPersonPeriod = _mock.StrictMock<IPersonStartDateFromPersonPeriod>();
            _target = new RankedPersonBasedOnStartDate(_personStartDateFromPersonPeriod);
            _person1 = _mock.StrictMock<IPerson>();
            _person2 = _mock.StrictMock<IPerson>();
            _person3 = _mock.StrictMock<IPerson>();
        }

        [Test]
        public void RankedPersonWithMultiplePersonPeriod()
        {
            var personList = new List<IPerson>() {_person1, _person2, _person3};
            var today = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person1 )).Return(today.AddDays(-5) );
                Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person2 )).Return(today );
                Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person3 )).Return(today.AddDays(-15) );
            }
            using (_mock.Playback())
            {
                var result = _target.GetRankedPersonList(personList).ToList() ;
                Assert.AreEqual(result.Count(), 3);
                Assert.AreEqual(result[0],_person3 );
                Assert.AreEqual(result[1],_person1 );
                Assert.AreEqual(result[2],_person2 );
            }
        }

        [Test]
        public void RankedPersonWithAllInFuture()
        {
            var personList = new List<IPerson>() { _person1, _person2, _person3 };
            var today = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person1)).Return(today.AddDays(5));
                Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person2)).Return(today.AddDays(2 ));
                Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person3)).Return(today.AddDays(20));
            }
            using (_mock.Playback())
            {
                var result = _target.GetRankedPersonList(personList).ToList();
                Assert.AreEqual(result.Count(), 3);
                Assert.AreEqual(result[0], _person2);
                Assert.AreEqual(result[1], _person1);
                Assert.AreEqual(result[2], _person3);
            }
        }
    }

    
}
