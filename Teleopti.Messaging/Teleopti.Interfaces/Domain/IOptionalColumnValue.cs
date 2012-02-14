using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// The value for an optional column
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2009-04-14
    /// </remarks>
    public interface IOptionalColumnValue : IAggregateEntity
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: Viraj Siriwardana
        /// Created date: 2008-07-24
        /// </remarks>
        Guid? ReferenceId { get; set; }
    }
}