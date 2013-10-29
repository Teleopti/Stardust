using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
    public class TeamBlockTargetValueCalculatorTest
    {
        private TeamBlockTargetValueCalculator _target;
        private List<double> _inputValues;

        [SetUp]
        public void Setup()
        {
            _inputValues = new List<double> {-.2, 3.26, 1.75, 1.01, 0.62};
        }

        [Test]
        public void VerifyConstructorsAndInitialData()
        {
            _target = new TeamBlockTargetValueCalculator(_inputValues);
            int count = _inputValues.Count;
            _inputValues.Add((double)1 / 0); // NaN value 

            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IgnoreNonNumberValues);

            _target = new TeamBlockTargetValueCalculator();
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IgnoreNonNumberValues);
            Assert.AreEqual(0, _target.Count);

            _target = new TeamBlockTargetValueCalculator(_inputValues, false);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IgnoreNonNumberValues);
            Assert.AreEqual(count + 1, _target.Count);

            _target = new TeamBlockTargetValueCalculator(false);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IgnoreNonNumberValues);
            Assert.AreEqual(0, _target.Count);

            _target = new TeamBlockTargetValueCalculator(_inputValues);
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IgnoreNonNumberValues);
            Assert.AreEqual(count, _target.Count);
        }

        
        [Test]
        public void VerifyAddAndClear()
        {
            _target = new TeamBlockTargetValueCalculator(_inputValues);
            int count = _target.Count;
            double value = 10.2d;
            _target.AddItem(value);
            Assert.AreEqual(++count, _target.Count);
            IPopulationStatisticsData result = _target[5];
            Assert.AreEqual(value, result.Value);

            _target.Clear();
            Assert.AreEqual(0, _target.Count);
        }

        [Test]
        public void VerifyAddItemDoesNotAddNonNumberValue()
        {
            _target = new TeamBlockTargetValueCalculator(_inputValues);
            int count = _target.Count;
            _target.AddItem((double)1 / 0); // add infinitive
            Assert.AreEqual(count, _target.Count);
        }

        [Test]
        public void VerifyAddItemAddNonNumberValueWhenIgnoreNonNumberValuesIsFalse()
        {
            _target = new TeamBlockTargetValueCalculator(_inputValues, false);
            int count = _target.Count;
            _target.AddItem((double)1 / 0); // add infinitive
            Assert.AreEqual(++count, _target.Count);
        }

        [Test]
        public void VerifyCommonStatisticData()
        {
            _target = new TeamBlockTargetValueCalculator(_inputValues);
            Assert.AreEqual(5, _target.Count);
            Assert.AreEqual(6.44, _target.Summa,0.01d);
            Assert.AreEqual(1.29d, _target.Average, 0.01d);
            Assert.AreEqual(1.17, _target.StandardDeviation, 0.01d);
            Assert.AreEqual(_target.AbsoluteSum + _target.StandardDeviation, _target.Teleopti, 0.01d);

            var inputValues = new List<double> {-0.99d, 30.61d, 25.46d, 19.89d, 13.74d};
            _target = new TeamBlockTargetValueCalculator(inputValues);
            _target.Analyze();
            
            Assert.AreEqual(5, _target.Count);
            Assert.AreEqual(20.84, _target.RootMeanSquare, 0.01d);
        }

        [Test]
        public void VerifyItemsStatisticData()
        {
            _target = new TeamBlockTargetValueCalculator(_inputValues);
            Assert.AreEqual(-1.49d, _target[0].DeviationFromAverage, 0.01d);
            Assert.AreEqual(1.98d, _target[1].DeviationFromAverage, 0.01d);
            Assert.AreEqual(0.46d, _target[2].DeviationFromAverage, 0.01d);
            Assert.AreEqual(-0.28d, _target[3].DeviationFromAverage, 0.01d);
            Assert.AreEqual(-0.67d, _target[4].DeviationFromAverage, 0.01d);
        }

        [Test]
        public void TeleoptiShouldCalculateCorrect()
        {
            _target = new TeamBlockTargetValueCalculator(_inputValues);
            _target.Clear();
            _target.AddItem(10);
            _target.AddItem(-10);
            _target.Analyze();
            Assert.AreEqual(30d, _target.Teleopti);
        }
    }


}
