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
			var period = new DateOnlyPeriod(2015, 10, 12, 2016,1,1);

			Target.Execute(new IntradayOptimizationCommand {Period = period, Agents = new[] {new Person().WithId()}});

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single()
				.Period.Should().Be.EqualTo(period);
		}

		[Test]
		public void ShouldSetAgentIds()
		{
			var agents = new [] {new Person().WithId()};

			Target.Execute(new IntradayOptimizationCommand { Period = new DateOnlyPeriod(), Agents = agents });

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Single().AgentIds.Single()
				.Should().Be.EqualTo(agents.Single().Id.Value);
		}

		[Test, Ignore("not yet fixed")]
		public void ShouldCreateTwoEventsIfTwoAgentsWithDifferentSkills()
		{
			var agent1 = new Person().WithId();
			agent1.AddSkill(new PersonSkill(new Skill("_", "_", Color.Empty, 1, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)), new Percent(1)), new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			var agent2 = new Person().WithId();
			agent2.AddSkill(new PersonSkill(new Skill("_", "_", Color.Empty, 1, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony)), new Percent(1)), new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));

			Target.Execute(new IntradayOptimizationCommand {Period = new DateOnlyPeriod(), Agents = new[] {agent1, agent2}});

			EventPublisher.PublishedEvents.OfType<OptimizationWasOrdered>().Count()
				.Should().Be.EqualTo(2);
		}
	}
}