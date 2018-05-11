using System;

namespace StaffHubPoC.Util
{
	public class DateTimePeriod
	{
		public DateTimePeriod(DateTime startDateTime, DateTime endDateTime)
		{
			if(endDateTime <= startDateTime) throw new Exception("StartDateTime cannot be larger than EndDatetime.");
			StartDateTime = startDateTime;
			EndDateTime = endDateTime;
		}

		public DateTime StartDateTime { get; }
		public DateTime EndDateTime { get; }
	}
}
