using System;
using System.Collections.Generic;
using System.Globalization;
using Domain;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter2;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverterTest.CollectionConverter2
{
    /// <summary>
    /// Tests for ClassName
    /// </summary>
    [TestFixture]
    public class WorkloadConverterTest
    {
        /// <summary>
        /// Determines whether this instance [can convert old object].
        /// </summary>
        [Test]
        public void CanConvertOldObject()
        {
            //create a forecastQueuesourceMap list
            ObjectPairCollection<int, QueueSource> queueSourceDic = new ObjectPairCollection<int, QueueSource>();

            //****THIS WILL MAP old queueId(99) to foreCastId(12)
            int queueId = 99;
            int foreCastId = 12;

            IDictionary<int, IntegerPair> queueSourceMap = new Dictionary<int, IntegerPair>();
            IntegerPair forecastQueueMap = new IntegerPair(foreCastId, queueId);
            queueSourceMap.Add(1, forecastQueueMap); //Key 1 has no meaning

            QueueSource qs =
                new QueueSource(queueId.ToString(CultureInfo.CurrentCulture),
                                queueId.ToString(CultureInfo.CurrentCulture), queueId);

            //The queue is the key to the QueueSource
            queueSourceDic.Add(queueId, qs);

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.QueueSource = queueSourceDic;

            MockRepository mocks = new MockRepository();

            //1. Setup
            IUnitOfWork uowMock = mocks.CreateMock<IUnitOfWork>();
            IRepository<Workload> workloadRepositoryMock = mocks.CreateMock<IRepository<Workload>>();
            IDictionary<IntegerDateKey, ForecastDay> forecastDayDictionary =
                new Dictionary<IntegerDateKey, ForecastDay>();

            ForecastData oldForecastData = new ForecastData(32, 12, 33, 23);
            IDictionary<int, ForecastData> dataDict = new Dictionary<int, ForecastData>();
            //Insert into dict

            dataDict.Add(4, oldForecastData);

            //Create old Forecast
            string oldName = "Trendy Forecast";
            string oldDescription = "Super trendy forecast";
            Forecast oldForecast = new Forecast(1, oldName, oldDescription);
            IDictionary<int, Forecast> oldForecastDic = new Dictionary<int, Forecast>();
            oldForecastDic.Add(oldForecast.Id, oldForecast);

            //Create forecast date
            DateTime forecastDate = new DateTime(2006, 12, 31);

            //Create old forecast day
            ForecastDay oldForecastday = new ForecastDay(oldForecast, forecastDate, null, null, dataDict, null, null);
            IntegerDateKey idk = new IntegerDateKey(1, forecastDate);

            forecastDayDictionary.Add(idk, oldForecastday);

            TaskLayerMapper taskLayerMapper = new TaskLayerMapper(TimeZoneInfo.Utc, DateTime.Now.Date, 15);

            ObjectPairCollection<Forecast, Workload> retList = new ObjectPairCollection<Forecast, Workload>();

            //2. Record expectations
            using (mocks.Record())
            {
                workloadRepositoryMock.Add(null);
                LastCall.Repeat.Once().IgnoreArguments();

                uowMock.PersistAll();
                LastCall.Repeat.AtLeastOnce();

                uowMock.Remove(null);
                LastCall.Repeat.AtLeastOnce().IgnoreArguments();
            }

            //3. "Playback" excpectionation
            using (mocks.Playback())
            {
                WorkloadConverter.Convert(uowMock, workloadRepositoryMock, oldForecastDic, forecastDayDictionary,
                                          new WorkloadMapper(mappedObjectPair,queueSourceMap), taskLayerMapper, retList);

                Workload wl = retList.GetPaired(oldForecast);
                Assert.AreEqual(oldForecast.Name, wl.Name);
                Assert.AreEqual(oldForecast.Description, wl.Description);
            }
        }

        /// <summary>
        /// Determines whether this instance [can convert old forecast with invalid date (in DST transition)].
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-10-22
        /// </remarks>
        [Test]
        public void CanConvertOldForecastWithInvalidDate()
        {
            MockRepository mocks = new MockRepository();

            //create a forecastQueuesourceMap list
            ObjectPairCollection<int, QueueSource> queueSourceDic = new ObjectPairCollection<int, QueueSource>();

            //****THIS WILL MAP old queueId(99) to foreCastId(12)
            int queueId = 99;
            int foreCastId = 12;

            IDictionary<int, IntegerPair> queueSourceMap = new Dictionary<int, IntegerPair>();
            IntegerPair forecastQueueMap = new IntegerPair(foreCastId, queueId);
            queueSourceMap.Add(1, forecastQueueMap); //Key 1 has no meaning

            QueueSource qs =
                new QueueSource(queueId.ToString(CultureInfo.CurrentCulture),
                                queueId.ToString(CultureInfo.CurrentCulture), queueId);

            //The queue is the key to the QueueSource
            queueSourceDic.Add(queueId, qs);

            MappedObjectPair mappedObjectPair = new MappedObjectPair();
            mappedObjectPair.QueueSource = queueSourceDic;

            //1. Setup
            IUnitOfWork uowMock = mocks.CreateMock<IUnitOfWork>();
            IRepository<Workload> workloadRepositoryMock = mocks.CreateMock<IRepository<Workload>>();
            IDictionary<IntegerDateKey, ForecastDay> forecastDayDictionary =
                new Dictionary<IntegerDateKey, ForecastDay>();

            ForecastData oldForecastData = new ForecastData(9, 12, 33, 23);
            IDictionary<int, ForecastData> dataDict = new Dictionary<int, ForecastData>();
            //Insert into dict
            dataDict.Add(3, oldForecastData);

            //Create old Forecast
            string oldName = "Trendy Forecast";
            string oldDescription = "Super trendy forecast";
            Forecast oldForecast = new Forecast(1, oldName, oldDescription);
            IDictionary<int, Forecast> oldForecastDic = new Dictionary<int, Forecast>();
            oldForecastDic.Add(oldForecast.Id, oldForecast);

            //Create forecast date
            DateTime forecastDate = new DateTime(2007, 03, 25);

            //Create old forecast day
            ForecastDay oldForecastday = new ForecastDay(oldForecast, forecastDate, null, null, dataDict, null, null);
            IntegerDateKey idk = new IntegerDateKey(1, forecastDate);

            forecastDayDictionary.Add(idk, oldForecastday);

            TaskLayerMapper taskLayerMapper =
                new TaskLayerMapper(TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"), forecastDate, 15);

            ObjectPairCollection<Forecast, Workload> retList = new ObjectPairCollection<Forecast, Workload>();

            //2. Record expectations
            using (mocks.Record())
            {
                workloadRepositoryMock.Add(null);

                LastCall.Repeat.Once().IgnoreArguments();

                uowMock.PersistAll();
                LastCall.Repeat.AtLeastOnce();
            }

            //3. "Playback" excpectionation r
            using (mocks.Playback())
            {
                WorkloadConverter.Convert(uowMock, workloadRepositoryMock, oldForecastDic, forecastDayDictionary,
                                          new WorkloadMapper(mappedObjectPair,queueSourceMap), taskLayerMapper, retList);

                Workload wl = retList.GetPaired(oldForecast);
                Assert.AreEqual(0, wl.LayerCollection.Count);
                Assert.AreEqual(oldForecast.Name, retList.GetPaired(oldForecast).Name);
                Assert.AreEqual(oldForecast.Description, retList.GetPaired(oldForecast).Description);
            }
        }
    }
}