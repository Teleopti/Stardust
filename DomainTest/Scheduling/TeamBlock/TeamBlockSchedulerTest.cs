using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
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

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
		    _singleDayScheduler = _mocks.StrictMock<ITeamBlockSingleDayScheduler>();
		    _roleModelSelector = _mocks.StrictMock<ITeamBlockRoleModelSelector>();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
			//seems only be tested when toggle is on so I hard code it here
		    _target = new TeamBlockScheduler(_singleDayScheduler, _roleModelSelector, _teamBlockClearer, _teamBlockSchedulingOptions);
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
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_shiftNudgeDirective = new ShiftNudgeDirective();
		}

		[Test]
		public void ShouldSchedule()
		{
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction()) ).Return(_shift);
				Expect.Call(() => _singleDayScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly,
					_shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, _shiftNudgeDirective.EffectiveRestriction )).Return(true);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= null).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions,
					_schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective);
				Assert.That(result, Is.True);
			}
		}

		

		[Test]
		public void ShouldClearTeamBlockAndRetryIfNoSuccessAndUseSameShift()
		{
			_schedulingOptions.BlockSameShift = true;
			_schedulingOptions.UseBlock = true;

			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction())).Return(_shift).Repeat.Twice();
				Expect.Call(() => _singleDayScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly,_shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer,_schedulingResultStateHolder, _shiftNudgeDirective.EffectiveRestriction)).Return(false);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(()=>_teamBlockClearer.ClearTeamBlock(_schedulingOptions, _schedulePartModifyAndRollbackService,_teamBlockInfo));
				Expect.Call(() => _singleDayScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, _shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective.EffectiveRestriction)).Return(true);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= null).IgnoreArguments();
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions,_schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective);
				Assert.That(result, Is.True);
			}	
		}

		[Test]
		public void ShouldRollBackIfFailedRetryOnUseSameShift()
		{
			_schedulingOptions.BlockSameShift = true;
			_schedulingOptions.UseBlock = true;

			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction())).Return(_shift).Repeat.Twice();
				Expect.Call(() => _singleDayScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, _shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective.EffectiveRestriction)).Return(false);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _schedulePartModifyAndRollbackService, _teamBlockInfo));
				Expect.Call(() => _singleDayScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, _shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective.EffectiveRestriction)).Return(false);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(()=>_schedulePartModifyAndRollbackService.Rollback());
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false)).Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective);
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

			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction())).Return(_shift);
				Expect.Call(() => _singleDayScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, _shift, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective.EffectiveRestriction)).Return(false);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _schedulePartModifyAndRollbackService, _teamBlockInfo));
				Expect.Call(_shift.TheMainShift).Return(mainShift);
				Expect.Call(mainShift.ShiftCategory).Return(shiftCategory);
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction())).Return(null);
				Expect.Call(() => _singleDayScheduler.DayScheduled += null).IgnoreArguments();
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _dateOnly, null, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective.EffectiveRestriction)).Return(false);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= null).IgnoreArguments();

				Expect.Call(() => _schedulePartModifyAndRollbackService.Rollback());
				Expect.Call(_resourceCalculateDelayer.CalculateIfNeeded(_dateOnly, null, false)).Return(true);

			}

			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective);
				Assert.That(result, Is.False);	
			}
		}


		[Test]
		public void ShouldNotifySubscribersWhenScheduleFailed()
		{
			_target.DayScheduled += targetDayScheduled;
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions, new EffectiveRestriction())).Return(null);
			}
			using (_mocks.Playback())
			{
				Assert.That(_isScheduleFailed, Is.False);
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions,
														  _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, _shiftNudgeDirective);

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
