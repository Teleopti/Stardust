﻿using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class ScheduleCommandHandlerTest
	{
		public SchedulingCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;
		//remove me when islands works with team
		public SchedulingOptionsProvider SchedulingOptionsProvider;

		[Test]
		public void ShouldNotCreateEventsForIslandsWithNoAgentsThatAreSetToBeOptimized()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new SchedulingCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToSchedule = new[] { agent1 } });

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Single().Agents.Should().Have.SameValuesAs(agent1.Id.Value);
		}

		[Test]
		public void ShouldSetSpecificAgentsToOptimizeWhenMultipleIsland()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new SchedulingCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToSchedule = new[] { agent1, agent2 } });

			var events = EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().ToArray();
			var event1 = events[0];
			var event2 = events[1];
			event1.Agents.Single()
				.Should().Not.Be.EqualTo(event2.Agents.Single());
		}

		[Test]
		public void ShouldNotCreateEventsForIslandsIfTeamSchedulingIsUsed()
		{
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag)
			});

			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new SchedulingCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10) });

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Single().Agents.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotCreateEventsForIslandsIfTeamSchedulingIsUsed_OnlySelectedAgents()
		{
			SchedulingOptionsProvider.SetFromTest(new SchedulingOptions
			{
				UseTeam = true,
				GroupOnGroupPageForTeamBlockPer = new GroupPageLight("_", GroupPageType.RuleSetBag)
			});

			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new SchedulingCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToSchedule = new[] { agent1 } });

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Single().Agents.Count().Should().Be.EqualTo(1);
		}
	}
}