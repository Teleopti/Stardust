using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Time;

namespace Teleopti.Ccc.DomainTest.Forecasting.Import
{
    [TestFixture]
    public class ForecastsFileRowCreatorTest
    {
        private const string fileContentWithAgent = "Insurance,20120301 12:45,20120301 13:00,17,179,0,4.75";

        [Test]
        public void ShouldCreateFromRow()
        {
            var forecastRow = ForecastsFileRowCreator.Create(new CsvFileRow(fileContentWithAgent),
                                                             new CccTimeZoneInfo(
                                                                 TimeZoneInfo.FindSystemTimeZoneById(
                                                                     "W. Europe Standard Time")));
            var row = new ForecastsFileRow
                          {
                              TaskTime = 179,
                              AfterTaskTime = 0,
                              Agents = 4.75,
                              LocalDateTimeFrom = new DateTime(2012, 3, 1, 12, 45, 0),
                              LocalDateTimeTo = new DateTime(2012, 3, 1, 13, 0, 0),
                              UtcDateTimeFrom = new DateTime(2012, 3, 1, 11, 45, 0, DateTimeKind.Utc),
                              UtcDateTimeTo = new DateTime(2012, 3, 1, 12, 0, 0, DateTimeKind.Utc),
                              SkillName = "Insurance",
                              Tasks = 17
                          };

            Assert.That(forecastRow.Equals(row), Is.True);
        }

        [Test]
        public void ShouldCheckValidColumn()
        {
            Assert.That(ForecastsFileRowCreator.IsFileColumnValid(new CsvFileRow(fileContentWithAgent)), Is.True);

            var row = new CsvFileRow("Insurance,20120301 12:45,20120301 13:00,17,179");
            Assert.That(ForecastsFileRowCreator.IsFileColumnValid(row), Is.False);
        }
    }
}
