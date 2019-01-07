using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public class VisualLayerCalendarDayExtractor
    {
        public IList<VisualLayerForWebDisplay> CreateVisualPeriods(DateOnly date, IEnumerable<IVisualLayer> visualLayerCollection, TimeZoneInfo timeZone, bool allowCrossNight = false)
        {
            var returnList = new List<VisualLayerForWebDisplay>();

			var completeDate = date.ToDateTimePeriod(timeZone).ChangeEndTime(TimeSpan.FromSeconds(-1));
			foreach (var visualLayer in visualLayerCollection)
            {
                var layer = visualLayer as VisualLayer;
                if (layer == null)
                    continue;

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