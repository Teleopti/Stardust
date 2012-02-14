using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Eventargs
    /// </summary>
    public class BlockSchedulingServiceEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the percentage completed.
        /// </summary>
        /// <value>The percentage completed.</value>
        public int PercentageCompleted { get; set; }
    }
}