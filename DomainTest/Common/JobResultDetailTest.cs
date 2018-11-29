using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class JobResultDetailTest
    {
        private IJobResultDetail target;

        [Test]
        public void ShouldHandleErrorWithoutInnerException()
        {
            target = new JobResultDetail(DetailLevel.Error, "my error", DateTime.UtcNow, new Exception("a test"));

            Assert.IsNull(target.InnerExceptionMessage);
            Assert.IsNull(target.InnerExceptionStackTrace);
            Assert.IsTrue(string.IsNullOrEmpty(target.ExceptionStackTrace));
            Assert.IsTrue(target.ExceptionMessage.Equals("a test"));
        }

        [Test]
        public void ShouldHandleErrorWithInnerException()
        {
            target = new JobResultDetail(DetailLevel.Error, "my error", DateTime.UtcNow, new Exception("a test", new Exception("inner exc")));

            Assert.IsTrue(target.InnerExceptionMessage.Equals("inner exc"));
            Assert.IsTrue(string.IsNullOrEmpty(target.InnerExceptionStackTrace));
            Assert.IsTrue(string.IsNullOrEmpty(target.ExceptionStackTrace));
            Assert.IsTrue(target.ExceptionMessage.Equals("a test"));
        }

        [Test]
        public void ShouldHandleErrorWithoutException()
        {
            var timestamp = DateTime.UtcNow;
            target = new JobResultDetail(DetailLevel.Warning, "my warning", timestamp, null);

            Assert.IsNull(target.ExceptionMessage);
            Assert.IsNull(target.ExceptionStackTrace);
            Assert.IsNull(target.InnerExceptionMessage);
            Assert.IsNull(target.InnerExceptionStackTrace);
            Assert.IsTrue(target.Timestamp.Equals(timestamp));
            Assert.IsTrue(target.DetailLevel.Equals(DetailLevel.Warning));
            Assert.IsTrue(target.Message.Equals("my warning"));
        }

        [Test]
        public void ShouldHaveDefaultConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(target.GetType(),true));
        }
    }
}
