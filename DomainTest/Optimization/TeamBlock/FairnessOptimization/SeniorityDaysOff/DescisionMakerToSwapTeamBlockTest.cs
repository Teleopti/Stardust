using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.EqualNumberOfCategory;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public class DescisionMakerToSwapTeamBlockTest
    {
        private MockRepository _mock;
        private IDescisionMakerToSwapTeamBlock _target;
        private ITeamBlockInfo _teamBlockInfo1;
        private ITeamBlockInfo _teamBlockInfo2;
        private ITeamBlockInfo _teamBlockInfo3;
        private IFilterOnSwapableTeamBlocks _filterOnSwapableTeamBlock;
        private ISeniorityCalculatorForTeamBlock _seniorityCalculatorForTeamBlock;
        private ITeamBlockLocatorWithHighestPoints _teamBlockLocatorWithHigestPoints;
        private ITeamBlockSwapper _teamBlockSwapper;
        private IWeekDayPoints _weekDayPoints;
        private IScheduleDictionary _scheduleDictionary;
        private ISchedulePartModifyAndRollbackService _rollbackService;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _filterOnSwapableTeamBlock = _mock.StrictMock<IFilterOnSwapableTeamBlocks>();
            _seniorityCalculatorForTeamBlock = _mock.StrictMock<ISeniorityCalculatorForTeamBlock>();
            _teamBlockLocatorWithHigestPoints = _mock.StrictMock<ITeamBlockLocatorWithHighestPoints>();
            _teamBlockSwapper = _mock.StrictMock<ITeamBlockSwapper>();
            _target = new DescisionMakerToSwapTeamBlock(_filterOnSwapableTeamBlock,_seniorityCalculatorForTeamBlock,_teamBlockLocatorWithHigestPoints,_teamBlockSwapper);
            _teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
            _teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
            _teamBlockInfo3 = _mock.StrictMock<ITeamBlockInfo>();
            _weekDayPoints = new WeekDayPoints();
            _scheduleDictionary = _mock.StrictMock<IScheduleDictionary>();
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();

        }

        [Test]
        public void ShouldReturnFalseForEmptyList()
        {
            var mostSeniorTeamBlock = _teamBlockInfo1;
            var teamBlocksToWorkWith = new List<ITeamBlockInfo>() { _teamBlockInfo2, _teamBlockInfo3 };
            using (_mock.Record())
            {
                Expect.Call(_filterOnSwapableTeamBlock.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock))
                      .IgnoreArguments()
                      .Return(teamBlocksToWorkWith);

                Expect.Call(_seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(teamBlocksToWorkWith,
                                                                                          _weekDayPoints
                                                                                              .GetWeekDaysPoints()))
                      .Return(new Dictionary<ITeamBlockInfo, double>( ));
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.TrySwapForMostSenior(teamBlocksToWorkWith, mostSeniorTeamBlock, _rollbackService, _scheduleDictionary, _weekDayPoints.GetWeekDaysPoints()));
            }

        }

        [Test]
        public void ShouldReturnFalseIfSwappbleListIsEmpty()
        {
            var mostSeniorTeamBlock = _teamBlockInfo1;
            var teamBlocksToWorkWith = new List<ITeamBlockInfo>() {_teamBlockInfo2, _teamBlockInfo3};
            IDictionary<ITeamBlockInfo, double> teamBlockSeneriotyList = new Dictionary<ITeamBlockInfo, double>();
            teamBlockSeneriotyList.Add(_teamBlockInfo3,2);
            teamBlockSeneriotyList.Add(_teamBlockInfo2,5);
            teamBlockSeneriotyList.Add(_teamBlockInfo1,3);
            using (_mock.Record())
            {
                Expect.Call(_filterOnSwapableTeamBlock.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock))
                      .IgnoreArguments()
                      .Return(new List<ITeamBlockInfo>());
                
                Expect.Call(_seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(teamBlocksToWorkWith,
                                                                                          _weekDayPoints
                                                                                              .GetWeekDaysPoints())).IgnoreArguments()
                      .Return(teamBlockSeneriotyList);
            }  
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.TrySwapForMostSenior(teamBlocksToWorkWith,mostSeniorTeamBlock ,_rollbackService,_scheduleDictionary,_weekDayPoints.GetWeekDaysPoints() ));
            }

        }

        [Test]
        public void ShouldReturnTrueWhenSwapped()
        {
            var mostSeniorTeamBlock = _teamBlockInfo1;
            var teamBlocksToWorkWith = new List<ITeamBlockInfo>() { _teamBlockInfo2, _teamBlockInfo3 };
            IDictionary<ITeamBlockInfo, double> teamBlockSeneriotyList = new Dictionary<ITeamBlockInfo, double>();
            teamBlockSeneriotyList.Add(_teamBlockInfo3, 2);
            teamBlockSeneriotyList.Add(_teamBlockInfo2, 5);
            teamBlockSeneriotyList.Add(_teamBlockInfo1, 3);
            using (_mock.Record())
            {
                Expect.Call(_filterOnSwapableTeamBlock.Filter(teamBlocksToWorkWith, mostSeniorTeamBlock))
                      .IgnoreArguments()
                      .Return(teamBlocksToWorkWith);

                Expect.Call(_seniorityCalculatorForTeamBlock.CreateWeekDayValueDictionary(teamBlocksToWorkWith,
                                                                                          _weekDayPoints
                                                                                              .GetWeekDaysPoints()))
                      .Return(teamBlockSeneriotyList);
                Expect.Call(_teamBlockLocatorWithHigestPoints.FindBestTeamBlockToSwapWith(teamBlocksToWorkWith,
                                                                                          teamBlockSeneriotyList))
                      .IgnoreArguments()
                      .Return(_teamBlockInfo2);
                Expect.Call(_teamBlockSwapper.TrySwap(mostSeniorTeamBlock, _teamBlockInfo2, _rollbackService,
                                                      _scheduleDictionary)).IgnoreArguments().Return(true);
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.TrySwapForMostSenior(teamBlocksToWorkWith, mostSeniorTeamBlock, _rollbackService, _scheduleDictionary, _weekDayPoints.GetWeekDaysPoints()));
            }

        }
    }
}
