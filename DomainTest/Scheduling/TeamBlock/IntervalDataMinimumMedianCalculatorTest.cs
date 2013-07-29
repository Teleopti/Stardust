using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
    [TestFixture]
    public class IntervalDataExtendedMedianCalculatorTest
    {
        private IIntervalDataCalculator _target;

        [SetUp]
        public void Setup()
        {
            _target = new IntervalDataEntendedMedianCalculator();
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
        public void ShouldCalculateMaximumMedian()
        {
            var intervalValue = new List<double>() { 0, 2, 5, 3, 5, 2, 4, 2, 3, 5, 5 };
            Assert.AreEqual(5, _target.Calculate(intervalValue));

            intervalValue = new List<double>() { 16, 5, 14, 14, 4, 17, 7 };
            Assert.AreEqual(14, _target.Calculate(intervalValue));
        }

        [Test]
        public void ShouldHandleEmptyList()
        {
            var intervalValue = new List<double>();
            Assert.AreEqual(0f, _target.Calculate(intervalValue));
        }
    }
}
