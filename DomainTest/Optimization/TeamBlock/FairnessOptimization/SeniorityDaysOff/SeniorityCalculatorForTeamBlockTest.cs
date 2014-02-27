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
    public class SeniorityCalculatorForTeamBlockTest
    {
        private MockRepository _mock;
        private ISeniorityCalculatorForTeamBlock _target;
        private IWeekDayPoints  _weekDayPoints;
        private ITeamBlockInfo _teamBlockInfo1;
        private ITeamBlockInfo _teamBlockInfo2;
        private IWeekDayPointCalculatorForTeamBlock _weekDayPointCalculatorForTeamBlock;

        [SetUp ]
        public void Setup()
        {
            _mock = new MockRepository();
            _weekDayPoints = new WeekDayPoints();
            _teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
            _teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
            _weekDayPointCalculatorForTeamBlock = _mock.StrictMock<IWeekDayPointCalculatorForTeamBlock>();
            _target = new SeniorityCalculatorForTeamBlock(_weekDayPointCalculatorForTeamBlock);
        }

        [Test]
        public void ShouldReturnEmptyDictionary()
        {
            IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo>();
            using (_mock.Record())
            {
                
            }
            using (_mock.Playback())
            {
                var result = _target.CreateWeekDayValueDictionary(teamBlockList, _weekDayPoints.GetWeekDaysPoints());
                Assert.AreEqual(result.Count(),0);
            }
        }

        [Test]
        public void ShouldReturnSeniority()
        {
            IList<ITeamBlockInfo> teamBlockList = new List<ITeamBlockInfo>(){_teamBlockInfo1,_teamBlockInfo2};
            using (_mock.Record())
            {
                Expect.Call(_weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(_teamBlockInfo1,
                                                                                               _weekDayPoints
                                                                                                   .GetWeekDaysPoints()))
                      .Return(5);
                Expect.Call(_weekDayPointCalculatorForTeamBlock.CalculateDaysOffSeniorityValue(_teamBlockInfo2,
                                                                                               _weekDayPoints
                                                                                                   .GetWeekDaysPoints()))
                      .Return(15);
            }
            using (_mock.Playback())
            {
                var result = _target.CreateWeekDayValueDictionary(teamBlockList, _weekDayPoints.GetWeekDaysPoints());
                Assert.AreEqual(result[_teamBlockInfo1 ], 5);
                Assert.AreEqual(result[_teamBlockInfo2 ], 15);
            }
        }
    }
}
