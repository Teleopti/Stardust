﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class DoNotRunNightRestWhiteSpotDesktopTest : SchedulingScenario, ISetup
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public CountCallsToNightRestWhiteSpotSolverServiceFactory NightRestWhiteSpotSolverService;

		[Test]
		public void ShouldNotRunNightlyRestIfCancelled([Values(typeof(FakeCancelSchedulingProgress), typeof(FakeCloseSchedulingProgress))] Type shedulingProgressType)
		{
			var schedulingProgress = (ISchedulingProgress)Activator.CreateInstance(shedulingProgressType);
			var schedulingCallback = new CancelSchedulingCallback();
			var schedulingOptions = new SchedulingOptions
			{
				RefreshRate = 1 //to force ISchedulingProgress to be called so our "cancel click" will be triggered
			};
			var firstDay = new DateOnly(2015, 10, 12);
			var scenario = new Scenario("_");
			var activity = new Activity().WithId();
			var skill = new Skill().For(activity).IsOpen().WithId();
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay).WithId();
			var skillDays = skill.CreateSkillDayWithDemand(scenario, firstDay, 1);
			SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);

			Target.Execute(schedulingCallback, schedulingOptions, schedulingProgress, new[]{agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1));

			NightRestWhiteSpotSolverService.NumberOfNightRestWhiteSpotServiceCalls
				.Should().Be.EqualTo(0);
		}


		public override void Setup(ISystem system, IIocConfiguration configuration)
		{
			base.Setup(system, configuration);
			system.UseTestDouble<CountCallsToNightRestWhiteSpotSolverServiceFactory>().For<INightRestWhiteSpotSolverServiceFactory>();
		}

		public DoNotRunNightRestWhiteSpotDesktopTest(SeperateWebRequest seperateWebRequest, RemoveClassicShiftCategory resourcePlannerRemoveClassicShiftCat46582, RemoveImplicitResCalcContext removeImplicitResCalcContext46680, bool resourcePlannerBpoScheduling46265) : base(seperateWebRequest, resourcePlannerRemoveClassicShiftCat46582, removeImplicitResCalcContext46680)
		{
		}
	}
}
