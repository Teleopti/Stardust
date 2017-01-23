using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	public class ShiftCategoryLimitationTest
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test, Ignore("Starting with some experimention for 42680")]
		public void fOo()
		{
			var date = new DateOnly(2017, 1, 22);
			var shiftCategory = new ShiftCategory("_");
			var scenario = new Scenario("_");
			var optimizerOriginalPreferences = new OptimizerOriginalPreferences
			{
				SchedulingOptions =
				{
					GroupOnGroupPageForTeamBlockPer = new GroupPageLight("not interesting", GroupPageType.SingleAgent),
					UseTeam = true
				}
			};
			var agent = new Person().WithSchedulePeriodOneWeek(date).WithPersonPeriod();
			agent.SchedulePeriod(date).AddShiftCategoryLimitation(new ShiftCategoryLimitation(shiftCategory)
			{
				MaxNumberOf = 3
			});
			var ass = new PersonAssignment(agent, scenario, date).ShiftCategory(shiftCategory);
			var stateholder = SchedulerStateHolderFrom.Fill(scenario, date.ToDateOnlyPeriod(), new [] {agent}, new [] {ass}, Enumerable.Empty<ISkillDay>());
			
			Target.Execute(optimizerOriginalPreferences, new NoSchedulingProgress(), new[] { stateholder.Schedules[agent].ScheduledDay(date) }, null, null);

		}
	}
}