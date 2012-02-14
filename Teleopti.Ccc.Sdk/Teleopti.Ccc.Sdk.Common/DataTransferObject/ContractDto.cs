using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Contract details
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class ContractDto : Dto
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContractDto"/> class.
        /// </summary>
        public ContractDto()
        {
            AvailableOvertimeDefinitionSets = new List<Guid>();
        }

        /// <summary>
        /// Gets or sets the decription.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the employment type.
        /// </summary>
        [DataMember]
        public EmploymentType EmploymentType { get; set; }

        /// <summary>
        /// Gets the id's of the available overtime definition sets.
        /// </summary>
        [DataMember]
        public ICollection<Guid> AvailableOvertimeDefinitionSets { get; private set; }

        /// <summary>
        /// Gets or sets the deleted flag.
        /// </summary>
        /// <remarks>When loading contracts from the SDK this flag indicates that the contract should not be used anymore.</remarks>
        [DataMember(IsRequired = false,Order = 1)]
        public bool IsDeleted { get; set; }
    }
}