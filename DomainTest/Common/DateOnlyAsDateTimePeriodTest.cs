using System;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    public class DateOnlyAsDateTimePeriodTest
    {

        [Test]
        public void VerifyPeriod()
        {
            var sourceZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time"));
            var date = new DateOnly(2000, 1, 1);
            var target = new DateOnlyAsDateTimePeriod(date, sourceZone);
            var expectedPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2).MovePeriod(-sourceZone.BaseUtcOffset);
            
            Assert.AreEqual(expectedPeriod, target.Period());
        }

        [Test]
        public void ResultIsCachedAndLogicOnlyRunsOnce()
        {
            var zone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time"));
            var date = new DateOnly(2000, 1, 1);
            var target = new DateOnlyAsDateTimePeriod(date, zone);
            var realResult = target.Period();
            //hack
            zone.GetType().GetField("_timeZoneInfo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(zone, null);
            var wouldCrashWithoutCacheResult = target.Period();
            
            Assert.AreEqual(realResult, wouldCrashWithoutCacheResult);
        }
    }
}