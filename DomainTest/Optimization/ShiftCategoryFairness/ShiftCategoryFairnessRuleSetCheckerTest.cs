using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
    [TestFixture]
    public class ShiftCategoryFairnessRuleSetCheckerTest
    {
        private PersonPeriod _personPeriod1, _personPeriod2;
        private ShiftCategoryFairnessRuleSetChecker _target;

        [SetUp]
        public void Setup()
        {
            var ruleSetBag = new RuleSetBag();
            _target = new ShiftCategoryFairnessRuleSetChecker();
            _personPeriod1 = new PersonPeriod(new DateOnly(2001, 01, 01),
                                              new PersonContract(new Contract("Main"),
                                                                 new PartTimePercentage("Percentage"),
                                                                 new ContractSchedule("Schedule")),
                                              new Team()) {RuleSetBag = ruleSetBag};
            _personPeriod2 = new PersonPeriod(new DateOnly(2001, 01, 01),
                                              new PersonContract(new Contract("Main"),
                                                                 new PartTimePercentage("Percentage"),
                                                                 new ContractSchedule("Schedule")),
                                              new Team()) {RuleSetBag = ruleSetBag};
        }

        [Test]
        public void SameRuleSetBagShouldReturnTrue()
        {
            var result = _target.Check(_personPeriod1, _personPeriod2);

            Assert.That(result, Is.True);
        }

        [Test]
        public void DifferentRuleSetBagShouldReturnFalse()
        {
            _personPeriod2.RuleSetBag = new RuleSetBag();
            var result = _target.Check(_personPeriod1, _personPeriod2);

            Assert.That(result, Is.False);
        }

        [Test]
        public void SameRuleDifferentBagShouldReturnTrue()
        {
            var ruleSet = new WorkShiftRuleSet(new WorkShiftTemplateGenerator(new Activity("An activity"),
                                                                              new TimePeriodWithSegment(0, 0, 1, 0, 30),
                                                                              new TimePeriodWithSegment(12, 0, 13, 0, 30),
                                                                              new ShiftCategory("Day")));
            _personPeriod2.RuleSetBag = new RuleSetBag();
            var result = _target.Check(_personPeriod1, _personPeriod2);
            Assert.That(result, Is.False);
            _personPeriod1.RuleSetBag.AddRuleSet(ruleSet);
            _personPeriod2.RuleSetBag.AddRuleSet(ruleSet);
            result = _target.Check(_personPeriod1, _personPeriod2);
            Assert.That(result, Is.True);
        }
    }
}
