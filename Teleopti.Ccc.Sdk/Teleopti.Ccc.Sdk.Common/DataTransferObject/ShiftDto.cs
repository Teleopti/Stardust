using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Base class for different kinds of shift containers (<see cref="MainShiftDto"/>, personal shifts and overtime shifts).
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ShiftDto : Dto
    {
        private ICollection<ActivityLayerDto> _layerCollection = new List<ActivityLayerDto>();

        /// <summary>
        /// Gets the collection of layers with details about this shift.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "The setter is used in serialization."), DataMember]
        public ICollection<ActivityLayerDto> LayerCollection
        {
            get { return _layerCollection; }
            private set
            {
                if (value!=null)
                {
                    _layerCollection = new List<ActivityLayerDto>(value);
                }
            }
        }
    }
}