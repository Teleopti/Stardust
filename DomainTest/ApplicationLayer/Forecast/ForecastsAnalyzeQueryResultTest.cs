using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Forecast
{
    [TestFixture]
    public class ForecastsAnalyzeQueryResultTest
    {
        private IForecastsAnalyzeQueryResult _target;

        [SetUp]
        public void Setup()
        {
            _target = new ForecastsAnalyzeQueryResult();
        }

        [Test]
        public void ShouldSetResultValues()
        {
            var dateTime = new DateOnly(2012, 3, 1);
            _target.ErrorMessage = "Something wrong!";
            _target.IntervalLength = TimeSpan.FromTicks(9000000);
            _target.Period = new DateOnlyPeriod(dateTime, dateTime);
            _target.SkillName = "Insurance";
            var openHours = new WorkloadDayOpenHoursContainer();
            openHours.AddOpenHour(dateTime, new TimePeriod(12, 0, 12, 15));
            var forecasts = new ForecastFileContainer();
            forecasts.AddForecastsRow(dateTime, new ForecastsRow
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
                                        });
            _target.WorkloadDayOpenHours = openHours;
            _target.ForecastFileContainer = forecasts;

            Assert.That(_target.ErrorMessage, Is.EqualTo("Something wrong!"));
            Assert.That(_target.IntervalLength, Is.EqualTo(TimeSpan.FromTicks(9000000)));
            Assert.That(_target.ForecastFileContainer, Is.EqualTo(forecasts));
            Assert.That(_target.WorkloadDayOpenHours, Is.EqualTo(openHours));
        }

        [Test]
        public void ShouldShowFailedWhenThereHasErrorMessage()
        {
            _target.ErrorMessage = "Something wrong!";

            Assert.That(_target.Succeeded, Is.False);
        } 
        
        [Test]
        public void ShouldShowSucceededWhenThereHasNoErrorMessage()
        {
            _target.ErrorMessage = null;

            Assert.That(_target.Succeeded, Is.True);
        }
    }
}
