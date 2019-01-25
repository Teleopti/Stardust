using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Islands.ClientModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands
{
	[DomainTest]
	public class IslandModelTest : ITestInterceptor
	{
		public IslandModelFactory IslandModelFactory;
		public FakePersonRepository PersonRepository;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;

		[Test]
		public void ShouldCreateIslands()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1);
			PersonRepository.Has(skill2);

			var model = IslandModelFactory.Create();

			model.MoreIslandsBySkillReducing.Islands.Count().Should().Be.EqualTo(2);
			model.BasicIslands.Islands.Count().Should().Be.EqualTo(2);
			model.FewerIslandsByMerging.Islands.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCreateSkillSetsInSameIsland()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1);
			PersonRepository.Has(skill1, skill2);

			var model = IslandModelFactory.Create();

			model.MoreIslandsBySkillReducing.Islands.Single().SkillSets.Count().Should().Be.EqualTo(2);
			model.BasicIslands.Islands.Single().SkillSets.Count().Should().Be.EqualTo(2);
			model.FewerIslandsByMerging.Islands.Single().SkillSets.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCreateSkillsInSkillSet()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);

			var model = IslandModelFactory.Create();

			model.MoreIslandsBySkillReducing.Islands.Single().SkillSets.Single().Skills.Select(x => x.Name).Should().Have.SameValuesAs("1", "2");
			model.BasicIslands.Islands.Single().SkillSets.Single().Skills.Select(x => x.Name).Should().Have.SameValuesAs("1", "2");
			model.FewerIslandsByMerging.Islands.Single().SkillSets.Single().Skills.Select(x => x.Name).Should().Have.SameValuesAs("1", "2");
		}

		[Test]
		public void ShouldSetActivityNameOnSkill()
		{
			var activityName = RandomName.Make();
			var skill = new Skill {Activity = new Activity(activityName)};

			PersonRepository.Has(skill);

			var model = IslandModelFactory.Create();
			model.BasicIslands.Islands.Single().SkillSets.Single().Skills.Single().ActivityName.Should().Be.EqualTo(activityName);
			model.MoreIslandsBySkillReducing.Islands.Single().SkillSets.Single().Skills.Single().ActivityName.Should().Be.EqualTo(activityName);
			model.FewerIslandsByMerging.Islands.Single().SkillSets.Single().Skills.Single().ActivityName.Should().Be.EqualTo(activityName);
		}

		[Test]
		public void ShouldCountNumberOfAgentsOnSkillSet()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill1, skill2);

			var model = IslandModelFactory.Create();

			model.MoreIslandsBySkillReducing.Islands.Single().SkillSets.Single().NumberOfAgentsOnSkillSet.Should().Be.EqualTo(3);
			model.BasicIslands.Islands.Single().SkillSets.Single().NumberOfAgentsOnSkillSet.Should().Be.EqualTo(3);
			model.FewerIslandsByMerging.Islands.Single().SkillSets.Single().NumberOfAgentsOnSkillSet.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldOrderByNumberOfAgentsOnSkillSet()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill1);

			var model = IslandModelFactory.Create();

			model.MoreIslandsBySkillReducing.Islands.Single().SkillSets.First().Skills.Count().Should().Be.EqualTo(2);
			model.BasicIslands.Islands.Single().SkillSets.First().Skills.Count().Should().Be.EqualTo(2);
			model.FewerIslandsByMerging.Islands.Single().SkillSets.First().Skills.Count().Should().Be.EqualTo(2);
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

			model.MoreIslandsBySkillReducing.Islands.Single().SkillSets.Single(x => x.Skills.Count()==2).Skills.First().NumberOfAgentsOnSkill.Should().Be.EqualTo(2);
			model.BasicIslands.Islands.Single().SkillSets.Single(x => x.Skills.Count()==2).Skills.First().NumberOfAgentsOnSkill.Should().Be.EqualTo(2);
			model.FewerIslandsByMerging.Islands.Single().SkillSets.Single(x => x.Skills.Count()==2).Skills.First().NumberOfAgentsOnSkill.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldOrderByNumberOfAgentsOnSkill()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill2);

			var model = IslandModelFactory.Create();

			model.MoreIslandsBySkillReducing.Islands.Single().SkillSets.Single(x => x.Skills.Count() == 2).Skills.First().Name.Should().Be.EqualTo(skill2.Name);
			model.BasicIslands.Islands.Single().SkillSets.Single(x => x.Skills.Count() == 2).Skills.First().Name.Should().Be.EqualTo(skill2.Name);
			model.FewerIslandsByMerging.Islands.Single().SkillSets.Single(x => x.Skills.Count() == 2).Skills.First().Name.Should().Be.EqualTo(skill2.Name);
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

			model.MoreIslandsBySkillReducing.Islands.Select(island => island.NumberOfAgentsOnIsland).Should().Have.SameValuesAs(1, 2);
			model.BasicIslands.Islands.Select(island => island.NumberOfAgentsOnIsland).Should().Have.SameValuesAs(1, 2);
			model.FewerIslandsByMerging.Islands.Select(island => island.NumberOfAgentsOnIsland).Should().Have.SameValuesAs(1, 2);
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

			model.MoreIslandsBySkillReducing.Islands.First().NumberOfAgentsOnIsland.Should().Be.EqualTo(2);
			model.BasicIslands.Islands.First().NumberOfAgentsOnIsland.Should().Be.EqualTo(2);
			model.FewerIslandsByMerging.Islands.First().NumberOfAgentsOnIsland.Should().Be.EqualTo(2);
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

			model.MoreIslandsBySkillReducing.NumberOfAgentsOnAllIslands.Should().Be.EqualTo(3);
			model.BasicIslands.NumberOfAgentsOnAllIslands.Should().Be.EqualTo(3);
			model.FewerIslandsByMerging.NumberOfAgentsOnAllIslands.Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldSetTimeToGenerateIslands()
		{
			var skill = new Skill("_");
			PersonRepository.Has(skill);

			var model = IslandModelFactory.Create();

			//not really asserting any "real" value - just here as info for us devs
			model.MoreIslandsBySkillReducing.TimeToGenerateInMs.Should().Be.GreaterThan(0);
			model.BasicIslands.TimeToGenerateInMs.Should().Be.GreaterThan(0);
			model.FewerIslandsByMerging.TimeToGenerateInMs.Should().Be.GreaterThan(0);
		}

		public void OnBefore()
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
		}
	}
}