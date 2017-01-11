using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.Islands
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public class IslandModelTest
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

			model.AfterReducing.Islands.Count().Should().Be.EqualTo(2);
			model.BeforeReducing.Islands.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCreateSkillGroupsInSameIsland()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1);
			PersonRepository.Has(skill1, skill2);

			var model = IslandModelFactory.Create();

			model.AfterReducing.Islands.Single().SkillGroups.Count().Should().Be.EqualTo(2);
			model.BeforeReducing.Islands.Single().SkillGroups.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCreateSkillsInSkillGroup()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);

			var model = IslandModelFactory.Create();

			model.AfterReducing.Islands.Single().SkillGroups.Single().Skills.Select(x => x.Name).Should().Have.SameValuesAs("1", "2");
			model.BeforeReducing.Islands.Single().SkillGroups.Single().Skills.Select(x => x.Name).Should().Have.SameValuesAs("1", "2");
		}

		[Test]
		public void ShouldSetActivityNameOnSkill()
		{
			var activityName = RandomName.Make();
			var skill = new Skill {Activity = new Activity(activityName)};

			PersonRepository.Has(skill);

			var model = IslandModelFactory.Create();
			model.BeforeReducing.Islands.Single().SkillGroups.Single().Skills.Single().ActivityName.Should().Be.EqualTo(activityName);
			model.AfterReducing.Islands.Single().SkillGroups.Single().Skills.Single().ActivityName.Should().Be.EqualTo(activityName);
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

			model.AfterReducing.Islands.Single().SkillGroups.Single().NumberOfAgentsOnSkillGroup.Should().Be.EqualTo(3);
			model.BeforeReducing.Islands.Single().SkillGroups.Single().NumberOfAgentsOnSkillGroup.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldOrderByNumberOfAgentsOnSkillGroup()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill1);

			var model = IslandModelFactory.Create();

			model.AfterReducing.Islands.Single().SkillGroups.First().Skills.Count().Should().Be.EqualTo(2);
			model.BeforeReducing.Islands.Single().SkillGroups.First().Skills.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCountNumberOfAgentsOnSkill()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill2);
			PersonRepository.Has(skill1);

			var model = IslandModelFactory.Create();

			model.AfterReducing.Islands.Single().SkillGroups.Single(x => x.Skills.Count()==2).Skills.First().NumberOfAgentsOnSkill.Should().Be.EqualTo(2);
			model.BeforeReducing.Islands.Single().SkillGroups.Single(x => x.Skills.Count()==2).Skills.First().NumberOfAgentsOnSkill.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldOrderByNumberOfAgentsOnSkill()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill2);

			var model = IslandModelFactory.Create();

			model.AfterReducing.Islands.Single().SkillGroups.Single(x => x.Skills.Count() == 2).Skills.First().Name.Should().Be.EqualTo(skill2.Name);
			model.BeforeReducing.Islands.Single().SkillGroups.Single(x => x.Skills.Count() == 2).Skills.First().Name.Should().Be.EqualTo(skill2.Name);
		}

		[Test]
		public void ShouldCountNumberOfAgentsOnIsland()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1);
			PersonRepository.Has(skill2);
			PersonRepository.Has(skill2);

			var model = IslandModelFactory.Create();

			model.AfterReducing.Islands.Select(island => island.NumberOfAgentsOnIsland).Should().Have.SameValuesAs(1, 2);
			model.BeforeReducing.Islands.Select(island => island.NumberOfAgentsOnIsland).Should().Have.SameValuesAs(1, 2);
		}

		[Test]
		public void ShouldOrderByNumberOfAgentsOnIsland()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1);
			PersonRepository.Has(skill2);
			PersonRepository.Has(skill2);

			var model = IslandModelFactory.Create();

			model.AfterReducing.Islands.First().NumberOfAgentsOnIsland.Should().Be.EqualTo(2);
			model.BeforeReducing.Islands.First().NumberOfAgentsOnIsland.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldCountNumberOfAgentsOnAllIslands()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1);
			PersonRepository.Has(skill2);
			PersonRepository.Has(skill2);

			var model = IslandModelFactory.Create();

			model.AfterReducing.NumberOfAgentsOnAllIsland.Should().Be.EqualTo(3);
			model.BeforeReducing.NumberOfAgentsOnAllIsland.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldSetTimeToGenerateIslands()
		{
			var skill = new Skill("_");
			PersonRepository.Has(skill);

			var model = IslandModelFactory.Create();

			//not really asserting any "real" value - just here as info for us devs
			model.AfterReducing.TimeToGenerateInMs.Should().Be.GreaterThan(0);
			model.BeforeReducing.TimeToGenerateInMs.Should().Be.GreaterThan(0);
		}
	}
}