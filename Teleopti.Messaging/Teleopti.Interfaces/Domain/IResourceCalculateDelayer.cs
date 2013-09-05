using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Used to decide if it is time to do a resource calculation
	/// </summary>
	public interface IResourceCalculateDelayer
	{
		/// <summary>
		/// Calculates if needed.
		/// </summary>
		/// <param name="scheduleDateOnly">The schedule date only.</param>
		/// <param name="workShiftProjectionPeriod">The work shift projection period.</param>
		/// <param name="addedSchedules">The added schedules.</param>
		/// <returns></returns>
		bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod,
		                       IList<IScheduleDay> addedSchedules);

		/// <summary>
		/// Calculates if needed.
		/// </summary>
		/// <param name="scheduleDateOnly">The schedule date only.</param>
		/// <param name="workShiftProjectionPeriod">The work shift projection period.</param>
		/// <param name="addedSchedules">The added schedules.</param>
		/// <param name="removedSchedules">The removed schedules.</param>
		/// <returns></returns>
		bool CalculateIfNeeded(DateOnly scheduleDateOnly, DateTimePeriod? workShiftProjectionPeriod,
							   IList<IScheduleDay> addedSchedules, IList<IScheduleDay> removedSchedules);
	}
}