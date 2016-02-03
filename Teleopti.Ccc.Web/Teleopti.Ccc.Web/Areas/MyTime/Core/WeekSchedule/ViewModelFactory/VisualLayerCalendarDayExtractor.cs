using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory
{
    public class VisualLayerCalendarDayExtractor
    {
        public IList<VisualLayerForWebDisplay> CreateVisualPeriods(DateTime date, IEnumerable<IVisualLayer> visualLayerCollection, TimeZoneInfo timeZone)
        {
            var returnList = new List<VisualLayerForWebDisplay>();
            
            foreach (var visualLayer in visualLayerCollection)
            {
                var layer = visualLayer as VisualLayer;
                if (layer == null)
                    continue;

                DateTimePeriod visualPeriod;
                if (visualLayer.Period.StartDateTimeLocal(timeZone).Date < date)
                {
                    // Layer starts yesterday
                    if (visualLayer.Period.EndDateTimeLocal(timeZone).Date == date)
                    {
                        // ...and ends today
                        visualPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(date.Date, timeZone), visualLayer.Period.EndDateTime);
                    }
                    else
                    {
                        // ...and ends yesterday
                        continue;
                    }
                }
                else if (visualLayer.Period.StartDateTimeLocal(timeZone).Date == date)
                {
                    // Layer starts today
                    if (visualLayer.Period.EndDateTimeLocal(timeZone).Date > date)
                    {
                        // ...and ends tomorrow
                        visualPeriod = new DateTimePeriod(visualLayer.Period.StartDateTime,
                                                          TimeZoneHelper.ConvertToUtc(date.Date.Add(new TimeSpan(23, 59, 59)), timeZone));
                    }
                    else
                    {
                        // ...and ends today
                        visualPeriod = new DateTimePeriod(visualLayer.Period.StartDateTime,
                                                          visualLayer.Period.EndDateTime);
                    }
                }
                else
                {
                    // Layer starts tomorrow
                    continue;
                }

                var layerForDisplay = new VisualLayerForWebDisplay(visualLayer.Payload, visualLayer.Period, layer.HighestPriorityActivity, visualLayer.Person)
                                          {
                                              VisualPeriod = visualPeriod,
											  DefinitionSet = visualLayer.DefinitionSet
                                          };
                returnList.Add(layerForDisplay);
            }

            return returnList;
        }
    }
}