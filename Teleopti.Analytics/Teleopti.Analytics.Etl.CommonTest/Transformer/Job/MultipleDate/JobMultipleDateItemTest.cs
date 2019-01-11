using System;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;

namespace Teleopti.Analytics.Etl.CommonTest.Transformer.Job.MultipleDate
{
    [TestFixture]
    public class JobMultipleDateItemTest
    {
        private IJobMultipleDateItem _jobMultipleDateItem;
        private DateTime _dt1;
        private DateTime _dt2;
        private TimeZoneInfo _TimeZoneInfo;

        [SetUp]
        public void Setup()
        {
            TimeZoneInfo wEuropeStdTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            _TimeZoneInfo = (wEuropeStdTimeZoneInfo);
            _dt1 = new DateTime(2006, 1, 1);
            _dt2 = new DateTime(2006, 2, 1);
            _jobMultipleDateItem = new JobMultipleDateItem(DateTimeKind.Local, _dt1, _dt2, wEuropeStdTimeZoneInfo);

        }
        [Test]
        public void VerifyProperties()  
        {
            // Local date
            Assert.AreEqual(_dt1, _jobMultipleDateItem.StartDateLocal);
            Assert.AreEqual(_dt2, _jobMultipleDateItem.EndDateLocal);

            // Utc date
            Assert.AreEqual(TimeZoneInfo.ConvertTimeToUtc(_dt1, _TimeZoneInfo), _jobMultipleDateItem.StartDateUtc);
            Assert.AreEqual(TimeZoneInfo.ConvertTimeToUtc(_dt2, _TimeZoneInfo), _jobMultipleDateItem.EndDateUtc);

            // Utc floor and ceiling
            Assert.AreEqual(TimeZoneInfo.ConvertTimeToUtc(_dt1, _TimeZoneInfo).Date, _jobMultipleDateItem.StartDateUtcFloor);
            Assert.AreEqual(TimeZoneInfo.ConvertTimeToUtc(_dt2, _TimeZoneInfo).AddDays(1).AddMilliseconds(-1),
                            _jobMultipleDateItem.EndDateUtcCeiling);
        }
    }
}