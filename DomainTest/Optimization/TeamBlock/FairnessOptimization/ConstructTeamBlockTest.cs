using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class ConstructTeamBlockTest
	{
		private MockRepository _mock;
		private ITeamInfoFactory _teamInfoFactory;
		private ITeamBlockInfoFactory _teamBlockInfoFactory;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IPerson _person;
		private IList<IPerson> _persons;
		private ITeamBlockInfo _teamBlockInfo;
		private ITeamInfo _teamInfo;
		private ConstructTeamBlock _target;
		private IList<IScheduleMatrixPro> _allPersonMatrixList;
		private IScheduleMatrixPro _matrix1;
		private IScheduleMatrixPro _matrix2;
		private IGroupPageLight _groupOnGroupPageForTeamBlockPer;
		private IPerson _person2;
		private IBlockInfo _blockInfo;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamInfoFactory = _mock.StrictMock<ITeamInfoFactory>();
			_teamBlockInfoFactory = _mock.StrictMock<ITeamBlockInfoFactory>();
			_dateOnlyPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);
			_person = PersonFactory.CreatePerson("Person");
			_person2 = PersonFactory.CreatePerson("Person2");
			_persons = new List<IPerson>{_person};
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_target = new ConstructTeamBlock(_teamInfoFactory, _teamBlockInfoFactory);
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
			_allPersonMatrixList = new List<IScheduleMatrixPro>{_matrix1,_matrix2};
			_groupOnGroupPageForTeamBlockPer = _mock.StrictMock<IGroupPageLight>();
			_blockInfo = _mock.StrictMock<IBlockInfo>();
		}

		[Test]
		public void ShouldNotConstructIfTeamBlockIsNull()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _allPersonMatrixList)).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_allPersonMatrixList, _dateOnlyPeriod, _persons, BlockFinderType.BetweenDayOff, _groupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(0, result.Count);
			}
		}

		[Test]
		public void ShouldNotConstructIfTeamBlockIsSelectedPerson()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _allPersonMatrixList)).Return(_teamInfo );
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() {_person2});
				Expect.Call(()=>_teamInfo.LockMember(_person2));
				Expect.Call(_groupOnGroupPageForTeamBlockPer.Key).Return("something");
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnlyPeriod.StartDate, BlockFinderType.BetweenDayOff,
					false)).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_allPersonMatrixList, _dateOnlyPeriod, _persons, BlockFinderType.BetweenDayOff, _groupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(0, result.Count);
			}
		}

		[Test]
		public void ShouldConstructTeamBlock()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _allPersonMatrixList)).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() { _person2 });
				Expect.Call(() => _teamInfo.LockMember(_person2));
				Expect.Call(_groupOnGroupPageForTeamBlockPer.Key).Return("something");
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnlyPeriod.StartDate, BlockFinderType.BetweenDayOff,
					false)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(_dateOnlyPeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_allPersonMatrixList, _dateOnlyPeriod, _persons, BlockFinderType.BetweenDayOff, _groupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(1, result.Count);
			}
		}

		[Test]
		public void ShouldConstructTeamBlockWithSingleAgentTeam()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, _dateOnlyPeriod, _allPersonMatrixList)).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() { _person2 });
				Expect.Call(() => _teamInfo.LockMember(_person2));
				Expect.Call(_groupOnGroupPageForTeamBlockPer.Key).Return("SingleAgentTeam");
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnlyPeriod.StartDate, BlockFinderType.BetweenDayOff,
					true)).Return(_teamBlockInfo);
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(_dateOnlyPeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_allPersonMatrixList, _dateOnlyPeriod, _persons, BlockFinderType.BetweenDayOff, _groupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(1, result.Count);
			}
		}
	}
}
