using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.ShiftCategoryDistribution
{
    [TestFixture]
    public class ShiftFairnessTest
    {
        [Test]
        public void VerifyThatShiftCategoryIsPopulated()
        {
            const int minimumValue = 4;
            const int maximumValue = 8;
            const double averageValue = 6;
            const double standardDeviationValue = 2.5;
            var shiftCategory = new ShiftCategory("Test1");
            var shiftFairness = new ShiftFairness(shiftCategory , minimumValue, maximumValue, averageValue, standardDeviationValue);
            Assert.AreEqual(shiftFairness.ShiftCategory,shiftCategory );
        }
        [Test]
        public void VerifyThatMinimumValueIsPopulated()
        {
            const int minimumValue = 4;
            const int maximumValue = 8;
            const double averageValue = 6;
            const double standardDeviationValue = 2.5;
            var shiftCategory = new ShiftCategory("Test1");
            var shiftFairness = new ShiftFairness(shiftCategory, minimumValue, maximumValue, averageValue, standardDeviationValue);
            Assert.AreEqual(shiftFairness.MinimumValue , minimumValue);
        }
        [Test]
        public void VerifyThatMaximumValueIsPopulated()
        {
            const int minimumValue = 4;
            const int maximumValue = 8;
            const double averageValue = 6;
            const double standardDeviationValue = 2.5;
            var shiftCategory = new ShiftCategory("Test1");
            var shiftFairness = new ShiftFairness(shiftCategory, minimumValue, maximumValue, averageValue, standardDeviationValue);
            Assert.AreEqual(shiftFairness.MaximumValue , maximumValue );
        }
        [Test]
        public void VerifyThatAverageValueIsPopulated()
        {
            const int minimumValue = 4;
            const int maximumValue = 8;
            const double averageValue = 6;
            const double standardDeviationValue = 2.5;
            var shiftCategory = new ShiftCategory("Test1");
            var shiftFairness = new ShiftFairness(shiftCategory, minimumValue, maximumValue, averageValue, standardDeviationValue);
            Assert.AreEqual(shiftFairness.AverageValue, averageValue);
        }
        [Test]
        public void VerifyThatStandardDeviationValueIsPopulated()
        {
            const int minimumValue = 4;
            const int maximumValue = 8;
            const double averageValue = 6;
            const double standardDeviationValue = 2.5;
            var shiftCategory = new ShiftCategory("Test1");
            var shiftFairness = new ShiftFairness(shiftCategory, minimumValue, maximumValue, averageValue, standardDeviationValue);
            Assert.AreEqual(shiftFairness.StandardDeviationValue , standardDeviationValue);
        }
    }
}
