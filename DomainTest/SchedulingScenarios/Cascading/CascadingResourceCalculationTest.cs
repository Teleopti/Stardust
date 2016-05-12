using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Cascading
{
	[DomainTest]
	public class CascadingResourceCalculationTest
	{
		public CascadingResourceCalculation Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
			
		[Test]
		public void ShouldCalculateNonCascadingSkills()
		{
			var scenario = new Scenario("_");
			var activity = new Activity("_");
			var dateOnly = DateOnly.Today;
			var skill = new Skill("_", "_", Color.Empty, 15, new SkillTypePhone(new Description(), ForecastSource.InboundTelephony))
				{
					Activity = activity,
					TimeZone = TimeZoneInfo.Utc
				};
			WorkloadFactory.CreateWorkloadWithOpenHours(skill, new TimePeriod(8, 0, 9, 0));
			var skillDay = skill.CreateSkillDayWithDemand(scenario, dateOnly, TimeSpan.FromMinutes(30)); //15 minuter per intervall
			var agent = new Person();
			agent.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			agent.AddPeriodWithSkill(new PersonPeriod(DateOnly.MinValue, new PersonContract(new Contract("_"), new PartTimePercentage("_"), new ContractSchedule("_")), new Team { Site = new Site("_") }), skill);
			var assignment = new PersonAssignment(agent, scenario, dateOnly);
			assignment.AddActivity(activity, new TimePeriod(5, 0, 10, 0)); //1 snubbe per interval
			SchedulerStateHolder.Fill(scenario, new DateOnlyPeriod(dateOnly, dateOnly), new[] { agent }, new[] {assignment}, skillDay);

			Target.ForDay(dateOnly);

			skillDay.SkillStaffPeriodCollection.First().AbsoluteDifference
				.Should().Be.EqualTo(-1); 
		}
	}
}