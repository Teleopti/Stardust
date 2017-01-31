using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    [TestFixture]
    public class TeamTest
    {
        private ITeam _target;
		
        [SetUp]
        public void Setup()
        {
            _target = TeamFactory.CreateSimpleTeam();
        }
		
        [Test]
        public void CanCreateAndPropertiesAreSet()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(null, _target.Id);
            Assert.IsNull(_target.Site);
            Assert.IsTrue(_target.IsChoosable);
            Assert.IsNull(_target.Scorecard);
        }

        [Test]
        public void VerifyScorecardForTeamWorks()
        {
            MockRepository mocks = new MockRepository();

            IScorecard scorecard = mocks.DynamicMock<IScorecard>();
            _target.Scorecard = scorecard;

            Assert.AreEqual(scorecard,_target.Scorecard);
        }
		
        [Test]
        public void VerifyNameCanBeSetAndGet()
        {
            string setValue = "Set Name";
            _target.SetDescription(new Description(setValue));
            string resultValue = _target.Description.Name;

            Assert.AreEqual(setValue, resultValue);
        }

        [Test]
        public void VerifyCanSetSite()
        {
            ISite site = SiteFactory.CreateSimpleSite("Site1");
            _target.Site = site;
            Assert.AreSame(site, _target.Site);

        }
        [Test]
        public void VerifySiteAndTeam()
        {
            string name = "Happy Agents";
            _target.SetDescription(new Description(name));
            ISite site = SiteFactory.CreateSimpleSite("Site1");
            _target.Site = site;
            string expectedResult = string.Concat(site.Description.Name, "/", _target.Description.Name);
            Assert.AreEqual(expectedResult, _target.SiteAndTeam);
        }
    }
}