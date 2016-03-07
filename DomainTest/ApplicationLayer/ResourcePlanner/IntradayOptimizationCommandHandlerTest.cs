using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.ResourcePlanner;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.TestCommon;
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

		[Test]
		public void ShouldSetPeriod()
		{
			var agent = new Person().WithId();
			var personPeriod = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
			personPeriod.AddPersonSkill(new PersonSkill(new Skill("_", "_", Color.Empty, 1, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)).WithId(), new Percent(1)));
			agent.AddPersonPeriod(personPeriod);
			var period = new DateOnlyPeriod(2015, 10, 12, 2016,1,1);

			Target.Execute(new IntradayOptimizationCommand {Period = period, Agents = new[] {agent}});

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single()
				.Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldSetAgentIds()
		{
			var agent = new Person().WithId();
			var period = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
			period.AddPersonSkill(new PersonSkill(new Skill("_", "_", Color.Empty, 1, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)).WithId(), new Percent(1)));
			agent.AddPersonPeriod(period);

			Target.Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10), Agents = new [] { agent }});

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single().AgentIds.Single()
				.Should().Be.EqualTo(agent.Id.Value);
		}

		[Test]
		public void ShouldCreateTwoEventsIfTwoAgentsWithDifferentSkills()
		{
			var agent1 = new Person().WithId();
			var period1 = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
			period1.AddPersonSkill(new PersonSkill(new Skill("_", "_", Color.Empty, 1, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)).WithId(), new Percent(1)));
			agent1.AddPersonPeriod(period1);
			var agent2 = new Person().WithId();
			var period2 = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
			period2.AddPersonSkill(new PersonSkill(new Skill("_", "_", Color.Empty, 1, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)).WithId(), new Percent(1)));
			agent2.AddPersonPeriod(period2);

			Target.Execute(new IntradayOptimizationCommand
			{
				Period = new DateOnlyPeriod(2000, 1, 1, 2000, 1, 10),
				Agents = new[] {agent1, agent2}
			});

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count()
				.Should().Be.EqualTo(2);
		}
	}
}