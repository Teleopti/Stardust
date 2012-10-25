using NUnit.Framework;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.DomainTest.WebReport
{
    [TestFixture]
    public class ReportDataForecastVersusActualWorkloadTest
    {
        private ReportDataForecastVersusActualWorkload _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReportDataForecastVersusActualWorkload("20120101-20121010", 100, 80, 12);
        }

        [Test]
        public void ShouldSetValue()
        {
            Assert.That(_target.ForecastedCalls, Is.EqualTo(100));
            Assert.That(_target.OfferedCalls, Is.EqualTo(80));
            Assert.That(_target.PeriodNumber, Is.EqualTo(12));
            Assert.That(_target.Period, Is.EqualTo("20120101-20121010"));
        }
    }
}
