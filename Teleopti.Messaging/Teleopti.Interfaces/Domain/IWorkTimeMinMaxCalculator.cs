using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Interface for work time min/max calculator
	/// </summary>
	public interface IWorkTimeMinMaxCalculator
	{
		/// <summary>
		/// Calculate the min/max work times for a day using an alread loaded schedule day
		/// </summary>
		/// <param name="date"></param>
		/// <param name="person"></param>
		/// <param name="scheduleDay"></param>
		/// <param name="preferenceType"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		IWorkTimeMinMax WorkTimeMinMax(DateOnly date, IPerson person, IScheduleDay scheduleDay, out PreferenceType? preferenceType);
	}

	/// <summary>
	/// 
	/// </summary>
	public enum PreferenceType
	{
		/// <summary>
		/// 
		/// </summary>
		Absence,
		/// <summary>
		/// 
		/// </summary>
		DayOff,
		/// <summary>
		/// 
		/// </summary>
		ShiftCategory
	}
}