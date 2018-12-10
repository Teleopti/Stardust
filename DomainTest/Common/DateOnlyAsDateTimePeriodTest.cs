using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Common
{
    public class DateOnlyAsDateTimePeriodTest
    {
        [Test]
        public void VerifyPeriod()
        {
            var sourceZone = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");
            var date = new DateOnly(2000, 1, 1);
            var target = new DateOnlyAsDateTimePeriod(date, sourceZone);
            var expectedPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2).MovePeriod(-sourceZone.BaseUtcOffset);
            
            Assert.AreEqual(expectedPeriod, target.Period());
        }

        [Test]
        public void ResultIsCachedAndLogicOnlyRunsOnce()
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById("UTC-02");
            var date = new DateOnly(2000, 1, 1);
            var target = new DateOnlyAsDateTimePeriod(date, zone);
            var realResult = target.Period();
            var wouldCrashWithoutCacheResult = target.Period();
            
            Assert.AreEqual(realResult, wouldCrashWithoutCacheResult);
        }

        [Test]
        public void TestPeriodToReturnTheTimeInProvidedTimeZone()
        {
            var zone = TimeZoneInfoFactory.BrazilTimeZoneInfo();
            var  date = new DateOnlyPeriod(new DateOnly(2013,02,15), new DateOnly( 2013,02,17) );
            IDateOnlyPeriodAsDateTimePeriod  target = new DateOnlyPeriodAsDateTimePeriod( date, zone);
            var targetResult = target.Period(zone);
            Assert.AreEqual(targetResult.StartDateTime,new DateTime(2013,02,15,2,0,0) );
            Assert.AreEqual(targetResult.EndDateTime ,new DateTime(2013,02,18,3,0,0) );
        }

	    [Test]
	    public void ShouldReturnSameDateTimePeriodAsDateOnlyPeriodWould()
	    {
            var sourceZone = TimeZoneInfo.FindSystemTimeZoneById("US Mountain Standard Time");
            var date = new DateOnly(2016, 3, 3);
            var target = new DateOnlyAsDateTimePeriod(date, sourceZone);
		    var compare = new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(date, date), sourceZone);
			Assert.AreEqual(compare.Period(), target.Period());
        }	    
    }
}