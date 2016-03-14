using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[DomainTest]
	public class OptimizationBug37126Test : ISetup
	{
		public IOptimizationCommand Target;
		public ISchedulerStateHolder StateHolder;
		public IGroupPagePerDateHolder GroupPagePerDateHolder;
		public ILifetimeScope Scope;
		public IOptimizationPreferences OptimizationPreferences;
		public IMatrixListFactory MatrixListFactory;
		public IRequiredScheduleHelper RequiredScheduleHelper;
		public IResourceOptimizationHelperExtended ResourceCalculator;

		[Test]
		public void ShouldNotCrashWhenDoingRollback()
		{
			BusinessUnitFactory.CreateNewBusinessUnitUsedInTest();

			var date = new DateOnly(2014, 3, 31);
			var scenario = new Scenario("Default").WithId();
			var shiftCategory = new ShiftCategory("DY").WithId();
			var shiftCategoryAM = new ShiftCategory("AM").WithId();
			var activity = new Activity("Phone") { InContractTime = true, InWorkTime = true, InPaidTime = true, RequiresSkill = true};
			var skill = SkillFactory.CreateSkill("Direct Sales").WithId();
			skill.Activity = activity;

			var workShiftRuleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 9, 0, 15), new TimePeriodWithSegment(15, 0, 17, 0, 15), shiftCategoryAM));
			workShiftRuleSet.AddExtender(new ActivityRelativeStartExtender(ActivityFactory.CreateActivity("Lunch"),new TimePeriodWithSegment(1,0,1,0,15), new TimePeriodWithSegment(4,0,5,0,15)));
			var bag = new RuleSetBag(workShiftRuleSet);
			var person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson().WithId(),
				new DateOnly(2014, 3, 1));
			person.AddSkill(skill, new DateOnly(2014, 3, 1));
			var personPeriod = person.Period(new DateOnly(2014, 3, 1));
			personPeriod.RuleSetBag = bag;
			personPeriod.Team.Site = SiteFactory.CreateSimpleSite();
			personPeriod.PersonContract.Contract.NegativePeriodWorkTimeTolerance = TimeSpan.FromHours(24);
			personPeriod.PersonContract.Contract.PositivePeriodWorkTimeTolerance = TimeSpan.FromHours(24);
			
			var dictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2014, 3, 22, 2014, 4, 4));
			dictionary.UsePermissions(false);
			dictionary.SetUndoRedoContainer(new UndoRedoContainer(500));
			dictionary.AddPersonAbsence(PersonAbsenceFactory.CreatePersonAbsence(person,scenario, new DateTimePeriod(2014, 3, 24, 0, 2014, 3, 25, 0)));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 3, 25, 8, 2014, 3, 25, 22), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 3, 26, 8, 2014, 3, 26, 22), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 3, 27, 8, 2014, 3, 27, 22), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 3, 28, 12, 2014, 3, 28, 20), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2014, 3, 29), DayOffFactory.CreateDayOff()));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2014, 3, 30), DayOffFactory.CreateDayOff()));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 3, 31, 8, 2014, 3, 31, 22), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithDayOff(scenario, person, new DateOnly(2014, 4, 1), DayOffFactory.CreateDayOff()));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 4, 2, 8, 2014, 4, 2, 22), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 4, 3, 12, 2014, 4, 3, 21), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 4, 4, 8, 2014, 4, 4, 22), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 4, 5, 8, 2014, 4, 5, 18), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 4, 6, 11, 2014, 4, 6, 21), shiftCategory, scenario));
			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, person, new DateTimePeriod(2014, 4, 7, 8, 2014, 4, 7, 22), shiftCategory, scenario));
			dictionary.AddScheduleDataManyPeople(new PreferenceDay(person,new DateOnly(2014,3,31), new PreferenceRestriction {ShiftCategory = shiftCategory}));

			StateHolder.SetRequestedScenario(scenario);
			StateHolder.SchedulingResultState.Schedules = dictionary;
			StateHolder.AllPermittedPersons.Add(person);
			StateHolder.SchedulingResultState.PersonsInOrganization = StateHolder.AllPermittedPersons;
			StateHolder.FilterPersons(StateHolder.AllPermittedPersons);
			StateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(2014, 3, 22, 2014, 4, 4), TimeZoneInfo.Utc);
			StateHolder.SchedulingResultState.AddSkills(skill);

			var skillDays = Enumerable.Range(-7, 10).Select(i => createSkillDay(skill, date.AddDays(i), scenario)).ToArray();
			StateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IList<ISkillDay>> { {skill,skillDays}};

			ResourceCalculator.ResourceCalculateAllDays(new NoSchedulingProgress(), false);
			
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
			Target.Execute(
				new OptimizerOriginalPreferences(new SchedulingOptions {TagToUseOnScheduling = NullScheduleTag.Instance, GroupOnGroupPageForTeamBlockPer = GroupPageLight.SingleAgentGroup(UserTexts.Resources.NoTeam) }),
				new NoSchedulingProgress(), StateHolder,
				new[] {dictionary[person].ScheduledDay(date)}, GroupPagePerDateHolder,
				new ScheduleOptimizerHelper(Scope, new OptimizerHelperHelper(), RequiredScheduleHelper,
					MatrixListFactory), OptimizationPreferences, false, new DaysOffPreferences(),
				new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			dictionary[person].ScheduledDay(date).PersonAssignment().ShiftLayers.Should().Be.Empty();
		}

		private static SkillDay createSkillDay(ISkill skill, DateOnly date, Scenario scenario)
		{
			var dateAsUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
			var period = new DateTimePeriod(dateAsUtc, dateAsUtc.AddHours(0.25));
			
			var skillDay = new SkillDay(date, skill, scenario, new List<IWorkloadDay>(), new List<ISkillDataPeriod>());
			skillDay.SetupSkillDay();
			var newSkillStaffPeriodValues = new NewSkillStaffPeriodValues(Enumerable.Range(0,96).Select(i =>
			{
				var skillStaffPeriod = new SkillStaffPeriod(period.MovePeriod(TimeSpan.FromMinutes(i*15)), new Task(2, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2)),
				ServiceAgreement.DefaultValues(), skill.SkillType.StaffingCalculatorService);
				skillStaffPeriod.Payload.ManualAgents = i >= 8 ? 2 : 0;
				return skillStaffPeriod;
			}).ToArray());
			skillDay.SetCalculatedStaffCollection(newSkillStaffPeriodValues);
			skillDay.SkillDayCalculator = new SkillDayCalculator(skill, new[] {skillDay}, new DateOnlyPeriod(2014, 3, 22, 2014, 4, 7));
			newSkillStaffPeriodValues.BatchCompleted();
			return skillDay;
		}

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FillSchedulerStateHolderFromRam>().For<IFillSchedulerStateHolder>();
		}
	}
}