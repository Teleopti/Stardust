using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class TeamBlockPriorityDefinitionInfoTest
	{
		private MockRepository _mock;
		private ITeamBlockInfoPriority _teamBlockInfoPriority1;
		private ITeamBlockInfoPriority _teamBlockInfoPriority2;
		private TeamBlockPriorityDefinitionInfo _target;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamBlockInfoPriority1 = _mock.StrictMock<ITeamBlockInfoPriority>();
			_teamBlockInfoPriority2 = _mock.StrictMock<ITeamBlockInfoPriority>();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_target = new TeamBlockPriorityDefinitionInfo();
			_target.AddItem(_teamBlockInfoPriority1, _teamBlockInfo1, 1);
			_target.AddItem(_teamBlockInfoPriority2, _teamBlockInfo2, 2);
		}

		[Test]
		public void ShouldGetListSortedFromHightToLowSeniority()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfoPriority1.TeamBlockInfo).Return(_teamBlockInfo1);
				Expect.Call(_teamBlockInfoPriority2.TeamBlockInfo).Return(_teamBlockInfo2);
				Expect.Call(_teamBlockInfoPriority1.Seniority).Return(1.3d).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockInfoPriority2.Seniority).Return(1.7d).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.HighToLowSeniorityListBlockInfo;
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(_teamBlockInfo1, result[0]);
				Assert.AreEqual(_teamBlockInfo2, result[1]);
			}
		}

		[Test]
		public void ShouldGetShiftCategoryPriorityForBlock()
		{
			var result = _target.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2);
			Assert.AreEqual(2, result);	
		}

		[Test]
		public void ShouldGetPrioritySortedFromHighToLow()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfoPriority1.ShiftCategoryPriority).Return(1);
				Expect.Call(_teamBlockInfoPriority2.ShiftCategoryPriority).Return(2);
				Expect.Call(_teamBlockInfoPriority1.TeamBlockInfo).Return(_teamBlockInfo1);
				Expect.Call(_teamBlockInfoPriority2.TeamBlockInfo).Return(_teamBlockInfo2);
			}

			using (_mock.Playback())
			{
				var result = _target.HighToLowShiftCategoryPriority();
				Assert.AreEqual(_teamBlockInfo2, result[0]);
				Assert.AreEqual(_teamBlockInfo1, result[1]);
			}
		}
	}
}
