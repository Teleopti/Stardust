using System;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;

namespace Teleopti.Ccc.DomainTest.Forecasting.Import
{
    [TestFixture]
    public class ForecastsRowTest
    {
        private ForecastsRow _row;

        [Test]
        public void ShouldSetValues()
        {
            var tasks = 12;
            var taskTime = 110.02;
            var afterTaskTime = 121.30;
            var agents = 2;
            var utcDateTimeFrom = new DateTime(2011, 1, 1, 6, 15, 0, DateTimeKind.Utc);
            var utcDateTimeTo = new DateTime(2011, 1, 1, 6, 30, 0, DateTimeKind.Utc);
            var localDateTimeFrom = new DateTime(2011, 1, 1, 8, 15, 0);
            var localDateTimeTo = new DateTime(2011, 1, 1, 8, 30, 0);
            var skillName = "Insurance";
            _row = new ForecastsRow
                       {
                           TaskTime = taskTime,
                           AfterTaskTime = afterTaskTime,
                           Agents = agents,
                           LocalDateTimeFrom = localDateTimeFrom,
                           LocalDateTimeTo = localDateTimeTo,
                           SkillName = skillName,
                           Tasks = tasks,
                           UtcDateTimeFrom = utcDateTimeFrom,
                           UtcDateTimeTo = utcDateTimeTo
                       };

            Assert.That(_row.SkillName, Is.EqualTo(skillName));
            Assert.That(_row.TaskTime, Is.EqualTo(taskTime));
            Assert.That(_row.Tasks, Is.EqualTo(tasks));
            Assert.That(_row.AfterTaskTime, Is.EqualTo(afterTaskTime));
            Assert.That(_row.Agents, Is.EqualTo(agents));
            Assert.That(_row.LocalDateTimeFrom, Is.EqualTo(localDateTimeFrom));
            Assert.That(_row.LocalDateTimeTo, Is.EqualTo(localDateTimeTo));
        }

        [Test]
        public void ShouldDeserializeFromConstructor()
        {
            const string row = "Insurance,20120326 02:00,20120326 02:15,20120326 02:00,20120326 02:15,17,179,0,4.05, ";
            var forecastsRow = new ForecastsRow(row);

            Assert.That(forecastsRow.SkillName, Is.EqualTo("Insurance"));
            Assert.That(forecastsRow.TaskTime, Is.EqualTo(179));
            Assert.That(forecastsRow.Tasks, Is.EqualTo(17));
            Assert.That(forecastsRow.AfterTaskTime, Is.EqualTo(0));
            Assert.That(forecastsRow.Agents, Is.EqualTo(4.05));
            Assert.That(forecastsRow.LocalDateTimeFrom,
                        Is.EqualTo(DateTime.ParseExact("20120326 02:00", "yyyyMMdd HH:mm", CultureInfo.InvariantCulture)));
            Assert.That(forecastsRow.LocalDateTimeTo,
                        Is.EqualTo(DateTime.ParseExact("20120326 02:15", "yyyyMMdd HH:mm", CultureInfo.InvariantCulture)));
            Assert.That(forecastsRow.UtcDateTimeFrom,
                        Is.EqualTo(
                            new DateTime(
                                DateTime.ParseExact("20120326 02:00", "yyyyMMdd HH:mm", CultureInfo.InvariantCulture).
                                    Ticks, DateTimeKind.Utc)));
            Assert.That(forecastsRow.UtcDateTimeTo,
                        Is.EqualTo(
                            new DateTime(
                                DateTime.ParseExact("20120326 02:15", "yyyyMMdd HH:mm", CultureInfo.InvariantCulture).
                                    Ticks, DateTimeKind.Utc)));
            Assert.That(forecastsRow.Shrinkage, Is.Null);
        }
    }
}
