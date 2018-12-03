using System;
using System.Globalization;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Date helper class 
	/// </summary>
	public static class DateHelper
	{
		static DateHelper()
		{
			var calendar = new UmAlQuraCalendar();
			MaxSmallDateTime = calendar.MaxSupportedDateTime.Date;
			MinSmallDateTime = calendar.MinSupportedDateTime;
		}

		/// <summary>
		/// Gets the min small date time.
		/// </summary>
		/// <value>The min small date time.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-01-28
		/// </remarks>
		public static DateTime MinSmallDateTime { get; }

		/// <summary>
		/// Gets the max small date time.
		/// </summary>
		/// <value>The max small date time.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2009-01-28
		/// </remarks>
		public static DateTime MaxSmallDateTime { get; }
	}
}