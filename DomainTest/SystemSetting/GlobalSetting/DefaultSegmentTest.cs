using NUnit.Framework;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.DomainTest.SystemSetting.GlobalSetting
{
    [TestFixture]
    public class DefaultSegmentTest
    {
        private DefaultSegment _target;

        [SetUp]
        public void Setup()
        {
            _target = new DefaultSegment();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(15, _target.SegmentLength);
            _target.SegmentLength = 30;
            Assert.AreEqual(30, _target.SegmentLength);
        }
    }
}