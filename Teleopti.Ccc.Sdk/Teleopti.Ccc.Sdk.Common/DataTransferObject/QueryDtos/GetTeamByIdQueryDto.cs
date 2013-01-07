using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get <see cref="TeamDto"/> by Id.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetTeamByIdQueryDto : QueryDto
    {
        /// <summary>
        /// Gets and sets the mandatory team Id.
        /// </summary>
        /// <value>The team's Id.</value>
        [DataMember]
        public Guid TeamId
        {
            get; set;
        }
    }
}
