using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class SeniorTeamBlockLocatorTest
    {
        private MockRepository _mocks;
        private ISeniorTeamBlockLocator _target;
        private ITeamBlockPoints _teamBlockPoint1;
        private ITeamBlockPoints _teamBlockPoint2;
        private ITeamBlockPoints _teamBlockPoint3;
        private ITeamBlockInfo _teamBlockInfo1;
        private ITeamBlockInfo _teamBlockInfo2;
        private ITeamBlockInfo _teamBlockInfo3;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new SeniorTeamBlockLocator();
            _teamBlockInfo1 = null;
            _teamBlockInfo2 = null;
            _teamBlockInfo3 = null;
            _teamBlockPoint1 = new TeamBlockPoints(_teamBlockInfo1,5);
            _teamBlockPoint2 = new TeamBlockPoints(_teamBlockInfo2,15);
            _teamBlockPoint3 = new TeamBlockPoints(_teamBlockInfo3,10);
            
        }

        [Test]
        public void ShouldReturnNullIfListIsEmpty()
        {
            var teamBlockPointList = new List<ITeamBlockPoints>();
            using (_mocks.Record())
            {
            }
            using (_mocks.Playback())
            {
                var result = _target.FindMostSeniorTeamBlock(teamBlockPointList);
                Assert.IsNull( result);
            }
        }

        [Test]
        public void ShouldReturnMostSeniorTeamBlock()
        {
            var teamBlockPointList = new List<ITeamBlockPoints>{_teamBlockPoint1 ,_teamBlockPoint2,_teamBlockPoint3};
            using (_mocks.Record())
            {
            }
            using (_mocks.Playback())
            {
                var result = _target.FindMostSeniorTeamBlock(teamBlockPointList);
                Assert.AreEqual(result,_teamBlockInfo2);
            }
        }
    }
}
