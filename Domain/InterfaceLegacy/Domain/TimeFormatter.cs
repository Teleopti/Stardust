using System;
using System.Globalization;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Time formatter
	/// </summary>
	public class TimeFormatter
	{
		private readonly IUserCulture _culture;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="culture">time culture</param>
		public TimeFormatter(IUserCulture culture) {
			_culture = culture;
		}

		/// <summary>
		/// TimeSpan to xx:yy format, xx hours, yy minutes
		/// </summary>
		/// <param name="timeSpan">TimeSpan want to format</param>
		/// <returns></returns>
		public string GetLongHourMinuteTimeString(TimeSpan timeSpan)
		{
			string separator = _culture.GetCulture().DateTimeFormat.TimeSeparator;
			
			string signChar = string.Empty;
			if (timeSpan < TimeSpan.Zero)
			{
				signChar = "-";
				timeSpan = timeSpan.Negate();
			}

			int hour = (int)timeSpan.TotalHours;
			int minutes = timeSpan.Minutes;

			if (timeSpan.Seconds >= 30)
				minutes += 1;
			if (minutes == 60)
			{
				hour++;
				minutes = 0;
			}

			string min = Convert.ToString(minutes, CultureInfo.CurrentCulture);
			string hours = Convert.ToString(hour, CultureInfo.CurrentCulture);

			if (min.Length == 1)
				min = "0" + min;

			return string.Concat(signChar, hours, separator, min);
		}

	}
}