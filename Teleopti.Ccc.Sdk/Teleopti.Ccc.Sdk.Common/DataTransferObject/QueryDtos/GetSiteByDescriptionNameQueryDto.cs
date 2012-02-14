using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get site by name.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetSiteByDescriptionNameQueryDto : QueryDto
    {
        /// <summary>
        /// Gets and sets site name.
        /// </summary>
        /// <value>The site's name.</value>
        [DataMember]
        public string DescriptionName
        {
            get; set;
        }
    }
}
