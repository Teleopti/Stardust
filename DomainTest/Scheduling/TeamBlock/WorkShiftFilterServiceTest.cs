using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.DomainTest.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Secrets.WorkShiftCalculator;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	//TODO! Remove this test when we have more "real" teamblock scheduling tests
	[TestWithStaticDependenciesAvoidUse]
	[TestFixture]
	public class WorkShiftFilterServiceTest
	{
		private readonly DateOnly _dateOnly = new DateOnly(2013, 3, 1);
		private readonly TimeZoneInfo _timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Atlantic Standard Time");
		
		private IWorkShiftFilterService _target;
		private ISchedulingOptions _schedulingOptions;
		private IScheduleMatrixPro _matrix;
		private IPerson _person;
		private IActivity _activity;
		private WorkShiftFinderResult _finderResult;
		private ITeamBlockInfo _teamBlockInfo;
		private Group _group;
		private List<IScheduleMatrixPro> _matrixList;
	    private ITeamInfo _teamInfo;
		private IScheduleDictionary _schedules;

		[SetUp]
		public void Setup()
		{
			_schedulingOptions = new SchedulingOptions
			{
				WorkShiftLengthHintOption = WorkShiftLengthHintOption.AverageWorkTime,
				MainShiftOptimizeActivitySpecification = null,
				UsePreferences = false,
				UseRotations = false,
				UseAvailability = false,
				UseStudentAvailability = false
			};

			_activity = ActivityFactory.CreateActivity("sd");
			var skill = new Skill("_").For(_activity);
			_person = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(new Person(), _dateOnly);
			_person.AddPersonPeriod(new PersonPeriod(new DateOnly(2012, 12, 12), PersonContractFactory.CreatePersonContract(), new Team()));
			_person.AddSkill(skill, _dateOnly);

			_matrix = ScheduleMatrixProFactory.Create(new DateOnlyPeriod(_dateOnly, _dateOnly),
				new SchedulingResultStateHolder {Schedules = new ScheduleDictionaryForTest(new Scenario("Default"), _dateOnly.Date)}, _person,
				new VirtualSchedulePeriod(_person, _dateOnly, new VirtualSchedulePeriodSplitChecker(_person)));
			_finderResult = new WorkShiftFinderResult(_person, _dateOnly);
			_schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			_schedules.Expect(x => x[_person].ScheduledDay(_dateOnly)).Return(new SchedulePartFactoryForDomain().CreatePart());
			var validDateTimePeriodShiftFilter = new ValidDateTimePeriodShiftFilter(new FakeTimeZoneGuard(_timeZoneInfo), new UserCulture(CurrentTeleoptiPrincipal.Make()));
			_target = new WorkShiftFilterService(new ActivityRestrictionsShiftFilter(),
				new BusinessRulesShiftFilter(validDateTimePeriodShiftFilter,
					new LongestPeriodForAssignmentCalculator()),
				new CommonMainShiftFilter(new ScheduleDayEquator(new EditableShiftMapper())),
				new ContractTimeShiftFilter(
					() =>
						new WorkShiftMinMaxCalculator(new PossibleMinMaxWorkShiftLengthExtractorForTest(),
							new SchedulePeriodTargetTimeCalculatorForTest(new MinMax<TimeSpan>(TimeSpan.FromHours(8),TimeSpan.FromHours(8))), new WorkShiftWeekMinMaxCalculator()),
					new SwedishCulture()),
				new DisallowedShiftCategoriesShiftFilter(), new EffectiveRestrictionShiftFilter(),
				new MainShiftOptimizeActivitiesSpecificationShiftFilter(),
				new NotOverWritableActivitiesShiftFilter(),
				new PersonalShiftsShiftFilter(new PersonalShiftMeetingTimeChecker()),
				new ShiftCategoryRestrictionShiftFilter(),
				new TimeLimitsRestrictionShiftFilter(validDateTimePeriodShiftFilter, new LatestStartTimeLimitationShiftFilter(),
					new EarliestEndTimeLimitationShiftFilter()), new WorkTimeLimitationShiftFilter(new SwedishCulture()),
				new ShiftLengthDecider(new DesiredShiftLengthCalculator(new SchedulePeriodTargetTimeCalculator())),
				new WorkShiftMinMaxCalculator(new PossibleMinMaxWorkShiftLengthExtractorForTest(),
					new SchedulePeriodTargetTimeCalculatorForTest(new MinMax<TimeSpan>(TimeSpan.FromHours(8), TimeSpan.FromHours(8))), new WorkShiftWeekMinMaxCalculator()),
				new CommonActivityFilter(),
				new RuleSetAccordingToAccessabilityFilter(new RuleSetBagExtractorProvider(),
					new TeamBlockIncludedWorkShiftRuleFilter(), new RuleSetSkillActivityChecker()),
				new ShiftProjectionCacheManager(new ShiftFromMasterActivityService(), new RuleSetDeletedActivityChecker(),
					new RuleSetDeletedShiftCategoryChecker(),
					new RuleSetProjectionEntityService(new ShiftCreatorService(new CreateWorkShiftsFromTemplate())),
					new WorkShiftFromEditableShift()), new RuleSetPersonalSkillsActivityFilter(new RuleSetSkillActivityChecker(), new PersonalSkillsProvider()),
				new DisallowedShiftProjectionCachesFilter(), new ActivityRequiresSkillProjectionFilter(new PersonalSkillsProvider()),
				new OpenHoursFilter(new IsAnySkillOpen()));
			
			_group = new Group(new List<IPerson>{_person}, "Hej");
			_matrixList = new List<IScheduleMatrixPro> { _matrix };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>>{ _matrixList };
			_teamInfo = new TeamInfo(_group, groupMatrixes);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly));
			_teamBlockInfo = new TeamBlockInfo(_teamInfo, blockInfo);
		}

		[Test]
		public void ShouldReturnNullIfEffectiveRestrictionIsNull()
		{
			var dateOnly = new DateOnly(2012, 12, 12);

			var retShift = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, dateOnly, _teamBlockInfo, null,
			                              _schedulingOptions, _finderResult, true, false, new List<ISkillDay>());
			Assert.IsNull(retShift);
		}

		[Test]
		public void ShouldReturnNullIfTeamBlockInfoIsNull()
		{
			var dateOnly = new DateOnly(2012, 12, 12);
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var retShift = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, dateOnly, null, effectiveRestriction,
			                              _schedulingOptions, _finderResult, true, false, new List<ISkillDay>());
			Assert.IsNull(retShift);
		}

		[Test]
		public void ShouldReturnNullIfMatrixListIsEmpty()
		{
			var dateOnly = new DateOnly(2012, 12, 12);
			var group = new Group(new List<IPerson> { _person }, "Hej");
			var matrixList = new List<IScheduleMatrixPro>();
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			var teaminfo = new TeamInfo(group, groupMatrixes);
			var blockInfo = new BlockInfo(new DateOnlyPeriod(dateOnly, dateOnly));
			var teamBlockInfo = new TeamBlockInfo(teaminfo, blockInfo);
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var retShift = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, dateOnly, teamBlockInfo, effectiveRestriction,
										  _schedulingOptions, _finderResult, true, false, new List<ISkillDay>());
			Assert.IsNull(retShift);
		}
	
		[Test]
		public void ShouldReturnNullIfCurrentSchedulePeriodIsInvalid()
		{
			var dateOnly = new DateOnly(2012, 12, 12);
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			var retShift = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, dateOnly, _teamBlockInfo, effectiveRestriction,
			                              _schedulingOptions, _finderResult, true, false, new List<ISkillDay>());
			Assert.IsNull(retShift);
		}

		[Test]
		public void ShouldFilterWorkShiftsForRoleModel()
		{
			var effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(), new EndTimeLimitation(),
				new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IEditableShift>();
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;
			_schedulingOptions.BlockSameShift = true;
			var workShiftRuleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), ShiftCategoryFactory.CreateShiftCategory()));
			
			var ruleSetBag = new RuleSetBag();
			ruleSetBag.AddRuleSet(workShiftRuleSet);
			_person.Period(_dateOnly).RuleSetBag = ruleSetBag;

			var retShift = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, _dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions,
				_finderResult, true, false, new List<ISkillDay>());
			Assert.IsNotNull(retShift);
		}

		[Test]
		public void ShouldFilterWorkShiftsAwayWhenActivitiesInMasterActivityRequiresSkillTeamMembersDoesntHave()
		{
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
				new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IEditableShift>();
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;
			_schedulingOptions.BlockSameShift = true;
			_activity.RequiresSkill = true;

			var masterActivity = new MasterActivity();
			masterActivity.UpdateActivityCollection(new List<IActivity>{_activity});
			_person.RemoveSkill(_person.Period(_dateOnly).PersonSkillCollection.Single().Skill, _person.Period(_dateOnly));
			_person.AddSkill(new PersonSkill(SkillFactory.CreateSkill("Skill with other activity"), new Percent(1)), _person.Period(_dateOnly));

			var workShiftRuleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(masterActivity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), ShiftCategoryFactory.CreateShiftCategory()));

			var ruleSetBag = new RuleSetBag();
			ruleSetBag.AddRuleSet(workShiftRuleSet);
			_person.Period(_dateOnly).RuleSetBag = ruleSetBag;

			var retShift = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, _dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions,
				_finderResult, true, false, new List<ISkillDay>());
			Assert.IsNull(retShift);
		}

		[Test]
		public void ShouldReturnNullIfShiftListIsNull()
		{
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
				new EndTimeLimitation(), new WorkTimeLimitation(), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new Not<IEditableShift>();
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;

			var workShiftRuleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), ShiftCategoryFactory.CreateShiftCategory()));

			var ruleSetBag = new RuleSetBag();
			ruleSetBag.AddRuleSet(workShiftRuleSet);
			_person.Period(_dateOnly).RuleSetBag = ruleSetBag;

			var retShift = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, _dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions,
				_finderResult, true, false, new List<ISkillDay>());
			Assert.IsNull(retShift);
		}

		[Test]
		public void ShouldReturnNullIfEffectiveRestrictionShiftFilterSaysFalse()
		{
			_activity.InWorkTime = true;
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
				new EndTimeLimitation(), new WorkTimeLimitation(null,TimeSpan.FromHours(7)), null, null, null, new List<IActivityRestriction>());
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IEditableShift>();

			var workShiftRuleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), ShiftCategoryFactory.CreateShiftCategory()));

			var ruleSetBag = new RuleSetBag();
			ruleSetBag.AddRuleSet(workShiftRuleSet);
			_person.Period(_dateOnly).RuleSetBag = ruleSetBag;

			var retShift = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, _dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions,
				_finderResult, true, false, new List<ISkillDay>());
			Assert.IsNull(retShift);
		}

		[Test]
		public void ShouldReturnNoShiftsWhenNotMatchingRequestedShiftCategory()
		{
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("cat");
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
				new EndTimeLimitation(),
				new WorkTimeLimitation(), null, null, null,
				new List<IActivityRestriction>());
			
			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IEditableShift>();
			_schedulingOptions.ShiftCategory = shiftCategory;
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;

			var workShiftRuleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), ShiftCategoryFactory.CreateShiftCategory("other")));

			var ruleSetBag = new RuleSetBag();
			ruleSetBag.AddRuleSet(workShiftRuleSet);
			_person.Period(_dateOnly).RuleSetBag = ruleSetBag;

			var result = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, _dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions, _finderResult, true, false, new List<ISkillDay>());
			result.Should().Be.Null();
		}

		[Test]
		public void ShouldReturnGetShiftCategoryFromSchedulingOptions()
		{
			var shiftCategory = ShiftCategoryFactory.CreateShiftCategory("cat");
			IEffectiveRestriction effectiveRestriction = new EffectiveRestriction(new StartTimeLimitation(),
				new EndTimeLimitation(),
				new WorkTimeLimitation(), null, null, null,
				new List<IActivityRestriction>());

			_schedulingOptions.MainShiftOptimizeActivitySpecification = new All<IEditableShift>();
			_schedulingOptions.ShiftCategory = shiftCategory;
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;

			var workShiftRuleSet =
				new WorkShiftRuleSet(new WorkShiftTemplateGenerator(_activity, new TimePeriodWithSegment(8, 0, 8, 0, 15),
					new TimePeriodWithSegment(16, 0, 16, 0, 15), shiftCategory));

			var ruleSetBag = new RuleSetBag();
			ruleSetBag.AddRuleSet(workShiftRuleSet);
			_person.Period(_dateOnly).RuleSetBag = ruleSetBag;

			var result = _target.FilterForRoleModel(new GroupPersonSkillAggregator(new PersonalSkillsProvider()), _schedules, _dateOnly, _teamBlockInfo, effectiveRestriction, _schedulingOptions, _finderResult, true, false, new List<ISkillDay>());
			result.Should().Not.Be.Null();
		}
	}
}
