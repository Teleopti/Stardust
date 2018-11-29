using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;

using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.SeatLimitations
{
    [TestFixture]
    public class MaxSeatSitesExtractorTest
    {
        private MockRepository _mocks;
        private MaxSeatSitesExtractor _target;
        private DateOnlyPeriod _dateOnlyPeriod;
        private IList<IPerson> _peopleList;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _peopleList = new List<IPerson>();
            _target = new MaxSeatSitesExtractor();
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
            Expect.Call(person.PersonPeriods(_dateOnlyPeriod)).Return(new [] {personPeriod});
            Expect.Call(personPeriod.Team).Return(team);
            
            _mocks.ReplayAll();

            _peopleList.Add(person);
			var result = _target.MaxSeatSites(_dateOnlyPeriod, _peopleList);
            Assert.That(result.Length, Is.EqualTo(1));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldContinueWhenNoPersonPeriod()
        {
            var person = _mocks.StrictMock<IPerson>();
            Expect.Call(person.PersonPeriods(_dateOnlyPeriod)).Return(new IPersonPeriod[0]);
            
            _mocks.ReplayAll();

            _peopleList.Add(person);
			var result = _target.MaxSeatSites(_dateOnlyPeriod, _peopleList);
            Assert.That(result.Length, Is.EqualTo(0));
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
            Expect.Call(person.PersonPeriods(_dateOnlyPeriod)).Return(new []{personPeriod});
            Expect.Call(personPeriod.Team).Return(team);
            
            _mocks.ReplayAll();

            _peopleList.Add(person);
			var result = _target.MaxSeatSites(_dateOnlyPeriod, _peopleList);
            Assert.That(result.Length, Is.EqualTo(0));
            _mocks.VerifyAll();
        }
    }
}