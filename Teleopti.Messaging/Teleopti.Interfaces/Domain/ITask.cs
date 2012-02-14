using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds information about a person's task including time length information.
    /// </summary>
    public interface ITask : IEquatable<ITask>
    {
        /// <summary>
        /// Gets the tasks.
        /// </summary>
        /// <value>The tasks.</value>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2007-11-15
        /// </remarks>
        double Tasks { get; }

        /// <summary>
        /// Gets the average task time.
        /// </summary>
        /// <value>The average task time.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 21.12.2007
        /// </remarks>
        TimeSpan AverageTaskTime { get; }

        /// <summary>
        /// Gets the average after task time.
        /// </summary>
        /// <value>The average after task time.</value>
        /// <remarks>
        /// Created by: micke
        /// Created date: 21.12.2007
        /// </remarks>
        TimeSpan AverageAfterTaskTime { get; }
    }
}
