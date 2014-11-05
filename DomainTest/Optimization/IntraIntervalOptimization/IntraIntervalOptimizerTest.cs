using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
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
		private IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private ISkillStaffPeriodEvaluator _skillStaffPeriodEvaluator;
		private ISchedulingOptions _schedulingOptions;
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
		private IEditableShift _editableShift;
		private TimeZoneInfo _timeZone;
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

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
			_teamBlockInfoFactory = _mock.StrictMock<ITeamBlockInfoFactory>();
			_teamBlockScheduler = _mock.StrictMock<ITeamBlockScheduler>();
			_shiftProjectionCacheManager = _mock.StrictMock<IShiftProjectionCacheManager>();
			_skillStaffPeriodEvaluator = _mock.StrictMock<ISkillStaffPeriodEvaluator>();
			_deleteAndResourceCalculateService = _mock.StrictMock<IDeleteAndResourceCalculateService>();
			_intraIntervalIssueCalculator = _mock.StrictMock<IIntraIntervalIssueCalculator>();
			_target = new IntraIntervalOptimizer(_teamInfoFactory, _teamBlockInfoFactory, _teamBlockScheduler,_shiftProjectionCacheManager, _skillStaffPeriodEvaluator,_deleteAndResourceCalculateService, _intraIntervalIssueCalculator);
			_schedulingOptions = new SchedulingOptions();
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
			_editableShift = _mock.StrictMock<IEditableShift>();
			_timeZone = _person.PermissionInformation.DefaultTimeZone();
			_shiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_skillStaffPeriodBefore = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodAfter = _mock.StrictMock<ISkillStaffPeriod>();
			_skillStaffPeriodIssuesAfter = new List<ISkillStaffPeriod>{_skillStaffPeriodAfter};
			_skillStaffPeriodIssuesBefore = new List<ISkillStaffPeriod> { _skillStaffPeriodBefore };
			_scheduleDayList = new List<IScheduleDay> { _scheduleDay };
		}

		[Test]
		public void ShouldReturnIssuesWhenResultGetsBetterButNotSolved()
		{
			_issusesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;

			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;


			using (_mock.Record())
			{
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(_editableShift, _dateOnly, _timeZone)).Return(_shiftProjectionCache);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService,_resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(true).IgnoreArguments();
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(true);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.IsNotEmpty(result.IssuesOnDay);
			}
		}

		[Test]
		public void ShouldReturnIssuesWhenResultGetsBetterButNotSolvedDayAfter()
		{
			_issusesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;

			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;


			using (_mock.Record())
			{
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(_editableShift, _dateOnly, _timeZone)).Return(_shiftProjectionCache);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(true).IgnoreArguments();
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, true);
				Assert.IsNotEmpty(result.IssuesOnDayAfter);
			}
		}

		[Test]
		public void ShouldReturnNoIssuesWhenSolved()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(_editableShift, _dateOnly, _timeZone)).Return(_shiftProjectionCache);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(true).IgnoreArguments();
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.IsEmpty(result.IssuesOnDay);
			}
		}

		[Test]
		public void ShouldReturnNoIssuesWhenSolvedDayAfter()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(_editableShift, _dateOnly, _timeZone)).Return(_shiftProjectionCache);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(true).IgnoreArguments();
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, true);
				Assert.IsEmpty(result.IssuesOnDayAfter);
			}
		}

		[Test]
		public void ShouldRollbackResourceCalculateAndReturnIssuesWhenScheduleFailed()
		{
			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;

			using (_mock.Record())
			{
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(_editableShift, _dateOnly, _timeZone)).Return(_shiftProjectionCache);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(false).IgnoreArguments();
				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null)).Return(true);
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.IsNotEmpty(result.IssuesOnDay);
			}
		}

		[Test]
		public void ShouldBreakWhenTryingToOptimizeIdenticalShifProjectionCaches()
		{
			_issusesBefore.IssuesOnDayBefore = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;

			_issusesAfter.IssuesOnDayBefore = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;

			using (_mock.Record())
			{
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay).Repeat.Twice();
				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift).Repeat.Twice();
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(_editableShift, _dateOnly, _timeZone)).Return(_shiftProjectionCache).Repeat.Twice();
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(true).IgnoreArguments();
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.IsNotEmpty(result.IssuesOnDay);
			}		
		}

		[Test]
		public void ShouldUserCancel()
		{
			_issusesBefore.IssuesOnDay = _skillStaffPeriodIssuesBefore;
			_issusesBefore.IssuesOnDayAfter = _skillStaffPeriodIssuesBefore;

			_issusesAfter.IssuesOnDay = _skillStaffPeriodIssuesAfter;
			_issusesAfter.IssuesOnDayAfter = _skillStaffPeriodIssuesAfter;

			using (_mock.Record())
			{
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_schedulingResultStateHolder.Schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.GetEditorShift()).Return(_editableShift);
				Expect.Call(_shiftProjectionCacheManager.ShiftProjectionCacheFromShift(_editableShift, _dateOnly, _timeZone)).Return(_shiftProjectionCache);
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnly, _allScheduleMatrixPros)).Return(_teamInfo);
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnly, _schedulingOptions.BlockFinderTypeForAdvanceScheduling, true)).Return(_teamBlockInfo);
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_scheduleDayList, _rollbackService, true));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(true).IgnoreArguments();
				Expect.Call(_intraIntervalIssueCalculator.CalculateIssues(_schedulingResultStateHolder, _skill, _dateOnly)).Return(_issusesAfter);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(false);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsWorse(_skillStaffPeriodIssuesBefore, _skillStaffPeriodIssuesAfter)).Return(true);
			}

			using (_mock.Playback())
			{
				_target.ReportProgress += _target_ReportProgress;
				_target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore, false);
				Assert.IsTrue(_target.IsCanceled);
				_target.ReportProgress -= _target_ReportProgress;
				_target.Reset();
				Assert.IsFalse(_target.IsCanceled);
			}			
		}

		void _target_ReportProgress(object sender, ResourceOptimizerProgressEventArgs e)
		{
			e.Cancel = true;
		}
	}
}
