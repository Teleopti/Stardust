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
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.Scheduling
{
	[DomainTest]
	[UseIocForFatClient]
	public class SchedulingCallbackTest : SchedulingScenario
	{
		public DesktopScheduling Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderFrom;

		[Test]
		public void ShouldDoSuccessfulCallbacks()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_").WithId();
			var skill = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0)) };
			var agent = new Person().WithId()
					.InTimeZone(TimeZoneInfo.Utc)
					.WithPersonPeriod(ruleSet, contract, skill)
					.WithSchedulePeriodOneWeek(firstDay);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1);
			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);

			var callbackTracker = new TrackSchedulingCallback();
			Target.Execute(callbackTracker, new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			callbackTracker.SuccessfulScheduling().Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldDoUnsuccesfulCallbacks()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_").WithId();
			var skill = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(9, 0, 9, 0, 15), new ShiftCategory("_").WithId()));
			var agent = new Person().WithId()
				.InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet, new Contract("_"), skill)
				.WithSchedulePeriodOneWeek(firstDay);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1);
			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);

			var callbackTracker = new TrackSchedulingCallback();
			Target.Execute(callbackTracker, new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			callbackTracker.UnSuccessfulScheduling().Should().Be.GreaterThanOrEqualTo(7);
		}

		[Test]
		public void ShouldCancel()
		{
			var firstDay = new DateOnly(2017, 5, 15);
			var period = DateOnlyPeriod.CreateWithNumberOfWeeks(firstDay, 1);
			var activity = new Activity("_").WithId();
			var skill = new Skill("A").For(activity).InTimeZone(TimeZoneInfo.Utc).WithId().IsOpen();
			var scenario = new Scenario("_");
			var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(8, 0, 8, 0, 15), new TimePeriodWithSegment(16, 0, 16, 0, 15), new ShiftCategory("_").WithId()));
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(0), TimeSpan.FromHours(90), TimeSpan.FromHours(8), TimeSpan.FromHours(0)) };
			var agent = new Person().WithId()
				.InTimeZone(TimeZoneInfo.Utc)
				.WithPersonPeriod(ruleSet, contract, skill)
				.WithSchedulePeriodOneWeek(firstDay);
			var skillDays = skill.CreateSkillDaysWithDemandOnConsecutiveDays(scenario, firstDay, 1, 1, 1, 1, 1, 1, 1);
			SchedulerStateHolderFrom.Fill(scenario, period, new[] { agent }, Enumerable.Empty<IPersonAssignment>(), skillDays);

			var callbackTracker = new CancelSchedulingCallback();
			Target.Execute(callbackTracker, new SchedulingOptions(), new NoSchedulingProgress(), new[] { agent }, period);
			callbackTracker.NumberOfScheduleAttempts.Should().Be.LessThanOrEqualTo(1);
		}

		public SchedulingCallbackTest(ResourcePlannerTestParameters resourcePlannerTestParameters) : base(resourcePlannerTestParameters)
		{
		}
	}
}