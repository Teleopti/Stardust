using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for overtime shifts
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-02-05
    /// </remarks>
	public interface IOvertimeShift : IShift, IAggregateEntity
    {
        /// <summary>
        /// Gets the Layercollection with definition set included.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-05
        /// </remarks>
        IEnumerable<IOvertimeShiftActivityLayer> LayerCollectionWithDefinitionSet();
    }
}
