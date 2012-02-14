using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest
{
    [TestFixture]
    public class DayOffOptimizationWeekendLegalStateValidatorListCreatorTest
    {
        private IDayOffLegalStateValidatorListCreator _target;

        [SetUp]
        public void Setup()
        {
            IDayOffPlannerRules rules = new DayOffPlannerRules();
            rules.UseFreeWeekends = true;
            IOfficialWeekendDays weekendDays = new OfficialWeekendDays(new CultureInfo(1053));
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