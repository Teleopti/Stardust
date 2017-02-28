﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockSchedulerTest
	{
		private TeamBlockScheduler _target;
		private MockRepository _mocks;
		private ITeamBlockSingleDayScheduler _singleDayScheduler;
		private ITeamBlockRoleModelSelector _roleModelSelector;
		private DateOnly _dateOnly;
		private IPerson _person1;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private Group _group;
		private DateOnlyPeriod _blockPeriod;
		private TeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private IShiftProjectionCache _shift;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private bool _isScheduleFailed;
		private ITeamBlockClearer _teamBlockClearer;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private ShiftNudgeDirective _shiftNudgeDirective;
		private readonly GroupPersonSkillAggregator groupPersonSkillAggregator = new GroupPersonSkillAggregator(new PersonalSkillsProvider());

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
		    _singleDayScheduler = _mocks.StrictMock<ITeamBlockSingleDayScheduler>();
		    _roleModelSelector = _mocks.StrictMock<ITeamBlockRoleModelSelector>();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
			//seems only be tested when toggle is on so I hard code it here
		    _target = new TeamBlockScheduler(_singleDayScheduler, _roleModelSelector, _teamBlockClearer, _teamBlockSchedulingOptions, groupPersonSkillAggregator);
			_dateOnly = new DateOnly(2013, 11, 12);
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_group= new Group(new List<IPerson> { _person1 }, "Hej");
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1 };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(_group, groupMatrixes);
			_blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_blockPeriod));
			_schedulingOptions = new SchedulingOptions();
			_shift = _mocks.StrictMock<IShiftProjectionCache>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingResultStateHolder = _mocks.Stub<ISchedulingResultStateHolder>();
			_shiftNudgeDirective = new ShiftNudgeDirective();
		}

		[Test]
		public void ShouldSchedule()
		{
			var schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			var skillDays = Enumerable.Empty<ISkillDay>();
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(schedules, skillDays, null, _teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction(), groupPersonSkillAggregator) ).Return(_shift);
				
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(null, _teamBlockInfo, _schedulingOptions, _dateOnly,
					_shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, null,
					null, _shiftNudgeDirective.EffectiveRestriction, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder),  null )).Return(true).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions,
					_schedulePartModifyAndRollbackService, _resourceCalculateDelayer, skillDays, schedules, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator);
				Assert.That(result, Is.True);
			}
		}

		

		[Test]
		public void ShouldClearTeamBlockAndRetryIfNoSuccessAndUseSameShift()
		{
			_schedulingOptions.BlockSameShift = true;
			_schedulingOptions.UseBlock = true;
			var schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			var skillDays = Enumerable.Empty<ISkillDay>();
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(schedules, skillDays,null,_teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction(), groupPersonSkillAggregator)).Return(_shift).Repeat.Twice();
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(null, _teamBlockInfo, _schedulingOptions, _dateOnly,_shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer,null, null, _shiftNudgeDirective.EffectiveRestriction, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null)).Return(false).IgnoreArguments();
				Expect.Call(()=>_teamBlockClearer.ClearTeamBlock(_schedulingOptions, _schedulePartModifyAndRollbackService,_teamBlockInfo));
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(null, _teamBlockInfo, _schedulingOptions, _dateOnly, _shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective.EffectiveRestriction, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null)).Return(true).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions,_schedulePartModifyAndRollbackService, _resourceCalculateDelayer, skillDays, schedules, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator);
				Assert.That(result, Is.True);
			}	
		}

		[Test]
		public void ShouldRollBackIfFailedRetryOnUseSameShift()
		{
			_schedulingOptions.BlockSameShift = true;
			_schedulingOptions.UseBlock = true;
			var schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			var skillDays = Enumerable.Empty<ISkillDay>();
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(schedules, skillDays, null, _teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction(), groupPersonSkillAggregator)).Return(_shift).Repeat.Twice();
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(null, _teamBlockInfo, _schedulingOptions, _dateOnly, _shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective.EffectiveRestriction, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null)).Return(false).IgnoreArguments();
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _schedulePartModifyAndRollbackService, _teamBlockInfo));
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(null, _teamBlockInfo, _schedulingOptions, _dateOnly, _shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective.EffectiveRestriction, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null)).Return(false).IgnoreArguments();
				Expect.Call(()=>_schedulePartModifyAndRollbackService.RollbackMinimumChecks());
				_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, skillDays, schedules, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator);
				Assert.That(result, Is.False);
			}
		}

		[Test]
		public void ShouldRollBackIfFailedRetryOnUseSameShiftCategory()
		{
			_schedulingOptions.BlockSameShiftCategory = true;
			_schedulingOptions.UseBlock = true;
			_schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.BetweenDayOff;
			var mainShift = _mocks.StrictMock<IEditableShift>();
			var shiftCategory = new ShiftCategory("shiftCategory");
			var schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			var skillDays = Enumerable.Empty<ISkillDay>();
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(schedules, skillDays, null, _teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction(), groupPersonSkillAggregator)).Return(_shift);
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(null, _teamBlockInfo, _schedulingOptions, _dateOnly, _shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective.EffectiveRestriction, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null)).Return(false).IgnoreArguments();
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _schedulePartModifyAndRollbackService, _teamBlockInfo));
				Expect.Call(_shift.TheMainShift).Return(mainShift);
				Expect.Call(mainShift.ShiftCategory).Return(shiftCategory);
				Expect.Call(_roleModelSelector.Select(schedules, skillDays, null, _teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction(), groupPersonSkillAggregator)).Return(null);
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(null, _teamBlockInfo, _schedulingOptions, _dateOnly, null, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, null, null, _shiftNudgeDirective.EffectiveRestriction, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), null)).Return(false).IgnoreArguments();
				Expect.Call(() => _schedulePartModifyAndRollbackService.RollbackMinimumChecks());
				_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false);

			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, skillDays, schedules, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator);
				Assert.That(result, Is.False);	
			}
		}


		[Test]
		public void ShouldNotifySubscribersWhenScheduleFailed()
		{
			_target.DayScheduled += targetDayScheduled;
			var schedules = MockRepository.GenerateMock<IScheduleDictionary>();
			var skillDays = Enumerable.Empty<ISkillDay>();
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(schedules, skillDays, null, _teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction(), groupPersonSkillAggregator)).Return(null);
			}
			using (_mocks.Playback())
			{
				Assert.That(_isScheduleFailed, Is.False);
				var result = _target.ScheduleTeamBlockDay(null, _teamBlockInfo, _dateOnly, _schedulingOptions,
														  _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, skillDays, schedules, _shiftNudgeDirective, NewBusinessRuleCollection.AllForScheduling(_schedulingResultStateHolder), groupPersonSkillAggregator);

				Assert.That(result, Is.False);
				Assert.That(_isScheduleFailed, Is.True);
			}
			_target.DayScheduled -= targetDayScheduled;
		}

		private void targetDayScheduled(object sender, SchedulingServiceBaseEventArgs e)
		{
			if (e is SchedulingServiceFailedEventArgs)
				_isScheduleFailed = true;
		}	
	}
}
