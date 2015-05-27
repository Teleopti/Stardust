using System;
using System.Collections.Generic;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Transformer.ScheduleThreading
{
    public interface IScheduleTransformer
    {
        event RowsUpdatedEventHandler RowsUpdatedEvent;
        void Transform(IList<IScheduleDay> scheduleList, DateTime insertDateTime, IJobParameters jobParameters, IThreadPool threadPool);
    }
}