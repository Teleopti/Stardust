using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class RuleSetBagsOfGroupOfPeopleCanHaveShortBreakTest
    {
        private RuleSetBagsOfGroupOfPeopleCanHaveShortBreak _target;
        private MockRepository _mock;
        private IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader _loader;
        private IWorkRuleSetExtractorForGroupOfPeople _workRuleSetExtractorForGroupOfPeople;
        private IWorkShiftRuleSetCanHaveShortBreak _workShiftRuleSetCanHaveShortBreak;
        private IList<IPerson> _persons;
        private DateOnlyPeriod _period;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();
            _loader = _mock.StrictMock<IRuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader>();
            _workRuleSetExtractorForGroupOfPeople = _mock.StrictMock<IWorkRuleSetExtractorForGroupOfPeople>();
            _workShiftRuleSetCanHaveShortBreak = _mock.StrictMock<IWorkShiftRuleSetCanHaveShortBreak>();
            _persons = new List<IPerson> { PersonFactory.CreatePerson("A"), PersonFactory.CreatePerson("B") };
            _period = new DateOnlyPeriod();
            _target = new RuleSetBagsOfGroupOfPeopleCanHaveShortBreak(_loader);

        }

        [Test]
        public void AllComponentsShouldBeLoadedAndOnlyOnce()
        {
            using (_mock.Record())
            {
                Expect.Call(_loader.CreateWorkRuleSetExtractor(_persons))
                    .Return(_workRuleSetExtractorForGroupOfPeople);
                Expect.Call(_loader.CreateWorkShiftRuleSetCanHaveShortBreak(_persons))
                    .Return(_workShiftRuleSetCanHaveShortBreak);
                Expect.Call(_workRuleSetExtractorForGroupOfPeople.ExtractRuleSets(_period))
                    .Return(new List<IWorkShiftRuleSet>());
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_persons, _period));
            }
        }

        [Test]
        public void AllWorkShiftRuleSetShouldBeCheckedForFalseResult()
        {
            IWorkShiftRuleSet workShiftRuleSet1 = _mock.StrictMock<IWorkShiftRuleSet>();
            IWorkShiftRuleSet workShiftRuleSet2 = _mock.StrictMock<IWorkShiftRuleSet>();
            IList<IWorkShiftRuleSet> workShiftRuleSets = new List<IWorkShiftRuleSet> { workShiftRuleSet1, workShiftRuleSet2 };

            using (_mock.Record())
            {
                Expect.Call(_loader.CreateWorkRuleSetExtractor(_persons))
                    .Return(_workRuleSetExtractorForGroupOfPeople);
                Expect.Call(_loader.CreateWorkShiftRuleSetCanHaveShortBreak(_persons))
                    .Return(_workShiftRuleSetCanHaveShortBreak);
                Expect.Call(_workRuleSetExtractorForGroupOfPeople.ExtractRuleSets(_period))
                    .Return(workShiftRuleSets);

                Expect.Call(_workShiftRuleSetCanHaveShortBreak.CanHaveShortBreak(workShiftRuleSet1))
                    .Return(false);
                Expect.Call(_workShiftRuleSetCanHaveShortBreak.CanHaveShortBreak(workShiftRuleSet2))
                    .Return(false);
            }
            using (_mock.Playback())
            {
                Assert.IsFalse(_target.CanHaveShortBreak(_persons, _period));
            }
        }

        [Test]
        public void ShouldJumpOutAfterTheFirstWorkShiftRuleSetResultIsTrue()
        {
            IWorkShiftRuleSet workShiftRuleSet1 = _mock.StrictMock<IWorkShiftRuleSet>();
            IWorkShiftRuleSet workShiftRuleSet2 = _mock.StrictMock<IWorkShiftRuleSet>();
            IList<IWorkShiftRuleSet> workShiftRuleSets = new List<IWorkShiftRuleSet> { workShiftRuleSet1, workShiftRuleSet2 };

            using (_mock.Record())
            {
                Expect.Call(_loader.CreateWorkRuleSetExtractor(_persons))
                    .Return(_workRuleSetExtractorForGroupOfPeople);
                Expect.Call(_loader.CreateWorkShiftRuleSetCanHaveShortBreak(_persons))
                    .Return(_workShiftRuleSetCanHaveShortBreak);
                Expect.Call(_workRuleSetExtractorForGroupOfPeople.ExtractRuleSets(_period))
                    .Return(workShiftRuleSets);

                Expect.Call(_workShiftRuleSetCanHaveShortBreak.CanHaveShortBreak(workShiftRuleSet1))
                    .Return(true);
                Expect.Call(workShiftRuleSet1.Description).Return(new Description("hej"));
            }
            using (_mock.Playback())
            {
                Assert.IsTrue(_target.CanHaveShortBreak(_persons, _period));
            }
        }
    }
}
