using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class TeamScorecardModelTest
    {
        private ITeam _team;
        private ITeamScorecardModel _target;

        [SetUp]
        public void Setup()
        {
            _team = TeamFactory.CreateSimpleTeam("Team1");
            _target = new TeamScorecardModel(_team);
        }

        [Test]
        public void VerifyScorecard()
        {
            var scorecard = new Scorecard{Name = "MyNewScorecard"};
            _target.Scorecard = scorecard;
            Assert.AreEqual(scorecard, _team.Scorecard);
        }

        [Test]
        public void VerifyScorecardNull()
        {
            var scorecard = new Scorecard { Name = "MyNewScorecard" };
            _team.Scorecard = scorecard;
            _target.Scorecard = ScorecardProvider.NullScorecard;
            Assert.IsNull(_team.Scorecard);
        }

        [Test]
        public void VerifyEmptyScorecardInsteadNull()
        {
            Assert.AreEqual(ScorecardProvider.NullScorecard,_target.Scorecard);
        }

        [Test]
        public void VerifyCanGetSiteAndTeam()
        {
            _team.Site = SiteFactory.CreateSimpleSite("Site1");
            Assert.AreEqual(_team.SiteAndTeam,_target.SiteAndTeam);
        }
    }
}
