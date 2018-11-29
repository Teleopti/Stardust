using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
    [Category("BucketB")]
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
   
            Assert.AreEqual(0, result.FilterPeople(new List<IPerson>()));
            Assert.AreEqual(0, result.FilterSkills(new ISkill[]{},null,null));
        }

		[Test]
		public void ShouldRemoveAllSkills()
		{
			var target = new PeopleAndSkillLoaderDeciderForTeamLeaderMode();
			var result = target.Execute(ScenarioFactory.CreateScenarioAggregate(), new DateTimePeriod(2011, 10, 10, 2011, 10, 11), new List<IPerson>());
			var skills = new ISkill[] {new Skill("_"), new Skill("_")};

			var removedSkills = result.FilterSkills(skills, x => {}, null);

			removedSkills.Should().Be.EqualTo(skills.Length);
		}
	}
}
