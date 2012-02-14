using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get person by Id.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetPersonByIdQueryDto : QueryDto
    {
        /// <summary>
        /// Gets and sets person Id.
        /// </summary>
        /// <value>The person's Id.</value>
        [DataMember]
        public Guid PersonId
        {
            get; set;
        }
    }
}
