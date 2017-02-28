namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// 
    /// </summary>
    public interface IWorkTimeMinMax
    {
        /// <summary>
        /// Gets or sets the start time limitation.
        /// </summary>
        /// <value>The start time limitation.</value>
        StartTimeLimitation StartTimeLimitation { get; set; }
        /// <summary>
        /// Gets or sets the end time limitation.
        /// </summary>
        /// <value>The end time limitation.</value>
        EndTimeLimitation EndTimeLimitation { get; set; }
        /// <summary>
        /// Gets or sets the work time limitation.
        /// </summary>
        /// <value>The work time limitation.</value>
        WorkTimeLimitation WorkTimeLimitation { get; set; }

        /// <summary>
        /// Combines the specified work time min max with this instance.
        /// </summary>
        /// <param name="workTimeMinMax">The work time min max.</param>
        /// <returns></returns>
        IWorkTimeMinMax Combine(IWorkTimeMinMax workTimeMinMax);

    }
}
