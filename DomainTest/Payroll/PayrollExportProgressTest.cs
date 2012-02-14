using System;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Payroll
{
    [TestFixture]
    public class PayrollExportProgressTest
    {
        private IJobResultProgress _target;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Interfaces.Domain.JobResultProgress.set_Message(System.String)"), SetUp]
        public void Setup()
        {
            _target = new JobResultProgress {Message = @"test", JobResultId = Guid.NewGuid(), Percentage = 2};
        }

        [Test]
        public void ShouldGetPropertyValues()
        {
            Assert.AreNotEqual(Guid.Empty,_target.JobResultId);
            Assert.AreEqual("test",_target.Message);
            Assert.AreEqual(2,_target.Percentage);
        }
    }
}
