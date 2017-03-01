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
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.Wfm_ResourcePlanner_SchedulingOnStardust_42874)]
	public class WebIntradayOptimizationCommandHandlerTest : IntradayOptimizationCommandHandlerBaseTest
	{
		public IWebIntradayOptimizationCommandHandler Target;

		public override IIntradayOptimizationCommandHandler GetTarget()
		{
			return Target;
		}

		public override IEnumerable<OptimizationWasOrdered> GetOptimizationWasOrderedEvents()
		{
			return EventPublisher.PublishedEvents.OfType<WebIntradayOptimizationStardustEvent>().Select(x=>x.OptimizationWasOrdered);
		}
	}

	[TestFixture]
	[DomainTest]
	public class IntradayOptimizationCommandHandlerTest: IntradayOptimizationCommandHandlerBaseTest
	{
		public IDesktopIntradayOptimizationCommandHandler Target;

		public override IIntradayOptimizationCommandHandler GetTarget()
		{
			return Target;
		}

		public override IEnumerable<OptimizationWasOrdered> GetOptimizationWasOrderedEvents()
		{
			return EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>();
		}
	}

	public abstract class IntradayOptimizationCommandHandlerBaseTest
	{
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;
		public abstract IIntradayOptimizationCommandHandler GetTarget();
		public abstract IEnumerable<OptimizationWasOrdered> GetOptimizationWasOrderedEvents();

		[Test]
		public void ShouldSetPeriod()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016,1,1);
			PersonRepository.Has(agent);

			GetTarget().Execute(new IntradayOptimizationCommand {Period = period });

			var optimizationWasOrdered = GetOptimizationWasOrderedEvents().Single();
			optimizationWasOrdered.StartDate.Should().Be.EqualTo(period.StartDate);
			optimizationWasOrdered.EndDate.Should().Be.EqualTo(period.EndDate);
		}

		[Test]
		public void ShouldSetAgentIds()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent);

			GetTarget().Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10) });

			GetOptimizationWasOrderedEvents().Single().AgentsInIsland.Single().Should().Be.EqualTo(agent.Id.Value);
		}

		[Test]
		public void ShouldCreateTwoEventsIfTwoAgentsWithDifferentSkills()
		{
			var skill1 = new Skill().WithId();
			var skill2 = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill1);
			var agent2 = new Person().WithId().WithPersonPeriod(skill2);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			GetTarget().Execute(new IntradayOptimizationCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10)
			});

			GetOptimizationWasOrderedEvents().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCreateOneEventIfTwoAgentsWithAtLeastOneSkillTheSame()
		{
			var skill1 = new Skill().WithId();
			var skill2 = new Skill().WithId();
			var skill3 = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill1, skill2);
			var agent2 = new Person().WithId().WithPersonPeriod(skill2, skill3);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			GetTarget().Execute(new IntradayOptimizationCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10)
			});

			GetOptimizationWasOrderedEvents().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCreateTwoEventsIfSameSkillsButDiffersDueToPrimarySkill()
		{
			var skill1 = new Skill().WithId().CascadingIndex(1);
			var skill2 = new Skill().WithId().CascadingIndex(2);
			var agent1 = new Person().WithId().WithPersonPeriod(skill1, skill2);
			var agent2 = new Person().WithId().WithPersonPeriod(skill2);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			GetTarget().Execute(new IntradayOptimizationCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10)
			});

			GetOptimizationWasOrderedEvents().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSetAgentToOptimizeWhenOptimizeFullIsland()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			GetTarget().Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = new [] {agent1, agent2} });

			var @event = GetOptimizationWasOrderedEvents().Single();
			@event.AgentsToOptimize.Should().Have.SameValuesAs(@event.AgentsInIsland);
		}

		[Test]
		public void ShouldSetAgentsToOptimizeToAllInIslandIfNullIsPassed()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			GetTarget().Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = null });

			var @event = GetOptimizationWasOrderedEvents().Single();
			@event.AgentsToOptimize.Should().Have.SameValuesAs(agent1.Id.Value, agent2.Id.Value);
		}

		[Test]
		public void ShouldSetSpecificAgentsToOptimizeWhenOneIsland()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			GetTarget().Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = new[] {agent1} });

			var @event = GetOptimizationWasOrderedEvents().Single();
			@event.AgentsToOptimize.Should().Have.SameValuesAs(agent1.Id.Value);
		}

		[Test]
		public void ShouldSetSpecificAgentsToOptimizeWhenMultipleIsland()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			GetTarget().Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = new[] { agent1, agent2 } });

			var events = GetOptimizationWasOrderedEvents().ToArray();
			var event1 = events[0];
			var event2 = events[1];
			event1.AgentsToOptimize.Single()
				.Should().Not.Be.EqualTo(event2.AgentsToOptimize.Single());
		}

		[Test]
		public void ShouldNotCreateEventsForIslandsWithNoAgentsThatAreSetToBeOptimized()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			GetTarget().Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = new[] { agent1 } });

			GetOptimizationWasOrderedEvents().Single().AgentsToOptimize.Should().Have.SameValuesAs(agent1.Id.Value);
		}

		[Test]
		public void ShouldSetRandomCommandIdOnCommand()
		{
			var command1 = new IntradayOptimizationCommand();
			var command2 = new IntradayOptimizationCommand();
			command1.CommandId.Should().Not.Be.EqualTo(Guid.Empty);
			command1.CommandId.Should().Not.Be.EqualTo(command2.CommandId);
		}

		[Test]
		public void ShouldSetCommandIdOnEvent()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016, 1, 1);
			PersonRepository.Has(agent);
			var command = new IntradayOptimizationCommand { Period = period };

			GetTarget().Execute(command);

			GetOptimizationWasOrderedEvents().Single().CommandId.Should().Be.EqualTo(command.CommandId);
		}


		[Test]
		public void NoDuplicateSkillsInEventWhenMultipleSkillgroupsContainsSameSkill()
		{
			var skillA = new Skill("A").WithId();
			var skillB = new Skill("B").WithId();
			var agentsA = Enumerable.Range(0, 10).Select(x => new Person().WithPersonPeriod(skillA));
			var agentsAB = Enumerable.Range(0, 10).Select(x => new Person().WithPersonPeriod(skillA, skillB));
			agentsA.Union(agentsAB).ForEach(x => PersonRepository.Has(x));

			GetTarget().Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });

			GetOptimizationWasOrderedEvents().Single().Skills.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotCreateDuplicatesOfAgents()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill2);
			PersonRepository.Has(skill1);

			GetTarget().Execute(new IntradayOptimizationCommand { Period = DateOnly.Today.ToDateOnlyPeriod() });

			var theEvent = GetOptimizationWasOrderedEvents().Single();
			theEvent.AgentsInIsland.Count().Should().Be.EqualTo(3);
			theEvent.AgentsToOptimize.Count().Should().Be.EqualTo(3);
		}
	}
}