using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class PeopleAndSkillLoaderDeciderForTeamLeaderModeTest
    {
        [Test]
        public void ShouldExecute()
        {
			var target = new PeopleAndSkillLoaderDeciderForTeamLeaderMode();
			var result = target.Execute(ScenarioFactory.CreateScenarioAggregate(), new DateTimePeriod(2011, 10, 10, 2011, 10, 11), new List<IPerson>());
   
            Assert.IsNotNull(result.SiteGuidDependencies);
            Assert.IsNotNull(result.SkillGuidDependencies);
            Assert.IsNotNull(result.PeopleGuidDependencies);
        }

        [Test]
        public void ShouldReturnZeroValues()
        {
			var target = new PeopleAndSkillLoaderDeciderForTeamLeaderMode();
			var result = target.Execute(ScenarioFactory.CreateScenarioAggregate(), new DateTimePeriod(2011, 10, 10, 2011, 10, 11), new List<IPerson>());
   
            Assert.AreEqual(0f, result.PercentageOfPeopleFiltered);
            Assert.AreEqual(0, result.FilterPeople(new List<IPerson>()));
            Assert.AreEqual(0, result.FilterSkills(new ISkill[]{},null,null));
        }
    }
}
