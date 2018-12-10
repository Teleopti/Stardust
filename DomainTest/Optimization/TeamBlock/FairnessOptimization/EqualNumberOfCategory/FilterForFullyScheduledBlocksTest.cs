using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;



namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory
{
	[TestFixture]
	public class FilterForFullyScheduledBlocksTest
	{
		private MockRepository _mocks;
		private IFilterForFullyScheduledBlocks _target;
		private ITeamBlockInfo _teamBlockInfo;
		private IScheduleDictionary _scheduleDictionary;
		private IScheduleRange _range;
		private IScheduleDay _scheduleDay;
		private BlockInfo _blockInfo;
		private IPerson _person;
		private ITeamInfo _teamInfo;
		private IList<ITeamBlockInfo> _teamBlockInfos;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new FilterForFullyScheduledBlocks();
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_blockInfo = new BlockInfo(new DateOnlyPeriod());
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_scheduleDictionary = _mocks.StrictMock<IScheduleDictionary>();
			_range = _mocks.StrictMock<IScheduleRange>();
			_scheduleDay = _mocks.StrictMock<IScheduleDay>();
			_person = PersonFactory.CreatePerson();
			_teamBlockInfos = new List<ITeamBlockInfo> { _teamBlockInfo };
		}

		[Test]
		public void ShouldNotIncludeIfNotAllScheduled()
		{

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_scheduleDictionary[_person]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly())).Return(_scheduleDay);

				Expect.Call(_scheduleDay.IsScheduled()).Return(false);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(_teamBlockInfos, _scheduleDictionary);
				Assert.That(result.Count == 0);
			}
		}

		[Test]
		public void ShouldIncludeIfAllScheduled()
		{

			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_scheduleDictionary[_person]).Return(_range);
				Expect.Call(_range.ScheduledDay(new DateOnly())).Return(_scheduleDay);

				Expect.Call(_scheduleDay.IsScheduled()).Return(true);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(_teamBlockInfos, _scheduleDictionary);
				Assert.IsTrue(result.Contains(_teamBlockInfo));
			}
		}

	}
}