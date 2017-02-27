using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	public class IntradayOptimizationClassicTest : ISetup
	{
		public OptimizationExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		[Ignore("bug #43149")]
		public void ShouldHandleAlterBetweenInFarAwayTimeZone()
		{
			var activity = new Activity("_") {InWorkTime = true}.WithId();
			var dateOnly = new DateOnly(2017, 02, 27);
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo()).WithPersonPeriod(ruleSet, new Team()).WithSchedulePeriodOneDay(dateOnly);
			var asses = dateOnly.ToDateOnlyPeriod().Inflate(7).DayCollection()
				.Select(date => new PersonAssignment(agent, scenario, date)
				.WithLayer(activity, new TimePeriod(23, 24 + 7)));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, asses, Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = { TeamGroupPage = new GroupPageLight("_", GroupPageType.SingleAgent) },
				Shifts = {AlterBetween = true, SelectedTimePeriod = new TimePeriod(0, 24)}
			};

			Target.Execute(null, new NoSchedulingProgress(), stateHolder, new[] { stateHolder.Schedules[agent].ScheduledDay(dateOnly) }, optPreferences, false, null, null);

			//don't know what to assert on yet...
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder, ISynchronizeIntradayOptimizationResult, IOptimizationPreferencesProvider, IPeopleInOrganization>();
		}
	}
}