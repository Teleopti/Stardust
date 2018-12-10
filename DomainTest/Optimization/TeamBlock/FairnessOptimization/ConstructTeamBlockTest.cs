using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;


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
		private GroupPageLight _groupOnGroupPageForTeamBlockPer;
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
			_target = new ConstructTeamBlock(() => MockRepository.GenerateMock<ISchedulingResultStateHolder>(), _teamInfoFactory, _teamBlockInfoFactory);
			_matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
			_matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
			_allPersonMatrixList = new List<IScheduleMatrixPro>{_matrix1,_matrix2};
			_groupOnGroupPageForTeamBlockPer = new GroupPageLight(string.Empty, GroupPageType.Hierarchy);
			_blockInfo = _mock.StrictMock<IBlockInfo>();
		}

		[Test]
		public void ShouldNotConstructIfTeamBlockIsNull()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(null, _person, _dateOnlyPeriod, _allPersonMatrixList)).Return(null);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_allPersonMatrixList, _dateOnlyPeriod, _persons, new BetweenDayOffBlockFinder(), _groupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(0, result.Count);
			}
		}

		[Test]
		public void ShouldNotConstructIfTeamBlockIsSelectedPerson()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(null, _person, _dateOnlyPeriod, _allPersonMatrixList)).Return(_teamInfo );
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() {_person2});
				Expect.Call(()=>_teamInfo.LockMember(_dateOnlyPeriod, _person2));
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnlyPeriod.StartDate, new BetweenDayOffBlockFinder())).IgnoreArguments().Return(null);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_allPersonMatrixList, _dateOnlyPeriod, _persons, new BetweenDayOffBlockFinder(), _groupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(0, result.Count);
			}
		}

		[Test]
		public void ShouldConstructTeamBlock()
		{
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(null, _person, _dateOnlyPeriod, _allPersonMatrixList)).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() { _person2 });
				Expect.Call(() => _teamInfo.LockMember(_dateOnlyPeriod, _person2));
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnlyPeriod.StartDate, new BetweenDayOffBlockFinder())).IgnoreArguments().Return(_teamBlockInfo);
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(_dateOnlyPeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_allPersonMatrixList, _dateOnlyPeriod, _persons, new BetweenDayOffBlockFinder(), _groupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(1, result.Count);
			}
		}

		[Test]
		public void ShouldConstructTeamBlockWithSingleAgentTeam()
		{
			_groupOnGroupPageForTeamBlockPer = GroupPageLight.SingleAgentGroup(string.Empty);
			using (_mock.Record())
			{
				Expect.Call(_teamInfoFactory.CreateTeamInfo(null, _person, _dateOnlyPeriod, _allPersonMatrixList)).Return(_teamInfo);
				Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() { _person2 });
				Expect.Call(() => _teamInfo.LockMember(_dateOnlyPeriod, _person2));
				Expect.Call(_teamBlockInfoFactory.CreateTeamBlockInfo(_teamInfo, _dateOnlyPeriod.StartDate, new BetweenDayOffBlockFinder())).IgnoreArguments().Return(_teamBlockInfo);
				Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
				Expect.Call(_blockInfo.BlockPeriod).Return(_dateOnlyPeriod);
			}

			using (_mock.Playback())
			{
				var result = _target.Construct(_allPersonMatrixList, _dateOnlyPeriod, _persons, new BetweenDayOffBlockFinder(), _groupOnGroupPageForTeamBlockPer);
				Assert.AreEqual(1, result.Count);
			}
		}
	}
}
