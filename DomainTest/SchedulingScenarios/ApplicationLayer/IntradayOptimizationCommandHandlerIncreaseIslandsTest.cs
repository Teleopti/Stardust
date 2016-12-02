using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.ApplicationLayer
{
	//Reuse tests here later when same/similar stuff are used for creating islands when scheduling, DO opt etc

	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SplitBigIslands_42049)]
	public class IntradayOptimizationCommandHandlerIncreaseIslandsTest
	{
		public IntradayOptimizationCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;
		public ReduceIslandsLimits ReduceIslandsLimits;

		[TestCase(40, 9, ExpectedResult = 2)]
		[TestCase(1, 1, ExpectedResult = 1)]
		public int ShouldMakeTwoIslandsByMakingAgentsSingleSkilledIfOtherSkillgroupIsBigEnough(int agentsSkillA, int agentsSkillAB)
		{
			ReduceIslandsLimits.SetMinimumNumberOfAgentsInIsland_UseOnlyFromTest(5);
			var skillA = SkillFactory.CreateSkill("A").WithId();
			var skillB = SkillFactory.CreateSkill("B").WithId();
			var skillAagents = Enumerable.Range(0, agentsSkillA).Select(x => new Person().KnowsSkill(skillA));
			var skillABagents = Enumerable.Range(0, agentsSkillAB).Select(x => new Person().KnowsSkill(skillA, skillB));
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			Target.Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });

			return EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count();
		}

		[Test, Ignore("42049")]
		public void ShouldConsiderAlreadyReducedSkillGroupsWhenReducingSkillGroup()
		{
			ReduceIslandsLimits.SetMinimumNumberOfAgentsInIsland_UseOnlyFromTest(5);
			var skillA = SkillFactory.CreateSkill("A").WithId();
			var skillB = SkillFactory.CreateSkill("B").WithId();
			var skillC = SkillFactory.CreateSkill("C").WithId();
			var skillAagents = Enumerable.Repeat(new Person().KnowsSkill(skillA), 20);
			var skillABagents = Enumerable.Repeat(new Person().KnowsSkill(skillA, skillB), 5);
			var skillACagents = Enumerable.Repeat(new Person().KnowsSkill(skillA, skillC), 5);

			foreach (var skillAagent in skillAagents)
			{
				PersonRepository.Has(skillAagent);
			}

			foreach (var skillABagent in skillABagents)
			{
				PersonRepository.Has(skillABagent);
			}

			foreach (var skillACagent in skillACagents)
			{
				PersonRepository.Has(skillACagent);
			}

			Target.Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count().Should().Be(2);
		}


		[Ignore("42049")]
		[TestCase(40, 9, ExpectedResult = 2)]
		[TestCase(40, 2, ExpectedResult = 1)]
		public int ShouldNotReduceSkillGroupsWhenAgentsInIslandIsBelowMinimumAgents(int agentsSkillA, int agentsSkillB)
		{
			ReduceIslandsLimits.SetMinimumNumberOfAgentsInIsland_UseOnlyFromTest(45);
			var skillA = SkillFactory.CreateSkill("A").WithId();
			var skillB = SkillFactory.CreateSkill("B").WithId();
			var skillAagents = Enumerable.Repeat(new Person().KnowsSkill(skillA), agentsSkillA);
			var skillABagents = Enumerable.Repeat(new Person().KnowsSkill(skillA, skillB), agentsSkillB);
			
			foreach (var skillAagent in skillAagents)
			{
				PersonRepository.Has(skillAagent);
			}

			foreach (var skillABagent in skillABagents)
			{
				PersonRepository.Has(skillABagent);
			}

			Target.Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });

			return EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count();
		}
	}
}