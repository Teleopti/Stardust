using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.SeatLimitation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	[DomainTest]
	public class MaxSeatOptimizationTeamBlockTest
	{
		public MaxSeatOptimization Target;
		public GroupScheduleGroupPageDataProvider GroupScheduleGroupPageDataProvider;

		[Test, Ignore("40939")]
		public void ShouldChooseShiftForAllAgentsInTeam()
		{
			var site = new Site("_") { MaxSeats = 2 }.WithId();
			var team = new Team { Description = new Description("_"), Site = site};
			GroupScheduleGroupPageDataProvider.SetBusinessUnit_UseFromTestOnly(BusinessUnitFactory.CreateBusinessUnitAndAppend(team));

			var activity = new Activity("_") { RequiresSeat = true }.WithId();
			var dateOnly = DateOnly.Today;
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(9, 0, 9, 0, 60), new TimePeriodWithSegment(17, 0, 17, 0, 60), new ShiftCategory("_").WithId()));
			var agentScheduledForAnHourData = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, new Team { Site = site }, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 9, 0));
			var agentData1 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var agentData2 = MaxSeatDataFactory.CreateAgentWithAssignment(dateOnly, team, new RuleSetBag(ruleSet), scenario, activity, new TimePeriod(8, 0, 16, 0));
			var schedules = ScheduleDictionaryCreator.WithData(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Assignment, agentData2.Assignment, agentScheduledForAnHourData.Assignment });
			var optPreferences = new OptimizationPreferences
			{
				Extra =
				{
					UseTeams = true,
					TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy),
					UseTeamSameStartTime = true
				}
			};

			Target.Optimize(dateOnly.ToDateOnlyPeriod(), new[] { agentData1.Agent, agentData2.Agent }, schedules, scenario, optPreferences);

			schedules.SchedulesForDay(dateOnly)
				.Count(x => x.PersonAssignment().Period.StartDateTime.TimeOfDay == TimeSpan.FromHours(9))
				.Should().Be.EqualTo(2);
		}
	}
}