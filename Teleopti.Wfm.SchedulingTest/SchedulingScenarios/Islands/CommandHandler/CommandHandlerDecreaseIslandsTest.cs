using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands.CommandHandler
{
	public class CommandHandlerDecreaseIslandsTest : ResourcePlannerCommandHandlerTest
	{
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;
		public FakeEventPublisher EventPublisher;

		[Test]
		public void ShouldNotMergeIslandsIfBigEnough()
		{
			var agentsOnEachSkill = MergeIslandsSizeLimit.Limit / 2 + 1;
			var skillA = new Skill().WithId();
			var skillB = new Skill().WithId();
			var skillAagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillBagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillB).WithId());
			skillAagents.Union(skillBagents).ForEach(x => PersonRepository.Has(x));

			ExecuteTarget();
			
			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count()
				.Should().Be.EqualTo(2);
		}
		
		[Test]
		public void ShouldMergeSmallEnoughIslands()
		{
			var agentsOnEachSkill = MergeIslandsSizeLimit.Limit / 2 - 1;
			var skillA = new Skill().WithId();
			var skillB = new Skill().WithId();
			var skillAagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillA).WithId()).ToArray();
			var skillBagents = Enumerable.Range(0, agentsOnEachSkill).Select(x => new Person().WithPersonPeriod(skillB).WithId()).ToArray();
			var allAgents = skillAagents.Union(skillBagents).ForEach(x => PersonRepository.Has(x));

			ExecuteTarget();

			var island = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			island.AgentsInIsland.Should().Have.SameValuesAs(allAgents.Select(x => x.Id.Value));
			island.Skills.Should().Have.SameValuesAs(skillA.Id.Value, skillB.Id.Value);
		}
		
		[Test]
		public void ShouldMergeMoreThanTwoIslands()
		{
			var skillA = new Skill().WithId();
			var skillB = new Skill().WithId();
			var skillC = new Skill().WithId();
			PersonRepository.Has(new Person().WithId().WithPersonPeriod(skillA));
			PersonRepository.Has(new Person().WithId().WithPersonPeriod(skillB));
			PersonRepository.Has(new Person().WithId().WithPersonPeriod(skillC));
			
			ExecuteTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count()
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandleMixOfSmallAndLargeIslands()
		{
			var skillSmall1 = new Skill("small1").WithId();
			var skillSmall2 = new Skill("small2").WithId();
			var skillLarge = new Skill("large").WithId();
			PersonRepository.Has(new Person().WithId().WithPersonPeriod(skillSmall1));
			PersonRepository.Has(Enumerable.Range(0, MergeIslandsSizeLimit.Limit + 1).Select(x => new Person().WithPersonPeriod(skillLarge).WithId()));
			PersonRepository.Has(new Person().WithId().WithPersonPeriod(skillSmall2));
			
			ExecuteTarget();

			var islands = EventPublisher.PublishedEvents.OfType<IIslandInfo>();
			islands.Count()
				.Should().Be.EqualTo(2);
			islands.Single(x => x.AgentsInIsland.Count() == 2).Skills
				.Should().Have.SameValuesAs(skillSmall1.Id.Value, skillSmall2.Id.Value);
		}

		[Test]
		public void ShouldCreateMoreThanOneNewGroupOfAgentsIfNecessary()
		{
			for (var i = 0; i < MergeIslandsSizeLimit.Limit * 2 + 1; i++)
			{
				var skill = new Skill().WithId();
				PersonRepository.Has(new Person().WithId().WithPersonPeriod(skill));
			}
			
			ExecuteTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count()
				.Should().Be.EqualTo(3);
		}

		public CommandHandlerDecreaseIslandsTest(SUT sut) : base(sut)
		{
		}
	}
}