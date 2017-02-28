namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Staffing thresholds
    /// </summary>
    public enum StaffingThreshold
    {
        /// <summary>
        /// Ok
        /// </summary>
        Ok = 0,
        /// <summary>
        /// Overstaffing
        /// </summary>
        Overstaffing = 1,
        /// <summary>
        /// Understaffing
        /// </summary>
        Understaffing = 2,
        /// <summary>
        /// CriticalUnderstaffing
        /// </summary>
        CriticalUnderstaffing = 3
    }
}