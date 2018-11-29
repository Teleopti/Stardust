using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;


namespace Teleopti.Wfm.Adherence.Domain.Service
{
	public class ScheduledActivity
    {
		public Guid PersonId { get; set; }
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




		public int CheckSum()
		{
			unchecked
			{
				var hashCode = PayloadId.GetHashCode();
				hashCode = (hashCode * 397) ^ BelongsToDate.GetHashCode();
				hashCode = (hashCode * 397) ^ StartDateTime.GetHashCode();
				hashCode = (hashCode * 397) ^ EndDateTime.GetHashCode();
				hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (ShortName?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ DisplayColor;
				return hashCode;
			}
		}

	}

	public static class ScheduledActivityExtensions
	{
		public static int CheckSum(this IEnumerable<ScheduledActivity> activities)
		{
			unchecked
			{
				return activities.Aggregate(0, (cs, a) => cs * 31 + a.CheckSum());
			}
		}
	}

}