using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Islands.CommandHandler
{
	public class CommandHandlerGeneralTest : ResourcePlannerCommandHandlerTest
	{
		public FakeEventPublisher EventPublisher;
		public MergeIslandsSizeLimit MergeIslandsSizeLimit;

		[Test]
		public void ShouldSetPeriod()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016,1,1);
			PersonRepository.Has(agent);

			ExecuteTarget(period);

			var optimizationWasOrdered = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			optimizationWasOrdered.StartDate.Should().Be.EqualTo(period.StartDate);
			optimizationWasOrdered.EndDate.Should().Be.EqualTo(period.EndDate);
		}

		[Test]
		public void ShouldSetAgentIds()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent);

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().AgentsInIsland.Single().Should().Be.EqualTo(agent.Id.Value);
		}

		[Test]
		public void ShouldCreateTwoEventsIfTwoAgentsWithDifferentSkills()
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			var skill1 = new Skill().WithId();
			var skill2 = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill1);
			var agent2 = new Person().WithId().WithPersonPeriod(skill2);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

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

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldCreateTwoEventsIfSameSkillsButDiffersDueToPrimarySkill()
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			var skill1 = new Skill().WithId().CascadingIndex(1);
			var skill2 = new Skill().WithId().CascadingIndex(2);
			var agent1 = new Person().WithId().WithPersonPeriod(skill1, skill2);
			var agent2 = new Person().WithId().WithPersonPeriod(skill2);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10));

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

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1, agent2});

			var @event = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			@event.Agents.Should().Have.SameValuesAs(@event.AgentsInIsland);
		}

		[Test]
		public void ShouldSetSpecificAgentsToOptimizeWhenOneIsland()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1});

			var @event = EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single();
			@event.Agents.Should().Have.SameValuesAs(agent1.Id.Value);
		}

		[Test]
		public void ShouldSetSpecificAgentsToOptimizeWhenMultipleIsland()
		{
			MergeIslandsSizeLimit.TurnOff_UseOnlyFromTest();
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1, agent2});

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

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1});
			
			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().Agents.Should().Have.SameValuesAs(agent1.Id.Value);
		}

		[Test]
		public void ShouldSetRandomCommandIdOnCommand()
		{
			var command1 = ExecuteTarget(DateOnly.Today.ToDateOnlyPeriod());
			var command2 = ExecuteTarget(DateOnly.Today.ToDateOnlyPeriod());
			command1.CommandId.Should().Not.Be.EqualTo(Guid.Empty);
			command1.CommandId.Should().Not.Be.EqualTo(command2.CommandId);
		}

		[Test]
		public void ShouldSetCommandIdOnEvent()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016, 1, 1);
			PersonRepository.Has(agent);
			
			var command = ExecuteTarget(period);

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().CommandId.Should().Be.EqualTo(command.CommandId);
		}


		[Test]
		public void NoDuplicateSkillsInEventWhenMultipleSkillSetsContainsSameSkill()
		{
			var skillA = new Skill("A").WithId();
			var skillB = new Skill("B").WithId();
			var agentsA = Enumerable.Range(0, 10).Select(x => new Person().WithPersonPeriod(skillA).WithId());
			var agentsAB = Enumerable.Range(0, 10).Select(x => new Person().WithPersonPeriod(skillA, skillB).WithId());
			agentsA.Union(agentsAB).ForEach(x => PersonRepository.Has(x));

			ExecuteTarget(DateOnly.Today.ToDateOnlyPeriod());

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

			ExecuteTarget(DateOnly.Today.ToDateOnlyPeriod());

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

			ExecuteTarget(DateOnly.Today.ToDateOnlyPeriod(), new[]{agent1, agent2}, TeamBlockType.Team);

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().Agents.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotCreateEventsForIslandsIfTeamOptimizationIsUsed_OnlySelectedAgents()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			ExecuteTarget(DateOnly.Today.ToDateOnlyPeriod(), new[] {agent1}, TeamBlockType.Team);

			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().Agents.Count().Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldIncludeOtherAgentInIslandsIfSameTeam()
		{
			var team = new Team().WithId();
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(team, skill);
			var agent2 = new Person().WithId().WithPersonPeriod(team, skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1}, TeamBlockType.Team);
			
			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().AgentsInIsland
				.Should().Have.SameValuesAs(agent1.Id.Value, agent2.Id.Value);
		}
		
		[Test]
		public void ShouldIncludeOtherAgentsSkillInIslandsIfSameTeam()
		{
			var team = new Team().WithId();
			var skill1 = new Skill().WithId();
			var skill2 = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(team, skill1);
			var agent2 = new Person().WithId().WithPersonPeriod(team, skill1, skill2);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			ExecuteTarget(new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), new[] {agent1}, TeamBlockType.Team);
			
			EventPublisher.PublishedEvents.OfType<IIslandInfo>().Single().Skills
				.Should().Have.SameValuesAs(skill1.Id.Value, skill2.Id.Value);
		}
		
		public CommandHandlerGeneralTest(SUT sut) : base(sut)
		{
		}
	}
}