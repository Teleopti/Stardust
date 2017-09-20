using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Islands;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[DomainTest]
	public class AgentsToSkillGroupsTest : ISetup
	{
		public AgentsToSkillSets Target;
		public SkillSetContext Context;

		[Test]
		public void ShouldSplitTwoAgentsWithDifferentSkillsIntoTwoGroups()
		{
			var date = new DateOnly(2000, 1, 1);
			var agent1 = new Person();
			var agent2 = new Person();
			agent1.AddPersonPeriod(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			agent2.AddPersonPeriod(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			agent1.AddSkill(new Skill("_", "_", Color.Empty, 1, new SkillTypePhone(new Description(), ForecastSource.OutboundTelephony) ).WithId(), date);
			agent2.AddSkill(new Skill("_", "_", Color.Empty, 1, new SkillTypePhone(new Description(), ForecastSource.OutboundTelephony) ).WithId(), date);

			using (Context.Create(new[] { agent1, agent2 }, new DateOnlyPeriod(date, date)))
			{
				var result = Target.ToSkillGroups();
				result.Count().Should().Be.EqualTo(2);
				result.First().Count().Should().Be.EqualTo(1);
				result.Last().Count().Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldPlaceTwoAgentsWithSameSkillsIntoOneGroup()
		{
			var date = new DateOnly(2000, 1, 1);
			var agent1 = new Person();
			var agent2 = new Person();
			agent1.AddPersonPeriod(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			agent2.AddPersonPeriod(new PersonPeriod(date, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			var skill = new Skill("_", "_", Color.Empty, 1, new SkillTypePhone(new Description(), ForecastSource.OutboundTelephony)).WithId();
			agent1.AddSkill(skill, date);
			agent2.AddSkill(skill, date);

			using (Context.Create(new[] { agent1, agent2 }, new DateOnlyPeriod(date, date)))
			{
				Target.ToSkillGroups().Single()
					.Should().Have.SameValuesAs(agent1, agent2);
			}
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSchedulingResultStateHolder>().For<ISchedulingResultStateHolder>();
		}
	}
}