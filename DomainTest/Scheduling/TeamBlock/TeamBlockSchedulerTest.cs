using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
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
		private ISameShiftCategoryBlockScheduler _sameShiftCategoryBlockScheduler;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;
		private ITeamBlockSingleDayScheduler _singleDayScheduler;
		private ITeamBlockRoleModelSelector _roleModelSelector;
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
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private bool _isScheduleFailed;
		private IResourceCalculateDelayer _resourceCalculateDelayer;
		private ISchedulingResultStateHolder _schedulingResultStateHolder;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
		    _sameShiftCategoryBlockScheduler = _mocks.StrictMock<ISameShiftCategoryBlockScheduler>();
		    _teamBlockSchedulingOptions = _mocks.StrictMock<ITeamBlockSchedulingOptions>();
		    _singleDayScheduler = _mocks.StrictMock<ITeamBlockSingleDayScheduler>();
		    _roleModelSelector = _mocks.StrictMock<ITeamBlockRoleModelSelector>();
		    _target = new TeamBlockScheduler(_sameShiftCategoryBlockScheduler, _teamBlockSchedulingOptions,
		                                     _singleDayScheduler, _roleModelSelector);

			_dateOnly = new DateOnly(2013, 11, 12);
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			_person2 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("ball"), _dateOnly);
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_group= new Group(new List<IPerson> { _person1 }, "Hej");
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1 };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(_group, groupMatrixes);
			_blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_blockPeriod));
			_schedulingOptions = new SchedulingOptions();
			_selectedPersons = new List<IPerson> { _person1 };
			_shift = _mocks.StrictMock<IShiftProjectionCache>();
			_resourceCalculateDelayer = _mocks.StrictMock<IResourceCalculateDelayer>();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
		}

		[Test]
		public void ShouldScheduleSameShiftCategory()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(true);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(() => _sameShiftCategoryBlockScheduler.DayScheduled += _target.OnDayScheduled);
				Expect.Call(_sameShiftCategoryBlockScheduler.Schedule(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod,
					_selectedPersons, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder))
					.Return(true);
				Expect.Call(() => _sameShiftCategoryBlockScheduler.DayScheduled -= _target.OnDayScheduled);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod,
					_selectedPersons, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer,
					_schedulingResultStateHolder, null);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldScheduleOptionsOtherThanSameShiftCategory()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions)).Return(_shift);
				Expect.Call(() => _singleDayScheduler.DayScheduled += _target.OnDayScheduled);
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _selectedPersons, _dateOnly,
																  _shift, _blockPeriod, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder)).Return(true);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= _target.OnDayScheduled);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod,
					_selectedPersons, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, null);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void RestrictionAggregatorShouldConsumeSelectedTeamMembersOnly()
		{
			_selectedPersons = new List<IPerson> {_person2};
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);

			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod,
					_selectedPersons, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, null);
				Assert.That(result, Is.True);
			}
		}

		[Test]
		public void ShouldNotifySubscribersWhenScheduleFailed()
		{
			_target.DayScheduled += targetDayScheduled;
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSameShiftCategoryInTeamBlock(_schedulingOptions)).Return(false);
				Expect.Call(_teamBlockSchedulingOptions.IsBlockSchedulingWithSameShiftCategory(_schedulingOptions)).Return(false);
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions)).Return(null);
			}
			using (_mocks.Playback())
			{
				Assert.That(_isScheduleFailed, Is.False);
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod,
														  _selectedPersons, _schedulePartModifyAndRollbackService, _resourceCalculateDelayer, _schedulingResultStateHolder, null);

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
