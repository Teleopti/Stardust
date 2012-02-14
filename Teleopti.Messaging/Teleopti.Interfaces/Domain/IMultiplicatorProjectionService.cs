using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// For extracting Multiplicatorlayers from a visuallayercollection
    /// </summary>
    public interface IMultiplicatorProjectionService
    {
        /// <summary>
        /// Creates the projection.
        /// </summary>
        /// <returns></returns>
        IList<IMultiplicatorLayer> CreateProjection();
    }
}
