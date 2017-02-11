using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Ccc.Domain.UndoRedo;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.IntradayOptimization
{
	[DomainTest]
	public class IntradayOptimizationTeamBlockAsClassicTest : ISetup
	{
		public IOptimizationCommand Target;
		public ISchedulerStateHolder StateHolder;
		public IOptimizationPreferences OptimizationPreferences;
		public IResourceOptimizationHelperExtended ResourceCalculator;
		public FakeBusinessUnitRepository BusinessUnitRepository;

		[Test]
		public void ShouldNotRollBackIfSingleAgentSingleDayAndPeriodValueIsNotBetter()
		{
			BusinessUnitRepository.Has(ServiceLocatorForEntity.CurrentBusinessUnit.Current());
			var date = new DateOnly(2014, 4, 1);
			var scenario = new Scenario("Default").WithId();
			var shiftCategory = new ShiftCategory("DY").WithId();
			var shiftCategoryAm = new ShiftCategory("AM").WithId();
			var activity = new Activity("Phone")
			{
				InContractTime = true,
				InWorkTime = true,
				InPaidTime = true,
				RequiresSkill = true
			};
			var skill = SkillFactory.CreateSkill("Direct Sales").WithId().InTimeZone(TimeZoneInfo.Utc);
			skill.Activity = activity;

			var workShiftRuleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(activity, new TimePeriodWithSegment(7, 0, 7, 0, 15),
					new TimePeriodWithSegment(15, 0, 15, 0, 15), shiftCategoryAm));
			
			var bag = new RuleSetBag(workShiftRuleSet);
			var person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new[] {skill}).WithId();
			person.PermissionInformation.SetDefaultTimeZone(TimeZoneInfo.Utc);
			person.AddSchedulePeriod(new SchedulePeriod(date, SchedulePeriodType.Day, 1));
			var personPeriod = person.Period(date);
			personPeriod.RuleSetBag = bag;
			personPeriod.Team.Site = SiteFactory.CreateSimpleSite();
			personPeriod.PersonContract.Contract.EmploymentType = EmploymentType.FixedStaffDayWorkTime;

			var dictionary = new ScheduleDictionaryForTest(scenario, new DateTimePeriod(2014, 3, 22, 2014, 4, 4));
			dictionary.UsePermissions(false);
			dictionary.SetUndoRedoContainer(new UndoRedoContainer());

			dictionary.AddPersonAssignment(PersonAssignmentFactory.CreateAssignmentWithMainShift(person, scenario, activity,
				new DateTimePeriod(2014, 4, 1, 8, 2014, 4, 1, 16), shiftCategory));

			StateHolder.SetRequestedScenario(scenario);
			StateHolder.SchedulingResultState.Schedules = dictionary;
			StateHolder.AllPermittedPersons.Add(person);
			StateHolder.SchedulingResultState.PersonsInOrganization = StateHolder.AllPermittedPersons;
			StateHolder.FilterPersons(StateHolder.AllPermittedPersons);
			StateHolder.RequestedPeriod = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(date, date),
				TimeZoneInfo.Utc);
			StateHolder.SchedulingResultState.AddSkills(skill);

			var skillDay = createSkillDay(skill, date, scenario);
			StateHolder.SchedulingResultState.SkillDays = new Dictionary<ISkill, IEnumerable<ISkillDay>> {{skill, new[] {skillDay}}};

			ResourceCalculator.ResourceCalculateAllDays(new NoSchedulingProgress(), false);

			OptimizationPreferences.General = new GeneralPreferences
			{
				ScheduleTag = NullScheduleTag.Instance,
				OptimizationStepShiftsWithinDay = true
			};

			OptimizationPreferences.Advanced = new AdvancedPreferences
			{
				UserOptionMaxSeatsFeature = MaxSeatsFeatureOptions.DoNotConsiderMaxSeats
			};
			OptimizationPreferences.Extra = new ExtraPreferences
			{
				UseTeamBlockOption = true,
				UseBlockSameShiftCategory = true
			};

			var optimizerOriginalPrefs = new OptimizerOriginalPreferences(new SchedulingOptions
			{
				TagToUseOnScheduling = NullScheduleTag.Instance,
				BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay,
				GroupOnGroupPageForTeamBlockPer = GroupPageLight.SingleAgentGroup(UserTexts.Resources.NoTeam),
				UseTeam = true,

			});

			Target.Execute(
				optimizerOriginalPrefs,
				new NoSchedulingProgress(), StateHolder,
				new[] {dictionary[person].ScheduledDay(date)},
				OptimizationPreferences, false, new DaysOffPreferences(),
				new FixedDayOffOptimizationPreferenceProvider(new DaysOffPreferences()));

			dictionary[person].ScheduledDay(date).PersonAssignment().ShiftCategory.Should().Be.EqualTo(shiftCategoryAm);
		}

		private static SkillDay createSkillDay(ISkill skill, DateOnly date, Scenario scenario)
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