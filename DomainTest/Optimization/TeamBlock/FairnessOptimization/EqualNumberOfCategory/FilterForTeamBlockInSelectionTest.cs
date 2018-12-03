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
	public class FilterForTeamBlockInSelectionTest
	{
		private MockRepository _mocks;
		private IFilterForTeamBlockInSelection _target;
		private ITeamBlockInfo _teamBlockInfo;
		private BlockInfo _blockInfo;
		private IPerson _person1;
		private IPerson _person2;
		private ITeamInfo _teamInfo;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new FilterForTeamBlockInSelection();
			_teamBlockInfo = _mocks.StrictMock<ITeamBlockInfo>();
			_blockInfo = new BlockInfo(new DateOnlyPeriod(2013, 12, 1, 2013, 12, 31));
			_person1 = PersonFactory.CreatePerson();
			_person2 = PersonFactory.CreatePerson();
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
		}

		[Test]
		public void ShouldIncludeTeamBlocksInSelection()
		{
			var selectedPeriod = new DateOnlyPeriod(2013, 12, 1, 2013, 12, 31);
			var selectedPersons = new List<IPerson> {_person1, _person2};
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> {_teamBlockInfo}, selectedPersons, selectedPeriod);
				Assert.That(result[0].Equals(_teamBlockInfo));
			}
		}

		[Test]
		public void ShouldNotIncludeTeamBlocksWithMemberNotInSelection()
		{
			var selectedPeriod = new DateOnlyPeriod(2013, 12, 1, 2013, 12, 31);
			var selectedPersons = new List<IPerson> { _person1 };
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person1, _person2 });
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { _teamBlockInfo }, selectedPersons, selectedPeriod);
				Assert.That(result.Count == 0);
			}
		}

		[Test]
		public void ShouldNotIncludeTeamBlocksWithBlockOutsideSelection()
		{
			var selectedPeriod = new DateOnlyPeriod(2013, 12, 1, 2013, 12, 1);
			var selectedPersons = new List<IPerson> { _person1, _person2 };
			using (_mocks.Record())
			{
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
			}

			using (_mocks.Playback())
			{
				var result = _target.Filter(new List<ITeamBlockInfo> { _teamBlockInfo }, selectedPersons, selectedPeriod);
				Assert.That(result.Count == 0);
			}
		}

	}
}