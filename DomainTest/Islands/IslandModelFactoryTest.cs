using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Islands
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public class IslandModelFactoryTest
	{
		public IslandModelFactory IslandModelFactory;
		public FakePersonRepository PersonRepository;

		[Test]
		public void ShouldCreateIslands()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1);
			PersonRepository.Has(skill2);

			var model = IslandModelFactory.Create();

			model.Count()
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCreateSkillGroupsInSameIsland()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1);
			PersonRepository.Has(skill1, skill2);

			var model = IslandModelFactory.Create();

			model.Single().SkillGroups.Count()
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCreateSkillsInSkillGroup()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);

			var model = IslandModelFactory.Create();

			model.Single().SkillGroups.Single().Skills.Select(x => x.Name)
				.Should().Have.SameValuesAs("1", "2");
		}

		[Test]
		public void ShouldCountNumberOfAgentsOnSkillGroup()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill1, skill2);

			var model = IslandModelFactory.Create();

			model.Single().SkillGroups.Single().NumberOfAgentsOnSkillGroup
				.Should().Be.EqualTo(3);
		}

		[Test, Ignore("To be fixed")]
		public void ShouldCountNumberOfAgentsOnSkill()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill2);
			PersonRepository.Has(skill1);

			var model = IslandModelFactory.Create();

			model.Single().SkillGroups.Single(x => x.Skills.Count==2).Skills.First().NumberOfAgentsOnSkill
				.Should().Be.EqualTo(2);
		}
	}
}