using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class IntervalDataMinimumMedianCalculatorTest
    {
        private IIntervalDataCalculator _target;

        [SetUp]
        public void Setup()
        {
            _target = new IntervalDataMinimumMedianCalculator();
        }

        [Test]
        public void ShouldCalculateMinimumMedian()
        {
            var intervalValue = new List<double>() { 0,1,3,1,3,2, 3, 1,1,4,4 };
            Assert.AreEqual(3, _target.Calculate(intervalValue));

            intervalValue = new List<double>() { 14, 3, 12, 13, 3, 13,1 };
            Assert.AreEqual(13, _target.Calculate(intervalValue));
        }

        [Test]
        public void ShouldHandleEmptyList()
        {
            var intervalValue = new List<double>();
            Assert.AreEqual(0f, _target.Calculate(intervalValue));
        }
    }
}
