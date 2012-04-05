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
		/// Calculate the min/max work times for a schedule day
		/// </summary>
		/// <param name="scheduleDay"></param>
		/// <returns></returns>
		IWorkTimeMinMax WorkTimeMinMax(IScheduleDay scheduleDay);
	}
}