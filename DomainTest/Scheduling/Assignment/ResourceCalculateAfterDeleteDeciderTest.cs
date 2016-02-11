using System.Drawing;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.IocCommon;
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

		[Test]
		public void ShouldAlwaysDoCalculationIfNoOtherAgentHasSameSkills()
		{
			var me = new Person();
			var period = new PersonPeriod(new DateOnly(1900, 1, 1), new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team());
			period.AddPersonSkill(new PersonSkill(new Skill("_", "", Color.Empty, 1, new SkillTypePhone(new Description("_"), ForecastSource.OutboundTelephony)), new Percent()));
			me.AddPersonPeriod(period);

			SchedulingResultStateHolder.PersonsInOrganization = new[] {me};

			Target.DoCalculation(me, new DateOnly(2000,1,1))
				.Should().Be.True();
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeSchedulingResultStateHolder>().For<ISchedulingResultStateHolder>();
		}
	}
}