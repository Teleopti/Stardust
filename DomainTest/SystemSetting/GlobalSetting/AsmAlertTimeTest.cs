using NUnit.Framework;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.DomainTest.SystemSetting.GlobalSetting
{
    [TestFixture]
    public class AsmAlertTimeTest
    {
		 private AsmAlertTime _target;

        [SetUp]
        public void Setup()
        {
			  _target = new AsmAlertTime();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(60, _target.SecondsBeforeChange);
				_target.SecondsBeforeChange = 30;
				Assert.AreEqual(30, _target.SecondsBeforeChange);
        }
    }
}