using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
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
            _target = new ForecastsFileContentProvider();
        }

        [Test]
        [ExpectedException(typeof(ValidationException))]
        public void ShouldHandleWrongFileStream()
        {
            _fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,");

            _target.LoadContent(_fileContent, _timeZone);
        }

        [Test]
        public void ShouldLoadContentFromFileStream()
        {
            _fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,4.05");
            var row = new ForecastsFileRow
            {
                TaskTime = 179,
                AfterTaskTime = 0,
                Agents = 4.05,
                LocalDateTimeFrom = new DateTime(2012, 3, 1, 12, 45, 0),
                LocalDateTimeTo = new DateTime(2012, 3, 1, 13, 0, 0),
                SkillName = "Insurance",
                Tasks = 17,
                UtcDateTimeFrom = new DateTime(2012, 3, 1, 12, 45, 0, DateTimeKind.Utc),
                UtcDateTimeTo = new DateTime(2012, 3, 1, 13, 0, 0, DateTimeKind.Utc)
            };
            _target.LoadContent(_fileContent, _timeZone);
            
            Assert.That(_target.Forecasts.First(), Is.EqualTo(row));
        }

        [Test]
        public void ShouldAnalyzeFileContent()
        {
            var date = new DateOnly(2012, 3, 1);
            _fileContent = Encoding.UTF8.GetBytes("Insurance,20120301 12:45,20120301 13:00,17,179,0,4.05");
            var row = new ForecastsFileRow
            {
                TaskTime = 179,
                AfterTaskTime = 0,
                Agents = 4.05,
                LocalDateTimeFrom = new DateTime(2012, 3, 1, 12, 45, 0),
                LocalDateTimeTo = new DateTime(2012, 3, 1, 13, 0, 0),
                SkillName = "Insurance",
                Tasks = 17,
                UtcDateTimeFrom = new DateTime(2012, 3, 1, 12, 45, 0, DateTimeKind.Utc),
                UtcDateTimeTo = new DateTime(2012, 3, 1, 13, 0, 0, DateTimeKind.Utc)
            };
            _target.LoadContent(_fileContent, _timeZone);
            var result = _target.Analyze();

            Assert.That(result.ErrorMessage, Is.Null);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.SkillName, Is.EqualTo("Insurance"));
            Assert.That(result.Period, Is.EqualTo(new DateOnlyPeriod(date, date)));
            Assert.That(result.IntervalLengthTicks, Is.EqualTo(9000000000));
            Assert.That(result.WorkloadDayOpenHours.GetOpenHour(date), Is.EqualTo(new TimePeriod(12,45,13,0)));
            Assert.That(result.ForecastFileContainer.GetForecastsRows(date), Is.EqualTo(new []{row}));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ShouldHandleWrongStates()
        {
            _target.Analyze();
        }
    }
}
