using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.DomainTest.Forecasting.Import
{
    [TestFixture]
    public class ForecastsRowExtractorTest
    {
        private IForecastsRowExtractor _target;

        [SetUp]
        public void Setup()
        {
            _target = new ForecastsRowExtractor();
        }

        [Test]
        public void ShouldExtractRowFromString()
        {
            const string rowString = "Insurance,20120326 02:00,20120326 02:15,17,179,0,4.05";
            var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            var row = _target.Extract(rowString, timeZone);

            Assert.That(row.Tasks, Is.EqualTo(17));
            Assert.That(row.TaskTime, Is.EqualTo(179));
            Assert.That(row.AfterTaskTime, Is.EqualTo(0));
            Assert.That(row.Agents, Is.EqualTo(4.05));
            Assert.That(row.LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 26, 2, 0, 0)));
            Assert.That(row.LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 26, 2, 15, 0)));
            Assert.That(row.UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 26, 0, 0, 0)));
            Assert.That(row.UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 26, 0, 15, 0)));
        }

        [Test]
        [ExpectedException(typeof (ValidationException))]
        public void ShouldDetectInvalidTime()
        {
            var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            const string rowString = "Insurance,201203025 02:00,20120325 02:15,17,179,0,4.05";
            _target.Extract(rowString, timeZone);
        }
    }
}
