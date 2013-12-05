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
			_target.AddItem(_teamBlockInfoPriority1);
			_target.AddItem(_teamBlockInfoPriority2);
		}

		[Test]
		public void ShouldGetListSortedFromHightToLowSeniority()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfoPriority1.Seniority).Return(1.3d).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockInfoPriority2.Seniority).Return(1.7d).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.HighToLowSeniorityList.ToList();
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(_teamBlockInfoPriority1.Seniority, result[1]);
				Assert.AreEqual(_teamBlockInfoPriority2.Seniority, result[0]);
			}
		}

		[Test]
		public void ShouldGetListSortedFromLowToHighSeniority()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfoPriority1.Seniority).Return(1.3d).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockInfoPriority2.Seniority).Return(1.7d).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.LowToHighSeniorityList.ToList();
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(_teamBlockInfoPriority1.Seniority, result[0]);
				Assert.AreEqual(_teamBlockInfoPriority2.Seniority, result[1]);
			}	
		}

		[Test]
		public void ShouldGetListSortedFromHighToLowShiftCategoryPriority()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfoPriority1.ShiftCategoryPriority).Return(2).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockInfoPriority2.ShiftCategoryPriority).Return(3).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.HighToLowShiftCategoryPriorityList.ToList();
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(_teamBlockInfoPriority1.ShiftCategoryPriority, result[1]);
				Assert.AreEqual(_teamBlockInfoPriority2.ShiftCategoryPriority, result[0]);
			}
		}

		[Test]
		public void ShouldGetListSortedFromLowToHighShiftCategoryPriority()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfoPriority1.ShiftCategoryPriority).Return(2).Repeat.AtLeastOnce();
				Expect.Call(_teamBlockInfoPriority2.ShiftCategoryPriority).Return(3).Repeat.AtLeastOnce();
			}

			using (_mock.Playback())
			{
				var result = _target.LowToHighShiftCategoryPriorityList.ToList();
				Assert.AreEqual(2, result.Count());
				Assert.AreEqual(_teamBlockInfoPriority1.ShiftCategoryPriority, result[0]);
				Assert.AreEqual(_teamBlockInfoPriority2.ShiftCategoryPriority, result[1]);
			}
		}

		[Test]
		public void ShouldGetBlockWithSeniority()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfoPriority1.Seniority).Return(2d);
				Expect.Call(_teamBlockInfoPriority2.Seniority).Return(3.14d);
				Expect.Call(_teamBlockInfoPriority2.TeamBlockInfo).Return(_teamBlockInfo1);
			}

			using (_mock.Playback())
			{
				var result = _target.BlockOnSeniority(3.14d);
				Assert.AreEqual(_teamBlockInfo1, result);
			}
		}

		[Test]
		public void ShouldGetShiftCategoryPriorityForBlock()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfoPriority1.TeamBlockInfo).Return(_teamBlockInfo1);
				Expect.Call(_teamBlockInfoPriority2.TeamBlockInfo).Return(_teamBlockInfo2);
				Expect.Call(_teamBlockInfoPriority2.ShiftCategoryPriority).Return(3);
			}

			using (_mock.Playback())
			{
				var result = _target.GetShiftCategoryPriorityOfBlock(_teamBlockInfo2);
				Assert.AreEqual(3, result);
			}
		}
	}
}
