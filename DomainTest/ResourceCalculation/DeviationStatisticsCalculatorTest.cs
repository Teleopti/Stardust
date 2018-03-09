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
			_target = new DeviationStatisticsCalculator(new[]
			{
				new DeviationStatisticData(11.4d, 13.4d), //    2      
				new DeviationStatisticData(17.3d, 18.3d), //    1
				new DeviationStatisticData(21.3d, 20.5d), //   -0.8
				new DeviationStatisticData(25.9d, 25d), //   -0.9 
				new DeviationStatisticData(40.1d, 32.1d), //   -8
			});
		}

        [Test]
        public void VerifyConstructors()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyCommonStatisticData()
        {
            Assert.AreEqual(0.123484d, _target.RelativeStandardDeviation, 0.01d);
        }
		
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [Test]
        public void VerifyCalculatorWorksWithNonNumberValues()
        {
            var expected = new[]
			{
				new DeviationStatisticData(1,1.5),
				new DeviationStatisticData(0d,1)
			};

            Assert.IsFalse(double.IsNaN(new DeviationStatisticsCalculator(expected).RelativeStandardDeviation));
        }
    }
}
