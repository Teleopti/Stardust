using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamBlockInfoFactoryTest
	{
		private MockRepository _mocks;
		private ITeamBlockInfoFactory _target;
		private IDynamicBlockFinder _dynamicBlockFinder;
		private ITeamInfo _teamInfo;
		private IBlockInfo _blockInfo;
	    private ITeamMemberTerminationOnBlockSpecification _teamMemberTerminationOnBlockSpecification;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dynamicBlockFinder = _mocks.StrictMock<IDynamicBlockFinder>();
	        _teamMemberTerminationOnBlockSpecification = _mocks.StrictMock<ITeamMemberTerminationOnBlockSpecification>();
            _target = new TeamBlockInfoFactory(_dynamicBlockFinder, _teamMemberTerminationOnBlockSpecification);
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_blockInfo = _mocks.StrictMock<IBlockInfo>();
		}

		[Test]
		public void ShouldReturnNullIfTeamInfoIsNull()
		{
			ITeamBlockInfo result = _target.CreateTeamBlockInfo(null, new DateOnly(2013, 2, 27), new SingleDayBlockFinder());
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldReturnNullIfNoBlock()
		{
			using (_mocks.Record())
			{
                Expect.Call(_dynamicBlockFinder.ExtractBlockInfo(new DateOnly(2013, 2, 27), _teamInfo, new SingleDayBlockFinder())).IgnoreArguments()
							.Return(null);
			}

			using (_mocks.Playback())
			{
				ITeamBlockInfo result = _target.CreateTeamBlockInfo(_teamInfo, new DateOnly(2013, 2, 27), new SingleDayBlockFinder());
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldReturnNewTeamBlockInfoIfBlockFound()
		{
			var dateOnly = new DateOnly(2013, 2, 27);
			using (_mocks.Record())
			{
                Expect.Call(_dynamicBlockFinder.ExtractBlockInfo(new DateOnly(2013, 2, 27), _teamInfo, new SchedulePeriodBlockFinder())).IgnoreArguments()

						.Return(_blockInfo);
                Expect.Call(()=>_teamMemberTerminationOnBlockSpecification.LockTerminatedMembers(_teamInfo, _blockInfo));
			}

			using (_mocks.Playback())
			{
				ITeamBlockInfo result = _target.CreateTeamBlockInfo(_teamInfo, dateOnly, new SchedulePeriodBlockFinder());
				Assert.AreSame(_teamInfo, result.TeamInfo);
				Assert.AreSame(_blockInfo, result.BlockInfo);
			}
		}
	}
}