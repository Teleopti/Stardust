using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class ModifyCalculatorTest
    {
        private ModifyCalculator target;

        private readonly IList<double> _numbers = new List<double> { 2000, 1000, 3000 };

        [SetUp]
        public void Setup()
        {
            target = new ModifyCalculator(_numbers);
        }

        [Test]
        public void VerifyCanModifyTotal()
        {
            Assert.AreEqual(6000, target.Sum);
            target.ModifyTotal(3000);
            Assert.AreEqual(3000, target.ModifiedSum);
            Assert.AreEqual(1000, target.ModifiedValues[0]);
            Assert.AreEqual(500, target.ModifiedValues[1]);
            Assert.AreEqual(1500, target.ModifiedValues[2]);
            Assert.AreEqual(1000, target.Average);
            Assert.AreEqual(408, Math.Round(target.StandardDev, 0));
        }

        [Test]
        public void VerifyCanGetOriginalValues()
        {
            Assert.AreEqual(6000, target.Sum);
            target.ModifyTotal(3000);
            Assert.AreEqual(3000, target.ModifiedSum);
            Assert.AreEqual(2000, target.OriginalValues[0]);
            Assert.AreEqual(1000, target.OriginalValues[1]);
            Assert.AreEqual(3000, target.OriginalValues[2]);
            Assert.AreEqual(1000, target.Average);
            Assert.AreEqual(408, Math.Round(target.StandardDev, 0));
        }

        [Test]
        public void VerifyCanCalculateSum()
        {
            Assert.AreEqual(6000, target.Sum);
        }

        [Test]
        public void VerifyCanSummarizeElements()
        {
            Assert.AreEqual(3, target.ChosenAmount);
        }

        [Test]
        public void VerifyCanCalculateAverage()
        {
            Assert.AreEqual(0, target.Average);
        }

        [Test]
        public void VerifyCanCalculateStandardDev()
        {
            Assert.AreEqual(2160, Math.Round(target.StandardDev, 0));
        }

        [Test]
        public void VerifyCanCalculateInputPercentage()
        {
            target.CalculateCurrentPercentage(12000);
            Assert.AreEqual(100, target.UpdatePercent);
        }

        [Test]
        public void VerifyCanCalculateInputTotal()
        {
            target.CalculateCurrentTotal(100);
            Assert.AreEqual(12000, target.UpdateTotal);
        }

        [Test]
        public void VerifyCanSmoothenValues()
        {
            Assert.AreEqual(6000, target.Sum);
            target.ModifyTotal(4000);
            Assert.AreEqual(4000, target.ModifiedSum);
            Assert.AreEqual(1333, Math.Round(target.ModifiedValues[0], 0));
            Assert.AreEqual(667, Math.Round(target.ModifiedValues[1], 0));
            Assert.AreEqual(2000, Math.Round(target.ModifiedValues[2], 0));
            Assert.AreEqual(1333.3, Math.Round(target.Average, 1));
            Assert.AreEqual(544, Math.Round(target.StandardDev, 0));
            target.SmoothenValues(3);
            Assert.AreEqual(1091, Math.Round(target.SmoothenedValues[0], 0));
            Assert.AreEqual(1455, Math.Round(target.SmoothenedValues[1], 0));
            Assert.AreEqual(1455, Math.Round(target.SmoothenedValues[2], 0));
            Assert.AreEqual(1333.3, Math.Round(target.Average, 1));
        }
    }
}
