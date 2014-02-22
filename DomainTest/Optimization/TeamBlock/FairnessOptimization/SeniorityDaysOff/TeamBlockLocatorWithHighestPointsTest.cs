using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    [TestFixture]
    public class TeamBlockLocatorWithHighestPointsTest
    {
        private MockRepository _mock;
        private ITeamBlockLocatorWithHighestPoints _target;
        private ITeamBlockInfo _teamBlockInfo1;
        private ITeamBlockInfo _teamBlockInfo2;
        private ITeamBlockInfo _teamBlockInfo3;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _target = new TeamBlockLocatorWithHighestPoints();
            _teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
            _teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
            _teamBlockInfo3 = _mock.StrictMock<ITeamBlockInfo>();
        }

        [Test]
        public void ShouldReturnNullForEmptyList()
        {
             var result = _target.FindBestTeamBlockToSwapWith(new List<ITeamBlockInfo>( ),new Dictionary<ITeamBlockInfo, double>( ));
             Assert.IsNull( result);
        }

        [Test]
        public void ShouldReturnTeamBlockToSwapWith()
        {
            IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo>{_teamBlockInfo1,_teamBlockInfo2,_teamBlockInfo3};
            IDictionary<ITeamBlockInfo, double> teamBlockSeverityList=new Dictionary<ITeamBlockInfo, double>();
            teamBlockSeverityList.Add(_teamBlockInfo1,5);
            teamBlockSeverityList.Add(_teamBlockInfo2,15);
            teamBlockSeverityList.Add(_teamBlockInfo3,10);
            var result = _target.FindBestTeamBlockToSwapWith(teamBlockList, teamBlockSeverityList);
            Assert.AreEqual(result, _teamBlockInfo2);

        }
    }
}
