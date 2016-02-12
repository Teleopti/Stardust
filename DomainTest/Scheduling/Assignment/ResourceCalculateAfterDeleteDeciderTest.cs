using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[DomainTest]
	[Toggle(Toggles.ResourcePlanner_SkillGroupDeleteAfterCalculation_37048)]
	public class ResourceCalculateAfterDeleteDeciderTest : ISetup
	{
		public IResourceCalculateAfterDeleteDecider Target;
		public FakeSchedulingResultStateHolder SchedulingResultStateHolder;
		public LimitForNoResourceCalculation LimitForNoResourceCalculation;

		[Test]
		public void ShouldAlwaysDoCalculationIfNoOtherAgentHasSameSkills()
		{
			var me = new Person();
			var period = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
			period.AddPersonSkill(new PersonSkill(new Skill("_", "", Color.Empty, 1, new SkillTypePhone(new Description("_"), ForecastSource.OutboundTelephony)).WithId(), new Percent()));
			me.AddPersonPeriod(period);

			SchedulingResultStateHolder.PersonsInOrganization = new[] { me };

			Target.DoCalculation(me, new DateOnly(2000,1,1))
				.Should().Be.True();
		}

		[Test]
		public void ShouldNotDoCalculationIfAgentsWithSameSkillIsOverLimit()
		{
			LimitForNoResourceCalculation.SetFromTest(1);

			var date = new DateOnly(2000, 1, 1);
			var agent1 = new Person();
			var agent2 = new Person();
			agent1.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			agent2.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			var skill = new Skill("_", "", Color.Empty, 1, new SkillTypePhone(new Description("_"), ForecastSource.OutboundTelephony)).WithId();
			agent1.AddSkill(skill, date);
			agent2.AddSkill(skill, date);
			SchedulingResultStateHolder.PersonsInOrganization = new[] { agent1, agent2 };

			Target.DoCalculation(agent1, date)
				.Should().Be.False();
		}

		[Test]
		public void ShouldNotDoCalculationIfAgentsWithSameSkillIsSameAsLimit()
		{
			LimitForNoResourceCalculation.SetFromTest(2);

			var date = new DateOnly(2000, 1, 1);
			var agent1 = new Person();
			var agent2 = new Person();
			agent1.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			agent2.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			var skill = new Skill("_", "", Color.Empty, 1, new SkillTypePhone(new Description("_"), ForecastSource.OutboundTelephony)).WithId();
			agent1.AddSkill(skill, date);
			agent2.AddSkill(skill, date);
			SchedulingResultStateHolder.PersonsInOrganization = new[] { agent1, agent2 };

			Target.DoCalculation(agent1, date)
				.Should().Be.False();
		}

		[Test]
		public void ShouldDoCalculationIfAgentsWithSameSkillIsUnderLimit()
		{
			LimitForNoResourceCalculation.SetFromTest(3);

			var date = new DateOnly(2000, 1, 1);
			var agent1 = new Person();
			var agent2 = new Person();
			agent1.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			agent2.AddPersonPeriod(new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team()));
			var skill = new Skill("_", "", Color.Empty, 1, new SkillTypePhone(new Description("_"), ForecastSource.OutboundTelephony)).WithId();
			agent1.AddSkill(skill, date);
			agent2.AddSkill(skill, date);
			SchedulingResultStateHolder.PersonsInOrganization = new[] { agent1, agent2 };

			Target.DoCalculation(agent1, date)
				.Should().Be.True();
		}


		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSchedulingResultStateHolder>().For<ISchedulingResultStateHolder>();
		}
	}
}