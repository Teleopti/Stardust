using System.Globalization;

namespace Teleopti.Ccc.Web.Areas.Reporting.Reports.CCC
{
	public class IntervalToolTip
	{
		public int StartInterval { get; set; }
		public int EndInterval { get; set; }

		public int StartIntervalCounter { get; set; }
		public int EndIntervalCounter { get; set; }


		public string AbsenceOrActivityName { get; set; }

		public string ToolTip(int intervalsPerHour)
		{
			return string.Concat(AbsenceOrActivityName, "\n", getTimeString(intervalsPerHour, StartInterval), "-",
			                     getTimeString(intervalsPerHour, EndInterval + 1));
		}

		private static string getTimeString(int intervalsPerHour, int interval)
		{
			var hourPart = interval / intervalsPerHour;
			var minutePart = (interval % intervalsPerHour) * (60 / intervalsPerHour);
			return string.Concat(hourPart.ToString("00", CultureInfo.InvariantCulture), ":",
			                     minutePart.ToString("00", CultureInfo.InvariantCulture));
		}
	}
}