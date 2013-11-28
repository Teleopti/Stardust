using System.Collections.Generic;
using NUnit.Framework;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture, SetCulture("en-US"), SetUICulture("en-US")]
    public class PopulationStatisticsCalculatorTestSet
    {
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
        }

        [Test]
        public void VerifyCommonStatisticData()
        {
            Assert.AreEqual(5, _values.Count);
            Assert.AreEqual(116d, Domain.Calculation.Variances.AbsoluteSumma(_values));
			Assert.AreEqual(23.2d, Domain.Calculation.Variances.Average(_values));
			Assert.AreEqual(9.701, Domain.Calculation.Variances.StandardDeviation(_values), 0.01d);// 0.001d);
			Assert.AreEqual(25.146, Domain.Calculation.Variances.RMS(_values), 0.01d);
	        Assert.AreEqual(
		        Domain.Calculation.Variances.AbsoluteSumma(_values) + Domain.Calculation.Variances.StandardDeviation(_values),
		        Domain.Calculation.Variances.Teleopti(_values), 0.01d);
        }

		[Test]
		public void TeleoptiShouldCalculateCorrect()
		{
			Assert.AreEqual(30d, Domain.Calculation.Variances.Teleopti(new double[]{10,-10}));
		}
    }
}
