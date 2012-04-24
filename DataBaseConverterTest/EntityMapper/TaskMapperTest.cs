using System;
using NUnit.Framework;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.DatabaseConverterTest.Helpers;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;

namespace Teleopti.Ccc.DatabaseConverterTest.EntityMapper
{
    /// <summary>
    /// Tests for TaskMapper
    /// </summary>
    [TestFixture]
    public class TaskMapperTest : MapperTest<global::Domain.ForecastData>
    {
        double _tasks = 9.11;
        TimeSpan _avgTaskTime = new TimeSpan(0, 0, 120);
        TimeSpan _avgAfterTaskTime = new TimeSpan(0, 0, 10);

        /// <summary>
        /// Determines whether this type [can validate number of properties].
        /// </summary>
        [Test]
        public void CanValidateNumberOfProperties()
        {
            Assert.AreEqual(4, PropertyCounter.CountProperties(typeof (Task)));
        }


        /// <summary>
        /// Determines whether this instance [can map task6 X].
        /// </summary>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-10-19
        /// </remarks>
        [Test]
        public void CanMapTask6X()
        {
            TaskMapper taskMap = new TaskMapper();

            global::Domain.ForecastData oldForecastData = new global::Domain.ForecastData(15, (int)_tasks, (int)_avgTaskTime.TotalSeconds * (int)_tasks, (int)_avgAfterTaskTime.TotalSeconds * (int)_tasks);

            Task newTask = taskMap.Map(oldForecastData);

            Assert.AreEqual((int)_tasks, newTask.Tasks);
            Assert.AreEqual(_avgTaskTime, newTask.AverageTaskTime);
            Assert.AreEqual(_avgAfterTaskTime, newTask.AverageAfterTaskTime);
        }

        [Test]
        public void CanMapAverageTimesEqualZero()
        {
            TaskMapper taskMap = new TaskMapper();

            global::Domain.ForecastData oldForecastData = new global::Domain.ForecastData(15, (int)_tasks, (int)_avgTaskTime.TotalSeconds * 0, (int)_avgAfterTaskTime.TotalSeconds * 0);

            Task newTask = taskMap.Map(oldForecastData);

            Assert.AreEqual((int)_tasks, newTask.Tasks);
            Assert.AreEqual(TimeSpan.FromSeconds(1), newTask.AverageTaskTime);
            Assert.AreEqual(TimeSpan.FromSeconds(1), newTask.AverageAfterTaskTime);
        }

        protected override int NumberOfPropertiesToConvert
        {
            get { return 6; }
        }
    }
}