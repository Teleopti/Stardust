using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class TeamBlockInfoTest
    {
        private MockRepository _mocks;
        private ITeamInfo _teamInfo;
        private IBlockInfo _blockInfo;
        private TeamBlockInfo _target;
        private DateOnlyPeriod _blockPeriod;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            IGroupPerson groupPerson = new GroupPerson(new List<IPerson> { PersonFactory.CreatePerson("bill") }, DateOnly.MinValue, "Hej", Guid.NewGuid());
            _teamInfo = new TeamInfo(groupPerson, new List<IList<IScheduleMatrixPro>>());
            _blockPeriod = new DateOnlyPeriod(2013, 3, 15, 2013, 3, 15);
            _blockInfo = new BlockInfo(_blockPeriod);
            _target = new TeamBlockInfo(_teamInfo, _blockInfo);
        }

        [Test]
        public void ShouldGiveMatrixesFromATeamBlock()
        {
            var matrixes = new List<IScheduleMatrixPro>();

            var result = _target.MatrixesForGroupAndBlock();

            Assert.That(result, Is.EqualTo(matrixes));
        }

        [Test]
        public void ShouldBeSameWhenHashcodeOfTeamBlocksAreTheSame()
        {
            var teamBlock = new TeamBlockInfo(_teamInfo, _blockInfo);

            using (_mocks.Playback())
            {
                Assert.That(_target.Equals(teamBlock), Is.True);
            }
        }
    }
}
