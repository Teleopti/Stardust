using NUnit.Framework;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.DomainTest.WebReport
{
    [TestFixture]
    public class ReportControlSkillGetTest
    {
        private ReportControlSkillGet _target;

        [SetUp]
        public void Setup()
        {
            _target = new ReportControlSkillGet
                          {
                              Id = 1,
                              Name = "skill"
                          };
        }

        [Test]
        public void ShouldSetValue()
        {
            Assert.That(_target.Id, Is.EqualTo(1));
            Assert.That(_target.Name, Is.EqualTo("skill"));
        }
    }
}
