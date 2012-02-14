using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class DeviationStatisticsDataTest
    {
        private DeviationStatisticData _target;

        [Test]
        public void VerifyRelativeDeviation()
        {
            _target = new DeviationStatisticData(100, 50);
            Assert.AreEqual(-0.5d, _target.RelativeDeviation);
            
            _target = new DeviationStatisticData(50, 100);
            Assert.AreEqual(1d, _target.RelativeDeviation);
            
            _target = new DeviationStatisticData(0.5d, 2d);
            Assert.AreEqual(3d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5d, 0.6d);
            Assert.AreEqual(0.2d, _target.RelativeDeviation, 0.01);

            _target = new DeviationStatisticData(3d, 5d);
            Assert.AreEqual((5d-3d)/3d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0, 0);
            Assert.AreEqual(0d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(10, 0);
            Assert.AreEqual(-1d, _target.RelativeDeviation);
            
            _target = new DeviationStatisticData(0.5, 0);
            Assert.AreEqual(-1d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0, 0.5);
            Assert.AreEqual(double.NaN, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0, 10);
            Assert.AreEqual(double.NaN, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5, 0.5);
            Assert.AreEqual(0d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(50, 50);
            Assert.AreEqual(0d, _target.RelativeDeviation);
        }

        [Test]
        public void VerifyRelativeDeviationNotShownOver9Comma9WhenExpectedLessThan1()
        {
             _target = new DeviationStatisticData(0.5, 2.5);
            Assert.AreEqual(4d, _target.RelativeDeviation, 0.01);

            _target = new DeviationStatisticData(1, 5);
            Assert.AreEqual(4d, _target.RelativeDeviation, 0.01);

            _target = new DeviationStatisticData(1, 5.1);
            Assert.AreEqual(4.1d, _target.RelativeDeviation, 0.01);

            _target = new DeviationStatisticData(1, 10);
            Assert.AreEqual(9d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(1, 11);
            Assert.GreaterOrEqual(1, 1);
            Assert.AreEqual(10d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(1, 12);
            Assert.GreaterOrEqual(1, 1);
            Assert.AreEqual(11d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(1, 100);
            Assert.GreaterOrEqual(1, 1);
            Assert.AreEqual(99d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5, 10);
            Assert.AreEqual(9.99d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5, 11);
            Assert.Less(0.5, 1);
            Assert.AreEqual(9.99d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5, 12);
            Assert.Less(0.5, 1);
            Assert.AreEqual(9.99d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5, 100);
            Assert.Less(0.5, 1);
            Assert.AreEqual(9.99d, _target.RelativeDeviation);

        }

        [Test]
        public void VerifyRelativeDeviationForDisplay()
        {
            _target = new DeviationStatisticData(100, 50);
            Assert.AreEqual(-0.5d, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(50, 100);
            Assert.AreEqual(1d, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(0.5d, 2d);
            Assert.AreEqual(3d, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(0.5d, 0.6d);
            Assert.AreEqual(0.2d, _target.RelativeDeviationForDisplayOnly, 0.01);

            _target = new DeviationStatisticData(3d, 5d);
            Assert.AreEqual((5d - 3d) / 3d, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(0, 0);
            Assert.AreEqual(0d, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(10, 0);
            Assert.AreEqual(-1d, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(0.5, 0);
            Assert.AreEqual(-1d, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(0, 0.5);
            Assert.AreEqual(double.NaN, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(0, 10);
            Assert.AreEqual(double.NaN, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(0.5, 0.5);
            Assert.AreEqual(0d, _target.RelativeDeviationForDisplayOnly);

            _target = new DeviationStatisticData(50, 50);
            Assert.AreEqual(0d, _target.RelativeDeviationForDisplayOnly);

            // Max 9.99 tests 

            _target = new DeviationStatisticData(0.5, 5.4);
            Assert.AreEqual(9.8, _target.RelativeDeviation, 0.01);
            Assert.AreEqual(9.8, _target.RelativeDeviationForDisplayOnly, 0.01);

            _target = new DeviationStatisticData(0.5, 5.5);
            Assert.AreEqual(9.99, _target.RelativeDeviation, 0.01);
            Assert.AreEqual(double.NaN, _target.RelativeDeviationForDisplayOnly, 0.01);

            _target = new DeviationStatisticData(2, 22);
            Assert.AreEqual(double.NaN, _target.RelativeDeviationForDisplayOnly);

        }

        [Test]
        [Ignore]
        public void VerifyDeviationStatisticDataPro()
        {
            _target = new DeviationStatisticData(100, 50);
            Assert.AreEqual(-1d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(50, 100);
            Assert.AreEqual(1d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5d, 2d);
            Assert.AreEqual(3, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5d, 0.6d);
            Assert.AreEqual(0.2d, _target.RelativeDeviation, 0.01);

            _target = new DeviationStatisticData(3d, 5d);
            Assert.AreEqual((5d - 3d) / 3d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0, 0);
            Assert.AreEqual(0d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(10, 0);
            Assert.IsTrue(double.IsInfinity(_target.RelativeDeviation));

            _target = new DeviationStatisticData(0.5, 0);
            Assert.IsTrue(double.IsInfinity(_target.RelativeDeviation));

            _target = new DeviationStatisticData(0, 0.5);
            Assert.IsTrue(double.IsInfinity(_target.RelativeDeviation));

            _target = new DeviationStatisticData(0, 10);
            Assert.IsTrue(double.IsInfinity(_target.RelativeDeviation));

            _target = new DeviationStatisticData(0.5, 0.5);
            Assert.AreEqual(0d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5, 10);
            Assert.AreEqual(19.0d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(50, 50);
            Assert.AreEqual(0d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.99, 1.97);
            Assert.AreEqual(0.99, _target.RelativeDeviation, 0.01);

        }

        [Test]
        [Ignore]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyRelativeDeviationWithNegativeRealValue()
        {
            _target = new DeviationStatisticData(0, -2);
            Assert.AreEqual(0d, _target.RelativeDeviation);
        }

        [Test]
        [Ignore]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyRelativeDeviationWithNegativeExpectedValue()
        {
            _target = new DeviationStatisticData(-2, 0);
            Assert.AreEqual(0d, _target.RelativeDeviation);
        }

        //[Test]
        //[Ignore]
        //public void VerifyRelativeDeviationCapableToGetRightOrder()
        //{
        //    // test case 1 : the normal case
        //    double[] forecasted = { 2, 0.01, 5 };
        //    double[] scheduled = { 1, 0.001, 2 };
        //    // -0.5 ; -0.9 ; -0.6

        //    DeviationStatisticsCalculator calculator = new DeviationStatisticsCalculator(forecasted, scheduled);

        //    IList<DeviationStatisticData> rowList = calculator.DeviationStatisticDataItems;
        //    IList<DeviationStatisticData> orderedList = SortedRelativeDeviationList(calculator);

        //    // order> 0.5; 0.6; 0.9
        //    Assert.AreEqual(orderedList[0].AbsoluteDeviation, rowList[0].AbsoluteDeviation);
        //    Assert.AreEqual(orderedList[1].AbsoluteDeviation, rowList[2].AbsoluteDeviation);
        //    Assert.AreEqual(orderedList[2].AbsoluteDeviation, rowList[1].AbsoluteDeviation);

        //    // test case 2 : if scheduled is 0, then the one with the lower forecasted should be first
        //    forecasted = new double[] {5, 0.01, 2, 0.1, 11 };
        //    scheduled = new double[] { 0, 0, 0, 0, 0 };
        //    // -0.09 ; -0.6 ; -0.5 

        //    calculator = new DeviationStatisticsCalculator(forecasted, scheduled);

        //    rowList = calculator.DeviationStatisticDataItems;
        //    orderedList = SortedRelativeDeviationList(calculator);

        //    Assert.AreEqual(orderedList[0].AbsoluteDeviation, rowList[1].AbsoluteDeviation);
        //    Assert.AreEqual(orderedList[1].AbsoluteDeviation, rowList[3].AbsoluteDeviation);
        //    Assert.AreEqual(orderedList[2].AbsoluteDeviation, rowList[2].AbsoluteDeviation);
        //    Assert.AreEqual(orderedList[3].AbsoluteDeviation, rowList[0].AbsoluteDeviation);
        //    Assert.AreEqual(orderedList[4].AbsoluteDeviation, rowList[4].AbsoluteDeviation);
        //}

        //private IList<DeviationStatisticData> SortedRelativeDeviationList(DeviationStatisticsCalculator calculator)
        //{
        //    var customersWithOrders = from c in calculator.DeviationStatisticDataItems
        //                              orderby Math.Abs(c.RelativeDeviation)
        //                              select c;
        //    return customersWithOrders.ToList();
        //}
    }
}
