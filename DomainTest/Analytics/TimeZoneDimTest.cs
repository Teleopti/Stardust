using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.DomainTest.Analytics
{
    [TestFixture]
    public class TimeZoneDimTest
    {
        private TimeZoneDim _target1;
        private TimeZoneDim _target2;
        private int _martId1;
        private bool _isDefault;
        private int _utcConversion1;
        private int _utcConversionDst1;
        private int _martId2;
        private int _utcConversion2;
        private int _utcConversionDst2;
        private TimeZoneInfo _swedenTimeZone;
        private TimeZoneInfo _utcTimeZone;

        [SetUp]
        public void Setup()
        {
            _swedenTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            _utcConversion1 = Convert.ToInt32(_swedenTimeZone.BaseUtcOffset.TotalMinutes);
            _utcConversionDst1 = getUtcConversionIncludedDaylightSaving(_swedenTimeZone);
            _martId1 = 1;
            _isDefault = true;
            _target1 = new TimeZoneDim(_martId1, _swedenTimeZone.Id, _swedenTimeZone.DisplayName, _isDefault, _utcConversion1, _utcConversionDst1, false);

            _utcTimeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
            _martId2 = -1;
            _utcConversion2 = Convert.ToInt32(_utcTimeZone.BaseUtcOffset.TotalMinutes);
            _utcConversionDst2 = getUtcConversionIncludedDaylightSaving(_utcTimeZone);
            _target2 = new TimeZoneDim(_utcTimeZone, false, true);
        }

        [Test]
        public void VerifyTarget1()
        {
            Assert.AreEqual(_martId1, _target1.MartId);
            Assert.AreEqual(_swedenTimeZone.Id, _target1.TimeZoneCode);
            Assert.AreEqual(_swedenTimeZone.DisplayName, _target1.TimeZoneName);
            Assert.AreEqual(_isDefault, _target1.IsDefaultTimeZone);
            Assert.AreEqual(_utcConversion1, _target1.UtcConversion);
            Assert.AreEqual(_utcConversionDst1, _target1.UtcConversionDst);
        }

        [Test]
        public void VerifyTarget2()
        {
            Assert.AreEqual(_martId2, _target2.MartId);
            Assert.AreEqual(_utcTimeZone.Id, _target2.TimeZoneCode);
            Assert.AreEqual(_utcTimeZone.DisplayName, _target2.TimeZoneName);
            Assert.AreEqual(false, _target2.IsDefaultTimeZone);
            Assert.AreEqual(_utcConversion2, _target2.UtcConversion);
            Assert.AreEqual(_utcConversionDst2, _target2.UtcConversionDst);
        }

		[Test]
		public void ShouldBeDefaultAndUtcInUse()
		{
			_target1 = new TimeZoneDim(_swedenTimeZone, true, true);
			Assert.That(_target1.IsDefaultTimeZone, Is.True);
			Assert.That(_target1.IsUtcInUse, Is.True);
		}

        private static int getUtcConversionIncludedDaylightSaving(TimeZoneInfo timeZone)
        {
            int retVal = 0;

            TimeZoneInfo.AdjustmentRule[] adjustmentRules = timeZone.GetAdjustmentRules();
            if (adjustmentRules.Length > 0)
            {
                //Get daylight saving minutes from the first adjustment rule (should be the same for all rules of a time zone)
                retVal = Convert.ToInt32(adjustmentRules[0].DaylightDelta.TotalMinutes);
            }

            int utcConversion = Convert.ToInt32(timeZone.BaseUtcOffset.TotalMinutes);
            return retVal + utcConversion;
        }
    }
}
