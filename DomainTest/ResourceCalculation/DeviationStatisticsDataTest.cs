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
            Assert.AreEqual(19.0d, _target.RelativeDeviation);

			_target = new DeviationStatisticData(0.5, 10);
			Assert.AreEqual(double.NaN, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(0.5, 11);
            Assert.Less(0.5, 1);
            Assert.AreEqual(21.0d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5, 12);
            Assert.Less(0.5, 1);
            Assert.AreEqual(23.0d, _target.RelativeDeviation);

            _target = new DeviationStatisticData(0.5, 100);
            Assert.Less(0.5, 1);
            Assert.AreEqual(99.9d, _target.RelativeDeviation);

			_target = new DeviationStatisticData(0.5, 200);
			Assert.Less(0.5, 1);
			Assert.AreEqual(99.9d, _target.RelativeDeviation);

        }

        [Test]
        public void VerifyRelativeDeviationForDisplay()
        {
            _target = new DeviationStatisticData(100, 50);
            Assert.AreEqual(-0.5d, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(50, 100);
            Assert.AreEqual(1d, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(0.5d, 2d);
            Assert.AreEqual(3d, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(0.5d, 0.6d);
            Assert.AreEqual(0.2d, _target.RelativeDeviationForDisplay, 0.01);

            _target = new DeviationStatisticData(3d, 5d);
            Assert.AreEqual((5d - 3d) / 3d, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(0, 0);
            Assert.AreEqual(0d, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(10, 0);
            Assert.AreEqual(-1d, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(0.5, 0);
            Assert.AreEqual(-1d, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(0, 0.5);
            Assert.AreEqual(double.NaN, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(0, 10);
            Assert.AreEqual(double.NaN, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(0.5, 0.5);
            Assert.AreEqual(0d, _target.RelativeDeviationForDisplay);

            _target = new DeviationStatisticData(50, 50);
            Assert.AreEqual(0d, _target.RelativeDeviationForDisplay);

            // Max 9.99 tests 

            _target = new DeviationStatisticData(0.5, 5.4);
            Assert.AreEqual(9.8, _target.RelativeDeviation, 0.01);
            Assert.AreEqual(9.8, _target.RelativeDeviationForDisplay, 0.01);

            _target = new DeviationStatisticData(0.5, 5.5);
            Assert.AreEqual(9.99, _target.RelativeDeviation, 0.01);
            Assert.AreEqual(double.NaN, _target.RelativeDeviationForDisplay, 0.01);

            _target = new DeviationStatisticData(2, 22);
            Assert.AreEqual(double.NaN, _target.RelativeDeviationForDisplay);

        }
    }
}
