using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class PeopleAndSkillLoaderDeciderForTeamLeaderModeTest
    {
        private PeopleAndSkillLoaderDeciderForTeamLeaderMode _target;
        private MockRepository _mocks;
        private IScenario _scenario;

        [SetUp]
        public void Setup()
        {
            _target = new PeopleAndSkillLoaderDeciderForTeamLeaderMode();
            _mocks = new MockRepository();
            _scenario = _mocks.StrictMock<IScenario>();
        }

        [Test]
        public void ShouldExecute()
        {
            _target.Execute(_scenario, new DateTimePeriod(2011, 10, 10, 2011, 10, 11), new List<IPerson>());
   
            Assert.IsNotNull(_target.SiteGuidDependencies);
            Assert.IsNotNull(_target.SkillGuidDependencies);
            Assert.IsNotNull(_target.PeopleGuidDependencies);
        }

        [Test]
        public void ShouldReturnZeroValues()
        {
            Assert.AreEqual(0f, _target.PercentageOfPeopleFiltered);
            Assert.AreEqual(0, _target.FilterPeople(new List<IPerson>()));
            Assert.AreEqual(0, _target.FilterSkills(new List<ISkill>()));
        }
    }
}
