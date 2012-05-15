using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        [Test]
        public void VerifyPersonsInHierarchy()
        {
            ICollection<IPerson> candidates = new List<IPerson>();

            var dateTime = new DateOnlyPeriod(2000, 1, 1, 2002, 1, 1);
            IPerson person = PersonFactory.CreatePerson("Ola", "Håkansson");
            IPerson person2 = PersonFactory.CreatePerson("Roger", "Kratz");
            IPerson person3 = PersonFactory.CreatePerson("Ann", "Andersson");
            IPersonContract personContract = PersonContractFactory.CreatePersonContract();

            IPersonPeriod per = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 1, 1), personContract, _target);
            person.AddPersonPeriod(per);
            person2.AddPersonPeriod(per);
            person3.AddPersonPeriod(per);

            candidates.Add(person);
            candidates.Add(person2);
            candidates.Add(person3);
            ReadOnlyCollection<IPerson> lst = _target.PersonsInHierarchy(candidates, dateTime);

            Assert.IsNotNull(lst);
        }
    }
}