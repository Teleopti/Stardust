using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Rta.Server
{
    public interface ICurrentAndNextLayerExtractor
    {
        Tuple<ScheduleLayer, ScheduleLayer> CurrentLayerAndNext(DateTime onTime, IList<ScheduleLayer> layers);
    }
}