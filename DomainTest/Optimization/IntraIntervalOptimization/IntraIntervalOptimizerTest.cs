using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Security.Principal;
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
		private ITeamBlockClearer _teamBlockClearer;
		private ITeamBlockScheduler _teamBlockScheduler;
		private IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private ISkillDayIntraIntervalIssueExtractor _skillDayIntraIntervalIssueExtractor;
		private ISkillStaffPeriodEvaluator _skillStaffPeriodEvaluator;
		private ISchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IPerson _person;
		private DateOnly _dateOnly;
		private IList<IScheduleMatrixPro> _allScheduleMatrixPros;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISkill _skill;
		private IList<ISkillStaffPeriod> _issusesBefore;
		private IScheduleRange _scheduleRange;
		private IScheduleDay _scheduleDay;
		private IEditableShift _editableShift;
		private TimeZoneInfo _timeZone;
		private IShiftProjectionCache _shiftProjectionCache;
		private ITeamInfo _teamInfo;
		private ITeamBlockInfo _teamBlockInfo;
		private IList<ISkillDay> _skillDays;
		private ISkillStaffPeriod _skillStaffPeriod;
		private IList<ISkillStaffPeriod> _issuesAfter;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
			_teamBlockInfoFactory = _mock.StrictMock<ITeamBlockInfoFactory>();
			_teamBlockClearer = _mock.StrictMock<ITeamBlockClearer>();
			_teamBlockScheduler = _mock.StrictMock<ITeamBlockScheduler>();
			_shiftProjectionCacheManager = _mock.StrictMock<IShiftProjectionCacheManager>();
			_skillDayIntraIntervalIssueExtractor = _mock.StrictMock<ISkillDayIntraIntervalIssueExtractor>();
			_skillStaffPeriodEvaluator = _mock.StrictMock<ISkillStaffPeriodEvaluator>();
			_target = new IntraIntervalOptimizer(_teamInfoFactory, _teamBlockInfoFactory,_teamBlockClearer, _teamBlockScheduler,_shiftProjectionCacheManager, _skillDayIntraIntervalIssueExtractor,_skillStaffPeriodEvaluator);	
			
			_schedulingOptions = new SchedulingOptions();
			_rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingResultStateHolder = _mock.StrictMock<ISchedulingResultStateHolder>();
			_person = PersonFactory.CreatePerson("person");
			_dateOnly = new DateOnly(2014, 1, 1);
			_allScheduleMatrixPros = new List<IScheduleMatrixPro>();
			_resourceCalculateDelayer = _mock.StrictMock<IResourceCalculateDelayer>();
			_skill = SkillFactory.CreateSkill("skill");
			_issusesBefore = new List<ISkillStaffPeriod>();
			_scheduleRange = _mock.StrictMock<IScheduleRange>();
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_editableShift = _mock.StrictMock<IEditableShift>();
			_timeZone = _person.PermissionInformation.DefaultTimeZone();
			_shiftProjectionCache = _mock.StrictMock<IShiftProjectionCache>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_skillDays = new List<ISkillDay>();
			_skillStaffPeriod = _mock.StrictMock<ISkillStaffPeriod>();
			_issuesAfter = new List<ISkillStaffPeriod>{_skillStaffPeriod};
		}

		[Test]
		public void ShouldReturnIssuesWhenResultGetsBetter()
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
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService,_resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(true).IgnoreArguments();
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> {_dateOnly})).Return(_skillDays);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDays, _skill)).Return(_issuesAfter);
				Expect.Call(_skillStaffPeriodEvaluator.ResultIsBetter(_issusesBefore, _issuesAfter)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore);
				Assert.IsNotEmpty(result);
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
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(true).IgnoreArguments();
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDays, _skill)).Return(new List<ISkillStaffPeriod>());
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore);
				Assert.IsEmpty(result);
			}	
		}

		[Test]
		public void ShouldRollbackResourceCalculateAndReturnIssuesWhenScheduleFailed()
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
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_teamBlockScheduler.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, new ShiftNudgeDirective())).Return(false).IgnoreArguments();
				Expect.Call(() => _rollbackService.Rollback());
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null)).Return(true);
				Expect.Call(_schedulingResultStateHolder.SkillDaysOnDateOnly(new List<DateOnly> { _dateOnly })).Return(_skillDays);
				Expect.Call(_skillDayIntraIntervalIssueExtractor.Extract(_skillDays, _skill)).Return(_issuesAfter);
			}

			using (_mock.Playback())
			{
				var result = _target.Optimize(_schedulingOptions, _rollbackService, _schedulingResultStateHolder, _person, _dateOnly, _allScheduleMatrixPros, _resourceCalculateDelayer, _skill, _issusesBefore);
				Assert.IsNotEmpty(result);
			}		
		}
	}
}
