using System;
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
	[TestFixture]
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SchedulingIslands_44757)]
	public class SchedulingCommandHandlerTest
	{
		public SchedulingCommandHandler Target;
		public FakeEventPublisher EventPublisher;
		public FakePersonRepository PersonRepository;

		[Test]
		//green from start, if purist remove
		public void ShouldSetPeriod()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016, 1, 1);
			PersonRepository.Has(agent);

			Target.Execute(new SchedulingCommand
			{
				Period = period,
				AgentsToSchedule = new[] { agent }
			});

			var optimizationWasOrdered = EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Single();
			optimizationWasOrdered.StartDate.Should().Be.EqualTo(period.StartDate);
			optimizationWasOrdered.EndDate.Should().Be.EqualTo(period.EndDate);
		}

		[Test]
		//green from start, if purist remove
		public void ShouldSetAgentsToScheduleIds()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			PersonRepository.Has(agent);

			Target.Execute(new SchedulingCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10),
				AgentsToSchedule = new[] { agent }
			});

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>()
				.Single()
				.AgentsToSchedule.Single()
				.Should()
				.Be.EqualTo(agent.Id.Value);
		}

		[Test]
		//green from start, if purist remove
		public void ShouldSetRandomCommandIdOnCommand()
		{
			var command1 = new SchedulingCommand();
			var command2 = new SchedulingCommand();
			command1.CommandId.Should().Not.Be.EqualTo(Guid.Empty);
			command1.CommandId.Should().Not.Be.EqualTo(command2.CommandId);
		}

		[Test]
		//green from start, if purist remove
		public void ShouldSetCommandIdOnEvent()
		{
			var agent = new Person().WithId().WithPersonPeriod(new Skill().WithId());
			var period = new DateOnlyPeriod(2015, 10, 12, 2016, 1, 1);
			PersonRepository.Has(agent);
			var command = new SchedulingCommand { Period = period, AgentsToSchedule  = new[]{agent} };

			Target.Execute(command);

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Single().CommandId.Should().Be.EqualTo(command.CommandId);
		}

		[Test]
		//green from start, if purist remove
		public void ShouldSetSpecificAgentsToOptimizeWhenOneIsland()
		{
			var skill = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill);
			var agent2 = new Person().WithId().WithPersonPeriod(skill);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new SchedulingCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10),
				AgentsToSchedule = new[] {agent1}
			});

			var @event = EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Single();
			@event.AgentsToSchedule.Should().Have.SameValuesAs(agent1.Id.Value);
		}

		[Test]
		[Ignore("#45197")]
		public void ShouldCreateTwoEventsIfTwoAgentsWithDifferentSkills()
		{
			var skill1 = new Skill().WithId();
			var skill2 = new Skill().WithId();
			var agent1 = new Person().WithId().WithPersonPeriod(skill1);
			var agent2 = new Person().WithId().WithPersonPeriod(skill2);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new SchedulingCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10),
				AgentsToSchedule = new[] {agent1, agent2}
			});

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Count().Should().Be.EqualTo(2);
		}

		[Test]
		[Ignore("#45197 - will probably go green when ShouldCreateTwoEventsIfTwoAgentsWithDifferentSkills is fixed")]
		public void ShouldCreateTwoEventsIfSameSkillsButDiffersDueToPrimarySkill()
		{
			var skill1 = new Skill().WithId().CascadingIndex(1);
			var skill2 = new Skill().WithId().CascadingIndex(2);
			var agent1 = new Person().WithId().WithPersonPeriod(skill1, skill2);
			var agent2 = new Person().WithId().WithPersonPeriod(skill2);
			PersonRepository.Has(agent1);
			PersonRepository.Has(agent2);

			Target.Execute(new SchedulingCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10),
				AgentsToSchedule = new[] { agent1, agent2 }
			});

			EventPublisher.PublishedEvents.OfType<SchedulingWasOrdered>().Count().Should().Be.EqualTo(2);
		}
	}
}
