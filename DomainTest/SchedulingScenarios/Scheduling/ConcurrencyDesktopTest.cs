using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class ConcurrencyDesktopTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;
		
		[Test]
		public void ShouldNotCrashDueToMultipleNonExistingSchedulingRangesWhenFillingSchedulerStateHolder()
		{
			const int agentsInEachIsland = 10;
			const int numberOfIslands = 10;
			var date = new DateOnly(2017, 1, 10);
			var scenario = new Scenario("_");
			var agents = new List<IPerson>();
			var skillDays = new List<ISkillDay>();
			var activity = new Activity().WithId();
			for (var i = 0; i < numberOfIslands; i++)
			{
				var skill = new Skill().For(activity).WithId().IsOpen();
				var skillDay = skill.CreateSkillDayWithDemand(scenario, date, 10);
				skillDays.Add(skillDay);
				for (var j = 0; j < agentsInEachIsland; j++)
				{
					agents.Add(new Person().WithId()
						.WithPersonPeriod(skill)
						.WithSchedulePeriodOneWeek(date));					
				}
			}
			SchedulerStateHolderFrom.Fill(scenario, new DateOnly(2017, 1, 10), agents, skillDays);

			Assert.DoesNotThrow(() =>
			{
				Target.Execute(new NoSchedulingCallback(),
					new SchedulingOptions(), 
					new NoSchedulingProgress(),
					agents, 
					new DateOnlyPeriod(date, date.AddDays(1))
				);
			});
		}

		public ConcurrencyDesktopTest(SeperateWebRequest seperateWebRequest, RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582, bool resourcePlannerTimeZoneIssues45818, bool resourcePlannerXxl47258) : base(seperateWebRequest, resourcePlannerRemoveClassicShiftCat46582, resourcePlannerTimeZoneIssues45818, resourcePlannerXxl47258)
		{
		}
	}
}