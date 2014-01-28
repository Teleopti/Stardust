using NUnit.Framework;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.DomainTest.SystemSetting.GlobalSetting
{
    [TestFixture]
    public class AdherenceReportSettingTest
    {
        private AdherenceReportSetting _target;

        [SetUp]
        public void Setup()
        {
            _target = new AdherenceReportSetting();
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledReadyTime, _target.CalculationMethod);
            _target.CalculationMethod =
                AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledTime;
            Assert.AreEqual(AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledTime, _target.CalculationMethod);
        }

        [Test]
        public static void VerifyMapToMatrix()
        {
            Assert.AreEqual(1, (int)AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledReadyTime);
            Assert.AreEqual(2, (int)AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledTime);
            Assert.AreEqual(3, (int)AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime);
        }
    }
}