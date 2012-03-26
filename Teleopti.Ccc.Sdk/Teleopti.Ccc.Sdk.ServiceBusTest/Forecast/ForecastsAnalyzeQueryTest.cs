using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Sdk.ServiceBus.Forecast;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Forecast
{
    [TestFixture]
    public class ForecastsAnalyzeQueryTest
    {
        private IForecastsAnalyzeQuery _target;

        [SetUp]
        public void Setup()
        {
           _target = new ForecastsAnalyzeQuery();
        }

        [Test]
        public void ShouldAnalyzeForecasts()
        {
            var date = new DateOnly(2012, 3, 1);
            var forecastsRows = setUpForecasts();
            var result = _target.Run(forecastsRows,TimeSpan.Zero);

            Assert.That(result.ErrorMessage, Is.Null);
            Assert.That(result.Succeeded, Is.True);
            Assert.That(result.SkillName, Is.EqualTo("Insurance"));
            Assert.That(result.Period, Is.EqualTo(new DateOnlyPeriod(date, date)));
            Assert.That(result.IntervalLength, Is.EqualTo(TimeSpan.FromTicks(9000000000)));
            Assert.That(result.WorkloadDayOpenHours.GetOpenHour(date),Is.EqualTo(new TimePeriod(2, 0, 2, 30)));
            Assert.That(result.ForecastFileContainer.GetForecastsRows(date), Is.EqualTo(forecastsRows));
        }

        [Test]
        public void ShouldHandleOpenHoursWithMidnightBreak()
        {
            var forecastsRows = setUpForecastsWithMidnightBreak();
            var date = new DateOnly(2012, 3, 1);
            var result = _target.Run(forecastsRows,TimeSpan.FromHours(2));
            Assert.That(result.WorkloadDayOpenHours.GetOpenHour(date), Is.EqualTo(new TimePeriod(6, 0, 26, 0)));
        }

        private static IEnumerable<IForecastsFileRow> setUpForecasts()
        {
            var row1 = new ForecastsFileRow
            {
                TaskTime = 170,
                AfterTaskTime = 0,
                Agents = 2,
                LocalDateTimeFrom = new DateTime(2012, 3, 1, 2, 0, 0),
                LocalDateTimeTo = new DateTime(2012, 3, 1, 2, 15, 0),
                SkillName = "Insurance",
                Tasks = 10,
                UtcDateTimeFrom = new DateTime(2012, 3, 1, 1, 0, 0, DateTimeKind.Utc),
                UtcDateTimeTo = new DateTime(2012, 3, 1, 1, 15, 0, DateTimeKind.Utc)
            };
            var row2 = new ForecastsFileRow
            {
                TaskTime = 170,
                AfterTaskTime = 0,
                Agents = 2,
                LocalDateTimeFrom = new DateTime(2012, 3, 1, 2, 15, 0),
                LocalDateTimeTo = new DateTime(2012, 3, 1, 2, 30, 0),
                SkillName = "Insurance",
                Tasks = 10,
                UtcDateTimeFrom = new DateTime(2012, 3, 1, 1, 15, 0, DateTimeKind.Utc),
                UtcDateTimeTo = new DateTime(2012, 3, 1, 1, 30, 0, DateTimeKind.Utc)
            };
            return new[] {row1, row2};
        } 
        
        private IEnumerable<IForecastsFileRow> setUpForecastsWithMidnightBreak()
        {
            var row1 = new ForecastsFileRow
            {
                TaskTime = 170,
                AfterTaskTime = 0,
                Agents = 2,
                LocalDateTimeFrom = new DateTime(2012, 3, 1, 6, 0, 0),
                LocalDateTimeTo = new DateTime(2012, 3, 1, 6, 15, 0),
                SkillName = "Insurance",
                Tasks = 10,
                UtcDateTimeFrom = new DateTime(2012, 3, 1, 6, 0, 0, DateTimeKind.Utc),
                UtcDateTimeTo = new DateTime(2012, 3, 1, 6, 15, 0, DateTimeKind.Utc)
            };
            var row2 = new ForecastsFileRow
            {
                TaskTime = 170,
                AfterTaskTime = 0,
                Agents = 2,
                LocalDateTimeFrom = new DateTime(2012, 3, 2, 1, 45, 0),
                LocalDateTimeTo = new DateTime(2012, 3, 2, 2, 00, 0),
                SkillName = "Insurance",
                Tasks = 10,
                UtcDateTimeFrom = new DateTime(2012, 3, 2, 1, 45, 0, DateTimeKind.Utc),
                UtcDateTimeTo = new DateTime(2012, 3, 2, 2, 00, 0, DateTimeKind.Utc)
            };
            return new[] {row1, row2};
        }
    }
}
