using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class SameShiftCategoryBlockSchedulerTest
	{
		private MockRepository _mocks;
		private ITeamBlockRoleModelSelector _roleModelSelector;
		private ITeamBlockSingleDayScheduler _singleDayScheduler;
		private ITeamBlockSchedulingCompletionChecker _teamBlockSchedulingCompletionChecker;
		private ITeamBlockClearer _teamBlockClearer;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ISameShiftCategoryBlockScheduler _target;
		private DateOnly _dateOnly;
		private IPerson _person1;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private Group _group;
		private DateOnlyPeriod _blockPeriod;
		private TeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private List<IPerson> _selectedPersons;
		private IShiftProjectionCache _shift;
		private IPerson _person2;
		private bool _isScheduleFailed;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_roleModelSelector = _mocks.StrictMock<ITeamBlockRoleModelSelector>();
			_singleDayScheduler = _mocks.StrictMock<ITeamBlockSingleDayScheduler>();
			_teamBlockSchedulingCompletionChecker = _mocks.StrictMock<ITeamBlockSchedulingCompletionChecker>();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_target = new SameShiftCategoryBlockScheduler(_roleModelSelector, _singleDayScheduler,
			                                              _teamBlockSchedulingCompletionChecker, _teamBlockClearer);

			_dateOnly = new DateOnly(2013, 11, 12);
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("ball"), _dateOnly);
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_group = new Group(new List<IPerson> { _person1 }, "Hej");
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1 };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(_group, groupMatrixes);
			_blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_blockPeriod));
			_schedulingOptions = new SchedulingOptions();
			_selectedPersons = new List<IPerson> { _person1 };
			_shift = _mocks.StrictMock<IShiftProjectionCache>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
		}

		[Test]
		public void ShouldBeFalseIfRoleModelIsNull()
		{
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions)).Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Schedule(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod, _selectedPersons,
														  _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);

				Assert.That(result, Is.False);
			}
		}

		[Test]
		public void ShouldBanAShiftCategoryAndClearTeamBlock()
		{
			var workShift = _mocks.StrictMock<IWorkShift>();
			var cat = ShiftCategoryFactory.CreateShiftCategory("cat");
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions)).Return(_shift);
				Expect.Call(() => _singleDayScheduler.DayScheduled += _target.OnDayScheduled);
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _selectedPersons, _dateOnly, _shift,
														  _blockPeriod, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder)).Return(false);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(_shift.TheWorkShift).Return(workShift);
				Expect.Call(workShift.ShiftCategory).Return(cat);

				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
				                                                                                              _dateOnly,
				                                                                                              _selectedPersons))
				      .Return(false);
				Expect.Call(() => _teamBlockClearer.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo));
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions)).Return(null);
			}
			using (_mocks.Playback())
			{
				var result = _target.Schedule(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod, _selectedPersons,
														  _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);

				Assert.That(result, Is.False);
			}
		}

		[Test]
		public void ShouldNotBanAShiftCategoryIfSchedulesucceeded()
		{
			var workShift = _mocks.StrictMock<IWorkShift>();
			var cat = ShiftCategoryFactory.CreateShiftCategory("cat");
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions)).Return(_shift);
				Expect.Call(() => _singleDayScheduler.DayScheduled += _target.OnDayScheduled);
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _selectedPersons, _dateOnly, _shift,
														  _blockPeriod, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder)).Return(false);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= _target.OnDayScheduled);
				Expect.Call(_shift.TheWorkShift).Return(workShift);
				Expect.Call(workShift.ShiftCategory).Return(cat);

				Expect.Call(_teamBlockSchedulingCompletionChecker.IsDayScheduledInTeamBlockForSelectedPersons(_teamBlockInfo,
				                                                                                              _dateOnly,
				                                                                                              _selectedPersons))
				      .Return(true);
			}
			using (_mocks.Playback())
			{
				var result = _target.Schedule(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod, _selectedPersons,
														  _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);

				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void RestrictionAggregatorShouldConsumeSelectedTeamMembersOnly()
		{
			_selectedPersons = new List<IPerson> {_person2};

			var result = _target.Schedule(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod,
										  _selectedPersons,
														  _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);
			Assert.That(result, Is.True);
		}

		[Test]
		public void ShouldNotifySubscribersWhenScheduleFailed()
		{
			_target.DayScheduled += targetDayScheduled;
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions)).Return(null);
			}
			using (_mocks.Playback())
			{
				Assert.That(_isScheduleFailed, Is.False);
				var result = _target.Schedule(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod,
										  _selectedPersons, _rollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder);

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
