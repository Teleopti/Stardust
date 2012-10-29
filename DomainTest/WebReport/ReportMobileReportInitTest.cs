using NUnit.Framework;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.DomainTest.WebReport
{
    [TestFixture]
    public class ReportMobileReportInitTest
    {
        private ReportMobileReportInit _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReportMobileReportInit
                          {
                              IntervalFrom = 1,
                              IntervalTo = 2,
                              Scenario = 1,
                              ServiceLevelCalculationId = 12,
                              SkillSet = "phone",
                              TimeZone = 1,
                              WorkloadSet = "workload"
                          };
        }

        [Test]
        public void ShouldSetValue()
        {
            Assert.That(_target.IntervalFrom, Is.EqualTo(1));
            Assert.That(_target.IntervalTo, Is.EqualTo(2));
            Assert.That(_target.Scenario, Is.EqualTo(1));
            Assert.That(_target.ServiceLevelCalculationId, Is.EqualTo(12));
            Assert.That(_target.SkillSet, Is.EqualTo("phone"));
            Assert.That(_target.WorkloadSet, Is.EqualTo("workload"));
            Assert.That(_target.TimeZone, Is.EqualTo(1));
        }
    }
}
