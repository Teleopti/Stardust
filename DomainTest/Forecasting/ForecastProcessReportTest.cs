#region Imports

using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

#endregion

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests ForecastProcessReport
    /// </summary>
    /// <remarks>
    /// Created by: Sachintha Weerasekara
    /// Created date: 2008-10-01
    /// </remarks>
    [TestFixture]
    public class ForecastProcessReportTest
    {
        private ForecastProcessReport target;
        private IList<DateOnlyPeriod> periods;

        [SetUp]
        public void Setup()
        {
            periods = new List<DateOnlyPeriod>();
            var period = new DateOnlyPeriod(DateOnly.Today, DateOnly.Today.AddDays(1));
            periods.Add(period);
            target = new ForecastProcessReport(periods);
        }

        [Test]
        public void VerifyPeriodCollection()
        {
            var expextedValue = target.PeriodCollection;
            var actualValue = periods;

            Assert.AreSame(expextedValue, actualValue);
        }
    }
}
