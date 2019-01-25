using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Wfm.SchedulingTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	[UseIocForFatClient]
	[UseEventPublisher(typeof(SyncInFatClientProcessEventPublisher))]
	public class IntradayOptimizationClassicDesktopTest : IntradayOptimizationScenarioTest
	{
		public OptimizationDesktopExecuter Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldNotCrashAlterBetweenInFarAwayTimeZone()
		{
			var activity = new Activity("_") {InWorkTime = true}.WithId();
			var dateOnly = new DateOnly(2017, 02, 27);
			var scenario = new Scenario("_");
			var orgShiftCat = new ShiftCategory("org").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(8, 0, 8, 0, 60), new ShiftCategory("new").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfoFactory.ChinaTimeZoneInfo()).WithPersonPeriod(ruleSet, new Team()).WithSchedulePeriodOneDay(dateOnly);
			var asses = dateOnly.ToDateOnlyPeriod().Inflate(7).DayCollection()
				.Select(date => new PersonAssignment(agent, scenario, date).ShiftCategory(orgShiftCat).WithLayer(activity, new TimePeriod(23, 24 + 7)));
			var stateHolder = SchedulerStateHolderFrom.Fill(scenario, dateOnly.ToDateOnlyPeriod(), new[] { agent }, asses, Enumerable.Empty<ISkillDay>());
			var optPreferences = new OptimizationPreferences
			{
				General = { ScheduleTag = new ScheduleTag() },
				Extra = { TeamGroupPage = new GroupPageLight("_", GroupPageType.SingleAgent) },
				Shifts = {AlterBetween = true, SelectedTimePeriod = new TimePeriod(0, 24)}
			};

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingProgress(), stateHolder, new[]{agent}, dateOnly.ToDateOnlyPeriod(), optPreferences, null);
			});
		}
	}
}