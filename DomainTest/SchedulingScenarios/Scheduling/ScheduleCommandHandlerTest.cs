using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SchedulingIslands_44757)]
	public class ScheduleCommandHandlerTest
	{
		public SchedulingCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;

		[Test]
		public void ShouldNotCreateEventsForIslandsWithNoAgentsThatAreSetToBeOptimized()
		{
			var agent1 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var agent2 = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new SchedulingCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), AgentsToSchedule = new[] { agent1 } });

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Single().AgentsToSchedule.Should().Have.SameValuesAs(agent1.Id.Value);
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
			event1.AgentsToSchedule.Single()
				.Should().Not.Be.EqualTo(event2.AgentsToSchedule.Single());
		}
	}
}