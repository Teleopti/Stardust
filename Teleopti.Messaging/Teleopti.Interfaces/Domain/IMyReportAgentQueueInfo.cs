using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// IMyReportAgentQueueInfo interface
    /// </summary>
    /// <remarks>
    /// Created by:VirajS
    /// Created date: 11/24/2008
    /// </remarks>
    public interface IMyReportAgentQueueInfo : IEquatable<IMyReportAgentQueueInfo>
    {
        /// <summary>
        /// Gets or sets the name of the queue.
        /// </summary>
        /// <value>The name of the queue.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        string QueueName { get; set; }

        /// <summary>
        /// Gets or sets the answered contracts.
        /// </summary>
        /// <value>The answered contracts.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        int AnsweredContracts { get; set; }

        /// <summary>
        /// Gets or sets the average talk time.
        /// </summary>
        /// <value>The average talk time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        string AverageTalkTime { get; set; }

        /// <summary>
        /// Gets or sets the after work contact time.
        /// </summary>
        /// <value>The after work contact time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        string AfterWorkContactTime { get; set; }

        /// <summary>
        /// Gets or sets the total handling time.
        /// </summary>
        /// <value>The total handling time.</value>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 11/24/2008
        /// </remarks>
        string TotalHandlingTime { get; set; }
    }
}
