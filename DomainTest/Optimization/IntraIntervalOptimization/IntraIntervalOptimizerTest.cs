using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.IntraIntervalOptimization
{
	[TestFixture]
	public class IntraIntervalOptimizerTest
	{
		private IntraIntervalOptimizer _target;
		private MockRepository _mock;
		private ITeamInfoFactory _teamInfoFactory;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;
		private ITeamBlockScheduler _teamBlockScheduler;
		private ISkillStaffPeriodEvaluator _skillStaffPeriodEvaluator;
		private ISchedulingOptions _schedulingOptions;
		private IOptimizationPreferences _optimizationPreferences;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IPerson _person;
		private DateOnly _dateOnly;
		private IList<IScheduleMatrixPro> _allScheduleMatrixPros;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISkill _skill;
		private IIntraIntervalIssues _issusesBefore;
		private IScheduleRange _scheduleRange;
		private IScheduleDay _scheduleDay;
		private IShiftProjectionCache _shiftProjectionCache;
		private ITeamInfo _teamInfo;
		private ITeamBlockInfo _teamBlockInfo;
		private ISkillStaffPeriod _skillStaffPeriodBefore;
		private IList<ISkillStaffPeriod> _skillStaffPeriodIssuesAfter;
		private IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private List<IScheduleDay> _scheduleDayList;
		private IIntraIntervalIssueCalculator _intraIntervalIssueCalculator;
		private IntraIntervalIssues _issusesAfter;
		private ISkillStaffPeriod _skillStaffPeriodAfter;
		private List<ISkillStaffPeriod> _skillStaffPeriodIssuesBefore;
		private IShiftProjectionIntraIntervalBestFitCalculator _shiftProjectionIntraIntervalBestFitCalculator;
		private ITeamScheduling _teamScheduling;
		private IList<ISkillDay> _skillDays;
		private IList<ISkillDay> _skillDaysAfter;
		private ISkillDay _skillDay;
		private ISkillDay _skillDayAfter;
		private ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;

		private IList<ISkillStaffPeriod> _skillStaffPeriods;
		private IList<ISkillStaffPeriod> _skillStaffPeriodsAfter;
		private IList<IWorkShiftCalculationResultHolder> _workShiftCalculationResultHolders;
		private IWorkShiftCalculationResultHolder _workShiftCalculationResultHolder;
		private IMainShiftOptimizeActivitySpecificationSetter _mainShiftOptimizeActivitySpecificationSetter;
		private IEditableShift _editableShift;
		private double _limit;
		private ITeamBlockShiftCategoryLimitationValidator _teamBlockShiftCategoryLimitationValidator;
		

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
			_teamBlockInfoFactory = _mock.StrictMock<ITeamBlockInfoFactory>();
			_teamBlockScheduler = _mock.StrictMock<ITeamBlockScheduler>();
			_skillStaffPeriodEvaluator = _mock.StrictMock<ISkillStaffPeriodEvaluator>();
			_deleteAndResourceCalculateService = _mock.StrictMock<IDeleteAndResourceCalculateService>();
			_intraIntervalIssueCalculator = _mock.StrictMock<IIntraIntervalIssueCalculator>();
			_shiftProjectionIntraIntervalBestFitCalculator =_mock.StrictMultiMock<IShiftProjectionIntraIntervalBestFitCalculator>();
			_teamScheduling = _mock.StrictMock<ITeamScheduling>();
			_skillDayIntraIntervalIssueExtractor =_mock.StrictMock<ISkillDayIntraIntervalIssueExtractor>();
			_mainShiftOptimizeActivitySpecificationSetter = _mock.StrictMock<IMainShiftOptimizeActivitySpecificationSetter>();
			_teamBlockShiftCategoryLimitationValidator = _mock.StrictMock<ITeamBlockShiftCategoryLimitationValidator>();
			_target = new IntraIntervalOptimizer(_teamInfoFactory, _teamBlockInfoFactory, _teamBlockScheduler, _skillStaffPeriodEvaluator, _deleteAndResourceCalculateService, _intraIntervalIssueCalculator, _teamScheduling, _shiftProjectionIntraIntervalBestFitCalculator, _skillDayIntraIntervalIssueExtractor, _mainShiftOptimizeActivitySpecificationSetter, _teamBlockShiftCategoryLimitationValidator);
			_schedulingOptions = new SchedulingOptions();
			_optimizationPreferences = new OptimizationPreferences();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_person = PersonFactory.CreatePerson("person");
			_dateOnly = new DateOnly(2014, 1, 1);
			_allScheduleMatrixPros = new List<IScheduleMatrixPro>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_skill = SkillFactory.CreateSkill("skill");
			_issusesBefore = new IntraIntervalIssues();
			_issusesAfter = new IntraIntervalIssues();
			_scheduleRange = _mock.StrictMock<IScheduleRange>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_shiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_skillStaffPeriodBefore = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodAfter = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodIssuesAfter = new List<ISkillStaffPeriod> { _skillStaffPeriodAfter };
			_skillStaffPeriodIssuesBefore = new List<ISkillStaffPeriod> { _skillStaffPeriodBefore };
			_scheduleDayList = new List<IScheduleDay> { _scheduleDay };

			_skillDay = _mock.StrictMock<ISkillDay>();
			_skillDayAfter = _mock.StrictMock<ISkillDay>();
			_skillDays = new List<ISkillDay> { _skillDay };
			_skillDaysAfter = new List<ISkillDay> { _skillDayAfter };

			_skillStaffPeriods = new List<ISkillStaffPeriod> { _skillStaffPeriodBefore };
			_skillStaffPeriodsAfter = new List<ISkillStaffPeriod> { _skillStaffPeriodAfter };

			_workShiftCalculationResultHolders = new List<IWorkShiftCalculationResultHolder>();
			_workShiftCalculationResultHolder = new WorkShiftCalculationResult();
			_workShiftCalculationResultHolder.ShiftProjection = _shiftProjectionCache;

			_editableShift = new EditableShift(ShiftCategoryFactory.CreateShiftCategory());

			_limit = 0.7999;
		}

		[Test]
		public void ShouldReturnIssuesAfterWhenResultGetsBetterOnDayAndNotWorseOnDayAfter()
		{
			_issusesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;

			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;

			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);

				Expect.Call(() => _rollbackService.ClearModificationCollection());
				
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);

				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(_schedulingOptions,
						_optimizationPreferences, _editableShift, _dateOnly));
				
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true, true));

				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(1) })).Return(_skillDaysAfter);

				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDays, _skill)).Return(_skillStaffPeriods);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDaysAfter, _skill)).Return(_skillStaffPeriodsAfter);

				Expect.Call(_teamBlockScheduler.GetShiftProjectionCaches(_teamBlockInfo, _person, _dateOnly, _schedulingOptions, _schedulingResultStateHolder)).Return(_workShiftCalculationResultHolders);

				Expect.Call(_shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(_workShiftCalculationResultHolders, new List<ISkillStaffPeriod>(), _skill, _limit)).Return(_workShiftCalculationResultHolder).IgnoreArguments();
				Expect.Call(() =>_teamScheduling.ExecutePerDayPerPerson(_person, _dateOnly, _teamBlockInfo, _shiftProjectionCache, _rollbackService,_resourceCalculateDelayer));
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter, _limit )).Return(true);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter, _limit)).Return(false);
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, null, _optimizationPreferences)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.AreEqual(_issusesAfter, result);
			}
		}

		[Test]
		public void ShouldReturnIssuesAfterWhenResultGetsBetterOnDayAfterAndNotWorseOnDay()
		{
			_issusesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;

			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;

			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);

				Expect.Call(() => _rollbackService.ClearModificationCollection());

				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);

				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(_schedulingOptions,
						_optimizationPreferences, _editableShift, _dateOnly));

				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true, true));

				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(1) })).Return(_skillDaysAfter);

				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDays, _skill)).Return(_skillStaffPeriods);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDaysAfter, _skill)).Return(_skillStaffPeriodsAfter);

				Expect.Call(_teamBlockScheduler.GetShiftProjectionCaches(_teamBlockInfo, _person, _dateOnly, _schedulingOptions, _schedulingResultStateHolder)).Return(_workShiftCalculationResultHolders);

				Expect.Call(_shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(_workShiftCalculationResultHolders, new List<ISkillStaffPeriod>(), _skill, _limit)).Return(_workShiftCalculationResultHolder).IgnoreArguments();
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person, _dateOnly, _teamBlockInfo, _shiftProjectionCache, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter, _limit)).Return(true);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter, _limit)).Return(false);
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, null, _optimizationPreferences)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, true);
				Assert.AreEqual(_issusesAfter, result);
			}
		}

		[Test]
		public void ShouldReturnIssuesBeforeWhenCouldNotSchedule()
		{
			_issusesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;

			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;

			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);

				Expect.Call(() => _rollbackService.ClearModificationCollection());

				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(_schedulingOptions,
						_optimizationPreferences, _editableShift, _dateOnly));

				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true, true));

				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(1) })).Return(_skillDaysAfter);

				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDays, _skill)).Return(_skillStaffPeriods);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDaysAfter, _skill)).Return(_skillStaffPeriodsAfter);

				Expect.Call(_teamBlockScheduler.GetShiftProjectionCaches(_teamBlockInfo, _person, _dateOnly, _schedulingOptions, _schedulingResultStateHolder)).Return(_workShiftCalculationResultHolders);

				Expect.Call(_shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(_workShiftCalculationResultHolders, new List<ISkillStaffPeriod>(), _skill, _limit)).Return(_workShiftCalculationResultHolder).IgnoreArguments();
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person, _dateOnly, _teamBlockInfo, _shiftProjectionCache, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(false);

				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, true)).Return(true);	
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.AreEqual(_issusesBefore, result);
			}
		}

		[Test]
		public void ShouldReturnIssuesBeforeWhenShiftCategoryLimitationsIsBroken()
		{
			_issusesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;

			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;

			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);

				Expect.Call(() => _rollbackService.ClearModificationCollection());

				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(_schedulingOptions,
						_optimizationPreferences, _editableShift, _dateOnly));

				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);

				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true, true));

				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(1) })).Return(_skillDaysAfter);

				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDays, _skill)).Return(_skillStaffPeriods);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDaysAfter, _skill)).Return(_skillStaffPeriodsAfter);

				Expect.Call(_teamBlockScheduler.GetShiftProjectionCaches(_teamBlockInfo, _person, _dateOnly, _schedulingOptions, _schedulingResultStateHolder)).Return(_workShiftCalculationResultHolders);

				Expect.Call(_shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(_workShiftCalculationResultHolders, new List<ISkillStaffPeriod>(), _skill, _limit)).Return(_workShiftCalculationResultHolder).IgnoreArguments();
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person, _dateOnly, _teamBlockInfo, _shiftProjectionCache, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);

				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, null, _optimizationPreferences)).Return(false);

				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, true)).Return(true);

				
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.AreEqual(_issusesBefore, result);
			}
		}

		[Test]
		public void ShouldReturnIssuesBeforeWhenResultGetsWorse()
		{
			_issusesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;

			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;

			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);

				Expect.Call(() => _rollbackService.ClearModificationCollection());

				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(() => _mainShiftOptimizeActivitySpecificationSetter.SetMainShiftOptimizeActivitySpecification(_schedulingOptions,
						_optimizationPreferences, _editableShift, _dateOnly));

				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);

				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true, true));

				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly.AddDays(1) })).Return(_skillDaysAfter);

				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDays, _skill)).Return(_skillStaffPeriods);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.ExtractAll(_skillDaysAfter, _skill)).Return(_skillStaffPeriodsAfter);

				Expect.Call(_teamBlockScheduler.GetShiftProjectionCaches(_teamBlockInfo, _person, _dateOnly, _schedulingOptions, _schedulingResultStateHolder)).Return(_workShiftCalculationResultHolders);

				Expect.Call(_shiftProjectionIntraIntervalBestFitCalculator.GetShiftProjectionCachesSortedByBestIntraIntervalFit(_workShiftCalculationResultHolders, new List<ISkillStaffPeriod>(), _skill, _limit)).Return(_workShiftCalculationResultHolder).IgnoreArguments();
				Expect.Call(() => _teamScheduling.ExecutePerDayPerPerson(_person, _dateOnly, _teamBlockInfo, _shiftProjectionCache, _rollbackService, _resourceCalculateDelayer));
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter, _limit)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter, _limit)).Return(true);

				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, true)).Return(true);
				Expect.Call(_teamBlockShiftCategoryLimitationValidator.Validate(_teamBlockInfo, null, _optimizationPreferences)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.AreEqual(_issusesBefore, result);
			}
		}

		[Test]
		public void ShouldNotContinueIfTeamBlockInfoIsNull()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _optimizationPreferences, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.AreEqual(_issusesBefore, result);
			}
		}
	}
}
