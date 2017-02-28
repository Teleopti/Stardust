using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Hold information about when and who created this object.
    /// </summary>
    public interface ICreateInfo
    {
        /// <summary>
        /// Gets the Person that first saved this entity.
        /// </summary>
        /// <value>The Person.</value>
        IPerson CreatedBy { get; }

        /// <summary>
        /// Gets the time when this entity was first saved
        /// </summary>
        /// <value>The time.</value>
        DateTime? CreatedOn { get; }
    }
}