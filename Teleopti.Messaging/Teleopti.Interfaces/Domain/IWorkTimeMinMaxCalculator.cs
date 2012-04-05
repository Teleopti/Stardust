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
		/// <returns></returns>
		IWorkTimeMinMax WorkTimeMinMax(DateOnly date, IPerson person, IScheduleDay scheduleDay);
	}
}