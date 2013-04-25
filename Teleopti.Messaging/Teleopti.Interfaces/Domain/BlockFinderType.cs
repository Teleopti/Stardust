namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// The type of the blockfinder to use
    /// </summary>
    public enum BlockFinderType
    {
        /// <summary>
        /// Do not use any block scheduling
        /// </summary>
        None,

        /// <summary>
        /// Finds block between two days off
        /// </summary>
        BetweenDayOff,

        /// <summary>
        /// Finds a block representing the whole schedule period
        /// </summary>
        SchedulePeriod,

        /// <summary>
        /// Finds block between ConSecutive Shifts
        /// </summary>
        ConsecutiveShifts,

        /// <summary>
        /// Finds block between calender week
        /// </summary>
        Weeks,

		/// <summary>
		/// To use when each day is considered a block
		/// </summary>
		SingleDay
       
    }
}