using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.Specification;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

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
		private ITeamInfoFactory _teamInfoFactory;
		private IList<IScheduleMatrixPro> _allMatrixList;
		private IPerson _person;
	    private ITeamMemberTerminationOnBlockSpecification _teamMemberTerminationOnBlockSpecification;

	    [SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_dynamicBlockFinder = _mocks.StrictMock<IDynamicBlockFinder>();
			_teamInfoFactory = _mocks.StrictMock<ITeamInfoFactory>();
	        _teamMemberTerminationOnBlockSpecification = _mocks.StrictMock<ITeamMemberTerminationOnBlockSpecification>();
            _target = new TeamBlockInfoFactory(_dynamicBlockFinder, _teamInfoFactory, _teamMemberTerminationOnBlockSpecification);
			_teamInfo = _mocks.StrictMock<ITeamInfo>();
			_blockInfo = _mocks.StrictMock<IBlockInfo>();
			_allMatrixList = new List<IScheduleMatrixPro>();
			_person = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill>());
		}

		[Test]
		public void ShouldReturnNullIfTeamInfoIsNull()
		{
			ITeamBlockInfo result = _target.CreateTeamBlockInfo(null, new DateOnly(2013, 2, 27), BlockFinderType.None, false, _allMatrixList);
			Assert.IsNull(result);
		}

		[Test]
		public void ShouldReturnNullIfNoBlock()
		{
			using (_mocks.Record())
			{
                Expect.Call(_dynamicBlockFinder.ExtractBlockInfo(new DateOnly(2013, 2, 27), _teamInfo, BlockFinderType.None, false))
				      .Return(null);
			}

			using (_mocks.Playback())
			{
				ITeamBlockInfo result = _target.CreateTeamBlockInfo(_teamInfo, new DateOnly(2013, 2, 27), BlockFinderType.None, false, _allMatrixList);
				Assert.IsNull(result);
			}
		}

		[Test]
		public void ShouldReturnNewTeamBlockInfoIfBlockFound()
		{
			using (_mocks.Record())
			{
                Expect.Call(_dynamicBlockFinder.ExtractBlockInfo(new DateOnly(2013, 2, 27), _teamInfo, BlockFinderType.SchedulePeriod, false))
					  .Return(_blockInfo);
                Expect.Call(_teamMemberTerminationOnBlockSpecification.IsSatisfy(_teamInfo, _blockInfo)).Return(true);
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson> { _person });
				Expect.Call(_blockInfo.BlockPeriod).Return(new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue));
				Expect.Call(_teamInfoFactory.CreateTeamInfo(_person, new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MaxValue),
				                                            _allMatrixList)).Return(_teamInfo);
			}

			using (_mocks.Playback())
			{
				ITeamBlockInfo result = _target.CreateTeamBlockInfo(_teamInfo, new DateOnly(2013, 2, 27), BlockFinderType.SchedulePeriod, false, _allMatrixList);
				Assert.AreSame(_teamInfo, result.TeamInfo);
				Assert.AreSame(_blockInfo, result.BlockInfo);
			}
		}

        [Test]
        public void ReturnNullIfAnyMemberOfTeamIsTerminated()
        {
            using (_mocks.Record())
            {
                Expect.Call(_dynamicBlockFinder.ExtractBlockInfo(new DateOnly(2013, 2, 27), _teamInfo, BlockFinderType.SchedulePeriod, false))
                      .Return(_blockInfo);
                Expect.Call(_teamMemberTerminationOnBlockSpecification.IsSatisfy(_teamInfo, _blockInfo)).Return(false);
            }

            using (_mocks.Playback())
            {
                ITeamBlockInfo result = _target.CreateTeamBlockInfo(_teamInfo, new DateOnly(2013, 2, 27), BlockFinderType.SchedulePeriod, false, _allMatrixList);
                Assert.IsNull( result );
            }
        }

	}
}