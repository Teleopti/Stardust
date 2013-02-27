using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
	[TestFixture]
	public class TeamBlockInfoFactoryTest
	{
		private MockRepository _mocks;
		private ITeamBlockInfoFactory _target;
		private IDynamicBlockFinder _dynamicBlockFinder;
		private ITeamInfo _teamInfo;
		private IBlockInfo _blockInfo;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dynamicBlockFinder = _mocks.StrictMock<IDynamicBlockFinder>();
			_target = new TeamBlockInfoFactory(_dynamicBlockFinder);
			_teamInfo = _mocks.Stub<ITeamInfo>();
			_blockInfo = _mocks.Stub<IBlockInfo>();
		}

		[Test]
		public void ShouldReturnNullIfNoBlock()
		{
			using (_mocks.Record())
			{
				Expect.Call(_dynamicBlockFinder.ExtractBlockInfo(new DateOnly(2013, 2, 27), _teamInfo, BlockFinderType.None))
				      .Return(null);
			}

			using (_mocks.Playback())
			{
				ITeamBlockInfo result = _target.CreateTeamBlockInfo(_teamInfo, new DateOnly(2013, 2, 27), BlockFinderType.None);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldReturnNewTeamBlockInfoIfBlockFound()
		{
			using (_mocks.Record())
			{
				Expect.Call(_dynamicBlockFinder.ExtractBlockInfo(new DateOnly(2013, 2, 27), _teamInfo, BlockFinderType.SchedulePeriod))
					  .Return(_blockInfo);
			}

			using (_mocks.Playback())
			{
				ITeamBlockInfo result = _target.CreateTeamBlockInfo(_teamInfo, new DateOnly(2013, 2, 27), BlockFinderType.SchedulePeriod);
				Assert.AreSame(_teamInfo, result.TeamInfo);
				Assert.AreSame(_blockInfo, result.BlockInfo);
			}
		}

	}
}