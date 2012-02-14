using System;
using System.Collections.Generic;
using Domain;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Forecasting.Template;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests TaskPeriodMapper
    /// </summary>
    [TestFixture]
    public class TaskPeriodMapperTest : MapperTest<ForecastData>
    {

        protected override int NumberOfPropertiesToConvert
        {
            get { return 6; }
        }


        /// <summary>
        /// Determines whether this instance [can map task layer].
        /// </summary>
        [Test]
        public void CanMapTaskLayer()
        {
            double tasks = 9;
            int totalTime = 122*(int)tasks;
            int totalAfterCallWorkTime = 13 * (int)tasks;
            int interval = 26; //06:30
            int intervalLength = 15;
            ISkill skill = SkillFactory.CreateSkill("TestSkill");

            IDictionary<ForecastData, Forecast> parents = new Dictionary<ForecastData, Forecast>();
            MappedObjectPair mapped = new MappedObjectPair();
            Workload workload = new Workload(skill);

            //CreateProjection old forecast
            Forecast oldForecast = new Forecast(12, "Name", "Desc");
            //CreateProjection old forecaste data
            ForecastData oldForecastData = new ForecastData(interval, (int)tasks, totalTime, totalAfterCallWorkTime);
            parents[oldForecastData] = oldForecast;
            ObjectPairCollection<Forecast, IWorkload> pair = new ObjectPairCollection<Forecast, IWorkload>();
            pair.Add(oldForecast, workload);
            mapped.Workload = pair;

            //CreateProjection forecast date
            DateTime forecastDate = new DateTime(2006, 12, 15, 0, 0,0, DateTimeKind.Utc);

            //CreateProjection Expected date Intervall=26	= 06:30 Will be -> '2006-12-15 06:30'
            TimeSpan expectedIntervallTime = new TimeSpan(6, 30, 0);


            TaskPeriodMapper target = new TaskPeriodMapper(mapped, new CccTimeZoneInfo(TimeZoneInfo.Utc), forecastDate, intervalLength);

            //Map the tasklayer
            TemplateTaskPeriod newTaskLayer = target.Map(oldForecastData);

            Assert.AreEqual(forecastDate.Add(expectedIntervallTime), newTaskLayer.Period.StartDateTime);
            Assert.AreEqual(forecastDate.Add(expectedIntervallTime.Add(new TimeSpan(0, 15, 0))), newTaskLayer.Period.EndDateTime);

            Assert.AreEqual(tasks, newTaskLayer.Task.Tasks);
            Assert.AreEqual(122, newTaskLayer.Task.AverageTaskTime.TotalSeconds);
            Assert.AreEqual(13, newTaskLayer.Task.AverageAfterTaskTime.TotalSeconds);

        }
    }
}