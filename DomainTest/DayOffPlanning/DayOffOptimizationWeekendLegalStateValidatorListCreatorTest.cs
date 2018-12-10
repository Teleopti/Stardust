using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;


namespace Teleopti.Ccc.DomainTest.DayOffPlanning
{
    [TestFixture]
    public class DayOffOptimizationWeekendLegalStateValidatorListCreatorTest
    {
        private IDayOffLegalStateValidatorListCreator _target;

        [SetUp]
        public void Setup()
        {
            IDaysOffPreferences rules = new DaysOffPreferences();
            rules.UseFullWeekendsOff = true;
            IOfficialWeekendDays weekendDays = new OfficialWeekendDays();
            _target = new DayOffOptimizationWeekendLegalStateValidatorListCreator(rules, weekendDays,
                                                                                  new MinMax<int>(7, 21));
        }

        [Test]
        public void SimpleTest()
        {
            IList<IDayOffLegalStateValidator> result = _target.BuildActiveValidatorList();
            Assert.AreEqual(1, result.Count);
        }
    }
}