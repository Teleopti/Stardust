using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get site by Id.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetSiteByIdQueryDto : QueryDto
    {
        /// <summary>
        /// Gets and sets site Id.
        /// </summary>
        /// <value>The site's Id.</value>
        [DataMember]
        public Guid SiteId
        {
            get; set;
        }
    }
}
