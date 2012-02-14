using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Accessories
{

    /// <summary>
    /// Delegation for calculating the AggregatedPermittedData
    /// </summary>
    public delegate IAvailableData CalculateAggregatedPermittedDataMethod(IApplicationFunction applicationFunction);

    /// <summary>
    /// Caches the temporary AggregatedPermittedData for peformance reasons.
    /// </summary>
    public class AggregatedPermittedDataDynamicCache : IAggregatedPermittedDataDynamicCache
    {

        private IApplicationFunction _applicationFunction;
        private IAvailableData _cachedAvailableData;
        private readonly CalculateAggregatedPermittedDataMethod _calculateAggregatedPermittedDataMethod;
        private bool _enabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregatedPermittedDataDynamicCache"/> class.
        /// </summary>
        /// <param name="calculateAggregatedPermittedDataMethod">The calculate aggregated permitted data method.</param>
        public AggregatedPermittedDataDynamicCache(CalculateAggregatedPermittedDataMethod calculateAggregatedPermittedDataMethod)
        {
            _calculateAggregatedPermittedDataMethod = calculateAggregatedPermittedDataMethod;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AggregatedPermittedDataDynamicCache"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// Gets the aggregated permitted data.
        /// </summary>
        /// <param name="applicationFunction">The application function.</param>
        /// <returns></returns>
        public IAvailableData AggregatedPermittedData(IApplicationFunction applicationFunction)
        {
            if (!_enabled || _cachedAvailableData == null || !applicationFunction.Equals(_applicationFunction))
            {
                _applicationFunction = applicationFunction;
                _cachedAvailableData = _calculateAggregatedPermittedDataMethod(applicationFunction);
            }
            return _cachedAvailableData;
        }

        /// <summary>
        /// Deletes the cached data.
        /// </summary>
        public void DeleteCache()
        {
            _cachedAvailableData = null;
        }
    }
}
