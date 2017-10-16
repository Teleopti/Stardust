using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingDesktopBlockContractTimeTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;

		private readonly TimeSpan _expectedContractTime;

		public SchedulingDesktopBlockContractTimeTest(bool runInSeperateWebRequest, bool resourcePlannerEasierBlockScheduling46155) : base(runInSeperateWebRequest, resourcePlannerEasierBlockScheduling46155)
		{
			_expectedContractTime = TimeSpan.FromHours(resourcePlannerEasierBlockScheduling46155 ? 176 : 160);
		}
		
		[TestCase(true, false)]
		[TestCase(false, true)]
		public void ShouldHandleBlockBetweenDaysOffWhenMonthEndsOnTuesday(bool sameShiftCategory, bool sameShift)
		{
			var period = new DateOnlyPeriod(2017, 10, 1, 2017, 11, 4);
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).WithId().InTimeZone(TimeZoneInfo.Utc).IsOpen(new TimePeriod(8, 20));
			var scenario = new Scenario();
			var shiftCategory = new ShiftCategory("_").WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity,
				new TimePeriodWithSegment(8, 0, 12, 0, 60),
				new TimePeriodWithSegment(16, 0, 20, 0, 60), shiftCategory));
			ruleSet.AddLimiter(new ContractTimeLimiter(new TimePeriod(8, 8), TimeSpan.FromMinutes(60)));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet, new ContractScheduleWorkingMondayToFriday(), skill)
				.WithSchedulePeriodOneMonth(period.StartDate);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, period, 1);
			var stateHolder = SchedulerStateHolder.Fill(scenario, period, new[] {agent}, Enumerable.Empty<IScheduleData>(), skillDays);
			var schedulingOptions = new SchedulingOptions
			{
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff,
				BlockSameShiftCategory = sameShiftCategory,
				BlockSameShift = sameShift,
				UseBlock = true
			};

			Target.Execute(new NoSchedulingCallback(), schedulingOptions, new NoSchedulingProgress(), new[] {agent}, period);

			stateHolder.Schedules[agent].CalculatedCurrentScheduleSummary(new DateOnlyPeriod(2017, 10, 1, 2017, 10, 31))
				.ContractTime.Should().Be.EqualTo(_expectedContractTime);
		}
	}
}