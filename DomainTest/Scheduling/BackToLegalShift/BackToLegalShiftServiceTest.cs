using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.BackToLegalShift;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.BackToLegalShift
{
	[TestFixture]
	public class BackToLegalShiftServiceTest
	{
		private MockRepository _mocks;
		private IBackToLegalShiftService _target;
		private IBackToLegalShiftWorker _backToLegalShiftWorker;
		private IFirstShiftInTeamBlockFinder _firstShiftInTeamBlockFinder;
		private ILegalShiftDecider _legalShiftDecider;
		private ITeamBlockInfo _teamBlock;
		private SchedulingOptions _schedulingOptions;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;
		private IScheduleDictionary _schedules;
		private ISchedulePartModifyAndRollbackService _rollBackService;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ITeamInfo _teamInfo;
		private IPerson _person;
		private IBlockInfo _blockInfo;
		private ShiftProjectionCache _shiftProjectionCache;
		private IWorkShiftFinderResultHolder _workShiftFinderResultHolder;
		private IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDay _scheduleDay;
		private IPersonAssignment _personAssignment;
		private DateOnly _dateOnly;
		private IScheduleRange _scheduleRange;

		[SetUp]
		public void Setup()
		{
			_dateOnly = new DateOnly(DateTime.Now);
			_mocks = new MockRepository();
			_backToLegalShiftWorker = _mocks.StrictMock<IBackToLegalShiftWorker>();
			_firstShiftInTeamBlockFinder = _mocks.StrictMock<IFirstShiftInTeamBlockFinder>();
			_legalShiftDecider = _mocks.StrictMock<ILegalShiftDecider>();
			_workShiftFinderResultHolder = new WorkShiftFinderResultHolder();
			_dayOffsInPeriodCalculator = _mocks.StrictMock<IDayOffsInPeriodCalculator>();
			_target = new BackToLegalShiftService(_backToLegalShiftWorker, _firstShiftInTeamBlockFinder, _legalShiftDecider, _workShiftFinderResultHolder, _dayOffsInPeriodCalculator, new DoNothingScheduleDayChangeCallBack());
			_teamBlock = _mocks.StrictMock<ITeamBlockInfo>();
			_schedulingOptions = new SchedulingOptions();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_schedules = _mocks.StrictMock<IScheduleDictionary>();
			_rollBackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly());
			_person.Period(_dateOnly).RuleSetBag = new RuleSetBag();
			_blockInfo = new BlockInfo(new DateOnlyPeriod(_dateOnly, _dateOnly));
			_shiftProjectionCache = _mocks.StrictMock<ShiftProjectionCache>();
			_scheduleMatrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_personAssignment = _mocks.StrictMock<IPersonAssignment>();
			_personAssignment = new PersonAssignment(_person, new Scenario("_"), _dateOnly);
			_scheduleRange = _mocks.StrictMock<IScheduleRange>();
		}

		[Test]
		public void ShouldRescheduleIfShiftNotLegal()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_teamBlock.MatrixesForGroupAndBlock()).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro });
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_blockInfo.BlockPeriod.StartDate)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(_personAssignment);

				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlock, _person, _dateOnly, _schedules))
					.Return(_shiftProjectionCache);
				Expect.Call(_legalShiftDecider.IsLegalShift(_person.Period(new DateOnly()).RuleSetBag, _shiftProjectionCache, _scheduleDay)).Return(false);
				int x;
				IList<IScheduleDay> y;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_person.VirtualSchedulePeriod(_dateOnly),
					out x, out y)).Return(true).OutRef(1, y);
				Expect.Call(_backToLegalShiftWorker.ReSchedule(_teamBlock, _schedulingOptions, _shiftProjectionCache,
					_rollBackService, _resourceCalculateDelayer, _schedulingResultStateHolder)).Return(true);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.Any();
				Expect.Call(_schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
			}
			using (_mocks.Playback())
			{
				_target.Execute(new List<ITeamBlockInfo> { _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
					_rollBackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldReportBackFailedReschedule()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_teamBlock.MatrixesForGroupAndBlock()).Return(new List<IScheduleMatrixPro> { _scheduleMatrixPro });
				Expect.Call(_scheduleMatrixPro.GetScheduleDayByKey(_blockInfo.BlockPeriod.StartDate)).Return(_scheduleDayPro);
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.PersonAssignment(true)).Return(_personAssignment);
				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlock, _person, _dateOnly, _schedules))
					.Return(_shiftProjectionCache);
				Expect.Call(_legalShiftDecider.IsLegalShift(_person.Period(_dateOnly).RuleSetBag, _shiftProjectionCache, _scheduleDay)).Return(false);
				int x;
				IList<IScheduleDay> y;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_person.VirtualSchedulePeriod(_dateOnly),
					out x, out y)).Return(true).OutRef(1, y);
				Expect.Call(_backToLegalShiftWorker.ReSchedule(_teamBlock, _schedulingOptions, _shiftProjectionCache,
					_rollBackService, _resourceCalculateDelayer, _schedulingResultStateHolder)).Return(false);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.Any();
				Expect.Call(_schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
			}
			using (_mocks.Playback())
			{
				_target.Execute(new List<ITeamBlockInfo> { _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
					_rollBackService, _resourceCalculateDelayer);
				Assert.AreEqual(_person, _workShiftFinderResultHolder.GetResults()[0].Person);
				Assert.AreEqual(_dateOnly, _workShiftFinderResultHolder.GetResults()[0].ScheduleDate);
			}
		}

		[Test]
		public void ShouldReportBackFailedRescheduleIfTooFewDaysOff()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlock, _person, _dateOnly, _schedules))
					.Return(_shiftProjectionCache);
				Expect.Call(_legalShiftDecider.IsLegalShift(_person.Period(_dateOnly).RuleSetBag, _shiftProjectionCache, _scheduleDay)).Return(false);
				int x;
				IList<IScheduleDay> y;
				Expect.Call(_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(_person.VirtualSchedulePeriod(_dateOnly),
					out x, out y)).Return(false).OutRef(1, new List<IScheduleDay>());
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.Any();
				Expect.Call(_schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
			}
			using (_mocks.Playback())
			{
				_target.Execute(new List<ITeamBlockInfo> { _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
					_rollBackService, _resourceCalculateDelayer);
				Assert.AreEqual(_person, _workShiftFinderResultHolder.GetResults()[0].Person);
				Assert.AreEqual(_dateOnly, _workShiftFinderResultHolder.GetResults()[0].ScheduleDate);
			}
		}

		[Test]
		public void ShouldNotRescheduleIfShiftLegal()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlock, _person, _dateOnly, _schedules))
					.Return(_shiftProjectionCache);
				Expect.Call(_legalShiftDecider.IsLegalShift(_person.Period(_dateOnly).RuleSetBag, _shiftProjectionCache, _scheduleDay)).Return(true);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.Any();
				Expect.Call(_schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
			}
			using (_mocks.Playback())
			{
				_target.Execute(new List<ITeamBlockInfo> { _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
					_rollBackService, _resourceCalculateDelayer);
			}
		}

		[Test]
		public void ShouldTrowIfTeamMembersMoreThanOne()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person, _person });
			}
			Assert.Throws<ArgumentException>(() =>
			{
				using (_mocks.Playback())
				{
					_target.Execute(new List<ITeamBlockInfo> { _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
						_rollBackService, _resourceCalculateDelayer);
				}

			});

		}

		[Test]
		public void ShouldTrowIfBlockPeriodMoreThanOneDay()
		{
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2014, 9, 23, 2014, 9, 24));
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);
			}
			Assert.Throws<ArgumentException>(() =>
			{
				using (_mocks.Playback())
				{
					_target.Execute(new List<ITeamBlockInfo> { _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
						_rollBackService, _resourceCalculateDelayer);
				}
			});

		}

		[Test]
		public void ShouldCancel()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				//the loop
				Expect.Call(_teamBlock.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_teamBlock.BlockInfo).Return(_blockInfo);

				Expect.Call(_firstShiftInTeamBlockFinder.FindFirst(_teamBlock, _person, _dateOnly, _schedules))
					.Return(_shiftProjectionCache);
				Expect.Call(_legalShiftDecider.IsLegalShift(_person.Period(_dateOnly).RuleSetBag, _shiftProjectionCache, _scheduleDay)).Return(true);
				Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedules).Repeat.Any();
				Expect.Call(_schedules[_person]).Return(_scheduleRange);
				Expect.Call(_scheduleRange.ScheduledDay(_dateOnly)).Return(_scheduleDay);
			}
			using (_mocks.Playback())
			{
				_target.Progress += _target_Progress;
				_target.Execute(new List<ITeamBlockInfo> { _teamBlock, _teamBlock }, _schedulingOptions, _schedulingResultStateHolder,
					_rollBackService, _resourceCalculateDelayer);
				_target.Progress -= _target_Progress;
			}
		}

		private void _target_Progress(object sender, BackToLegalShiftArgs e)
		{
			Assert.AreEqual(2, e.TotalBlocks);
			Assert.AreEqual(1, e.ProcessedBlocks);
			e.Cancel = true;
		}

	}
}