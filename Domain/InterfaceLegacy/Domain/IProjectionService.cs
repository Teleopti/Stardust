namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for projection services
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-02-25
    /// </remarks>
    public interface IProjectionService
    {
        /// <summary>
        /// Creates a projection.
        /// Returns full projection
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-29
        /// </remarks>
        IVisualLayerCollection CreateProjection();
    }
}