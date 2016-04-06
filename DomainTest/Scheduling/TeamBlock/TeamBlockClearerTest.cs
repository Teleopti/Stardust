using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockClearerTest
	{
		private MockRepository _mocks;
		private ITeamBlockClearer _target;
		private IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private SchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ITeamBlockInfo _teamBlockInfo;
		private IPerson _person1;
		private IScheduleDay _scheduleDay;
		private IList<IScheduleDay> _toRemoveList;
		private Group _group;
		private IScheduleMatrixPro _matrix;
		private IVirtualSchedulePeriod _schedulePeriod;
		private IScheduleDayPro _scheduleDayPro;
		private DateOnlyPeriod _dateOnlyPeriod;
		private DateOnly _dateOnly;

		[SetUp]
		public void Setup()
		{
			_dateOnly = new DateOnly(2015, 1, 1);
			_dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
			_mocks = new MockRepository();
			_deleteAndResourceCalculateService = _mocks.StrictMock<IDeleteAndResourceCalculateService>();
			_target = new TeamBlockClearer(_deleteAndResourceCalculateService, null);
			_schedulingOptions = new SchedulingOptions();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			_group = new Group(new List<IPerson>{ _person1 }, "Hej");
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			var matrixes = new List<IScheduleMatrixPro> { _matrix };
			IList<IList<IScheduleMatrixPro>> groupMatrixes = new List<IList<IScheduleMatrixPro>>();
			groupMatrixes.Add(matrixes);
			ITeamInfo teamInfo = new TeamInfo(_group, groupMatrixes);
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_dateOnlyPeriod));
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_toRemoveList = new List<IScheduleDay> { _scheduleDay };
			_schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
		}

		[Test]
		public void ShouldClearBlock()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_matrix.Person).Return(_person1);
				Expect.Call(_matrix.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
				Expect.Call(_matrix.UnlockedDays)
				      .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> {_scheduleDayPro}));
				Expect.Call(_scheduleDayPro.DaySchedulePart()).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(_toRemoveList,
				                                                                             _rollbackService,
				                                                                             _schedulingOptions.ConsiderShortBreaks, false));
			}

			using (_mocks.Playback())
			{
				_target.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo);
			}
		}

		[Test]
		public void ShouldNotClearLockedBlock()
		{
			using (_mocks.Record())
			{
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay>(), _rollbackService, _schedulingOptions.ConsiderShortBreaks, false));
			}

			using (_mocks.Playback())
			{
				
				_teamBlockInfo.TeamInfo.LockMember(_dateOnlyPeriod, _person1);
				_target.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo);
			}
		}

		[Test]
		public void ShouldNotDeleteLockedDayInMatrix()
		{
			using (_mocks.Record())
			{
				Expect.Call(_matrix.SchedulePeriod).Return(_schedulePeriod);
				Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_matrix.Person).Return(_person1);
				Expect.Call(_matrix.GetScheduleDayByKey(_dateOnly)).Return(_scheduleDayPro);
				Expect.Call(_matrix.UnlockedDays)
					  .Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>()));
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay>(),
																							 _rollbackService,
																							 _schedulingOptions.ConsiderShortBreaks, false));
			}

			using (_mocks.Playback())
			{
				_target.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo);
			}
		}

		[Test]
		public void ShouldHandleNullMatrix()
		{
			ITeamInfo teamInfo = new TeamInfo(_group, new List<IList<IScheduleMatrixPro>>());
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(_dateOnlyPeriod));

			using (_mocks.Record())
			{
				Expect.Call(() => _deleteAndResourceCalculateService.DeleteWithResourceCalculation(new List<IScheduleDay>(), 
																							 _rollbackService,
																							 _schedulingOptions.ConsiderShortBreaks, false));
			}

			using (_mocks.Playback())
			{
				_target.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo);
			}
		}
	}
}