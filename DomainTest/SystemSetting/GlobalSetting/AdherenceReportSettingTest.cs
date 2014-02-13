using System;
using NUnit.Framework;
using SharpTestsEx;
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
        public void VerifyMapToMatrixContract()
        {
					_target.CalculationMethod = AdherenceReportSettingCalculationMethod.ReadyTimeVSContractScheduleTime;
	        _target.AdherenceIdForReport().Should().Be.EqualTo(3);
        }

	    [Test]
	    public void VerifyMapToMatrixScheduleTime()
	    {
				_target.CalculationMethod = AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledTime;
				_target.AdherenceIdForReport().Should().Be.EqualTo(2);
	    }

			[Test]
			public void VerifyMapToMatrixReadyTime()
			{
				_target.CalculationMethod = AdherenceReportSettingCalculationMethod.ReadyTimeVSScheduledReadyTime;
				_target.AdherenceIdForReport().Should().Be.EqualTo(1);
			}

			[Test]
			public void VerifyMapToIllegal()
			{
				_target.CalculationMethod = (AdherenceReportSettingCalculationMethod) 47;
				Assert.Throws<NotSupportedException>(() => _target.AdherenceIdForReport());
			}
    }
}