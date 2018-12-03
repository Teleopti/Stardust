using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TimeLayer
{
    /// <summary>
    /// Class used for visualization of multiplicator definition sets when working with the definitions
    /// </summary>
    public class MultiplicatorLayer : Layer<IMultiplicator>, IMultiplicatorLayer
    {
        private readonly IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;
        private DateTimePeriod _layerOriginalPeriod;

        public MultiplicatorLayer(IMultiplicatorDefinitionSet multiplicatorDefinitionSet, IMultiplicator value, DateTimePeriod period)
            : base(value, period)
        {
            _multiplicatorDefinitionSet = multiplicatorDefinitionSet;
            _layerOriginalPeriod = period;
        }

        public IMultiplicatorDefinitionSet MultiplicatorDefinitionSet
        {
            get { return _multiplicatorDefinitionSet; }
        }

        /// <summary>
        /// Gets the layer original period, for display purposes. 
        /// </summary>
        /// <value>The layer original period.</value>
        public DateTimePeriod LayerOriginalPeriod
        {
            get
            {
                 return _layerOriginalPeriod;
            }
            set
            {
                _layerOriginalPeriod = value;
            }
        }
    }
}
