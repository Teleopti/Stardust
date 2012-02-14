using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture, SetCulture("en-US"), SetUICulture("en-US")]
    public class PopulationStatisticsCalculatorTestSet
    {
        private PopulationStatisticsCalculator _target;
        private IList<double> _values;

        [SetUp]
        public void Setup()
        {
            _values = new List<double>();
            _values.Add(11.4d);
            _values.Add(17.3d);
            _values.Add(21.3d);
            _values.Add(25.9d);
            _values.Add(40.1d);

            _target = new PopulationStatisticsCalculator(_values);
        }

        [Test]
        public void VerifyConstructorsAndInitialData()
        {

            int count = _values.Count;
            _values.Add((double)1/0); // NaN value 

            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IgnoreNonNumberValues);

            _target = new PopulationStatisticsCalculator();
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IgnoreNonNumberValues);
            Assert.AreEqual(0, _target.Count);

            _target = new PopulationStatisticsCalculator(_values, false);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IgnoreNonNumberValues);
            Assert.AreEqual(count+1, _target.Count);

            _target = new PopulationStatisticsCalculator(false);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.IgnoreNonNumberValues);
            Assert.AreEqual(0, _target.Count);

            _target = new PopulationStatisticsCalculator(_values);
            Assert.IsNotNull(_target);
            Assert.IsTrue(_target.IgnoreNonNumberValues);
            Assert.AreEqual(count, _target.Count);
        }

        [Test]
        public void VerifyItem()
        {
            IPopulationStatisticsData result = _target[1];
            Assert.AreEqual(17.3d, result.Value);
        }

        [Test]
        public void VerifyAddItem()
        {
            int count = _target.Count;
            double value = 10.2d;
            _target.AddItem(value);
            Assert.AreEqual(++count,_target.Count);
            IPopulationStatisticsData result = _target[5];
            Assert.AreEqual(value, result.Value);
        }

        [Test]
        public void VerifyClear()
        {
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
            int count = _target.Count;
            _target.AddItem((double)1/0); // add infinitive
            Assert.AreEqual(count, _target.Count);
        }

        [Test]
        public void VerifyAddItemAddNonNumberValueWhenIgnoreNonNumberValuesIsFalse()
        {
            _target = new PopulationStatisticsCalculator(_values, false);
            int count = _target.Count;
            _target.AddItem((double)1 / 0); // add infinitive
            Assert.AreEqual(++count, _target.Count);
        }

        [Test]
        public void VerifyCommonStatisticData()
        {
            _target.Analyze();

            Assert.AreEqual(5, _target.Count);
            Assert.AreEqual(116d, _target.Summa);
            Assert.AreEqual(23.2d, _target.Average);
            Assert.AreEqual(9.701, _target.StandardDeviation, 0.01d);// 0.001d);
            Assert.AreEqual(25.146, _target.RootMeanSquare, 0.01d);
        }

        [Test]
        public void VerifyItemsStatisticData()
        {
            _target.Analyze();

            Assert.AreEqual(-11.8d, _target[0].DeviationFromAverage, 0.01d);
            Assert.AreEqual(-5.9d, _target[1].DeviationFromAverage, 0.01d);
            Assert.AreEqual(-1.9d, _target[2].DeviationFromAverage, 0.01d);
            Assert.AreEqual(2.7d, _target[3].DeviationFromAverage, 0.01d);
            Assert.AreEqual(16.9d, _target[4].DeviationFromAverage, 0.01d);
        }

        [Test]
        public void VerifyToString()
        {
            Assert.AreEqual("11.4;17.3;21.3;25.9;40.1;", _target.ToString());
        }
    }
}
