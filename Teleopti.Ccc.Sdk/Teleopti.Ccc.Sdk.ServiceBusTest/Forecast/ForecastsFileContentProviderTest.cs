using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
    [TestFixture]
    public class ForecastsFileContentProviderTest
    {
        private IForecastsFileContentProvider _target;
        private byte[] _fileContent;
        private ICccTimeZoneInfo _timeZone;

        [SetUp]
        public void Setup()
        {
            _timeZone = new CccTimeZoneInfo(TimeZoneInfo.Utc);
            _target = new ForecastsFileContentProvider(new ForecastsRowExtractor());
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void ShouldHandleWrongFileStream()
        {
            _fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,");

            _target.LoadContent(_fileContent, _timeZone, TimeSpan.Zero);
        }

        [Test]
        public void ShouldLoadContentFromFileStream()
        {
            _fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,4.05");
            var forecastRow = _target.LoadContent(_fileContent, _timeZone, TimeSpan.Zero).First();

            Assert.That(forecastRow.TaskTime, Is.EqualTo(179));
            Assert.That(forecastRow.AfterTaskTime, Is.EqualTo(0));
            Assert.That(forecastRow.Agents, Is.EqualTo(4.05));
            Assert.That(forecastRow.LocalDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 1, 12, 45, 0)));
            Assert.That(forecastRow.LocalDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 1, 13, 0, 0)));
            Assert.That(forecastRow.SkillName, Is.EqualTo("Insurance"));
            Assert.That(forecastRow.Tasks, Is.EqualTo(17));
            Assert.That(forecastRow.UtcDateTimeFrom, Is.EqualTo(new DateTime(2012, 3, 1, 12, 45, 0, DateTimeKind.Utc)));
            Assert.That(forecastRow.UtcDateTimeTo, Is.EqualTo(new DateTime(2012, 3, 1, 13, 0, 0, DateTimeKind.Utc)));
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void ShouldDetectInvalidTime()
        {
            var timeZone = new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _fileContent = Encoding.UTF8.GetBytes("Insurance,20120325 02:00,20120325 02:15,17,179,0,4.05");
            _target.LoadContent(_fileContent, timeZone, TimeSpan.Zero);
        }

        [Test]
        public void ShouldHandleTimeWithMidnightBreak()
        {
            
        }
    }
}
