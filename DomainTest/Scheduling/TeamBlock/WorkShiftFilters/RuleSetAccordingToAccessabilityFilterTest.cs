using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
    [TestFixture]
    public class RuleSetAccordingToAccessabilityFilterTest
    {
        private IRuleSetAccordingToAccessabilityFilter _target;
        private IWorkShiftRuleSet _workShiftRuleSet1;
        private MockRepository _mock;
        private IRuleSetBag _ruleSetBag;
        private ITeamBlockWorkShiftRuleFilter _teamBlockWorkShiftRuleFilter;
        private ITeamBlockRuleSetBagExtractor _teamBlockRuleSetBagExtractor;
        private ITeamBlockInfo _teamBlockInfo;
        private IBlockInfo _blockInfo;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _teamBlockWorkShiftRuleFilter = _mock.StrictMock<ITeamBlockWorkShiftRuleFilter>();
            _teamBlockRuleSetBagExtractor = _mock.StrictMock<ITeamBlockRuleSetBagExtractor>();
            _teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
            _blockInfo = _mock.StrictMock<IBlockInfo>();
            _target = new RuleSetAccordingToAccessabilityFilter(_teamBlockRuleSetBagExtractor, _teamBlockWorkShiftRuleFilter);
            _workShiftRuleSet1 = _mock.StrictMock<IWorkShiftRuleSet>();
            _ruleSetBag = _mock.StrictMock<IRuleSetBag>();

        }

        [Test]
        public void ShouldExecuteTeamBlock()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 05, 2014, 03, 09);

            using (_mock.Record())
            {
                Expect.Call(_teamBlockRuleSetBagExtractor.GetRuleSetBag(_teamBlockInfo)).IgnoreArguments()
                      .Return(new List<IRuleSetBag>() { _ruleSetBag });
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
                Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
                Expect.Call(_teamBlockWorkShiftRuleFilter.Filter(dateOnlyPeriod, new List<IRuleSetBag>() { _ruleSetBag })).IgnoreArguments()
                      .Return(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1 });
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(_teamBlockInfo);
                Assert.AreEqual(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1 }, result);
            }

        }

        [Test]
        public void ShouldExecuteTeamBlockOnSingleDay()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 05, 2014, 03, 09);

            using (_mock.Record())
            {
                Expect.Call(_teamBlockRuleSetBagExtractor.GetRuleSetBag(_teamBlockInfo, new DateOnly(2014, 03, 10))).IgnoreArguments()
                      .Return(new List<IRuleSetBag>() { _ruleSetBag });
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
                Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
                Expect.Call(_teamBlockWorkShiftRuleFilter.Filter(dateOnlyPeriod, new List<IRuleSetBag>() { _ruleSetBag })).IgnoreArguments()
                      .Return(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1 });
            }
            using (_mock.Playback())
            {
                var result = _target.Filter(_teamBlockInfo,new DateOnly(2014,03,10));
                Assert.AreEqual(new List<IWorkShiftRuleSet>() { _workShiftRuleSet1 }, result);
            }

        }

    }


}
