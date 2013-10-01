namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Holds info about the types projection of underlying layer
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-01-28
    /// </remarks>
    public interface IProjectionSource
    {
        /// <summary>
        /// Creates the projection.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-28
        /// </remarks>
        IProjectionService ProjectionService();
    }
}