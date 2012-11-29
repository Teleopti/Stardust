﻿using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
    [TestFixture]
    public class IntervalDataMedianCalculatorTest
    {
        private IIntervalDataCalculator _target;

        [SetUp]
        public void Setup()
        {
            _target = new IntervalDataMedianCalculator();
        }

        [Test]
        public void ShouldCalculateMedian()
        {
            var intervalValue = new List<double>( ){14,3,12.5,13,1};
            Assert.AreEqual(12.5, _target.Calculate(intervalValue));

            intervalValue = new List<double>() { 14, 3, 12, 13, 1,15 };
            Assert.AreEqual(12.5, _target.Calculate(intervalValue));
        }
    }

    
}
