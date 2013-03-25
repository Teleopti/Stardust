using System;
using System.Collections.Generic;
using System.ComponentModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
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
		private ISchedulingResultStateHolder _stateHolder;
		private IDeleteAndResourceCalculateService _deleteAndResourceCalculateService;
		private SchedulingOptions _schedulingOptions;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private ITeamBlockInfo _teamBlockInfo;
		private IScheduleDictionary _scheduleDictionary;
		private IPerson _person1;
		//private IPerson _person2;
		private IScheduleRange _range;
		private IScheduleDay _scheduleDay;
		private IList<IScheduleDay> _toRemoveList;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_stateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
			_deleteAndResourceCalculateService = _mocks.StrictMock<IDeleteAndResourceCalculateService>();
			_target = new TeamBlockClearer(_stateHolder, _deleteAndResourceCalculateService);
			_schedulingOptions = new SchedulingOptions();
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_person1 = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
			//_person2 = new Person();
			IGroupPerson groupPerson = new GroupPerson(new List<IPerson>{ _person1 }, DateOnly.MinValue, "Hej", Guid.NewGuid());
			ITeamInfo teamInfo = new TeamInfo(groupPerson, new List<IList<IScheduleMatrixPro>>());
			_teamBlockInfo = new TeamBlockInfo(teamInfo, new BlockInfo(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue)));
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_range = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_toRemoveList = new List<IScheduleDay> { _scheduleDay };
		}

		[Test]
		public void ShouldClearBlock()
		{

			using (_mocks.Record())
			{
				Expect.Call(_stateHolder.Schedules).Return(_scheduleDictionary);
				Expect.Call(_scheduleDictionary[_person1]).Return(_range);
				Expect.Call(_range.ScheduledDay(DateOnly.MinValue)).Return(_scheduleDay);
				Expect.Call(_scheduleDay.SignificantPart()).Return(SchedulePartView.MainShift);
				Expect.Call(_deleteAndResourceCalculateService.DeleteWithResourceCalculation(_toRemoveList,
				                                                                             _rollbackService,
				                                                                             _schedulingOptions.ConsiderShortBreaks))
					  .Return(_toRemoveList);
			}

			using (_mocks.Playback())
			{
				_target.ClearTeamBlock(_schedulingOptions, _rollbackService, _teamBlockInfo);
			}
      
      
		}

	}
}