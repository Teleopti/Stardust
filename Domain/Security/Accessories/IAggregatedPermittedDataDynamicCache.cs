using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Accessories
{
    /// <summary>
    /// Caches the temporary AggregatedPermittedData for peformance reasons.
    /// </summary>
    public interface IAggregatedPermittedDataDynamicCache
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AggregatedPermittedDataDynamicCache"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled
        { get; set; }

        /// <summary>
        /// Gets the aggregated permitted data.
        /// </summary>
        /// <param name="applicationFunction">The application function.</param>
        /// <returns></returns>
        IAvailableData AggregatedPermittedData(IApplicationFunction applicationFunction);

        /// <summary>
        /// Deletes the cached data.
        /// </summary>
        void DeleteCache();
    }
}