using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ResourcePlanner
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_IntradayIslands_36939)]
	public class IntradayOptimizationCommandHandlerTest
	{
		public IIntradayOptimizationCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;

		[Test]
		public void ShouldSetPeriod()
		{
			var agent = new Person().WithId();
			agent.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016,1,1);
			PersonRepository.Has(agent);

			Target.Execute(new IntradayOptimizationCommand {Period = period });

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single()
				.Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldSetAgentIds()
		{
			var agent = new Person().WithId();
			agent.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), new Skill().WithId());
			PersonRepository.Has(agent);

			Target.Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10) });

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single().AgentsInIsland.Single()
				.Should().Be.EqualTo(agent.Id.Value);
		}

		[Test]
		public void ShouldCreateTwoEventsIfTwoAgentsWithDifferentSkills()
		{
			var skill1 = new Skill().WithId();
			var skill2 = new Skill().WithId();
			var agent1 = new Person().WithId();
			agent1.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), skill1);
			var agent2 = new Person().WithId();
			agent2.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), skill2);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new IntradayOptimizationCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10)
			});

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count()
				.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldCreateOneEventIfTwoAgentsWithAtLeastOneSkillTheSame()
		{
			var skill1 = new Skill().WithId();
			var skill2 = new Skill().WithId();
			var skill3 = new Skill().WithId();
			var agent1 = new Person().WithId();
			agent1.AddPeriodWithSkills(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), 
				new[] { skill1, skill2});
			var agent2 = new Person().WithId();
			agent2.AddPeriodWithSkills(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), 
				new[] { skill2, skill3 });
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new IntradayOptimizationCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10)
			});

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count()
				.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldSetAgentToOptimizeWhenOptimizeFullIsland()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId();
			agent1.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), skill);
			var agent2 = new Person().WithId();
			agent2.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = new [] {agent1, agent2} });

			var @event = EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single();
			@event.AgentsToOptimize
				.Should().Have.SameValuesAs(@event.AgentsInIsland);
		}

		[Test]
		public void ShouldSetAgentsToOptimizeToAllInIslandIfNullIsPassed()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId();
			agent1.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), skill);
			var agent2 = new Person().WithId();
			agent2.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = null });

			var @event = EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single();
			@event.AgentsToOptimize
				.Should().Have.SameValuesAs(agent1.Id.Value, agent2.Id.Value);
		}

		[Test]
		public void ShouldSetSpecificAgentsToOptimizeWhenOneIsland()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId();
			agent1.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), skill);
			var agent2 = new Person().WithId();
			agent2.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = new[] {agent1} });

			var @event = EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single();
			@event.AgentsToOptimize
				.Should().Have.SameValuesAs(agent1.Id.Value);
		}

		[Test]
		public void ShouldSetSpecificAgentsToOptimizeWhenMultipleIsland()
		{
			var agent1 = new Person().WithId();
			agent1.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), new Skill().WithId());
			var agent2 = new Person().WithId();
			agent2.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = new[] { agent1, agent2 } });

			var events = EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().ToArray();
			var event1 = events[0];
			var event2 = events[1];
			event1.AgentsToOptimize.Single()
				.Should().Not.Be.EqualTo(event2.AgentsToOptimize.Single());
		}

		[Test]
		public void ShouldNotCreateEventsForIslandsWithNoAgentsThatAreSetToBeOptimized()
		{
			var agent1 = new Person().WithId();
			agent1.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), new Skill().WithId());
			var agent2 = new Person().WithId();
			agent2.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToOptimize = new[] { agent1 } });

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single().AgentsToOptimize
				.Should().Have.SameValuesAs(agent1.Id.Value);
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
			var agent = new Person().WithId();
			agent.AddPeriodWithSkill(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()), new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016, 1, 1);
			PersonRepository.Has(agent);
			var command = new IntradayOptimizationCommand { Period = period };

			Target.Execute(command);

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single().CommandId
				.Should().Be.EqualTo(command.CommandId);
		}
	}
}