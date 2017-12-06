using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands.CommandHandler
{
	public class CommandHandlerGeneralTest : ResourcePlannerCommandHandlerTest
	{
		private readonly SUT _sut;
		public IntradayOptimizationCommandHandler IntradayOptimizationCommandHandler;
		public SchedulingCommandHandler SchedulingCommandHandler;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;
		public OptimizationPreferencesDefaultValueProvider OptimizationPreferencesProvider;
		public SchedulingOptionsProvider SchedulingOptionsProvider;

		public CommandHandlerGeneralTest(SUT sut)
		{
			_sut = sut;
		}

		[Test]
		public void ShouldSetPeriod()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016,1,1);
			PersonRepository.Has(agent);

			executeTarget(period);

			var optimizationWasOrdered = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			optimizationWasOrdered.StartDate.Should().Be.EqualTo(period.StartDate);
			optimizationWasOrdered.EndDate.Should().Be.EqualTo(period.EndDate);
		}

		[Test]
		public void ShouldSetAgentIds()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent);

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().AgentsInIsland.Single().Should().Be.EqualTo(agent.Id.Value);
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

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be.EqualTo(2);
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

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be.EqualTo(1);
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

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldSetAgentToOptimizeWhenOptimizeFullIsland()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1, agent2});

			var @event = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			@event.Agents.Should().Have.SameValuesAs(@event.AgentsInIsland);
		}

		[Test]
		public void ShouldSetAgentsToOptimizeToAllInIslandIfNullIsPassed()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), null);

			var @event = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			@event.Agents.Should().Have.SameValuesAs(agent1.Id.Value, agent2.Id.Value);
		}

		[Test]
		public void ShouldSetSpecificAgentsToOptimizeWhenOneIsland()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1});

			var @event = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			@event.Agents.Should().Have.SameValuesAs(agent1.Id.Value);
		}

		[Test]
		public void ShouldSetSpecificAgentsToOptimizeWhenMultipleIsland()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1, agent2});

			var events = EventPublisher.PublishedEvents.OfType<IIslandInfo>().ToArray();
			var event1 = events[0];
			var event2 = events[1];
			event1.Agents.Single()
				.Should().Not.Be.EqualTo(event2.Agents.Single());
		}

		[Test]
		public void ShouldNotCreateEventsForIslandsWithNoAgentsThatAreSetToBeOptimized()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			executeTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1});
			
			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().Agents.Should().Have.SameValuesAs(agent1.Id.Value);
		}

		[Test]
		public void ShouldSetRandomCommandIdOnCommand()
		{
			var command1 = executeTarget(DateOnly.Today.ToDateOnlyPeriod());
			var command2 = executeTarget(DateOnly.Today.ToDateOnlyPeriod());
			command1.CommandId.Should().Not.Be.EqualTo(Guid.Empty);
			command1.CommandId.Should().Not.Be.EqualTo(command2.CommandId);
		}

		[Test]
		public void ShouldSetCommandIdOnEvent()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016, 1, 1);
			PersonRepository.Has(agent);
			
			var command = executeTarget(period);

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().CommandId.Should().Be.EqualTo(command.CommandId);
		}


		[Test]
		public void NoDuplicateSkillsInEventWhenMultipleSkillSetsContainsSameSkill()
		{
			var skillA = new Skill("A").WithId();
			var skillB = new Skill("B").WithId();
			var agentsA = Enumerable.Range(0, 10).Select(x => new Person().WithPersonPeriod(skillA));
			var agentsAB = Enumerable.Range(0, 10).Select(x => new Person().WithPersonPeriod(skillA, skillB));
			agentsA.Union(agentsAB).ForEach(x => PersonRepository.Has(x));

			executeTarget(DateOnly.Today.ToDateOnlyPeriod());

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().Skills.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotCreateDuplicatesOfAgents()
		{
			var skill1 = new Skill("1");
			var skill2 = new Skill("2");
			PersonRepository.Has(skill1, skill2);
			PersonRepository.Has(skill2);
			PersonRepository.Has(skill1);

			executeTarget(DateOnly.Today.ToDateOnlyPeriod());

			var theEvent = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			theEvent.AgentsInIsland.Count().Should().Be.EqualTo(3);
			theEvent.Agents.Count().Should().Be.EqualTo(3);
		}

		[Test]
		public void ShouldNotCreateEventsForIslandsIfTeamOptimizationIsUsed()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			executeTarget(DateOnly.Today.ToDateOnlyPeriod(),null, TeamBlockType.Team);

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().Agents.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotCreateEventsForIslandsIfTeamOptimizationIsUsed_OnlySelectedAgents()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			executeTarget(DateOnly.Today.ToDateOnlyPeriod(), new[] {agent1}, TeamBlockType.Team);

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().Agents.Count().Should().Be.EqualTo(1);
		}
		
		private ICommandIdentifier executeTarget(DateOnlyPeriod period, IEnumerable<IPerson> agents = null,TeamBlockType? teamBlockType = null)
		{
			switch (_sut)
			{
				case SUT.Scheduling:
					if (teamBlockType.HasValue)
					{
						SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
						{
							UseTeam = teamBlockType == TeamBlockType.Team
						});
					}
					var schedCmd = new SchedulingCommand {Period = period, AgentsToSchedule = agents};
					SchedulingCommandHandler.Execute(schedCmd);
					return schedCmd;
				case SUT.IntradayOptimization:
					if (teamBlockType.HasValue)
					{
						OptimizationPreferencesProvider.SetFromTestsOnly(new OptimizationPreferences
						{
							Extra = teamBlockType.Value.CreateExtraPreferences()
						});
					}
					var optCmd = new IntradayOptimizationCommand {Period = period, AgentsToOptimize = agents};
					IntradayOptimizationCommandHandler.Execute(optCmd);
					return optCmd;
				default:
					throw new NotSupportedException();
			}
		}
	}
}