using System;

// ReSharper disable once CheckNamespace
namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Return value of ParseTimeOfDayFromTimeSpan method
	/// </summary>
	public class ParsedTimeOfDay
	{
		/// <summary>
		/// Days that the time of day timespan included. 1 = Next day
		/// </summary>
		public int Days { get; set; }
		/// <summary>
		/// The time of day the timespan included
		/// </summary>
		public TimeSpan TimeOfDay { get; set; }
	}
}