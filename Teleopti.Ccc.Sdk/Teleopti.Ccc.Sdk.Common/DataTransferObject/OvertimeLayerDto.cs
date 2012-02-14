using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Sdk.Common.Contracts;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    /// <summary>
    /// Represents an OvertimeLayerDto object.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class OvertimeLayerDto : ActivityLayerDto
    {
        /// <summary>
        /// Gets or sets the id of the overtime definition set.
        /// </summary>
        /// <remarks>This can be matched with the results from <see cref="ITeleoptiOrganizationService.GetOvertimeDefinitions"/>.</remarks>
        [DataMember]
        public Guid OvertimeDefinitionSetId { get; set; }
    }
}