using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
    [TestFixture]
    public class ForecastsAnalyzeQueryTest
    {
        private IForecastsAnalyzeQuery _target;
        private ISkill _skill;

        [SetUp]
        public void Setup()
        {
            _skill = SkillFactory.CreateSkill("test skill");
            _skill.MidnightBreakOffset = TimeSpan.FromHours(2);
           _target = new ForecastsAnalyzeQuery();
        }

        [Test]
        public void ShouldAnalyzeForecasts()
        {
            var date = new DateOnly(2012, 3, 1);
            var forecastsRows = setUpForecasts();
            var result = _target.Run(forecastsRows, _skill);

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
            var result = _target.Run(forecastsRows,_skill);
            Assert.That(result.WorkloadDayOpenHours.GetOpenHour(date), Is.EqualTo(new TimePeriod(6, 0, 26, 0)));
        }

        [Test]
        public void ShouldHandleSummertime()
        {
            var row = new ForecastsRow
            {
                TaskTime = 170,
                AfterTaskTime = 0,
                Agents = 2,
                LocalDateTimeFrom = new DateTime(2012, 3, 25, 1, 45, 0),
                LocalDateTimeTo = new DateTime(2012, 3, 25, 3, 0, 0),
                SkillName = "Insurance",
                Tasks = 10,
                UtcDateTimeFrom = new DateTime(2012, 3, 25, 0, 45, 0, DateTimeKind.Utc),
                UtcDateTimeTo = new DateTime(2012, 3, 25, 1, 0, 0, DateTimeKind.Utc)
            };
            var result = _target.Run(new[] {row}, _skill);
            Assert.That(result.IntervalLength, Is.EqualTo(TimeSpan.FromTicks(9000000000)));
        }

		[Test]
		public void ShouldGetCorrectPeriod()
		{
			var row1 = new ForecastsRow
			{
				TaskTime = 170,
				AfterTaskTime = 0,
				Agents = 2,
				LocalDateTimeFrom = new DateTime(2012, 3, 1, 12, 0, 0),
				LocalDateTimeTo = new DateTime(2012, 3, 1, 12, 15, 0),
				SkillName = "Insurance",
				Tasks = 10,
				UtcDateTimeFrom = new DateTime(2012, 3, 1, 11, 0, 0, DateTimeKind.Utc),
				UtcDateTimeTo = new DateTime(2012, 3, 1, 11, 15, 0, DateTimeKind.Utc)
			};

			var row2 = new ForecastsRow
			{
				TaskTime = 170,
				AfterTaskTime = 0,
				Agents = 2,
				LocalDateTimeFrom = new DateTime(2012, 3, 2, 23, 45, 0),
				LocalDateTimeTo = new DateTime(2012, 3, 3, 0, 0, 0),
				SkillName = "Insurance",
				Tasks = 10,
				UtcDateTimeFrom = new DateTime(2012, 3, 1, 22, 45, 0, DateTimeKind.Utc),
				UtcDateTimeTo = new DateTime(2012, 3, 1, 23, 0, 0, DateTimeKind.Utc)
			};

			var result = _target.Run(new[] { row1, row2 }, _skill);
			Assert.That(result.Period.StartDate, Is.EqualTo(new DateOnly(2012, 3, 1)));
			Assert.That(result.Period.EndDate, Is.EqualTo(new DateOnly(2012, 3, 2)));
		}

        private static IEnumerable<IForecastsRow> setUpForecasts()
        {
            var row1 = new ForecastsRow
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
            var row2 = new ForecastsRow
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
        
        private static IEnumerable<IForecastsRow> setUpForecastsWithMidnightBreak()
        {
            var row1 = new ForecastsRow
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
            var row2 = new ForecastsRow
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
