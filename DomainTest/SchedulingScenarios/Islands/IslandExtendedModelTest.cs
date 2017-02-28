using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands
{
	[TestFixture]
	public class IslandExtendedModelTest
	{
		[Test]
		public void ShouldCollectAllSkillsOnIsland()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			var skillgroup1 = new SkillGroup(new[] { skill1, skill2 }, new IPerson[] { });
			var skillgroup2 = new SkillGroup(new[] { skill2 }, new IPerson[] { });
			var model = new Island(new[] {skillgroup1, skillgroup2}, new Dictionary<ISkill, int>()).CreatExtendedClientModel();

			model.SkillsInIsland.Count().Should().Be.EqualTo(2);
			model.SkillsInIsland.Should().Contain(skill1);
			model.SkillsInIsland.Should().Contain(skill2);
		}

		[Test]
		public void ShouldCollectAllSkillgroupsOnIsland()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			var skillgroup1 = new SkillGroup(new[] { skill1, skill2 }, new IPerson[] { });
			var skillgroup2 = new SkillGroup(new[] { skill2 }, new IPerson[] { });
			var model = new Island(new[] { skillgroup1, skillgroup2 }, new Dictionary<ISkill, int>()).CreatExtendedClientModel();

			model.SkillGroupsInIsland.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldAddAgentsToSkillGroup()
		{
			var skill1 = new Skill("1");		
			var agent1 = new Person();
			var skillgroup1 = new SkillGroup(new[] { skill1 }, new IPerson[] { agent1 });

			var model = new Island(new[] { skillgroup1 }, new Dictionary<ISkill, int>()).CreatExtendedClientModel();

			model.SkillGroupsInIsland.First().AgentsInSkillGroup.Count().Should().Be.EqualTo(1);
			model.SkillGroupsInIsland.First().AgentsInSkillGroup.Should().Contain(agent1);
		}
	}
}