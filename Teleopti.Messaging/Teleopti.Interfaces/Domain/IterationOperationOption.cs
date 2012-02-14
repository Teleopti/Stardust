namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Enumeration for which operation is active in the current optimization iteration.
    /// </summary>
    public enum IterationOperationOption
    {
        /// <summary>
        /// No optimization is selected
        /// </summary>
        None,
        /// <summary>
        /// Wordkshift optimization
        /// </summary>
        WorkShiftOptimization,
        /// <summary>
        /// Day off optimization
        /// </summary>
        DayOffOptimization,
        /// <summary>
        /// Intraday optimization
        /// </summary>
        IntradayOptimization
    }
}