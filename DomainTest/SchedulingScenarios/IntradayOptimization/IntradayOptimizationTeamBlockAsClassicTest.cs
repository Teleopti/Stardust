using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	public class IntradayOptimizationTeamBlockAsClassicTest : ISetup
	{
		public IOptimizationCommand Target;
		public Func<ISchedulerStateHolder> SchedulerStateHolderOrg;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldNotRollBackIfSingleAgentSingleDayAndPeriodValueIsNotBetter()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var date = new DateOnly(2014, 4, 1);
			var scenario = new Scenario("Default").WithId();
			var shiftCategory = new ShiftCategory("DY").WithId();
			var shiftCategoryAm = new ShiftCategory("AM").WithId();
			var activity = new Activity("Phone");
			var skill = new Skill().WithId().For(activity).InTimeZone(TimeZoneInfo.Utc);
			var workShiftRuleSet =new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 7, 0, 15), new TimePeriodWithSegment(15, 0, 15, 0, 15), shiftCategoryAm));
			var agent = new Person().WithPersonPeriod(workShiftRuleSet, skill).WithId().InTimeZone(TimeZoneInfo.Utc).WithSchedulePeriodOneDay(date);
			var ass = new PersonAssignment(agent, scenario, date).WithLayer(activity, new TimePeriod(8, 16)).ShiftCategory(shiftCategory);
			var skillDay = createSkillDay(skill, date, scenario);
			var stateHolder = SchedulerStateHolderOrg.Fill(scenario, date, new [] {agent}, new[] {ass}, skillDay);
			var optimizationPreferences = new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepShiftsWithinDay = true },
				Extra = new ExtraPreferences { UseTeamBlockOption = true, UseBlockSameShiftCategory = true }
			};

			Target.Execute(null, new NoSchedulingProgress(), stateHolder, new[] {stateHolder.Schedules[agent].ScheduledDay(date)}, optimizationPreferences, false, new DaysOffPreferences(), new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			stateHolder.Schedules[agent].ScheduledDay(date).PersonAssignment().ShiftCategory
				.Should().Be.EqualTo(shiftCategoryAm);
		}

		private static SkillDay createSkillDay(ISkill skill, DateOnly date, IScenario scenario)
		{
			var dateAsUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
			var period = new DateTimePeriod(dateAsUtc, dateAsUtc.AddHours(0.25));

			var skillDay = new SkillDay(date, skill, scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
			skillDay.SetupSkillDay();
			var newSkillStaffPeriodValues = new NewSkillStaffPeriodValues(Enumerable.Range(0, 96).Select(i =>
			{
				var skillStaffPeriod = new SkillStaffPeriod(period.MovePeriod(TimeSpan.FromMinutes(i*15)),
					new Task(2, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2)),
					ServiceAgreement.DefaultValues(), skill.SkillType.StaffingCalculatorService);
				skillStaffPeriod.Payload.ManualAgents = 0;
				return skillStaffPeriod;
			}).ToArray());
			skillDay.SetCalculatedStaffCollection(newSkillStaffPeriodValues);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay},
				new DateOnlyPeriod(2014, 3, 22, 2014, 4, 7));
			newSkillStaffPeriodValues.BatchCompleted();
			return skillDay;
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<DesktopOptimizationContext>().For<IFillSchedulerStateHolder>();
		}
	}
}