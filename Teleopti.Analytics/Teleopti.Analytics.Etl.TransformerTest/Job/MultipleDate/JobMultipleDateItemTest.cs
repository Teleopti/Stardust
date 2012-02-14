using System;
using NUnit.Framework;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.TransformerTest.Job.MultipleDate
{
    [TestFixture]
    public class JobMultipleDateItemTest
    {
        private IJobMultipleDateItem _jobMultipleDateItem;
        private DateTime _dt1;
        private DateTime _dt2;
        private ICccTimeZoneInfo _cccTimeZoneInfo;

        [SetUp]
        public void Setup()
        {
            TimeZoneInfo wEuropeStdTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            _cccTimeZoneInfo = new CccTimeZoneInfo(wEuropeStdTimeZoneInfo);
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
            Assert.AreEqual(_cccTimeZoneInfo.ConvertTimeToUtc(_dt1), _jobMultipleDateItem.StartDateUtc);
            Assert.AreEqual(_cccTimeZoneInfo.ConvertTimeToUtc(_dt2), _jobMultipleDateItem.EndDateUtc);

            // Utc floor and ceiling
            Assert.AreEqual(_cccTimeZoneInfo.ConvertTimeToUtc(_dt1).Date, _jobMultipleDateItem.StartDateUtcFloor);
            Assert.AreEqual(_cccTimeZoneInfo.ConvertTimeToUtc(_dt2).Date.AddDays(1).AddMilliseconds(-1),
                            _jobMultipleDateItem.EndDateUtcCeiling);
        }
    }
}