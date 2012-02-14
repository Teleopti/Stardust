using System;
using System.Collections.Generic;
using System.Text;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// DataProvider for tracker
    /// </summary>
    /// <remarks>
    /// Returns VisualLayerCollection used for a tracker to read from,
    /// wraps schedulepart, could be used for providing data from the db etc.
    /// Created by: henrika
    /// Created date: 2009-02-09
    /// </remarks>
    public interface ITrackingDataProvider
    {

        /// <summary>
        /// Returns the VisualLayers that the tracker should read from
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-02-09
        /// </remarks>
      IVisualLayerCollection CreateVisualLayerCollection();
        
    }
}
