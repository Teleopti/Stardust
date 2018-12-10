using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Validated volume information for one day and workload.
    /// </summary>
    public interface IValidatedVolumeDay : ITaskOwner, IAggregateRoot, IChangeInfo
    {
        /// <summary>
        /// Gets a value indicating whether this instance has values.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has values; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        bool HasValues { get; }

        /// <summary>
        /// Gets the workload.
        /// </summary>
        /// <value>The workload.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        IWorkload Workload { get; }

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <value>The date.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        DateOnly VolumeDayDate { get; }

        /// <summary>
        /// Gets or sets the task owner.
        /// </summary>
        /// <value>The task owner.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-28
        /// </remarks>
        ITaskOwner TaskOwner { get; set; }

        /// <summary>
        /// Gets or sets the validated tasks.
        /// </summary>
        /// <value>The validated tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        double ValidatedTasks { get; set; }

        /// <summary>
        /// Gets the original tasks.
        /// </summary>
        /// <value>The original tasks.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        double OriginalTasks { get; }

        /// <summary>
        /// Gets or sets the validated average task time.
        /// </summary>
        /// <value>The validated average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        TimeSpan ValidatedAverageTaskTime { get; set; }

        /// <summary>
        /// Gets the original average task time.
        /// </summary>
        /// <value>The original average task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        TimeSpan OriginalAverageTaskTime { get; }

        /// <summary>
        /// Gets or sets the validated average after task time.
        /// </summary>
        /// <value>The validated average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        TimeSpan ValidatedAverageAfterTaskTime { get; set; }

        /// <summary>
        /// Gets the original average after task time.
        /// </summary>
        /// <value>The original average after task time.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-27
        /// </remarks>
        TimeSpan OriginalAverageAfterTaskTime { get; }
    }
}