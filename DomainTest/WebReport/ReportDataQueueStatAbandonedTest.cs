using NUnit.Framework;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.DomainTest.WebReport
{
    [TestFixture]
    public class ReportDataQueueStatAbandonedTest
    {
        private ReportDataQueueStatAbandoned _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReportDataQueueStatAbandoned("20120101-20121010", 100, 10, 12);
        }

        [Test]
        public void ShouldSetValue()
        {
            Assert.That(_target.CallsAnswered, Is.EqualTo(100));
            Assert.That(_target.CallsAbandoned, Is.EqualTo(10));
            Assert.That(_target.PeriodNumber, Is.EqualTo(12));
            Assert.That(_target.Period, Is.EqualTo("20120101-20121010"));
        }
    }
}
