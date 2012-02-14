using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoaderTest
    {
        private RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader _target;
        private IList<IPerson> _persons;

        [SetUp]
        public void Setup()
        {
            _target = new RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader();
            _persons = new List<IPerson> {PersonFactory.CreatePerson("A"), PersonFactory.CreatePerson("B")};
        }

        [Test]
        public void NoneOfCreatedComponentShouldBeNull()
        {
            ITimePeriodCanHaveShortBreak timePeriodCanHaveShortBreak = RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader.CreateTimePeriodCanHaveShortBreak();
            ISkillExtractor skillExtractor = RuleSetBagsOfGroupOfPeopleCanHaveShortBreakLoader.CreatePersonalSkillsExtractor(_persons);
            Assert.IsNotNull(timePeriodCanHaveShortBreak);
            Assert.IsNotNull(skillExtractor);
            Assert.IsNotNull(_target.CreateWorkRuleSetExtractor(_persons));
            Assert.IsNotNull(_target.CreateWorkShiftRuleSetCanHaveShortBreak(_persons));
        }

    }
}