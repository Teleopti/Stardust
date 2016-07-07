using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get <see cref="TeamDto"/> by name.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetTeamByDescriptionNameQueryDto : QueryDto
    {
        /// <summary>
        /// Gets and sets the mandatory team name to query for.
        /// </summary>
        /// <value>The team's name.</value>
        [DataMember]
        public string DescriptionName
        {
            get; set;
        }
    }
}
