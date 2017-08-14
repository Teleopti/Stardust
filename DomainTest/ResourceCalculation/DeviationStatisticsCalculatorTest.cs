using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class DeviationStatisticsCalculatorTest
    {
        private DeviationStatisticsCalculator _target;

        [SetUp]
        public void Setup()
        {
            _target = new DeviationStatisticsCalculator();
            _target.AddItem(11.4d, 13.4d); //    2      
            _target.AddItem(17.3d, 18.3d); //    1
            _target.AddItem(21.3d, 20.5d); //   -0.8
            _target.AddItem(25.9d, 25d);   //   -0.9 
            _target.AddItem(40.1d, 32.1d); //   -8

        }

        [Test]
        public void VerifyConstructors()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCommonStatisticData()
        {
            Assert.AreEqual(-6.7d, _target.AbsoluteDeviationSumma, 0.01d);
            Assert.AreEqual(-1.34d, _target.AbsoluteDeviationAverage, 0.01d);
            Assert.AreEqual(3.50d, _target.AbsoluteStandardDeviation, 0.01d);
            Assert.AreEqual(3.75d, _target.AbsoluteRootMeanSquare, 0.01d);

            Assert.AreEqual(-0.03857, _target.RelativeDeviationSumma, 0.01d);
            Assert.AreEqual(-0.00771d, _target.RelativeDeviationAverage, 0.01d);
            Assert.AreEqual(0.123484d, _target.RelativeStandardDeviation, 0.01d);
            Assert.AreEqual(0.1237d, _target.RelativeRootMeanSquare, 0.01d);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyCalculatorConstructorWithLists()
        {
            double[] expected = { 11.4d, 17.3d, 21.3d, 25.9d, 40.1d };
            double[] real = { 13.4d, 18.3d, 20.5d, 25d, 32.1d };

            Assert.AreEqual(0.1237d, new DeviationStatisticsCalculator(expected, real).RelativeRootMeanSquare, 0.01d);

            expected = new double[] { 11.4d, 17.3d, 21.3d, 25.9d, 40.1d, 0.1d };
            real = new double[] { 13.4d, 18.3d, 20.5d, 25d, 32.1d, 5.0d};

            Assert.AreEqual(20d, new DeviationStatisticsCalculator(expected, real).RelativeRootMeanSquare, 0.01d);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyCalculatorConstructorWithNullExpectedParameter()
        {
            double[] real = { 13.4d, 18.3d, 20.5d, 25d, 32.1d };

	        Assert.Throws<ArgumentNullException>(() =>
	        {
				Assert.AreEqual(0.1237d, new DeviationStatisticsCalculator(null, real).RelativeRootMeanSquare, 0.01d);
			});
            
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyCalculatorConstructorWithNullRealParameter()
        {
            double[] expected = { 13.4d, 18.3d, 20.5d, 25d, 32.1d };

			Assert.Throws<ArgumentNullException>(() =>
			{
				Assert.AreEqual(0.1237d, new DeviationStatisticsCalculator(expected, null).RelativeRootMeanSquare, 0.01d);
			});
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyCalculatorConstructorWithDifferentLength()
        {
            double[] expected = { 11.4d, 17.3d, 21.3d, 25.9d, 40.1d, 0d };
            double[] real = { 13.4d, 18.3d, 20.5d, 25d, 32.1d };

			Assert.Throws<ArgumentException>(() =>
			{

				Assert.AreEqual(0.1237d, new DeviationStatisticsCalculator(expected, real).RelativeRootMeanSquare, 0.01d);
			});
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyCalculatorWorksWithNonNumberValues()
        {
            double[] expected = { 1, 0d  };
            double[] real = { 1.5, 1d };

            _target = new DeviationStatisticsCalculator(expected, real);
            Assert.IsFalse(double.IsNaN(new DeviationStatisticsCalculator(expected, real).RelativeStandardDeviation));
        }
    }
}
