using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning.Scheduling;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DayOffPlanningTest.Scheduling
{
    [TestFixture]
    public class ShiftCategoryFairnessShiftValueCalculatorTest
    {
        private IShiftCategoryFairnessShiftValueCalculator _target;


        [SetUp]
        public void Setup()
        {

            _target = new ShiftCategoryFairnessShiftValueCalculator();
        }

        [Test]
        public void VerifyModifiedShiftValueWithPositive()
        {
			double result = _target.ModifiedShiftValue(1000d, 1.6384d, 1000d, new SchedulingOptions { Fairness = new Percent(0.5d) });
            Assert.AreEqual(1319.2d, result);
        }

        [Test]
        public void VerifyModifiedShiftValueWithZero()
        {
			double result = _target.ModifiedShiftValue(0d, 2.25d, 1000d, new SchedulingOptions { Fairness = new Percent(0.5d) });
            Assert.AreEqual(1125d, result);
        }

        [Test]
        public void VerifyModifiedShiftValueWithNegative()
        {
			double result = _target.ModifiedShiftValue(-500d, 2.25d, 1000d, new SchedulingOptions { Fairness = new Percent(0.5d) });
            Assert.AreEqual(875d, result);
        }

        [Test]
        public void VerifyModifiedShiftValueWithFairnessPercent()
        {
            _target = new ShiftCategoryFairnessShiftValueCalculator();
			double result = _target.ModifiedShiftValue(-500d, 2.25d, 1000d, new SchedulingOptions { Fairness = new Percent(0.25d) });
            Assert.AreEqual(187.5d, result);
        }

        [Test]
        public void VerifyModifiedShiftValueWithReturningNegative()
        {
            _target = new ShiftCategoryFairnessShiftValueCalculator();
			double result = _target.ModifiedShiftValue(-1000d, 2.25d, 1000d, new SchedulingOptions { Fairness = new Percent(0.25d) });
            Assert.AreEqual(-187.5d, result);
        }

    }
}