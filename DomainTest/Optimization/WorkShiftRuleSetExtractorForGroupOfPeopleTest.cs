using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class WorkShiftRuleSetExtractorForGroupOfPeopleTest
    {
        private WorkRuleSetExtractorForGroupOfPeople _target;
        private MockRepository _mock; 
        private IList<IPerson> _persons;

        private IPerson _person1;
        private IPerson _person2;
        private IPerson _person3;

        private IPersonPeriod _personPeriod1;
        private IPersonPeriod _personPeriod2;
        private IPersonPeriod _personPeriod3;

        private IRuleSetBag _ruleSetBag1;
        private IRuleSetBag _ruleSetBag2;

        private IWorkShiftRuleSet _workShiftRuleSet1;
        private IWorkShiftRuleSet _workShiftRuleSet2;
        private IWorkShiftRuleSet _workShiftRuleSet3;
        private IWorkShiftRuleSet _workShiftRuleSet4;

        private IList<IWorkShiftRuleSet> _workShiftRuleSetForBag1;
        private IList<IWorkShiftRuleSet> _workShiftRuleSetForBag2;

        private DateOnlyPeriod _period;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();

            _person1 = PersonFactory.CreatePerson("A");
            _person2 = PersonFactory.CreatePerson("B");
            _person3 = PersonFactory.CreatePerson("C");

            _personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly());
            _personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly());
            _personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(new DateOnly());

            _person1.AddPersonPeriod(_personPeriod1);
            _person2.AddPersonPeriod(_personPeriod2);
            _person3.AddPersonPeriod(_personPeriod3);

            _persons = new List<IPerson>{ _person1, _person2, _person3};

            _ruleSetBag1 = _mock.StrictMock<IRuleSetBag>();
            _ruleSetBag2 = _mock.StrictMock<IRuleSetBag>();

            _workShiftRuleSet1 = _mock.StrictMock<IWorkShiftRuleSet>();
            _workShiftRuleSet2 = _mock.StrictMock<IWorkShiftRuleSet>();
            _workShiftRuleSet3 = _mock.StrictMock<IWorkShiftRuleSet>();
            _workShiftRuleSet4 = _mock.StrictMock<IWorkShiftRuleSet>();

            _workShiftRuleSetForBag1 = new List<IWorkShiftRuleSet>{ _workShiftRuleSet1, _workShiftRuleSet2, _workShiftRuleSet3 };
            _workShiftRuleSetForBag2 = new List<IWorkShiftRuleSet> { _workShiftRuleSet2, _workShiftRuleSet3, _workShiftRuleSet4 };
            
            _period = new DateOnlyPeriod(2001, 01, 01, 2001, 01, 02);

            _target = new WorkRuleSetExtractorForGroupOfPeople(_persons);
        }

        [Test]
        public void OnlyUniqueRuleSetBagsShouldOutcome()
        {
            const int uniqueRuleSetBags = 4;

            _personPeriod1.RuleSetBag = _ruleSetBag1;
            _personPeriod2.RuleSetBag = _ruleSetBag2;
            _personPeriod3.RuleSetBag = _ruleSetBag2;

            using(_mock.Record())
            {
                Expect.Call(_ruleSetBag1.RuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_workShiftRuleSetForBag1));
                Expect.Call(_ruleSetBag2.RuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_workShiftRuleSetForBag2));
            }
            using (_mock.Playback())
            {
                IList<IWorkShiftRuleSet> result = _target.ExtractRuleSets(_period).ToList();
                Assert.AreEqual(uniqueRuleSetBags, result.Count());
                Assert.AreEqual(_workShiftRuleSet1, result[0]);
            }
        }

        [Test]
        public void OnlyTheCurrentPersonPeriodShouldBeValid()
        {
            const int uniqueRuleSetBags = 3;

            IPersonPeriod validPersonPeriod = PersonPeriodFactory.CreatePersonPeriod(new DateOnly(2000, 01, 01));
            _person3.AddPersonPeriod(validPersonPeriod);

            _personPeriod1.RuleSetBag = _ruleSetBag2;
            _personPeriod2.RuleSetBag = _ruleSetBag2;

            _personPeriod3.RuleSetBag = _ruleSetBag1; // we override this ruleset
            validPersonPeriod.RuleSetBag = _ruleSetBag2;

            using (_mock.Record())
            {
                Expect.Call(_ruleSetBag2.RuleSetCollection)
                    .Return(new ReadOnlyCollection<IWorkShiftRuleSet>(_workShiftRuleSetForBag2));
            }
            using (_mock.Playback())
            {
                IEnumerable<IWorkShiftRuleSet> result = _target.ExtractRuleSets(_period);
                Assert.AreEqual(uniqueRuleSetBags, result.Count());
            }
        }
    }
}