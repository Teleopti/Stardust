using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands
{
	[TestFixture(SUT.IntradayOptimization)]
	[TestFixture(SUT.Scheduling)]
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SchedulingIslands_44757)]
	public class CommandHandlerIncreaseIslandsTest
	{
		private readonly SUT _sut;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;
		public ReduceIslandsLimits ReduceIslandsLimits;

		public IntradayOptimizationCommandHandler IntradayOptimizationCommandHandler;
		public SchedulingCommandHandler SchedulingCommandHandler;

		public CommandHandlerIncreaseIslandsTest(SUT sut)
		{
			_sut = sut;
		}

		[TestCase(5, 1, ExpectedResult = 2)]
		[TestCase(4, 1, ExpectedResult = 1)]
		public int ShouldRemoveSkillFromSkillGroupIfOtherSkillgroupIsBigEnough(int agentsSkillA, int agentsSkillAB)
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 5);
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			var skillAagents = Enumerable.Range(0, agentsSkillA).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, agentsSkillAB).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			return EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count();
		}


		[TestCase(6, 1, ExpectedResult = 2)]
		[TestCase(5, 1, ExpectedResult = 1)]
		public int ShouldNotRemoveSkillFromSkillGroupIfIslandIsNotBigEnough(int agentsSkillA, int agentsSkillAB)
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(7, 4);
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			var skillAagents = Enumerable.Range(0, agentsSkillA).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, agentsSkillAB).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			return EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count();
		}

		[Test]
		public void ShouldConsiderAlreadyReducedSkillGroupsWhenReducingSkillGroup()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 4);
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			var skillC = new Skill("C");
			var skillAagents21 = Enumerable.Range(0, 11).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents5 = Enumerable.Range(0, 5).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			var skillACagents5 = Enumerable.Range(0, 5).Select(x => new Person().WithPersonPeriod(skillA, skillC).WithId());
			skillAagents21.Union(skillABagents5).Union(skillACagents5).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be(2);
		}

		[Test]
		public void ShouldReduceBiggestSkillGroupFirst()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 5);
			var skillA = new Skill("A");
			var skillB = new Skill("B");
			var skillC = new Skill("C");
			var skillAagents = Enumerable.Range(0, 26).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, 7).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			var skillACagents = Enumerable.Range(0, 8).Select(x => new Person().WithPersonPeriod(skillA, skillC).WithId());
			skillAagents.Union(skillABagents).Union(skillACagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			var events = EventPublisher.PublishedEvents.OfType<IIslandInfo>();

			events.Count().Should().Be.EqualTo(2);
			events.Any(x => x.AgentsInIsland.Count() == 8).Should().Be.True();
		}

		[Test]
		public void ShouldNotRemoveSkillIfActivityCompositionChangesInSkillGroup()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 4);
			var skillA = new Skill("A").For(new Activity("A"));
			var skillB = new Skill("B").For(new Activity("B"));
			var skillAagents = Enumerable.Range(0, 16).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, 5).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be(1);
		}

		[Test]
		public void ShouldOnlyConsiderPrimarySkillsWhenReducingSkillGroup()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 4);
			var skillA = new Skill("A").CascadingIndex(1);
			var skillB = new Skill("B").CascadingIndex(2);
			var skillAagents = Enumerable.Range(0, 16).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, 5).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be(1);
		}

		[Test]
		public void ShouldNotCreateSeperateIslandsOfDifferentSkillsIfAnotherHaveThemBoth()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill2);
			PersonRepository.Has(skill1);

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be(1);
		}

		[Test]
		public void ShouldNotCreateSeperateIslandsOfDifferentSkillsIfAnotherHaveThemBothMultiple()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			var skill3 = new Skill("3");
			var skill4 = new Skill("4");
			var skill5 = new Skill("5");
			var skill6 = new Skill("6");
			PersonRepository.Has(skill1, skill2, skill3);
			PersonRepository.Has(skill4, skill5, skill6);
			PersonRepository.Has(skill3, skill4);

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be(1);
		}

		[Test]
		public void ShouldNotCauseStackOverflowExceptionDueToTooManyRecursiveCalls()
		{
			var skill = new Skill("shared");
			PersonRepository.Has(new Person().WithPersonPeriod(skill).WithId());
			for (var i = 0; i < 200; i++)
			{
				PersonRepository.Has(new Person().WithPersonPeriod(skill, new Skill(i.ToString())).WithId());
			}
			Assert.DoesNotThrow(executeTarget);
		}

		[Test]
		public void ShouldNotCreateMoreIslandsThanNecessary()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(8, 2);
			var skillA = new Skill("A").WithId();
			var skillB = new Skill("B").WithId();
			var skillC = new Skill("C").WithId();
			var skillD = new Skill("D").WithId();
			var skillE = new Skill("E").WithId();
			var skillF = new Skill("F").WithId();
			var skillABCagents = Enumerable.Range(0, 2).Select(x => new Person().WithPersonPeriod(skillA, skillB, skillC).WithId());
			var skillCEagents = Enumerable.Range(0, 5).Select(x => new Person().WithPersonPeriod(skillC, skillE).WithId());
			var skillCDagents = Enumerable.Range(0, 1).Select(x => new Person().WithPersonPeriod(skillC, skillD).WithId());
			var skillFEagents = Enumerable.Range(0, 1).Select(x => new Person().WithPersonPeriod(skillF, skillE).WithId());
			skillABCagents.Union(skillCEagents).Union(skillCDagents).Union(skillFEagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			var events = EventPublisher.PublishedEvents.OfType<IIslandInfo>();
			events.Count().Should().Be.EqualTo(2);
			events.Any(x => x.AgentsInIsland.Count() == 7).Should().Be.True();
			events.Any(x => x.AgentsInIsland.Count() == 2).Should().Be.True();
		}


		[Test]
		public void ShouldSplitIslandBecauseCaringAboutTheFirstLimit()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 2);
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(10, 1);

			var skillA = new Skill("A");
			var skillB = new Skill("B");
			var skillAagents = Enumerable.Range(0, 4).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, 1).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSplitIslandBecauseCaringAboutTheLastLimit()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(10, 1);
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(0, 2);

			var skillA = new Skill("A");
			var skillB = new Skill("B");
			var skillAagents = Enumerable.Range(0, 4).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, 1).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSplitIslandBecauseCaringAboutTheMaxLimit()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(1, 10);
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(2, 2);

			var skillA = new Skill("A");
			var skillB = new Skill("B");
			var skillAagents = Enumerable.Range(0, 4).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var skillABagents = Enumerable.Range(0, 1).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			skillAagents.Union(skillABagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotCreateIslandsOfAgentsKnowingNoSkills()
		{
			var skillA = new Skill("A");
			PersonRepository.Has(new Person().WithPersonPeriod(skillA).WithId());
			PersonRepository.Has(new Person().WithPersonPeriod().WithId()); //has personperiod with no skill -> should not be included
			PersonRepository.Has(new Person().WithId()); //has no personperiod -> should not be included

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().AgentsInIsland.Count().Should().Be.EqualTo(1);
		}

		[Test, Timeout(5000)]
		public void ShouldNotHangWhenHavingManySkillGroups_Bug42634()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(2, 2);

			const int numberOfAgents = 700;
			var allAgents = new List<IPerson>();
			var skillA = new Skill("A");
			for (var i = 0; i < numberOfAgents; i++)
			{
				var skillB = new Skill("B" + i);
				allAgents.Add(new Person().WithPersonPeriod(skillA, skillB).WithId());
			}
			allAgents.ForEach(x => PersonRepository.Has(x));

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count()
				.Should().Be.EqualTo(numberOfAgents);
		}

		[Test]
		public void ShouldNotReduceSkillGroupWhenPotentiallyBelowIslandLimit()
		{
			ReduceIslandsLimits.SetValues_UseOnlyFromTest(5, 1);
			var skillA = new Skill("A").WithId();
			var skillB = new Skill("B").WithId();
			var skillC = new Skill("C").WithId();
			var skillD = new Skill("D").WithId();
			var skillADagents = Enumerable.Range(0, 2).Select(x => new Person().WithPersonPeriod(skillA, skillD).WithId());
			var skillBDagents = Enumerable.Range(0, 2).Select(x => new Person().WithPersonPeriod(skillB, skillD).WithId());
			var skillCDagents = Enumerable.Range(0, 2).Select(x => new Person().WithPersonPeriod(skillC, skillD).WithId());
			var skillDagents = Enumerable.Range(0, 1).Select(x => new Person().WithPersonPeriod(skillD).WithId());
			skillADagents.Union(skillBDagents).Union(skillCDagents).Union(skillDagents).ForEach(x => PersonRepository.Has(x));

			executeTarget();

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be.EqualTo(3);
		}

		private void executeTarget()
		{
			switch (_sut)
			{
				case SUT.Scheduling:
					SchedulingCommandHandler.Execute(new SchedulingCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });
					break;
				case SUT.IntradayOptimization:
					IntradayOptimizationCommandHandler.Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });
					break;
				default:
					throw new NotSupportedException();
			}
		}

		public enum SUT
		{
			IntradayOptimization,
			Scheduling
		}
	}
}