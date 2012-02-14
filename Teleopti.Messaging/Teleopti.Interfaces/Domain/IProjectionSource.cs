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

        /// <summary>
        /// Gets a value indicating whether this instance has projection.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has projection; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-28
        /// </remarks>
        bool HasProjection { get; }
    }
}