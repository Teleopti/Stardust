using System.Collections.Generic;
using Ical.Net;
using Ical.Net.DataTypes;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public class CalendarTransformer : ICalendarTransformer
    {
        private readonly IJsonDeserializer _deserializer;

        public CalendarTransformer(IJsonDeserializer deserializer)
        {
            _deserializer = deserializer;
        }

        public string Transform(IEnumerable<PersonScheduleDayReadModel> scheduleDays)
        {
            var iCal = new Calendar { ProductId = "Teleopti" };

			iCal.AddProperty("X-PUBLISHED-TTL", "PT30M");       // Refresh Every 30 minutes
			iCal.AddProperty("X-WR-CALNAME", iCal.ProductId);

			foreach (var scheduleDay in scheduleDays)
            {
                if (scheduleDay.Model == null) continue;

                var shift = _deserializer.DeserializeObject<Model>(scheduleDay.Model).Shift;
                if (shift == null) continue;
                
                foreach (var layer in shift.Projection)
                {
                    var evt = iCal.Create<Ical.Net.CalendarComponents.CalendarEvent>();

                    evt.Start = new CalDateTime(layer.Start){ HasTime = true};
                    evt.End = new CalDateTime(layer.End) { HasTime = true };
                    evt.Summary = layer.Description;
                }
            }

            var serializer = new Ical.Net.Serialization.CalendarSerializer();
            var icsContent = serializer.SerializeToString(iCal);
            return icsContent;
        }
    }
}