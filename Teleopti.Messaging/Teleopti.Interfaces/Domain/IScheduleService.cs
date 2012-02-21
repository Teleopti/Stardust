using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Schedule a person on a day
	/// </summary>
	public interface IScheduleService
	{
		/// <summary>
		/// Gets the finder results.
		/// </summary>
		/// <value>The finder results.</value>
		ReadOnlyCollection<IWorkShiftFinderResult> FinderResults { get; }

        /// <summary>
        /// Schedules the person on day.
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="useOccupancyAdjustment">if set to <c>true</c> then use occupancy adjustment.</param>
        /// <param name="effectiveRestriction">The effective restriction.</param>
        /// <returns></returns>
	    bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            bool useOccupancyAdjustment,
			IEffectiveRestriction effectiveRestriction);

	    ///<summary>
	    ///</summary>
	    void ClearFinderResults();

        /// <summary>
        /// Uses the desired ShiftCategory when creating the restriction
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="useOccupancyAdjustment">if set to <c>true</c> then use occupancy adjustment.</param>
        /// <param name="useThisCategory">The use this category.</param>
        /// <returns></returns>
        bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            bool useOccupancyAdjustment,
            IShiftCategory useThisCategory);

        /// <summary>
        /// Creates the effective restriction and Schedule
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <param name="useOccupancyAdjustment">if set to <c>true</c> then use occupancy adjustment.</param>
        /// <returns></returns>
        bool SchedulePersonOnDay(
            IScheduleDay schedulePart,
            bool useOccupancyAdjustment);

        /// <summary>
        /// Gets the scheduling options.
        /// </summary>
        /// <value>The scheduling options.</value>
	    ISchedulingOptions SchedulingOptions { get; }
	}
}