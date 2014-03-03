using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class JuniorTeamBlockExtractorTest
    {
        private MockRepository _mock;
        private IJuniorTeamBlockExtractor _target;
        private ITeamBlockPoints _teamBlockPoint1;
        private ITeamBlockPoints _teamBlockPoint2;
        private ITeamBlockInfo _teamBlockInfo1;
        private ITeamBlockInfo _teamBlockInfo2;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new JuniorTeamBlockExtractor();
            _teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
            _teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
            //the higest number has the lowest rank
            _teamBlockPoint1 = new TeamBlockPoints(_teamBlockInfo1,20);
            _teamBlockPoint2 = new TeamBlockPoints(_teamBlockInfo2,10);
        }

        [Test]
        public void ShouldReturnNothingForEmptyList()
        {
            Assert.IsNull(_target.GetJuniorTeamBlockInfo(new List<ITeamBlockPoints>()));
        }

        [Test]
        public void ShouldReturnJuniorTeamBlock()
        {
            var teamBlockList = new List<ITeamBlockPoints>(){_teamBlockPoint1,_teamBlockPoint2};

            var result = _target.GetJuniorTeamBlockInfo(teamBlockList);
            Assert.AreEqual( result , _teamBlockInfo1 );
        }
    }
}
