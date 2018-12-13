using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingDesktopDifferenceSnapshotTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		[Ignore("#79787 Bug 2 be fixed")]
		public void ShouldNotResultInDirtySnapshotWhenHavingPAPlusAbsenceDayAfterScheduledDay()
		{
			var date = new DateOnly(2018, 9, 10);
			var activity = new Activity().WithId();
			var absence = new Absence().WithId();
			var skill = new Skill().For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 60), new TimePeriodWithSegment(16, 0, 16, 0, 60), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneDay(date);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(date, 1), 1);
			var absenceOnNextDay = new PersonAbsence(agent, scenario, new AbsenceLayer(absence, date.AddDays(1).ToDateTimePeriod(new TimePeriod(2, 3), agent.PermissionInformation.DefaultTimeZone())));
			var assNextDay = new PersonAssignment(agent, scenario, date.AddDays(1));
			var schedulerStateHolder = SchedulerStateHolderFrom.Fill(scenario, date.ToDateOnlyPeriod(), agent, new IScheduleData[]{absenceOnNextDay, assNextDay}, skillDays);
			
			Target.Execute(new NoSchedulingCallback(), new SchedulingOptions(), new NoSchedulingProgress(), new[]{agent}, date.ToDateOnlyPeriod());

			foreach (var differenceCollectionItem in schedulerStateHolder.Schedules.DifferenceSinceSnapshot())
			{
				if (differenceCollectionItem.CurrentItem is IPersonAssignment pa && pa.Date != date)
				{
					Assert.Fail("Should not modify personassignment on {0}!", pa.Date);
				}
			}
		}

		public SchedulingDesktopDifferenceSnapshotTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}