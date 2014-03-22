using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
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
		private GroupPerson _groupPerson;
		private DateOnlyPeriod _blockPeriod;
		private TeamBlockInfo _teamBlockInfo;
		private SchedulingOptions _schedulingOptions;
		private List<IPerson> _selectedPersons;
		private IShiftProjectionCache _shift;
		private ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private bool _isScheduleFailed;
		private ITeamBlockClearer _teamBlockClearer;
		private ITeamBlockSchedulingOptions _teamBlockSchedulingOptions;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
		    _singleDayScheduler = _mocks.StrictMock<ITeamBlockSingleDayScheduler>();
		    _roleModelSelector = _mocks.StrictMock<ITeamBlockRoleModelSelector>();
			_teamBlockClearer = _mocks.StrictMock<ITeamBlockClearer>();
			_teamBlockSchedulingOptions = new TeamBlockSchedulingOptions();
		    _target = new TeamBlockScheduler(_singleDayScheduler, _roleModelSelector, _teamBlockClearer, _teamBlockSchedulingOptions);

			_dateOnly = new DateOnly(2013, 11, 12);
			_person1 = PersonFactory.CreatePersonWithValidVirtualSchedulePeriod(PersonFactory.CreatePerson("bill"), _dateOnly);
			_scheduleMatrixPro1 = _mocks.StrictMock<IScheduleMatrixPro>();
			_groupPerson = new GroupPerson(new List<IPerson> { _person1 }, _dateOnly, "Hej", Guid.Empty);
			IList<IScheduleMatrixPro> matrixList = new List<IScheduleMatrixPro> { _scheduleMatrixPro1 };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>> { matrixList };
			ITeamInfo teamInfo = new TeamInfo(_groupPerson, groupMatrixes);
			_blockPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_blockPeriod));
			_schedulingOptions = new SchedulingOptions();
			_selectedPersons = new List<IPerson> { _person1 };
			_shift = _mocks.StrictMock<IShiftProjectionCache>();
			_schedulePartModifyAndRollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
		}

		
		[Test]
		public void ShouldSchedule()
		{
			using (_mocks.Record())
			{
				Expect.Call(_roleModelSelector.Select(_teamBlockInfo, _dateOnly, _person1, _schedulingOptions)).Return(_shift);
				Expect.Call(() => _singleDayScheduler.DayScheduled += _target.OnDayScheduled);
				Expect.Call(_singleDayScheduler.ScheduleSingleDay(_teamBlockInfo, _schedulingOptions, _selectedPersons, _dateOnly,
				                                                  _shift, _blockPeriod)).Return(true);
				Expect.Call(() => _singleDayScheduler.DayScheduled -= _target.OnDayScheduled);
			}
			using (_mocks.Playback())
			{
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod,
														  _selectedPersons, _schedulePartModifyAndRollbackService);
				Assert.That(result, Is.True);
			}
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
				var result = _target.ScheduleTeamBlockDay(_teamBlockInfo, _dateOnly, _schedulingOptions, _blockPeriod,
														  _selectedPersons, _schedulePartModifyAndRollbackService);

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
