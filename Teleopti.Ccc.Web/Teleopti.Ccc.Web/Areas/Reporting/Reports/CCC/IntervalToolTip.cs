using System;
using System.Globalization;

namespace Teleopti.Analytics.Portal.Reports.Ccc
{
	public class IntervalToolTip
	{
		public int StartInterval { get; set; }
		public int EndInterval { get; set; }

		public int StartIntervalCounter { get; set; }
		public int EndIntervalCounter { get; set; }


		public String AbsenceOrActivityName { get; set; }

		public String ToolTip(int intervalsPerHour)
		{
			return String.Concat(AbsenceOrActivityName, "\n", GetTimeString(intervalsPerHour, StartInterval), "-",
			                     GetTimeString(intervalsPerHour, EndInterval + 1));
		}

		private static String GetTimeString(int intervalsPerHour, int interval)
		{
			int hourPart = interval / intervalsPerHour;
			int minutePart = (interval % intervalsPerHour) * (60 / intervalsPerHour);
			return String.Concat(hourPart.ToString("00", CultureInfo.InvariantCulture), ":",
			                     minutePart.ToString("00", CultureInfo.InvariantCulture));
		}
	}
}