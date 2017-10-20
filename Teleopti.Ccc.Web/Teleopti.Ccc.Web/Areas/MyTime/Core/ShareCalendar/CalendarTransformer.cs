using System.Collections.Generic;
using Ical.Net;
using Ical.Net.DataTypes;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Event = Ical.Net.CalendarEvent;

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
                    var evt = iCal.Create<Event>();

                    evt.Start = new CalDateTime(layer.Start);
                    evt.End = new CalDateTime(layer.End);
                    evt.Summary = layer.Description;
                }
            }

            var serializer = new Ical.Net.Serialization.iCalendar.Serializers.CalendarSerializer();
            var icsContent = serializer.SerializeToString(iCal);
            return icsContent;
        }
    }
}