using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Marker interface for entities holding restrictions
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-10-22
    /// </remarks>
    public interface IRestrictionOwner
    {
        /// <summary>
        /// Gets the restriction collection.
        /// </summary>
        /// <value>The restriction collection.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-10-22
        /// </remarks>
        IEnumerable<IRestrictionBase> RestrictionBaseCollection { get; }
    }
}
