using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    /// <summary>
    /// Class for team tests
    /// </summary>
    [TestFixture]
    public class TeamTest
    {
        private ITeam _target;

        /// <summary>
        /// Runs once per test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = TeamFactory.CreateSimpleTeam();
        }

        /// <summary>
        /// Determines whether this instance can be created and properties are set.
        /// </summary>
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

        /// <summary>
        /// Verifies the name can be set and get.
        /// </summary>
        [Test]
        public void VerifyNameCanBeSetAndGet()
        {
            string setValue = "Set Name";
            _target.Description = new Description(setValue);
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
            _target.Description = new Description(name);
            ISite site = SiteFactory.CreateSimpleSite("Site1");
            _target.Site = site;
            string expectedResult = string.Concat(site.Description.Name, "/", _target.Description.Name);
            Assert.AreEqual(expectedResult, _target.SiteAndTeam);
        }
    }
}