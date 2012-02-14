using System;
using NUnit.Framework;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class AverageShiftLengthValueCalculatorTest
    {
        private AverageShiftLengthValueCalculator _target;

        [SetUp]
        public void Setup()
        {
            _target = new AverageShiftLengthValueCalculator();
        }

        [Test]
        public void VerifyPositiveShiftValue()
        {
            TimeSpan average = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(36));

            TimeSpan shift1 = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15));
            TimeSpan shift2 = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30));
            TimeSpan shift3 = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(45));
            TimeSpan shift4 = TimeSpan.FromHours(8);

            double result1 = _target.CalculateShiftValue(3, shift1, average);
            double result2 = _target.CalculateShiftValue(3, shift2, average);
            double result3 = _target.CalculateShiftValue(3, shift3, average);
            double result4 = _target.CalculateShiftValue(3, shift4, average);

            Assert.Greater(result2, result3);
            Assert.Greater(result3, result1);
            Assert.Greater(result1, result4);
        }

        [Test]
        public void VerifyNegativeShiftValue()
        {
            TimeSpan average = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(36));

            TimeSpan shift1 = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15));
            TimeSpan shift2 = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30));
            TimeSpan shift3 = TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(45));
            TimeSpan shift4 = TimeSpan.FromHours(8);

            double result1 = _target.CalculateShiftValue(-3, shift1, average);
            double result2 = _target.CalculateShiftValue(-3, shift2, average);
            double result3 = _target.CalculateShiftValue(-3, shift3, average);
            double result4 = _target.CalculateShiftValue(-3, shift4, average);

            Assert.Greater(result2, result3);
            Assert.Greater(result3, result1);
            Assert.Greater(result1, result4);
        }

        [Test]
        public void VerifyShiftPenaltyChooseTheBestShiftEvenIfOriginalShiftValueIsHigher()
        {
            TimeSpan average = TimeSpan.FromHours(7);

            TimeSpan shift1 = TimeSpan.FromHours(8);
            TimeSpan shift2 = TimeSpan.FromHours(9);

            double result1 = _target.CalculateShiftValue(10, shift1, average);
            double result2 = _target.CalculateShiftValue(15, shift2, average);

            Assert.Greater(result1, result2);
        }


        [Test]
        public void VerifyZeroOriginalValueKeepsTheValue()
        {
            TimeSpan average = TimeSpan.FromHours(7);
            TimeSpan shift1 = TimeSpan.FromHours(8);

            double originalValue = 0;

            double result1 = _target.CalculateShiftValue(originalValue, shift1, average);

            Assert.AreEqual(originalValue, result1);
        }

        [Test]
        public void VerifyAverageEqualsCurrentKeepsTheValue()
        {
            TimeSpan average = TimeSpan.FromHours(7);
            TimeSpan shift1 = TimeSpan.FromHours(7);

            double originalValue = 10;

            double result1 = _target.CalculateShiftValue(originalValue, shift1, average);

            Assert.AreEqual(originalValue, result1);
        }
    }
}