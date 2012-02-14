using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class FairnessValueCalculatorTest
    {
        private FairnessValueCalculator _target;
        private Percent _percentFairness;
        private double _maxFairnessPoint;
        private double _totalData;
        private IFairnessValueResult _agentData;

        [SetUp]
        public void Setup()
        {
            _percentFairness = new Percent(0.7);
            _maxFairnessPoint = 10;
            _totalData = 3;
            _agentData = new FairnessValueResult();
            _agentData.FairnessPoints = 50;
            _agentData.TotalNumberOfShifts = 10;

            _target = new FairnessValueCalculator(new SchedulingOptions{Fairness = _percentFairness});
        }

        [Test]
        public void CanCalculateFairness()
        {
            var result = _target.CalculateFairnessValue(-100, 1, _maxFairnessPoint, _totalData, _agentData, 250);
            Assert.AreEqual(145, Math.Round(result,1));
            result = _target.CalculateFairnessValue(-10, 2, _maxFairnessPoint, _totalData, _agentData, 250);
            Assert.AreEqual(163.2, Math.Round(result,1));
            result = _target.CalculateFairnessValue(90, 5, _maxFairnessPoint, _totalData, _agentData, 250);
            Assert.AreEqual(167, Math.Round(result, 0));

        }

        [Test]
        public void CalculateWithNoFairness()
        {
            _target = new FairnessValueCalculator(new SchedulingOptions { Fairness = new Percent(0)});
            var result = _target.CalculateFairnessValue(-34000, 1, _maxFairnessPoint, _totalData, _agentData, 250);
            Assert.AreEqual(-34000, Math.Round(result, 0));
            result = _target.CalculateFairnessValue(-5, 2, _maxFairnessPoint, _totalData, _agentData, 250);
            Assert.AreEqual(-5, Math.Round(result, 0));
            result = _target.CalculateFairnessValue(90, 5, _maxFairnessPoint, _totalData, _agentData, 250);
            Assert.AreEqual(90, Math.Round(result, 0));
        }

        [Test]
        public void CalculateWithOnlyFairness()
        {
            _target = new FairnessValueCalculator(new SchedulingOptions { Fairness = new Percent(1) });
            var result = _target.CalculateFairnessValue(-100, 1, _maxFairnessPoint, _totalData, _agentData, 250);
            Assert.AreEqual(250, Math.Round(result, 1));
            result = _target.CalculateFairnessValue(-10, 2, _maxFairnessPoint, _totalData, _agentData, 250);
            Assert.AreEqual(237.5, Math.Round(result, 1));
            result = _target.CalculateFairnessValue(90, 5, _maxFairnessPoint, _totalData, _agentData, 250);
            Assert.AreEqual(200, Math.Round(result, 0));
        }
    }
    

    
}
