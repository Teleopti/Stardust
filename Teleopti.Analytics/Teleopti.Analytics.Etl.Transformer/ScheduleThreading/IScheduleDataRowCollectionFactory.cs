using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
    public interface IScheduleDataRowCollectionFactory
    {
        ICollection<DataRow> CreateScheduleDataRowCollection(DataTable dataTable
                                                                      , ScheduleProjection scheduleProjection
                                                                      , IntervalBase interval
                                                                      , DateTimePeriod intervalPeriod
                                                                      , DateTime insertDateTime
                                                                      , int intervalsPerDay);
        
    }
}