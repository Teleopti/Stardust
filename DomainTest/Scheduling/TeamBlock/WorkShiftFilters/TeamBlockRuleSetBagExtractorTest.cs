using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.WorkShiftFilters
{
    [TestFixture]
    public class TeamBlockRuleSetBagExtractorTest
    {
        private ITeamBlockRuleSetBagExtractor _target;
        private ITeamBlockInfo _teamBlockInfo;
        private MockRepository _mock;
        private ITeamInfo _teamInfo;
        private IPerson _person1;
        private IPerson _person2;
        private IBlockInfo _blockInfo;
        private IPersonPeriod _personPeriod1;
        private IPersonPeriod _personPeriod2;
        private IPersonPeriod _personPeriod3;
        private IPersonPeriod _personPeriod4;
        private IRuleSetBag _ruleSetBag1;
        private IRuleSetBag _ruleSetBag2;
        private IRuleSetBag _ruleSetBag3;
        private IRuleSetBag _ruleSetBag4;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
            _teamInfo = _mock.StrictMock<ITeamInfo>();
            _person1 = _mock.StrictMock<IPerson>();
            _person2 = _mock.StrictMock<IPerson>();
            _blockInfo = _mock.StrictMock<IBlockInfo>();
            _personPeriod1 = _mock.StrictMock<IPersonPeriod>();
            _personPeriod2 = _mock.StrictMock<IPersonPeriod>();
            _personPeriod3 = _mock.StrictMock<IPersonPeriod>();
            _personPeriod4 = _mock.StrictMock<IPersonPeriod>();
            _ruleSetBag1 = _mock.StrictMock<IRuleSetBag>();
            _ruleSetBag2 = _mock.StrictMock<IRuleSetBag>();
            _ruleSetBag3 = _mock.StrictMock<IRuleSetBag>();
            _ruleSetBag4 = _mock.StrictMock<IRuleSetBag>();
            _target = new TeamBlockRuleSetBagExtractor();
        }

        [Test]
        public void ShouldReturnEmptyListIfTeamBlockIsEmpty()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>());
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(0, _target.GetRuleSetBag(_teamBlockInfo).Count());
            }

        }

        [Test]
        public void ShouldReturnRuleSetBagForSigleAgentSingleDay()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() { _person1 });
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
                Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
                Expect.Call(_person1.Period(new DateOnly(2014, 03, 10))).Return(_personPeriod1);
                Expect.Call(_personPeriod1.RuleSetBag).Return(_ruleSetBag1);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(1, _target.GetRuleSetBag(_teamBlockInfo).Count());
            }

        }

        [Test]
        public void ShouldReturnRuleSetBagForSigleAgentMultipleDays()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() { _person1 });
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo);
                Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod);
                Expect.Call(_person1.Period(new DateOnly(2014, 03, 10))).Return(_personPeriod1);
                Expect.Call(_personPeriod1.RuleSetBag).Return(_ruleSetBag1);
                Expect.Call(_person1.Period(new DateOnly(2014, 03, 11))).Return(_personPeriod2);
                Expect.Call(_personPeriod2.RuleSetBag).Return(_ruleSetBag2);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.GetRuleSetBag(_teamBlockInfo).Count());
            }
        }

        [Test]
        public void ShouldReturnRuleSetBagForTeamSingleDay()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 10);
            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() { _person1, _person2 });
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo).Repeat.Twice();
                Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod).Repeat.Twice();
                Expect.Call(_person1.Period(new DateOnly(2014, 03, 10))).Return(_personPeriod1);
                Expect.Call(_personPeriod1.RuleSetBag).Return(_ruleSetBag1);
                Expect.Call(_person2.Period(new DateOnly(2014, 03, 10))).Return(_personPeriod2);
                Expect.Call(_personPeriod2.RuleSetBag).Return(_ruleSetBag2);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.GetRuleSetBag(_teamBlockInfo).Count());
            }
        }

        [Test]
        public void ShouldReturnRuleSetBagForTeamMultipleDays()
        {
            var dateOnlyPeriod = new DateOnlyPeriod(2014, 03, 10, 2014, 03, 11);
            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() { _person1, _person2 });
                Expect.Call(_teamBlockInfo.BlockInfo).Return(_blockInfo).Repeat.Twice();
                Expect.Call(_blockInfo.BlockPeriod).Return(dateOnlyPeriod).Repeat.Twice();
                Expect.Call(_person1.Period(new DateOnly(2014, 03, 10))).Return(_personPeriod1);
                Expect.Call(_personPeriod1.RuleSetBag).Return(_ruleSetBag1);
                Expect.Call(_person2.Period(new DateOnly(2014, 03, 10))).Return(_personPeriod2);
                Expect.Call(_personPeriod2.RuleSetBag).Return(_ruleSetBag2);
                Expect.Call(_person1.Period(new DateOnly(2014, 03, 11))).Return(_personPeriod3);
                Expect.Call(_personPeriod3.RuleSetBag).Return(_ruleSetBag3);
                Expect.Call(_person2.Period(new DateOnly(2014, 03, 11))).Return(_personPeriod4);
                Expect.Call(_personPeriod4.RuleSetBag).Return(_ruleSetBag4);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(4, _target.GetRuleSetBag(_teamBlockInfo).Count());
            }
        }

        [Test]
        public void ShouldReturnRuleSetBagForTeamSingleDateOnly()
        {
            using (_mock.Record())
            {
                Expect.Call(_teamBlockInfo.TeamInfo).Return(_teamInfo);
                Expect.Call(_teamInfo.GroupMembers).Return(new List<IPerson>() { _person1, _person2 });
                Expect.Call(_person1.Period(new DateOnly(2014, 03, 10))).Return(_personPeriod1);
                Expect.Call(_personPeriod1.RuleSetBag).Return(_ruleSetBag1);
                Expect.Call(_person2.Period(new DateOnly(2014, 03, 10))).Return(_personPeriod2);
                Expect.Call(_personPeriod2.RuleSetBag).Return(_ruleSetBag2);
            }
            using (_mock.Playback())
            {
                Assert.AreEqual(2, _target.GetRuleSetBag(_teamBlockInfo,new DateOnly(2014,03,10)).Count());
            }
        }

    }


}
