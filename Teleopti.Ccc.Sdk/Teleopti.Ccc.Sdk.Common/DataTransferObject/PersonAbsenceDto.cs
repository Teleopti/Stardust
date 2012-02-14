using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// A person absence detail. Represents one absence layer.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PersonAbsenceDto : Dto
    {
        /// <summary>
        /// Gets or sets the absence layer details.
        /// </summary>
        [DataMember]
        public AbsenceLayerDto AbsenceLayer { get; set; }

        /// <summary>
        /// Gets or sets the version for check of optimistic locks.
        /// </summary>
        [DataMember]
        public int Version { get; set; }
    }
}