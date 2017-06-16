using System;
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
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[TestFixture(typeof(FakeCancelSchedulingProgress), true, true)]
	[TestFixture(typeof(FakeCancelSchedulingProgress), false, false)]
	[TestFixture(typeof(FakeCancelSchedulingProgress), true, false)]
	[TestFixture(typeof(FakeCloseSchedulingProgress), true, true)]
	[TestFixture(typeof(FakeCloseSchedulingProgress), false, false)]
	[TestFixture(typeof(FakeCloseSchedulingProgress), true, false)]
	public class DoNotRunNightRestWhiteSpotDesktopTest : SchedulingScenario, ISetup
	{
		private readonly Type _schedulingProgressFake;
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolder;
		public CountCallsToNightRestWhiteSpotSolverServiceFactory NightRestWhiteSpotSolverService;

		[Test]
		public void ShouldNotRunNightlyRestIfCancelled()
		{
			var schedulingProgress = (ISchedulingProgress)Activator.CreateInstance(_schedulingProgressFake);
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
			var agent = new Person().WithPersonPeriod(ruleSet, skill).WithSchedulePeriodOneWeek(firstDay);
			var skillDays = skill.CreateSkillDayWithDemand(scenario, firstDay, 1);
			SchedulerStateHolder.Fill(scenario, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);

			Target.Execute(schedulingCallback, schedulingOptions, schedulingProgress, new[]{agent}, DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1), null, null);

			NightRestWhiteSpotSolverService.NumberOfNightRestWhiteSpotServiceCalls
				.Should().Be.EqualTo(0);
		}

		public DoNotRunNightRestWhiteSpotDesktopTest(Type schedulingProgressFake, bool resourcePlannerMergeTeamblockClassicScheduling44289, bool resourcePlannerSchedulingIslands44757) 
			: base(resourcePlannerMergeTeamblockClassicScheduling44289, resourcePlannerSchedulingIslands44757)
		{
			_schedulingProgressFake = schedulingProgressFake;
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDoubleForType(_schedulingProgressFake).For<ISchedulingProgress>();
			system.UseTestDouble<CountCallsToNightRestWhiteSpotSolverServiceFactory>().For<INightRestWhiteSpotSolverServiceFactory>();
		}
	}
}
