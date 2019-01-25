using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.Islands
{
	[TestFixture]
	public class IslandExtendedModelTest
	{
		[Test]
		public void ShouldCollectAllSkillsOnIsland()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			var skillSet1 = new SkillSet(new HashSet<ISkill> { skill1, skill2 }, new HashSet<IPerson>());
			var skillSet2 = new SkillSet(new HashSet<ISkill> { skill2 }, new HashSet<IPerson>());
			var model = new Island(new[] {skillSet1, skillSet2}, new Dictionary<ISkill, int>()).CreatExtendedClientModel();

			model.SkillsInIsland.Count().Should().Be.EqualTo(2);
			model.SkillsInIsland.Should().Contain(skill1);
			model.SkillsInIsland.Should().Contain(skill2);
		}

		[Test]
		public void ShouldCollectAllSkillSetsOnIsland()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			var skillSet1 = new SkillSet(new HashSet<ISkill> { skill1, skill2 }, new HashSet<IPerson>());
			var skillSet2 = new SkillSet(new HashSet<ISkill> { skill2 }, new HashSet<IPerson>());
			var model = new Island(new[] { skillSet1, skillSet2 }, new Dictionary<ISkill, int>()).CreatExtendedClientModel();

			model.SkillSetsInIsland.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldAddAgentsToSkillSet()
		{
			var skill1 = new Skill("1");		
			var agent1 = new Person();
			var skillSet1 = new SkillSet(new HashSet<ISkill> { skill1 }, new HashSet<IPerson> { agent1 });

			var model = new Island(new[] { skillSet1 }, new Dictionary<ISkill, int>()).CreatExtendedClientModel();

			model.SkillSetsInIsland.First().AgentsInSkillSet.Count().Should().Be.EqualTo(1);
			model.SkillSetsInIsland.First().AgentsInSkillSet.Should().Contain(agent1);
		}
	}
}