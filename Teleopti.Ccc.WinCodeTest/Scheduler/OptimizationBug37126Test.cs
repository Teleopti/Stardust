using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Scheduling;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[DomainTest, UseIocForFatClient]
	public class OptimizationBug37126Test
	{
		public OptimizationDesktopExecuter Target;
		public IOptimizationPreferences OptimizationPreferences;
		public Func<ISchedulerStateHolder> StateHolder;

		[Test]
		public void ShouldNotCrashWhenDoingRollback()
		{
			var date = new DateOnly(2014, 3, 31);
			var scenario = new Scenario().WithId();
			var shiftCategory = new ShiftCategory().WithId();
			var shiftCategoryAm = new ShiftCategory().WithId();
			var activity = new Activity {InContractTime = true, InWorkTime = true, InPaidTime = true, RequiresSkill = true};
			var skill = new Skill().For(activity).WithId().IsOpen();
			var workShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 9, 0, 15),new TimePeriodWithSegment(15, 0, 17, 0, 15), shiftCategoryAm));
			workShiftRuleSet.AddExtender(new ActivityRelativeStartExtender(new Activity(),new TimePeriodWithSegment(1, 0, 1, 0, 15), new TimePeriodWithSegment(4, 0, 5, 0, 15)));
			var bag = new RuleSetBag(workShiftRuleSet);
			var contract = new Contract("_") { WorkTimeDirective = new WorkTimeDirective(TimeSpan.FromHours(1), TimeSpan.FromHours(80), TimeSpan.FromHours(11), TimeSpan.FromHours(36)) };
			var person = new Person().WithId().InTimeZone(TimeZoneInfo.Utc).WithPersonPeriod(bag, contract, skill).WithSchedulePeriodOneMonth(new DateOnly(2014,3,24));
			var data = new List<IPersistableScheduleData>
			{
				PersonAbsenceFactory.CreatePersonAbsence(person, scenario,new DateTimePeriod(2014, 3, 24, 0, 2014, 3, 25, 0)),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 3, 25, 8, 2014, 3, 25, 22), shiftCategory),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 3, 26, 8, 2014, 3, 26, 22), shiftCategory),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 3, 27, 8, 2014, 3, 27, 22), shiftCategory),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 3, 28, 12, 2014, 3, 28, 20), shiftCategory),
				PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, new DateOnly(2014, 3, 29), DayOffFactory.CreateDayOff()),
				PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, new DateOnly(2014, 3, 30), DayOffFactory.CreateDayOff()),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 3, 31, 8, 2014, 3, 31, 22), shiftCategory),
				new PreferenceDay(person, new DateOnly(2014, 3, 31), new PreferenceRestriction {ShiftCategory = shiftCategory}),
				PersonAssignmentFactory.CreateAssignmentWithDayOff(person, scenario, new DateOnly(2014, 4, 1), DayOffFactory.CreateDayOff()),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 4, 2, 8, 2014, 4, 2, 22), shiftCategory),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 4, 3, 12, 2014, 4, 3, 21), shiftCategory),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 4, 4, 8, 2014, 4, 4, 22), shiftCategory),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 4, 5, 8, 2014, 4, 5, 18), shiftCategory),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 4, 6, 11, 2014, 4, 6, 21), shiftCategory),
				PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity, new DateTimePeriod(2014, 4, 7, 8, 2014, 4, 7, 22), shiftCategory)
			};

			var skillDays = Enumerable.Range(-7, 10).Select(i => createSkillDay(skill, date.AddDays(i), scenario)).ToArray();
			OptimizationPreferences.General = new GeneralPreferences
			{
				OptimizationStepShiftsWithinDay = true,
				PreferencesValue = 0.7,
				UsePreferences = true,
				UseRotations = true,
				RotationsValue = 1,
				UseShiftCategoryLimitations = true,
				ScheduleTag = NullScheduleTag.Instance
			};
			OptimizationPreferences.Advanced = new AdvancedPreferences
			{
				UseMinimumStaffing = true,
				UseMaximumStaffing = true,
				UseAverageShiftLengths = true,
				UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.ConsiderMaxSeats
			};
			var stateHolder = StateHolder.Fill(scenario, new DateOnlyPeriod(new DateOnly(2014, 3, 22), new DateOnly(2014, 4, 8)), new[] { person }, data, skillDays);

			Target.Execute(new NoSchedulingProgress(), stateHolder,new[] { person }, date.ToDateOnlyPeriod(), OptimizationPreferences, new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			stateHolder.Schedules[person].ScheduledDay(date).PersonAssignment().ShiftLayers.Should().Be.Empty();
		}

		private static SkillDay createSkillDay(ISkill skill, DateOnly date, Scenario scenario)
		{
			var dateAsUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
			var period = new DateTimePeriod(dateAsUtc, dateAsUtc.AddHours(0.25));

			var skillDay = new SkillDay(date, skill, scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
			skillDay.SetupSkillDay();
			var newSkillStaffPeriodValues = new NewSkillStaffPeriodValues(Enumerable.Range(0, 96).Select(i =>
			{
				var skillStaffPeriod = new SkillStaffPeriod(period.MovePeriod(TimeSpan.FromMinutes(i * 15)), new Task(2, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2)),
				ServiceAgreement.DefaultValues());
				skillStaffPeriod.Payload.ManualAgents = i >= 8 ? 2 : 0;
				return skillStaffPeriod;
			}).ToArray());
			skillDay.SetCalculatedStaffCollection(newSkillStaffPeriodValues);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] { skillDay }, new DateOnlyPeriod(2014, 3, 22, 2014, 4, 7));
			newSkillStaffPeriodValues.BatchCompleted();
			return skillDay;
		}
	}
}