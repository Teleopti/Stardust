using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
    [TestFixture]
    public class MaxSeatSitesExtractorTest
    {
        private MockRepository _mocks;
        private MaxSeatSitesExtractor _target;
        private DateOnly _dateOnly;
        private DateOnlyPeriod _dateOnlyPeriod;
        private IList<IPerson> _peopleList;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _peopleList = new List<IPerson>();
            _target = new MaxSeatSitesExtractor();
            _dateOnly = new DateOnly(2011, 1, 19);
            _dateOnlyPeriod = new DateOnlyPeriod(2011, 1, 19, 2011, 1, 19);
        }

        [Test]
        public void ShouldFindSitesWithMaxSeatLimitations()
        {
            var person = _mocks.StrictMock<IPerson>();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var team = new Team();
            var site = new Site("site") {MaxSeats = 10};
            team.Site = site;
            Expect.Call(person.Period(_dateOnly)).Return(personPeriod);
            Expect.Call(personPeriod.Team).Return(team);
            Expect.Call(personPeriod.EndDate()).Return(_dateOnlyPeriod.EndDate);

            _mocks.ReplayAll();

            _peopleList.Add(person);
			var result = _target.MaxSeatSites(_dateOnlyPeriod, _peopleList);
            Assert.That(result.Count, Is.EqualTo(1));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldContinueWhenNoPersonPeriod()
        {
            var person = _mocks.StrictMock<IPerson>();
            Expect.Call(person.Period(_dateOnly)).Return(null);
            
            _mocks.ReplayAll();

            _peopleList.Add(person);
			var result = _target.MaxSeatSites(_dateOnlyPeriod, _peopleList);
            Assert.That(result.Count, Is.EqualTo(0));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldNotFindSitesWithNoMaxSeatLimitations()
        {
            var person = _mocks.StrictMock<IPerson>();
            var personPeriod = _mocks.StrictMock<IPersonPeriod>();
            var team = new Team();
            var site = new Site("site");
            team.Site = site;
            Expect.Call(person.Period(_dateOnly)).Return(personPeriod);
            Expect.Call(personPeriod.Team).Return(team);
            Expect.Call(personPeriod.EndDate()).Return(_dateOnlyPeriod.EndDate);

            _mocks.ReplayAll();

            _peopleList.Add(person);
			var result = _target.MaxSeatSites(_dateOnlyPeriod, _peopleList);
            Assert.That(result.Count, Is.EqualTo(0));
            _mocks.VerifyAll();
        }

        
    }

   
}