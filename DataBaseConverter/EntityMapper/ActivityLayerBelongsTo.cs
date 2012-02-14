namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Used to describe if activity layer for workshift, mainshift or personalshift.
    /// Would be nice if this wouldn't be needed
    /// </summary>
    public enum ActivityLayerBelongsTo
    {
        /// <summary>
        /// Used for mainshift
        /// </summary>
        MainShift,
        /// <summary>
        /// Used for personalshift
        /// </summary>
        PersonalShift,
        /// <summary>
        /// Used for workshift
        /// </summary>
        WorkShift,
        /// <summary>
        /// Used for overtime shift
        /// </summary>
        OvertimeShift
    }
}