using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public class VisualLayerCalendarDayExtractor
    {
        public IList<VisualLayerForWebDisplay> CreateVisualPeriods(DateOnly date, IEnumerable<IVisualLayer> visualLayerCollection, TimeZoneInfo timeZone, bool allowCrossNight = false)
        {
            var returnList = new List<VisualLayerForWebDisplay>();
            
            foreach (var visualLayer in visualLayerCollection)
            {
                var layer = visualLayer as VisualLayer;
                if (layer == null)
                    continue;

				var completeDate = date.ToDateTimePeriod(timeZone).ChangeEndTime(TimeSpan.FromSeconds(-1));

				if(allowCrossNight && layer.Period.EndDateTime > completeDate.EndDateTime)
				{
					completeDate = new DateTimePeriod(completeDate.StartDateTime, layer.Period.EndDateTime);
				}

				var sharedPeriod = completeDate.Intersection(visualLayer.Period);

				if (sharedPeriod.HasValue)
				{
					var visualPeriod = sharedPeriod.Value;
					var layerForDisplay = new VisualLayerForWebDisplay(visualLayer.Payload, visualLayer.Period,
						layer.HighestPriorityActivity)
					{
						VisualPeriod = visualPeriod,
						DefinitionSet = visualLayer.DefinitionSet
					};
					returnList.Add(layerForDisplay);
				}
			}

            return returnList;
        }
    }
}