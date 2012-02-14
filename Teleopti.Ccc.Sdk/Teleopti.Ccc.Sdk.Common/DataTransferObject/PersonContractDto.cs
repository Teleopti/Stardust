using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Details for the person contract information.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class PersonContractDto : Dto
    {
        /// <summary>
        /// Gets or sets the average work time per day.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1041:ProvideObsoleteAttributeMessage"), DataMember, Obsolete]
        public TimeSpan AverageWorkTimePerDay { get; set; }

        /// <summary>
        /// Gets or sets the contract id.
        /// </summary>
        /// <remarks>Can be matched with the result from <see cref="ITeleoptiOrganizationService.GetContracts"/> for more details.</remarks>
        [DataMember]
        public Guid? ContractId { get; set; }

        /// <summary>
        /// Gets or sets the part time percentage id.
        /// </summary>
        /// <remarks>Can be matched with the result from <see cref="ITeleoptiOrganizationService.GetPartTimePercentages"/> for more details.</remarks>
        [DataMember]
        public Guid? PartTimePercentageId { get; set; }

        /// <summary>
        /// Gets or sets the contract schedule id.
        /// </summary>
        /// <remarks>Can be matched with the result from <see cref="ITeleoptiOrganizationService.GetContractSchedules"/> for more details.</remarks>
        [DataMember(IsRequired = false, Order = 1)]
        public Guid? ContractScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the average work time per day.
        /// Use DateTime.Min and add the average work time
        /// </summary>
        /// <value>The average work time.</value>
        [DataMember(IsRequired = false, Order = 1)]
        public DateTime AverageWorkTime { get; set; }
    }
}