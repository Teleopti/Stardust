using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;

namespace Teleopti.Ccc.DomainTest.Forecasting.Import
{
    [TestFixture]
    public class ForecastsFileRowTest
    {
        private ForecastsFileRow _row;

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
            _row = new ForecastsFileRow
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
    }
}
