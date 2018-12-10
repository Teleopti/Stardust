using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;



namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class TeamBlockSwapperTest
	{
		private MockRepository _mocks;
		private ITeamBlockSwapper _target;
		private ISchedulePartModifyAndRollbackService _rollbackService;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _range1;
		private IScheduleRange _range2;
		private ISwapServiceNew _swapServiceNew;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private IScheduleDay _day1;
		private IScheduleDay _day2;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_swapServiceNew = _mocks.StrictMock<ISwapServiceNew>();
			_target = new TeamBlockSwapper(_swapServiceNew);
			_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mocks.StrictMock<ITeamBlockInfo>();
			_range1 = _mocks.StrictMock<IScheduleRange>();
			_range2 = _mocks.StrictMock<IScheduleRange>();
			_day1 = _mocks.StrictMock<IScheduleDay>();
			_day2 = _mocks.StrictMock<IScheduleDay>();
		}

		[Test]
		public void ShouldSwapIfNoBusinessRuleResponse()
		{
			var teamInfo1 = _mocks.StrictMock<ITeamInfo>();
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var swapList = new List<IScheduleDay> {_day1, _day2};
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(teamInfo1);
				Expect.Call(teamInfo1.GroupMembers).Return(new List<IPerson> { person1 });
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(new BlockInfo(new DateOnlyPeriod(2013, 12, 3, 2013, 12, 3)));
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(teamInfo1);
				Expect.Call(teamInfo1.GroupMembers).Return(new List<IPerson> { person2 });
				Expect.Call(_scheduleDictionary[person1]).Return(_range1);
				Expect.Call(_range1.ScheduledDay(new DateOnly(2013, 12, 3))).Return(_day1);
				Expect.Call(_scheduleDictionary[person2]).Return(_range2);
				Expect.Call(_range2.ScheduledDay(new DateOnly(2013, 12, 3))).Return(_day2);

				Expect.Call(_swapServiceNew.Swap(new List<IScheduleDay> {_day1, _day2}, _scheduleDictionary)).Return(swapList);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_rollbackService.ModifyParts(swapList)).Return(new List<IBusinessRuleResponse>());
			}

			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary);
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotSwapIfAnyBusinessRuleResponse()
		{
			var teamInfo1 = _mocks.StrictMock<ITeamInfo>();
			var person1 = PersonFactory.CreatePerson();
			var person2 = PersonFactory.CreatePerson();
			var swapList = new List<IScheduleDay> { _day1, _day2 };
			IBusinessRuleResponse response = new BusinessRuleResponse(typeof (NewDayOffRule), "", true, false,
			                                                          new DateTimePeriod(), person1,
			                                                          new DateOnlyPeriod(2013, 12, 3, 2013, 12, 3), "tjillevippen");
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(teamInfo1);
				Expect.Call(teamInfo1.GroupMembers).Return(new List<IPerson> { person1 });
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(new BlockInfo(new DateOnlyPeriod(2013, 12, 3, 2013, 12, 3)));
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(teamInfo1);
				Expect.Call(teamInfo1.GroupMembers).Return(new List<IPerson> { person2 });
				Expect.Call(_scheduleDictionary[person1]).Return(_range1);
				Expect.Call(_range1.ScheduledDay(new DateOnly(2013, 12, 3))).Return(_day1);
				Expect.Call(_scheduleDictionary[person2]).Return(_range2);
				Expect.Call(_range2.ScheduledDay(new DateOnly(2013, 12, 3))).Return(_day2);

				Expect.Call(_swapServiceNew.Swap(new List<IScheduleDay> { _day1, _day2 }, _scheduleDictionary)).Return(swapList);
				Expect.Call(() => _rollbackService.ClearModificationCollection());
				Expect.Call(_rollbackService.ModifyParts(swapList)).Return(new List<IBusinessRuleResponse> { response });
				Expect.Call(() => _rollbackService.Rollback());
			}

			using (_mocks.Playback())
			{
				var result = _target.TrySwap(_teamBlockInfo1, _teamBlockInfo2, _rollbackService, _scheduleDictionary);
				Assert.IsFalse(result);
			}
		}

	}
}