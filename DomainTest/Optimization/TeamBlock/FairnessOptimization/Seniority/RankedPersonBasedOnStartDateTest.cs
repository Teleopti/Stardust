using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;


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
	    private IPerson _person4;
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
	        _person4 = _mock.StrictMock<IPerson>();
        }

        [Test]
        public void ReturnNullIfPersonMissingInEmptyRankDictionary()
        {
            var personList = new List<IPerson>();
            var result = _target.GetRankForPerson(personList, _person1);
            Assert.IsNull(result);
        }

        [Test]
        public void ReturnNullIfPersonMissingInRankDictionary()
        {
            var personList = new List<IPerson>() {_person2, _person3};
            var today = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person2)).Return(today);
                Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person3)).Return(today.AddDays(-15));
            }
            using (_mock.Playback())
            {
                var result = _target.GetRankForPerson(personList, _person1);
                Assert.IsNull(result);
            }
        }

		[Test]
	    public void ShouldGetEqualRankWhenSameStartDate()
	    {
			var personList = new List<IPerson>() { _person1, _person2, _person3, _person4};
			var today = DateOnly.Today;
			using (_mock.Record())
			{
				Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person1)).Return(today);
				Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person2)).Return(today);
				Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person3)).Return(today.AddDays(-15));
				Expect.Call(_personStartDateFromPersonPeriod.GetPersonStartDate(_person4)).Return(today.AddDays(1));
			}
			using (_mock.Playback())
			{
				var result = _target.GetRankedPersonDictionary(personList).ToList();
				Assert.AreEqual(4 ,result.Count());
				Assert.AreEqual(0, result[0].Value);
				Assert.AreEqual(1,result[1].Value);
				Assert.AreEqual(1, result[2].Value);
				Assert.AreEqual(2, result[3].Value);
			}    
	    }
    }   
}
