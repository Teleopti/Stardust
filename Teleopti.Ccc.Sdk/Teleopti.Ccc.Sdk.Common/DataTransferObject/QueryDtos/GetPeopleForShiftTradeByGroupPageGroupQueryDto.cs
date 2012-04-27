using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos
{
    /// <summary>
    /// Specify a query to get people available to trade with.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/07/")]
    public class GetPeopleForShiftTradeByGroupPageGroupQueryDto : QueryDto
    {
        /// <summary>
        /// Gets or sets the id of the group inside the group page.
        /// </summary>
        [DataMember]
        public Guid GroupPageGroupId { get; set; }

        /// <summary>
        /// Gets or sets the date to query for.
        /// </summary>
        [DataMember]
        public DateOnlyDto QueryDate { get; set; }

        /// <summary>
        /// Gets or sets the Id of person.
        /// </summary>
        [DataMember]
        public Guid PersonId { get; set; }
    }
}
