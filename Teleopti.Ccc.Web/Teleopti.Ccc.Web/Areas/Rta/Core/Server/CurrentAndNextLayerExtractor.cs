using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
    public class CurrentAndNextLayerExtractor : ICurrentAndNextLayerExtractor
    {
        private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(CurrentAndNextLayerExtractor));

        public Tuple<ScheduleLayer, ScheduleLayer> CurrentLayerAndNext(DateTime onTime, IList<ScheduleLayer> layers)
        {
            if (!layers.Any()) return new Tuple<ScheduleLayer, ScheduleLayer>(null, null);

            LoggingSvc.Debug("Finding current layer and next");
            var scheduleLayers = layers.Where(l => l.EndDateTime > onTime).OrderBy(l => l.StartDateTime).ToList();
            
            ScheduleLayer scheduleLayer = null;
            ScheduleLayer nextLayer = null;
            if (scheduleLayers.Any())
                scheduleLayer = scheduleLayers[0];

            // no layer now
            if (scheduleLayer != null && scheduleLayer.StartDateTime > onTime)
            {
                nextLayer = scheduleLayer;
                scheduleLayer = null;
            }

            if (nextLayer == null && scheduleLayers.Count > 1)
                nextLayer = scheduleLayers[1];

            if (scheduleLayer != null && nextLayer != null)
                //scheduleLayer is the last in assignment
                if (scheduleLayer.EndDateTime != nextLayer.StartDateTime)
                    nextLayer = null;

            if (scheduleLayer != null)
                LoggingSvc.DebugFormat(CultureInfo.InvariantCulture, "Current layer = Name: {0}, StartTime: {1}, EndTime: {2}", scheduleLayer.Name, scheduleLayer.StartDateTime, scheduleLayer.EndDateTime);
            if (nextLayer != null)
                LoggingSvc.DebugFormat(CultureInfo.InvariantCulture, "Next layer = Name: {0}, StartTime: {1}, EndTime: {2}", nextLayer.Name, nextLayer.StartDateTime, nextLayer.EndDateTime);
            return new Tuple<ScheduleLayer, ScheduleLayer>(scheduleLayer, nextLayer);
        }
    }
}