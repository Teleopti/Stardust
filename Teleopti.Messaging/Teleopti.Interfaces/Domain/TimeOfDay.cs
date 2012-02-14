using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Represent a time of day
	/// </summary>
	/// 
	//todo - jag fixar strax
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct TimeOfDay
	{
		/// <summary>
		/// Ctor
		/// </summary>
		/// <param name="time"></param>
		public TimeOfDay(TimeSpan time) : this()
		{
			Time = time;
		}

		/// <summary>
		/// Returns time of day as <see cref="TimeSpan"/>
		/// </summary>
		public TimeSpan Time { get; private set; }
	}
}