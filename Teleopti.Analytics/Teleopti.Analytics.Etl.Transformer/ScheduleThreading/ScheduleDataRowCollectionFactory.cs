using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
    public class ScheduleDataRowCollectionFactory : IScheduleDataRowCollectionFactory
    {
        public ICollection<DataRow> CreateScheduleDataRowCollection(DataTable dataTable
                                        , ScheduleProjection scheduleProjection
                                        , IntervalBase interval
                                        , DateTimePeriod intervalPeriod
                                        , DateTime insertDateTime
                                        , int intervalsPerDay)
        {

            List<DataRow> list = new List<DataRow>();

            // Get list of layers inside the given interval period
            IVisualLayerCollection intervalLayerCollection =
                scheduleProjection.SchedulePartProjection.FilterLayers(intervalPeriod);

            foreach (VisualLayer layer in intervalLayerCollection)
            {
                DataRow dataRow = ScheduleDataRowFactory.CreateScheduleDataRow(dataTable, layer, scheduleProjection, interval, insertDateTime, intervalsPerDay);
                list.Add(dataRow);
            }
            return list;
        }




    }


}
