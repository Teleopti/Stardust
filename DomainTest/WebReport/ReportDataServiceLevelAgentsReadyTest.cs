using NUnit.Framework;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.DomainTest.WebReport
{
    [TestFixture]
    public class ReportDataServiceLevelAgentsReadyTest
    {
        private ReportDataServiceLevelAgentsReady _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReportDataServiceLevelAgentsReady("20120101-20121010", 100, 10, 80, 12);
        }

        [Test]
        public void ShouldSetValue()
        {
            Assert.That(_target.ScheduledAgentsReady, Is.EqualTo(100));
            Assert.That(_target.AgentsReady, Is.EqualTo(10));
            Assert.That(_target.ServiceLevel, Is.EqualTo(80));
            Assert.That(_target.PeriodNumber, Is.EqualTo(12));
            Assert.That(_target.Period, Is.EqualTo("20120101-20121010"));
        }
    }
}
