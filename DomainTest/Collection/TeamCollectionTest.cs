using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class TeamCollectionTest
    {
        private ITeamCollection _target;
        private DateOnly _dt;
        private ITeam _team1;
        private ITeam _team2;

        [SetUp]
        public void Setup()
        {
            _team1 = TeamFactory.CreateTeam("t1","s1");
            _team2 = TeamFactory.CreateTeam("t2","s2");
            _dt = new DateOnly(2000, 1, 1);
            _target = new TeamCollection("func", new[] { _team1,_team2 }, _dt);
        }

        [Test]
        public void VerifyAllPermittedTeams()
        {
            var mocks = new MockRepository();
            var authorization = mocks.StrictMock<IAuthorization>();
            using (mocks.Record())
            {
                Expect.Call(authorization.IsPermitted("func", _dt, _team1)).Return(false);
                Expect.Call(authorization.IsPermitted("func", _dt, _team2)).Return(true);
            }
            using (mocks.Playback())
            {
                using (CurrentAuthorization.ThreadlyUse(authorization))
                {
                    var ret = _target.AllPermittedTeams;
                    Assert.AreEqual(1, ret.Count());
                    Assert.AreEqual(_team2, ret.First());
                }
            }
        }

        [Test]
        public void VerifyAllPermittedSites()
        {
            var mocks = new MockRepository();
            var authorization = mocks.StrictMock<IAuthorization>();
            using (mocks.Record())
            {
                Expect.Call(authorization.IsPermitted("func", _dt, _team1.Site)).Return(false);
                Expect.Call(authorization.IsPermitted("func", _dt, _team2.Site)).Return(true);
            }
            using (mocks.Playback())
            {
                using (CurrentAuthorization.ThreadlyUse(authorization))
                {
                    var ret = _target.AllPermittedSites;
                    Assert.AreEqual(1, ret.Count());
                    Assert.AreEqual(_team2.Site, ret.First());
                }
            }
        }
    }
}