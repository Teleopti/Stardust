﻿using System.Linq;
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

		[TestCase(5, 1, ExpectedResult = 2)]
		[TestCase(4, 1, ExpectedResult = 1)]
		public int ShouldRemoveSkillFromSkillGroupIfOtherSkillgroupIsBigEnough(int agentsSkillA, int agentsSkillAB)
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 5);
			var skillA = SkillFactory.CreateSkill("A").WithId();
			var skillB = SkillFactory.CreateSkill("B").WithId();
			var skillAagents = Enumerable.Range(0, agentsSkillA).Select(x => new Person().KnowsSkill(skillA));
			var skillABagents = Enumerable.Range(0, agentsSkillAB).Select(x => new Person().KnowsSkill(skillA, skillB));
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			Target.Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });

			return EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count();
		}

		[TestCase(6, 1, ExpectedResult = 2)]
		[TestCase(5, 1, ExpectedResult = 1)]
		public int ShouldNotRemoveSkillFromSkillGroupIfIslandIsNotBigEnough(int agentsSkillA, int agentsSkillAB)
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(7, 4);
			var skillA = SkillFactory.CreateSkill("A").WithId();
			var skillB = SkillFactory.CreateSkill("B").WithId();
			var skillAagents = Enumerable.Range(0, agentsSkillA).Select(x => new Person().KnowsSkill(skillA));
			var skillABagents = Enumerable.Range(0, agentsSkillAB).Select(x => new Person().KnowsSkill(skillA, skillB));
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			Target.Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });

			return EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count();
		}

		[Test]
		public void ShouldConsiderAlreadyReducedSkillGroupsWhenReducingSkillGroup()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 4);
			var skillA = SkillFactory.CreateSkill("A").WithId();
			var skillB = SkillFactory.CreateSkill("B").WithId();
			var skillC = SkillFactory.CreateSkill("C").WithId();
			var skillAagents21 = Enumerable.Range(0, 11).Select(x => new Person().KnowsSkill(skillA));
			var skillABagents5 = Enumerable.Range(0, 5).Select(x => new Person().KnowsSkill(skillA, skillB));
			var skillACagents5 = Enumerable.Range(0, 5).Select(x => new Person().KnowsSkill(skillA, skillC));
			skillAagents21.Union(skillABagents5).Union(skillACagents5).ForEach(x => PersonRepository.Has(x));

			Target.Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count().Should().Be(2);
		}
	}
}