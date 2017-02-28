using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface holding simple audit info
    /// </summary>
    public interface IChangeInfo 
    {
        /// <summary>
        /// Gets the person that last saved this entity.
        /// </summary>
        /// <value>The Person.</value>
        IPerson UpdatedBy { get; }

        /// <summary>
        /// Gets the time when this entity last was updated.
        /// </summary>
        /// <value>The updated on.</value>
        DateTime? UpdatedOn { get; }
    }
}