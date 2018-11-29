namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	// Used for translation auto search, DO NOT REMOVE!
	// UserTexts.Resources.EmploymentTypeFixedStaffPeriodWorkTime
	// UserTexts.Resources.EmploymentTypeHourlyStaff
	// UserTexts.Resources.EmploymentTypeFixedStaffDayWorkTime

    /// <summary>
    /// Types of employments
    /// </summary>
    public enum EmploymentType
    {
        /// <summary>
        /// Employed as fixed staff with normal work time
        /// </summary>
        FixedStaffNormalWorkTime = 0,
        /// <summary>
        /// Employed at hourly basis
        /// </summary>
        HourlyStaff = 1,
        /// <summary>
        /// Employed as fixed staff with normal work time
        /// </summary>
        FixedStaffDayWorkTime = 3
    }
}