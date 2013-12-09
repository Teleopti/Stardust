using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class TeamBlockSwapTest
	{
		private MockRepository _mock;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private ISwapServiceNew _swapServiceNew;
		private TeamBlockSwap _target;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IScheduleDictionary _scheduleDictionary;
		private ITeamBlockSwapValidator _teamBlockSwapValidator;
		private ITeamBlockSwapDayValidator _teamBlockSwapDayValidator;
		private ISchedulePartModifyAndRollbackService _modifyAndRollbackService;
		private ITeamInfo _teamInfo1;
		private ITeamInfo _teamInfo2;
		private IGroupPerson _groupPerson1;
		private IGroupPerson _groupPerson2;
		private IPerson _person1;
		private IPerson _person2;
		private IList<IPerson> _persons1;
		private IList<IPerson> _persons2;
		private IBlockInfo _blockInfo1;
		private IBlockInfo _blockInfo2;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IScheduleRange _scheduleRange1;
		private IScheduleRange _scheduleRange2;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_dateOnlyPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);
			_scheduleDay1 = ScheduleDayFactory.Create(_dateOnlyPeriod.StartDate);
			_scheduleDay2 = ScheduleDayFactory.Create(_dateOnlyPeriod.StartDate);
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_swapServiceNew = _mock.StrictMock<ISwapServiceNew>();
			_scheduleMatrixPro1 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
			_teamBlockSwapValidator = _mock.StrictMock<ITeamBlockSwapValidator>();
			_teamBlockSwapDayValidator = _mock.StrictMock<ITeamBlockSwapDayValidator>();
			_modifyAndRollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
			_teamInfo1 = _mock.StrictMock<ITeamInfo>();
			_teamInfo2 = _mock.StrictMock<ITeamInfo>();
			_groupPerson1 = _mock.StrictMock<IGroupPerson>();
			_groupPerson2 = _mock.StrictMock<IGroupPerson>();
			_person1 = PersonFactory.CreatePerson("Person1");
			_person2 = PersonFactory.CreatePerson("Person2");
			_persons1 = new List<IPerson>{_person1};
			_persons2 = new List<IPerson>{_person2};
			_blockInfo1 = _mock.StrictMock<IBlockInfo>();
			_blockInfo2 = _mock.StrictMock<IBlockInfo>();
			_scheduleRange1 = _mock.StrictMock<IScheduleRange>();
			_scheduleRange2 = _mock.StrictMock<IScheduleRange>();
			_target = new TeamBlockSwap(_swapServiceNew, _teamBlockSwapValidator, _teamBlockSwapDayValidator);
		}

		[Test]
		public void ShouldSwap()
		{
			var swappedList = new List<IScheduleDay> {_scheduleDay1, _scheduleDay2};

			using (_mock.Record())
			{
				Expect.Call(_teamBlockSwapValidator.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo1.GroupPerson).Return(_groupPerson1);
				Expect.Call(_teamInfo2.GroupPerson).Return(_groupPerson2);
				Expect.Call(_groupPerson1.GroupMembers).Return(_persons1);
				Expect.Call(_groupPerson2.GroupMembers).Return(_persons2);
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
				Expect.Call(_teamBlockInfo2.BlockInfo).Return(_blockInfo2);
				Expect.Call(_blockInfo1.BlockPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_blockInfo2.BlockPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1);
				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2);
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnlyPeriod.StartDate)).Return(_scheduleDay1);
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnlyPeriod.StartDate)).Return(_scheduleDay2);
				Expect.Call(_teamBlockSwapDayValidator.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).Return(true);
				Expect.Call(_swapServiceNew.Swap(new List<IScheduleDay> {_scheduleDay1, _scheduleDay2}, _scheduleDictionary)).Return(swappedList);
				Expect.Call(()=>_modifyAndRollbackService.ClearModificationCollection()).Repeat.AtLeastOnce();
				Expect.Call(_modifyAndRollbackService.ModifyParts(swappedList));
			}

			using (_mock.Playback())
			{
				var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotSwapIfValidatorFails()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockSwapValidator.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(false);	
			}

			using (_mock.Playback())
			{
				var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary);
				Assert.IsFalse(result);	
			}
		}

		[Test]
		public void ShouldNotSwapIfValidatorDayFails()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockSwapValidator.ValidateCanSwap(_teamBlockInfo1, _teamBlockInfo2)).Return(true);
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo1.GroupPerson).Return(_groupPerson1);
				Expect.Call(_teamInfo2.GroupPerson).Return(_groupPerson2);
				Expect.Call(_groupPerson1.GroupMembers).Return(_persons1);
				Expect.Call(_groupPerson2.GroupMembers).Return(_persons2);
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
				Expect.Call(_teamBlockInfo2.BlockInfo).Return(_blockInfo2);
				Expect.Call(_blockInfo1.BlockPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_blockInfo2.BlockPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_scheduleDictionary[_person1]).Return(_scheduleRange1);
				Expect.Call(_scheduleDictionary[_person2]).Return(_scheduleRange2);
				Expect.Call(_scheduleRange1.ScheduledDay(_dateOnlyPeriod.StartDate)).Return(_scheduleDay1);
				Expect.Call(_scheduleRange2.ScheduledDay(_dateOnlyPeriod.StartDate)).Return(_scheduleDay2);
				Expect.Call(_teamBlockSwapDayValidator.ValidateSwapDays(_scheduleDay1, _scheduleDay2)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.Swap(_teamBlockInfo1, _teamBlockInfo2, _modifyAndRollbackService, _scheduleDictionary);
				Assert.IsFalse(result);
			}	
		}
	}
}
