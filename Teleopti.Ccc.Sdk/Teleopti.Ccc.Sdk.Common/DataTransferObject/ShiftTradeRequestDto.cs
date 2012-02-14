using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents a ShiftTradeRequestDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ShiftTradeRequestDto : RequestDto
    {
        private IList<ShiftTradeSwapDetailDto> _shiftTradeSwapDetails = new List<ShiftTradeSwapDetailDto>();

        /// <summary>
        /// Gets or sets the request status.
        /// </summary>
        /// <value>The request status.</value>
        [DataMember]
        public ShiftTradeStatusDto ShiftTradeStatus { get; set; }

        /// <summary>
        /// Gets or sets the type description.
        /// </summary>
        /// <value>The type description.</value>
        [DataMember]
        public string TypeDescription { get; set; }

        /// <summary>
        /// Gets or sets the shift trade swap details.
        /// </summary>
        /// <value>The shift trade swap details.</value>
        [DataMember]
        public IList<ShiftTradeSwapDetailDto> ShiftTradeSwapDetails
        {
            get { return _shiftTradeSwapDetails; }
            private set
            {
                if (value!=null)
                {
                    _shiftTradeSwapDetails = new List<ShiftTradeSwapDetailDto>(value);
                }
            }
        }
    }
}