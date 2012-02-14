using System;
using System.Collections.Generic;
using Domain;
using Teleopti.Ccc.DatabaseConverter.EntityMapper;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.DatabaseConverter.CollectionConverter2
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WorkloadConverter
    {
        private WorkloadConverter()
        {
        }


        /// <summary>
        /// Converts the specified unit of work.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="newRep">The new rep.</param>
        /// <param name="oldForecasts">The old forecasts.</param>
        /// <param name="forecastDayDictionary">The forecast day dictionary.</param>
        /// <param name="converter">The converter.</param>
        /// <param name="taskLayerMapper">The task layer mapper.</param>
        /// <param name="pairList">The pair list.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: micke
        /// Created date: 2007-10-19
        /// </remarks>
        public static ObjectPairCollection<Forecast, Workload> Convert(IUnitOfWork unitOfWork,
                                                                       IRepository<Workload> newRep,
                                                                       IDictionary<int, Forecast> oldForecasts,
                                                                       IDictionary<IntegerDateKey, ForecastDay>
                                                                           forecastDayDictionary,
                                                                       Mapper<Workload, Forecast> converter,
                                                                       Mapper<TaskLayer, ForecastData> taskLayerMapper,
                                                                       ObjectPairCollection<Forecast, Workload> pairList)
        {
            Console.Out.WriteLine("Number of WorkLoads = " + oldForecasts.Count);
            string output = "Converting Workloads with Forecast data";
            Console.Out.WriteLine(output);


            foreach (Forecast theOld in oldForecasts.Values)
            {
                // Check if this workload already have been created
                Workload theNew;
                theNew = pairList.GetPaired(theOld);
                if (theNew == null)
                {
                    theNew = converter.Map(theOld);
                    pairList.Add(theOld, theNew);
                    newRep.Add(theNew);
                }
            }

            foreach (KeyValuePair<IntegerDateKey, ForecastDay> forecastDay in forecastDayDictionary)
            {
                Console.Out.WriteLine("Converting " + forecastDay.Key);
                Forecast theOld = null;
                oldForecasts.TryGetValue(forecastDay.Key.IntegerValue, out theOld);
                if (theOld != null)
                {
                    foreach (KeyValuePair<int, ForecastData> valuePair in forecastDay.Value.ForecastDataCollection)
                    {
                        try
                        {
                            TaskLayer newTaskLayer = taskLayerMapper.Map(valuePair.Value);
                            pairList.GetPaired(theOld).LayerCollection.Add(newTaskLayer);
                        }
                        catch (ArgumentException ex)
                        {
                            //If the error occurs because an invalid time exists in data source nothing will be done.
                            //Otherwise the argument exception will be rethrown.
                            //TODO: Can't create a logic test case for this, but it can probably occur...
                            if (!(ex.ParamName == "dateTime" && ex.Message.Contains("invalid time")))
                                throw new ArgumentException(ex.Message, ex.ParamName, ex);
                        }
                    }
                }
            }

            unitOfWork.PersistAll();

            foreach (Forecast theOld in oldForecasts.Values)
            {
                Workload theNew;
                theNew = pairList.GetPaired(theOld);
                if (theNew != null)
                {
                    foreach (TaskLayer tLayer in theNew.LayerCollection)
                    {
                        unitOfWork.Remove(tLayer);    
                    }
                    theNew.LayerCollection.Clear();
                }
                //nu är jag här! ..här..fast telefonen ringer hela tiden
            }

            return pairList;
        }
    }
}