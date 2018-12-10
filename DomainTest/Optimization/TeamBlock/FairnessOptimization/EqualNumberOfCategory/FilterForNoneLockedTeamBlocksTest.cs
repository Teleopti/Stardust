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
	public class FilterForNoneLockedTeamBlocksTest
	{
		private MockRepository _mocks;
		private IFilterForNoneLockedTeamBlocks _target;
		private ITeamBlockInfo _teamBlockInfo1;
		private IList<ITeamBlockInfo> _teamBlockInfos;
		private IBlockInfo _blockInfo;
		private ITeamInfo _teamInfo;
		private IPerson _person;
		private IList<IPerson> _groupMembers;
		private IScheduleMatrixPro _matrix;
		private IScheduleDayPro _scheduleDayPro;


		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new FilterForNoneLockedTeamBlocks();
			_teamBlockInfo1 = _mocks.StrictMock<ITeamBlockInfo>();
			_teamBlockInfos = new List<ITeamBlockInfo> { _teamBlockInfo1 };
			_blockInfo = _mocks.StrictMock<IBlockInfo>();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_person = PersonFactory.CreatePerson();
			_groupMembers = new List<IPerson>{_person};
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro = _mocks.StrictMock<IScheduleDayPro>();
		}

		[Test]
		public void ShouldOnlyReturnUnlocked()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_teamInfo.MatrixForMemberAndDate(_person, DateOnly.MinValue)).Return(_matrix);
				Expect.Call(_matrix.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
				Expect.Call(_matrix.UnlockedDays)
				      .Return(new []{_scheduleDayPro});
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(_teamBlockInfos);
				Assert.IsTrue(result.Contains(_teamBlockInfo1));
			}
		}

		[Test]
		public void ShouldNotReturnLocked()
		{
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue));
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(_groupMembers);
				Expect.Call(_teamInfo.MatrixForMemberAndDate(_person, DateOnly.MinValue)).Return(_matrix);
				Expect.Call(_matrix.GetScheduleDayByKey(DateOnly.MinValue)).Return(_scheduleDayPro);
				Expect.Call(_matrix.UnlockedDays)
				      .Return(new IScheduleDayPro[0]);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(_teamBlockInfos);
				Assert.IsFalse(result.Contains(_teamBlockInfo1));
			}
		}

	}
}