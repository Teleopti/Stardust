using System.Collections.Generic;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Event = DDay.iCal.Event;

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
            var iCal = new iCalendar { ProductID = "Teleopti" };

            foreach (var scheduleDay in scheduleDays)
            {
                if (scheduleDay.Model == null) continue;

                var shift = _deserializer.DeserializeObject<Model>(scheduleDay.Model).Shift;
                if (shift == null) continue;
                
                foreach (var layer in shift.Projection)
                {
                    var evt = iCal.Create<Event>();

                    evt.Start = new iCalDateTime(layer.Start);
                    evt.End = new iCalDateTime(layer.End);
                    evt.Summary = layer.Description;
                }
            }

            var serializer = new iCalendarSerializer();
            var icsContent = serializer.SerializeToString(iCal);
            return icsContent;
        }
    }
}