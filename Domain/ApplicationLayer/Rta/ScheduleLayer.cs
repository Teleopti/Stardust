using System;
using System.Drawing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
    public class ScheduleLayer
    {
        public Guid PayloadId { get; set; }
		public DateOnly BelongsToDate { get; set; }
		public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int DisplayColor { get; set; }

	    public Color TheColor()
        {
            return Color.FromArgb(DisplayColor);
        }

        public DateTimePeriod Period()
        {
            return new DateTimePeriod(StartDateTime, EndDateTime);
        }
    }
}