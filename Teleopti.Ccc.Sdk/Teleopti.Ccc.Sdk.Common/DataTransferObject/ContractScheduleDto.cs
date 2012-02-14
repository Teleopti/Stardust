using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// The contract schedule.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/05/")]
    public class ContractScheduleDto : Dto
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the deleted flag.
        /// </summary>
        /// <remarks>When loading contracts from the SDK this flag indicates that the contract schedule should not be used anymore.</remarks>
        [DataMember]
        public bool IsDeleted { get; set; }
    }
}