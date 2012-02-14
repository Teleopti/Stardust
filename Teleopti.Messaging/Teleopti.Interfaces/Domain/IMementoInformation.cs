using System;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Information about the IMemento
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-11-18
    /// </remarks>
    public interface IMementoInformation
    {
        /// <summary>
        /// Gets the time when this memento was created.
        /// </summary>
        /// <value>The time.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-13
        /// </remarks>
        DateTime Time { get; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-11-14
        /// </remarks>
        string Description { get; }
    }
}
