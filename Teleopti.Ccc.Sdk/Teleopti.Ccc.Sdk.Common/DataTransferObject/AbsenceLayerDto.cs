using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents an AbsenceLayer object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class AbsenceLayerDto : LayerDto
    {
        /// <summary>
        /// Gets or sets the absence.
        /// </summary>
        /// <value>The absence.</value>
        [DataMember]
        public AbsenceDto Absence { get; set;}

    }
}