namespace Teleopti.Interfaces.Domain
{

    /// <summary>
    /// Used in queuesource
    /// </summary>
    public interface IStat
    {
        /// <summary>
        /// Gets or sets the number of tasks.
        /// </summary>
        /// <value>The number of tasks.</value>
        int NumberOfTasks { get; set; }

        /// <summary>
        /// Gets or sets the total time.
        /// </summary>
        /// <value>The total time.</value>
        long TotalTime { get; set; }

        /// <summary>
        /// Gets or sets the totalt wrapup time.
        /// </summary>
        /// <value>The totalt wrapup time.</value>
        long TotalWrapUptime { get; set; }
    }
}
